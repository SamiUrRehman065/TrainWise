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
from app.services.analyzer import analyze_dataset, get_dataset_path, load_dataframe, register_dataset
from app.services.model_factory import ModelFactory
from app.services.recommendation_engine import RecommendationEngine
from app.services.chart_builder import ChartBuilder


def train_model(request: TrainRequest) -> TrainResult:
    file_path = request.filePath or get_dataset_path(request.datasetId)
    if not file_path:
        raise ValueError("Dataset path not found. Please analyze the dataset first.")

    # Register so post-train analysis calls can also find it
    register_dataset(request.datasetId, file_path)

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
        if len(counts) > 1 and counts.min() > 6:
            imbalance_ratio = counts.max() / counts.min()
            if imbalance_ratio > 1.5:
                smote = SMOTE(random_state=42)
                x_train, y_train = smote.fit_resample(x_train, y_train)

    hyperparameters = _build_hyperparameters(request)
    model = ModelFactory.create(request.model.name, request.taskType, **hyperparameters)

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
        cv_model = ModelFactory.create(request.model.name, request.taskType, **hyperparameters)
        
        # SRS FR-5: Ensure CV folds are appropriate for dataset size
        # StratifiedKFold (used for classification) needs at least 'cv' samples per class
        min_samples = 5
        if request.taskType == "classification":
            class_counts = y.value_counts()
            min_samples = class_counts.min()
            
        n_folds = min(request.kFolds, len(x_df), min_samples)
        
        if n_folds > 1:
            try:
                scores = cross_val_score(cv_model, x_df, y, cv=n_folds, scoring=scoring)
                cv_scores = [round(float(s), 4) for s in scores]
                cv_mean = round(float(scores.mean()), 4)
            except Exception as cv_ex:
                print(f"CV Warning: {cv_ex}")
                cv_scores = None
                cv_mean = None

    # Attach Charts
    charts = {}
    if request.taskType == "classification":
        # Get probabilities if available
        y_probs = None
        if hasattr(model, "predict_proba"):
            y_probs = model.predict_proba(x_test)
        
        # Determine labels from y or label_encoder if used
        labels = sorted(y.unique().tolist())
        
        metrics = ClassificationMetrics(
            accuracy=float(accuracy_score(y_test, y_pred)),
            precision=float(precision_score(y_test, y_pred, average="macro", zero_division=0)),
            recall=float(recall_score(y_test, y_pred, average="macro", zero_division=0)),
            f1Score=float(f1_score(y_test, y_pred, average="macro", zero_division=0)),
            confusionMatrix=confusion_matrix(y_test, y_pred).tolist(),
            classLabels=[str(l) for l in labels]
        )
        
        charts["ConfusionMatrix"] = ChartBuilder.build_confusion_matrix(y_test, y_pred, labels)
        charts["ClassComparison"] = ChartBuilder.build_class_comparison(y_test, y_pred, labels)
        
        if y_probs is not None:
            roc = ChartBuilder.build_roc_curve(y_test, y_probs)
            if roc: charts["RocCurve"] = roc
            
            pr = ChartBuilder.build_precision_recall_curve(y_test, y_probs)
            if pr: charts["PrecisionRecall"] = pr
            
            prob = ChartBuilder.build_probability_distribution(y_probs)
            if prob: charts["ProbabilityDist"] = prob
        
        charts["FeatureImportance"] = ChartBuilder.build_feature_importance(model, x_df.columns.tolist())
        if cv_scores:
            cv = ChartBuilder.build_cv_metrics(cv_scores)
            if cv: charts["CrossValidation"] = cv

        result = TrainResult(
            experimentId="",
            modelName=request.model.name,
            taskType=request.taskType,
            classificationMetrics=metrics,
            regressionMetrics=None,
            featureImportances=feature_importances,
            trainingDurationSeconds=round(duration, 4),
            recommendations=[],
            cvScores=cv_scores,
            cvMean=cv_mean,
            charts=charts
        )
    else:
        # Use RMSE helper or legacy squared=False
        try:
            from sklearn.metrics import root_mean_squared_error
            rmse_val = float(root_mean_squared_error(y_test, y_pred))
        except ImportError:
            rmse_val = float(mean_squared_error(y_test, y_pred, squared=False))

        metrics = RegressionMetrics(
            r2Score=float(r2_score(y_test, y_pred)),
            rmse=rmse_val,
            mae=float(mean_absolute_error(y_test, y_pred)),
            mse=float(mean_squared_error(y_test, y_pred)),
        )
        
        charts["ActualVsPredicted"] = ChartBuilder.build_actual_vs_predicted(y_test, y_pred)
        charts["ResidualsScatter"] = ChartBuilder.build_residuals_scatter(y_test, y_pred)
        charts["ResidualsDistribution"] = ChartBuilder.build_residuals_distribution(y_test, y_pred)
        
        qq = ChartBuilder.build_qq_plot(y_test, y_pred)
        if qq: charts["QqPlot"] = qq
        
        charts["FeatureImportance"] = ChartBuilder.build_feature_importance(model, x_df.columns.tolist())
        if cv_scores:
            cv = ChartBuilder.build_cv_metrics(cv_scores)
            if cv: charts["CrossValidation"] = cv

        result = TrainResult(
            experimentId="",
            modelName=request.model.name,
            taskType=request.taskType,
            classificationMetrics=None,
            regressionMetrics=metrics,
            featureImportances=feature_importances,
            trainingDurationSeconds=round(duration, 4),
            recommendations=[],
            cvScores=cv_scores,
            cvMean=cv_mean,
            charts=charts
        )

    # Post-train recommendations
    analysis = analyze_dataset(file_path, request.datasetId)
    engine = RecommendationEngine()
    metrics_payload = _metrics_payload(result)
    config_payload = request.model_dump()
    analysis_payload = analysis.model_dump()
    result.recommendations = engine.evaluate_all(metrics_payload, config_payload, analysis_payload)
    
    # Persist Model
    import joblib
    import os
    models_dir = os.path.join(os.getcwd(), "models_store")
    os.makedirs(models_dir, exist_ok=True)
    
    # Unique name based on dataset and timestamp to avoid collisions
    model_filename = f"model_{request.datasetId}_{int(time.time())}.joblib"
    model_path = os.path.join(models_dir, model_filename)
    joblib.dump(model, model_path)
    
    # Store path in result for later retrieval
    result.modelPath = model_path
    
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
    categorical_cols = [col for col in df.columns if df[col].dtype == object or df[col].dtype.name == 'category']
    if not categorical_cols:
        return df

    result = df.copy()
    
    # Automatically drop identifiers/names and high-cardinality features
    cols_to_drop = []
    n_rows = len(result)
    for col in categorical_cols:
        n_unique = result[col].nunique()
        col_lower = col.lower()
        # Drop if it's an ID or Name column, or if it has >50 unique values (high cardinality)
        if "id" in col_lower or "name" in col_lower or n_unique > 50:
            cols_to_drop.append(col)
            
    if cols_to_drop:
        result = result.drop(columns=cols_to_drop)
        categorical_cols = [c for c in categorical_cols if c not in cols_to_drop]

    if not categorical_cols:
        return result

    if encoding == "onehot":
        return pd.get_dummies(result, columns=categorical_cols)

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
        "alpha": hyper.alpha,
        "max_depth": hyper.max_depth,
    }
    params = {key: value for key, value in params.items() if value is not None}
    
    if hyper.additionalParams:
        params.update(hyper.additionalParams)
        
    return params


def _extract_feature_importances(model: Any, columns: list[str]) -> Dict[str, float] | None:
    if hasattr(model, "feature_importances_"):
        values = getattr(model, "feature_importances_")
        return {column: float(value) for column, value in zip(columns, values)}
    elif hasattr(model, "coef_"):
        coefs = getattr(model, "coef_")
        if getattr(coefs, "ndim", 1) == 2:
            values = np.abs(coefs).mean(axis=0)
        else:
            values = np.abs(coefs)
        total = np.sum(values)
        if total > 0:
            values = values / total
        return {column: float(value) for column, value in zip(columns, values)}
    return None


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
        mse=float(mean_squared_error(y_test, y_pred)),
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
