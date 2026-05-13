from __future__ import annotations

import time
from typing import Any, Dict

import numpy as np
import pandas as pd
from imblearn.over_sampling import SMOTE
from sklearn.metrics import (
    accuracy_score,
    confusion_matrix,
    f1_score,
    mean_absolute_error,
    mean_squared_error,
    precision_score,
    r2_score,
    recall_score,
)
from sklearn.model_selection import cross_val_score, train_test_split
from sklearn.preprocessing import LabelEncoder, MinMaxScaler, StandardScaler

from app.models.train_request import TrainRequest
from app.models.train_result import TrainResult, ClassificationMetrics, RegressionMetrics
from app.services.analyzer import analyze_dataset, get_dataset_path, load_dataframe
from app.services.model_factory import ModelFactory
from app.services.recommendation_engine import RecommendationEngine


def train_model(request: TrainRequest) -> TrainResult:
    file_path = get_dataset_path(request.datasetId)
    if not file_path:
        raise ValueError("Dataset path not found. Please analyze the dataset first.")

    df = load_dataframe(file_path)
    if request.targetColumn not in df.columns:
        raise ValueError("Target column not found in dataset.")

    df = _apply_exclusions(df, request.targetColumn, request.preprocessing.excludeColumns)
    df = _apply_null_strategy(df, request.preprocessing.nullStrategy)
    df = df.dropna(subset=[request.targetColumn])

    y = df[request.targetColumn]
    x_df = df.drop(columns=[request.targetColumn])

    x_df = _encode_categoricals(x_df, request.preprocessing.encoding)
    x_df = _scale_features(x_df, request.preprocessing.scaling)

    label_encoder = None
    if request.taskType == "classification" and not np.issubdtype(y.dtype, np.number):
        label_encoder = LabelEncoder()
        y = pd.Series(label_encoder.fit_transform(y.astype(str)), index=y.index)

    train_ratio = request.trainTestSplit
    if train_ratio <= 0 or train_ratio >= 1:
        train_ratio = 0.8
    test_size = 1 - train_ratio

    x_train, x_test, y_train, y_test = train_test_split(
        x_df, y, test_size=test_size, random_state=42
    )

    if request.taskType == "classification" and request.preprocessing.applySmote:
        counts = y_train.value_counts()
        if len(counts) > 1:
            imbalance_ratio = counts.max() / counts.min()
            if imbalance_ratio > 1.5:
                smote = SMOTE(random_state=42)
                x_train, y_train = smote.fit_resample(x_train, y_train)

    hyperparameters = _build_hyperparameters(request)
    model = ModelFactory.create(request.model.name, **hyperparameters)

    start_time = time.perf_counter()
    model.fit(x_train, y_train)
    duration = time.perf_counter() - start_time

    y_pred = model.predict(x_test)
    feature_importances = _extract_feature_importances(model, x_df.columns)

    # Optional 5-fold cross-validation (SRS FR-5)
    cv_scores = None
    cv_mean = None
    if request.crossValidation:
        scoring = "accuracy" if request.taskType == "classification" else "r2"
        cv_model = ModelFactory.create(request.model.name, **hyperparameters)
        scores = cross_val_score(cv_model, x_df, y, cv=5, scoring=scoring)
        cv_scores = [round(float(s), 4) for s in scores]
        cv_mean = round(float(scores.mean()), 4)

    result = _build_result(
        request,
        y_test,
        y_pred,
        duration,
        feature_importances,
        cv_scores,
        cv_mean,
    )

    analysis = analyze_dataset(file_path, request.datasetId)
    engine = RecommendationEngine()
    metrics_payload = _metrics_payload(result)
    config_payload = request.model_dump()
    analysis_payload = analysis.model_dump()
    recommendations = engine.evaluate_all(metrics_payload, config_payload, analysis_payload)
    result.recommendations = recommendations
    return result


def _apply_exclusions(df: pd.DataFrame, target_column: str, exclusions: list[str]) -> pd.DataFrame:
    drop_columns = [col for col in exclusions if col in df.columns and col != target_column]
    if not drop_columns:
        return df
    return df.drop(columns=drop_columns)


def _apply_null_strategy(df: pd.DataFrame, strategy: str) -> pd.DataFrame:
    if strategy == "drop":
        return df.dropna()

    result = df.copy()
    numeric_cols = result.select_dtypes(include=[np.number]).columns
    categorical_cols = [col for col in result.columns if col not in numeric_cols]

    if strategy in {"mean", "median"}:
        for col in numeric_cols:
            fill_value = result[col].mean() if strategy == "mean" else result[col].median()
            result[col] = result[col].fillna(fill_value)
        for col in categorical_cols:
            result[col] = _fill_mode(result[col])
        return result

    if strategy == "mode":
        for col in result.columns:
            result[col] = _fill_mode(result[col])
        return result

    return result


def _fill_mode(series: pd.Series) -> pd.Series:
    if series.dropna().empty:
        return series.fillna("")
    mode_value = series.mode(dropna=True).iloc[0]
    return series.fillna(mode_value)


def _encode_categoricals(df: pd.DataFrame, encoding: str) -> pd.DataFrame:
    categorical_cols = [col for col in df.columns if df[col].dtype == object]
    if not categorical_cols:
        return df

    if encoding == "onehot":
        return pd.get_dummies(df, columns=categorical_cols)

    result = df.copy()
    for col in categorical_cols:
        result[col] = pd.factorize(result[col])[0]
    return result


def _scale_features(df: pd.DataFrame, scaling: str) -> pd.DataFrame:
    if scaling == "none":
        return df

    scaler = StandardScaler() if scaling == "standard" else MinMaxScaler()
    scaled = scaler.fit_transform(df.values)
    return pd.DataFrame(scaled, columns=df.columns, index=df.index)


def _build_hyperparameters(request: TrainRequest) -> Dict[str, Any]:
    hyper = request.model.hyperparameters
    params = {
        "n_estimators": hyper.n_estimators,
        "C": hyper.C,
        "kernel": hyper.kernel,
        "n_neighbors": hyper.n_neighbors,
        "max_iter": hyper.max_iter,
    }
    return {key: value for key, value in params.items() if value is not None}


def _extract_feature_importances(model: Any, columns: list[str]) -> Dict[str, float] | None:
    if not hasattr(model, "feature_importances_"):
        return None
    values = getattr(model, "feature_importances_")
    return {column: float(value) for column, value in zip(columns, values)}


def _build_result(
    request: TrainRequest,
    y_test: pd.Series,
    y_pred: np.ndarray,
    duration: float,
    feature_importances: Dict[str, float] | None,
    cv_scores: list[float] | None = None,
    cv_mean: float | None = None,
) -> TrainResult:
    if request.taskType == "classification":
        metrics = ClassificationMetrics(
            accuracy=float(accuracy_score(y_test, y_pred)),
            precision=float(precision_score(y_test, y_pred, average="macro", zero_division=0)),
            recall=float(recall_score(y_test, y_pred, average="macro", zero_division=0)),
            f1Score=float(f1_score(y_test, y_pred, average="macro", zero_division=0)),
            confusionMatrix=confusion_matrix(y_test, y_pred).tolist(),
        )
        return TrainResult(
            experimentId="",
            modelName=request.model.name,
            taskType=request.taskType,
            classificationMetrics=metrics,
            regressionMetrics=None,
            featureImportances=feature_importances,
            trainingDurationSeconds=round(duration, 4),
            recommendations=[],
            crossValidationScores=cv_scores,
            crossValidationMean=cv_mean,
        )

    metrics = RegressionMetrics(
        r2Score=float(r2_score(y_test, y_pred)),
        rmse=float(mean_squared_error(y_test, y_pred, squared=False)),
        mae=float(mean_absolute_error(y_test, y_pred)),
    )
    return TrainResult(
        experimentId="",
        modelName=request.model.name,
        taskType=request.taskType,
        classificationMetrics=None,
        regressionMetrics=metrics,
        featureImportances=feature_importances,
        trainingDurationSeconds=round(duration, 4),
        recommendations=[],
        crossValidationScores=cv_scores,
        crossValidationMean=cv_mean,
    )


def _metrics_payload(result: TrainResult) -> Dict[str, Any]:
    payload: Dict[str, Any] = {}
    if result.classificationMetrics is not None:
        payload["classificationMetrics"] = result.classificationMetrics.model_dump()
    if result.regressionMetrics is not None:
        payload["regressionMetrics"] = result.regressionMetrics.model_dump()
    return payload
