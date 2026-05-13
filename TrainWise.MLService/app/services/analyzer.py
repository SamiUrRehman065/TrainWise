from __future__ import annotations

from pathlib import Path
from typing import Dict, Optional

import numpy as np
import pandas as pd
from pandas.api.types import is_bool_dtype, is_datetime64_any_dtype, is_numeric_dtype

from app.models.dataset_summary import DatasetSummary, MissingValueInfo, SummaryStats
from app.models.eda_report import EdaReport, OutlierInfo, FeatureCorrelation

_DATASET_PATHS: Dict[str, str] = {}


def register_dataset(dataset_id: str, file_path: str) -> None:
    _DATASET_PATHS[dataset_id] = file_path


def get_dataset_path(dataset_id: str) -> Optional[str]:
    return _DATASET_PATHS.get(dataset_id)


def load_dataframe(file_path: str) -> pd.DataFrame:
    suffix = Path(file_path).suffix.lower()
    if suffix == ".csv":
        df = pd.read_csv(file_path)
    elif suffix in {".xlsx", ".xls"}:
        df = pd.read_excel(file_path)
    else:
        raise ValueError("Unsupported file type.")

    df.columns = [str(col) for col in df.columns]
    return df


def analyze_dataset(file_path: str, dataset_id: str) -> DatasetSummary:
    register_dataset(dataset_id, file_path)
    df = load_dataframe(file_path)

    row_count = int(df.shape[0])
    column_count = int(df.shape[1])

    column_types: Dict[str, str] = {}
    for column in df.columns:
        series = df[column]
        if is_numeric_dtype(series):
            column_types[column] = "numerical"
        elif is_datetime64_any_dtype(series):
            column_types[column] = "datetime"
        else:
            if series.dtype == object:
                parsed = pd.to_datetime(series, errors="coerce")
                if parsed.notna().mean() >= 0.8:
                    column_types[column] = "datetime"
                else:
                    column_types[column] = "categorical"
            elif is_bool_dtype(series):
                column_types[column] = "categorical"
            else:
                column_types[column] = "categorical"

    missing_values: Dict[str, MissingValueInfo] = {}
    for column in df.columns:
        count = int(df[column].isna().sum())
        percentage = (count / row_count * 100) if row_count > 0 else 0.0
        missing_values[column] = MissingValueInfo(count=count, percentage=round(percentage, 4))

    summary_stats: Dict[str, SummaryStats] = {}
    numeric_columns = df.select_dtypes(include=[np.number]).columns.tolist()
    for column in numeric_columns:
        series = df[column].dropna()
        if series.empty:
            continue
        summary_stats[column] = SummaryStats(
            mean=float(series.mean()),
            median=float(series.median()),
            std=float(series.std(ddof=0)),
            min=float(series.min()),
            max=float(series.max()),
        )

    class_distribution = None
    imbalance_ratio = None
    target_column = _infer_target_column(df)
    if target_column:
        target_series = df[target_column]
        if _is_classification_target(target_series):
            counts = target_series.value_counts(dropna=False)
            class_distribution = {str(k): int(v) for k, v in counts.to_dict().items()}
            if len(counts) > 1:
                min_count = int(counts.min())
                max_count = int(counts.max())
                if min_count > 0:
                    imbalance_ratio = round(max_count / min_count, 4)

    correlation_matrix: list[list[float]] = []
    if numeric_columns:
        corr = df[numeric_columns].corr(method="pearson").fillna(0)
        correlation_matrix = corr.values.tolist()

    return DatasetSummary(
        datasetId=dataset_id,
        rowCount=row_count,
        columnCount=column_count,
        columnTypes=column_types,
        missingValues=missing_values,
        summaryStats=summary_stats,
        classDistribution=class_distribution,
        correlationMatrix=correlation_matrix,
        imbalanceRatio=imbalance_ratio,
    )


def generate_eda_report(dataset_id: str, target_column: Optional[str] = None, file_path: Optional[str] = None) -> EdaReport:
    file_path = file_path or get_dataset_path(dataset_id)
    if not file_path:
        raise ValueError("Dataset path not found. Re-run analyze for this dataset.")

    df = load_dataframe(file_path)
    numeric_columns = df.select_dtypes(include=[np.number]).columns.tolist()
    categorical_columns = [c for c in df.columns if c not in numeric_columns]

    duplicate_rows = int(df.duplicated().sum())
    duplicate_percentage = round((duplicate_rows / len(df) * 100), 4) if len(df) else 0.0
    memory_usage_mb = round(float(df.memory_usage(deep=True).sum() / (1024 * 1024)), 4)

    outliers_by_column: Dict[str, OutlierInfo] = {}
    for col in numeric_columns:
        series = df[col].dropna()
        if series.empty:
            outliers_by_column[col] = OutlierInfo(count=0, percentage=0.0)
            continue
        q1 = series.quantile(0.25)
        q3 = series.quantile(0.75)
        iqr = q3 - q1
        lower = q1 - 1.5 * iqr
        upper = q3 + 1.5 * iqr
        count = int(((series < lower) | (series > upper)).sum())
        pct = round((count / len(series) * 100), 4) if len(series) else 0.0
        outliers_by_column[col] = OutlierInfo(count=count, percentage=pct)

    target_distribution = None
    top_target_correlations: List[FeatureCorrelation] = []
    if target_column and target_column in df.columns:
        target_series = df[target_column]
        if not is_numeric_dtype(target_series):
            counts = target_series.astype(str).value_counts(dropna=False).head(20)
            target_distribution = {str(k): int(v) for k, v in counts.to_dict().items()}
        elif target_column in numeric_columns:
            corr = df[numeric_columns].corr(method="pearson")[target_column].dropna().drop(labels=[target_column], errors="ignore")
            top_corr = corr.abs().sort_values(ascending=False).head(10)
            for feature in top_corr.index:
                top_target_correlations.append(
                    FeatureCorrelation(feature=str(feature), correlation=round(float(corr[feature]), 4))
                )

    return EdaReport(
        datasetId=dataset_id,
        targetColumn=target_column,
        numericColumns=[str(c) for c in numeric_columns],
        categoricalColumns=[str(c) for c in categorical_columns],
        duplicateRows=duplicate_rows,
        duplicatePercentage=duplicate_percentage,
        memoryUsageMb=memory_usage_mb,
        outliersByColumn=outliers_by_column,
        topTargetCorrelations=top_target_correlations,
        targetDistribution=target_distribution
    )


def _infer_target_column(df: pd.DataFrame) -> Optional[str]:
    if df.empty:
        return None

    candidates = [
        column for column in df.columns
        if str(column).lower() in {"target", "label", "class", "y"}
    ]
    if candidates:
        return candidates[-1]

    return str(df.columns[-1]) if len(df.columns) > 0 else None


def _is_classification_target(series: pd.Series) -> bool:
    if is_numeric_dtype(series):
        unique_values = series.dropna().nunique()
        if unique_values <= 20:
            return True
        if len(series) > 0 and (unique_values / len(series)) <= 0.1:
            return True
        return False

    return True
