import numpy as np
import pandas as pd
from typing import Dict, List, Optional, Tuple
from pathlib import Path
from scipy import stats
from sklearn.feature_selection import mutual_info_classif, mutual_info_regression
from sklearn.preprocessing import LabelEncoder
from app.models.dataset_intelligence import (
    DatasetIntelligence, DatasetOverview, FeatureProfile, CategoryFreq,
    CorrelationAnalysis, FeatureCorrelation, MissingValueAnalysis,
    TargetAnalysis, DataQualityReport, DataQualityIssue, MlReadinessReport
)
from app.services.analyzer import load_dataframe, _infer_target_column, _is_classification_target

def compute_intelligence(file_path: str, dataset_id: str) -> DatasetIntelligence:
    df = load_dataframe(file_path)
    
    # 1. Overview
    overview = _compute_overview(df, file_path)
    
    # 2. Features
    target_col = _infer_target_column(df)
    
    # Optional: MI Scores (expensive, so sample for large datasets)
    mi_scores = _compute_mutual_info(df, target_col)
    
    features = _compute_feature_profiles(df, target_col, mi_scores)
    
    # 3. Correlations
    correlations = _compute_correlations(df)
    
    # 4. Missing Values
    missing = _compute_missing_analysis(df)
    
    # 5. Target Analysis
    target_analysis = _compute_target_analysis(df, target_col)
    
    # 6. Quality Report
    quality = _compute_quality_report(df, features, overview, correlations)
    
    # 7. ML Readiness
    readiness = _compute_ml_readiness(overview, features, quality, target_analysis)
    
    return DatasetIntelligence(
        datasetId=dataset_id,
        overview=overview,
        features=features,
        correlations=correlations,
        missingValues=missing,
        targetAnalysis=target_analysis,
        qualityReport=quality,
        mlReadiness=readiness
    )

def _compute_overview(df: pd.DataFrame, file_path: str) -> DatasetOverview:
    row_count = len(df)
    col_count = len(df.columns)
    total_cells = row_count * col_count
    missing_cells = int(df.isna().sum().sum())
    
    type_counts = {"numerical": 0, "categorical": 0, "datetime": 0}
    for col in df.columns:
        if pd.api.types.is_numeric_dtype(df[col]):
            type_counts["numerical"] += 1
        elif pd.api.types.is_datetime64_any_dtype(df[col]):
            type_counts["datetime"] += 1
        else:
            type_counts["categorical"] += 1
            
    target_col = _infer_target_column(df)
    problem_type = "Unknown"
    if target_col:
        target_series = df[target_col]
        if _is_classification_target(target_series):
            unique_count = target_series.nunique()
            problem_type = "Binary Classification" if unique_count == 2 else "Multi-class Classification"
        else:
            problem_type = "Regression"

    return DatasetOverview(
        rowCount=row_count,
        columnCount=col_count,
        sizeBytes=Path(file_path).stat().st_size,
        duplicateRows=int(df.duplicated().sum()),
        duplicatePercentage=round((df.duplicated().sum() / row_count * 100), 2) if row_count > 0 else 0,
        totalCells=total_cells,
        missingCells=missing_cells,
        missingPercentage=round((missing_cells / total_cells * 100), 2) if total_cells > 0 else 0,
        typeDistribution=type_counts,
        targetColumn=target_col or "None",
        problemType=problem_type,
        sparsityScore=round((df.isna().sum().sum() + (df == 0).sum().sum()) / total_cells, 4) if total_cells > 0 else 0
    )

def _compute_feature_profiles(df: pd.DataFrame, target_col: Optional[str], mi_scores: Dict[str, float]) -> List[FeatureProfile]:
    profiles = []
    for col in df.columns:
        series = df[col]
        is_target = col == target_col
        is_num = pd.api.types.is_numeric_dtype(series)
        
        relevance = mi_scores.get(str(col), 0.0)
        
        # Stats
        stats_dict = {}
        hist_values = None
        hist_labels = None
        top_cats = None
        
        if is_num:
            clean_series = series.dropna()
            if not clean_series.empty:
                stats_dict = {
                    "mean": float(clean_series.mean()),
                    "median": float(clean_series.median()),
                    "std": float(clean_series.std()),
                    "min": float(clean_series.min()),
                    "max": float(clean_series.max()),
                    "skew": float(clean_series.skew()),
                    "kurtosis": float(clean_series.kurtosis()),
                    "zeros": float((clean_series == 0).sum())
                }
                # Histogram
                counts, bins = np.histogram(clean_series, bins=10)
                hist_values = [float(c) for c in counts]
                hist_labels = [f"{bins[i]:.2f}-{bins[i+1]:.2f}" for i in range(len(bins)-1)]
        else:
            # Categorical
            counts = series.value_counts().head(10)
            top_cats = [CategoryFreq(category=str(k), count=int(v)) for k, v in counts.items()]
            
        unique_count = int(series.nunique())
        missing_count = int(series.isna().sum())
        
        # Simple Outlier Detection (IQR)
        outlier_count = 0
        if is_num and not clean_series.empty:
            q1 = clean_series.quantile(0.25)
            q3 = clean_series.quantile(0.75)
            iqr = q3 - q1
            outlier_count = int(((clean_series < (q1 - 1.5 * iqr)) | (clean_series > (q3 + 1.5 * iqr))).sum())

        profiles.append(FeatureProfile(
            name=str(col),
            type="numerical" if is_num else "categorical",
            isTarget=is_target,
            isCandidateId=(unique_count == len(df) and not is_num),
            isConstant=(unique_count == 1),
            relevanceScore=relevance,
            stats=stats_dict,
            uniqueCount=unique_count,
            uniqueRatio=round(unique_count / len(df), 4) if len(df) > 0 else 0,
            missingCount=missing_count,
            missingPercentage=round(missing_count / len(df) * 100, 2) if len(df) > 0 else 0,
            outlierCount=outlier_count,
            outlierPercentage=round(outlier_count / len(df) * 100, 2) if len(df) > 0 else 0,
            histogramValues=hist_values,
            histogramLabels=hist_labels,
            topCategories=top_cats,
            alerts=_generate_feature_alerts(series, is_num, unique_count, missing_count, outlier_count),
            recommendations=_generate_feature_recs(series, is_num, unique_count, missing_count)
        ))
    return profiles

def _generate_feature_alerts(series, is_num, unique_count, missing_count, outlier_count) -> List[str]:
    alerts = []
    if missing_count / len(series) > 0.1: alerts.append("High missing values")
    if unique_count == 1: alerts.append("Constant column (no information)")
    if is_num and outlier_count / len(series) > 0.05: alerts.append("Significant outliers detected")
    if not is_num and unique_count / len(series) > 0.5 and unique_count > 20: alerts.append("High cardinality")
    return alerts

def _generate_feature_recs(series, is_num, unique_count, missing_count) -> List[str]:
    recs = []
    if missing_count > 0:
        recs.append("Apply imputation" if missing_count / len(series) < 0.4 else "Consider dropping due to high missingness")
    if not is_num and unique_count > 1:
        recs.append("Use One-Hot Encoding" if unique_count < 10 else "Use Target/Label Encoding")
    if is_num and abs(series.skew()) > 1:
        recs.append("Consider Log Transformation")
    return recs

def _compute_correlations(df: pd.DataFrame) -> CorrelationAnalysis:
    nums = df.select_dtypes(include=[np.number])
    if nums.empty:
        return CorrelationAnalysis(columns=[], matrix=[], highCorrelations=[])
    
    corr = nums.corr().fillna(0)
    high_corr = []
    for i in range(len(corr.columns)):
        for j in range(i+1, len(corr.columns)):
            val = corr.iloc[i, j]
            if abs(val) > 0.8:
                high_corr.append(FeatureCorrelation(feature=f"{corr.columns[i]} vs {corr.columns[j]}", correlation=round(float(val), 4)))
                
    return CorrelationAnalysis(
        columns=corr.columns.tolist(),
        matrix=corr.values.tolist(),
        highCorrelations=high_corr
    )

def _compute_missing_analysis(df: pd.DataFrame) -> MissingValueAnalysis:
    missing = df.isna().sum().to_dict()
    recs = []
    if df.isna().sum().sum() > 0:
        recs.append("Check for MNAR patterns if missingness is structured")
    return MissingValueAnalysis(
        missingByColumn={str(k): int(v) for k, v in missing.items()},
        patterns=["Random" if df.isna().sum().sum() > 0 else "None"],
        recommendations=recs
    )

def _compute_target_analysis(df: pd.DataFrame, target_col: Optional[str]) -> Optional[TargetAnalysis]:
    if not target_col or target_col not in df.columns:
        return None
    
    series = df[target_col]
    is_num = pd.api.types.is_numeric_dtype(series)
    
    stats_dict = {}
    dist = None
    if is_num:
        stats_dict = {"mean": float(series.mean()), "std": float(series.std()), "skew": float(series.skew())}
    else:
        dist = {str(k): int(v) for k, v in series.value_counts().head(20).items()}
        
    return TargetAnalysis(
        name=str(target_col),
        type="numerical" if is_num else "categorical",
        stats=stats_dict,
        distribution=dist,
        insights=["Highly skewed" if is_num and abs(series.skew()) > 1 else "Balanced classes" if not is_num and series.nunique() > 1 else "Check distribution"]
    )

def _compute_quality_report(df, features, overview, correlations) -> DataQualityReport:
    score = 100.0
    issues = []
    
    # Penalties
    if overview.missingPercentage > 5:
        score -= 10
        issues.append(DataQualityIssue(severity="Warning", type="Missing Values", message=f"Dataset has {overview.missingPercentage}% missing cells."))
    
    if overview.duplicatePercentage > 1:
        score -= 5
        issues.append(DataQualityIssue(severity="Info", type="Duplicates", message=f"{overview.duplicatePercentage}% duplicate rows detected."))
        
    for f in features:
        if f.isConstant:
            score -= 2
            issues.append(DataQualityIssue(severity="Warning", type="Constant Column", message=f"Column {f.name} provides no information.", column=f.name))
            
    return DataQualityReport(
        overallScore=max(0, score),
        riskLevel="Low" if score > 80 else "Medium" if score > 50 else "High",
        issues=issues
    )

def _compute_ml_readiness(overview, features, quality, target_analysis) -> MlReadinessReport:
    is_ready = quality.overallScore > 70
    preproc = []
    if overview.missingPercentage > 0: preproc.append("Imputation required")
    if overview.typeDistribution["categorical"] > 0: preproc.append("Categorical encoding required")
    
    models = ["Random Forest", "XGBoost"] if overview.problemType != "Regression" else ["Linear Regression", "Ridge"]
    
    return MlReadinessReport(
        isReady=is_ready,
        requiredPreprocessing=preproc,
        suitableModels=models,
        actionableSteps=["Handle missing values", "Encode categories"]
    )

def _compute_mutual_info(df: pd.DataFrame, target_col: Optional[str]) -> Dict[str, float]:
    if not target_col or target_col not in df.columns:
        return {}
        
    try:
        # Sample if too large for speed
        work_df = df.sample(min(5000, len(df))) if len(df) > 5000 else df.copy()
        
        # Preprocessing for MI
        X = work_df.drop(columns=[target_col])
        y = work_df[target_col]
        
        # Simple encoding for categoricals in X
        for col in X.columns:
            if not pd.api.types.is_numeric_dtype(X[col]):
                X[col] = LabelEncoder().fit_transform(X[col].astype(str))
        X = X.fillna(0)
        
        # Target encoding
        if not pd.api.types.is_numeric_dtype(y):
            y = LabelEncoder().fit_transform(y.astype(str))
            scores = mutual_info_classif(X, y)
        else:
            y = y.fillna(y.mean())
            scores = mutual_info_regression(X, y)
            
        # Normalize 0-1
        if scores.max() > 0:
            scores = scores / scores.max()
            
        return {str(col): float(s) for col, s in zip(X.columns, scores)}
    except Exception as e:
        print(f"MI Error: {e}")
        return {}
