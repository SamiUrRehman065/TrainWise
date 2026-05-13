from typing import Any, Dict, Optional, List
from app.models.recommendation import Recommendation

class RecommendationEngine:
    def evaluate_all(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> List[Recommendation]:
        rules = [
            self.rule_r01_missing_values,
            self.rule_r02_class_imbalance,
            self.rule_r03_overfitting_cv,
            self.rule_r04_low_f1,
            self.rule_r05_low_estimators,
            self.rule_r06_high_rmse,
            self.rule_r07_small_dataset,
            self.rule_r08_no_scaling_for_svm_knn,
        ]
        recommendations: List[Recommendation] = []
        for rule in rules:
            result = rule(metrics, config, analysis)
            if result is not None:
                recommendations.append(result)
        return recommendations

    def rule_r01_missing_values(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        missing_values = analysis.get("missingValues", {})
        for column, info in missing_values.items():
            pct = (info or {}).get("percentage")
            if pct is not None and pct > 20:
                return Recommendation(
                    ruleId="R-01",
                    message=(
                        f"High missing data detected in {column}. "
                        "Consider dropping this column or using model-based imputation."
                    ),
                )
        return None

    def rule_r02_class_imbalance(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        imbalance_ratio = analysis.get("imbalanceRatio")
        apply_smote = config.get("preprocessing", {}).get("applySmote", False)
        if imbalance_ratio is not None and imbalance_ratio > 2 and not apply_smote:
            return Recommendation(
                ruleId="R-02",
                message="Dataset imbalance detected. Enable SMOTE oversampling to improve minority-class recall.",
            )
        return None

    def rule_r03_overfitting_cv(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        accuracy = metrics.get("classificationMetrics", {}).get("accuracy")
        cross_validation = config.get("crossValidation", False)
        if accuracy is not None and accuracy > 0.98 and not cross_validation:
            return Recommendation(
                ruleId="R-03",
                message="Near-perfect accuracy may indicate overfitting. Enable 5-fold cross-validation to verify.",
            )
        return None

    def rule_r04_low_f1(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        f1 = metrics.get("classificationMetrics", {}).get("f1Score")
        if f1 is not None and f1 < 0.60:
            return Recommendation(
                ruleId="R-04",
                message="Low F1-Score. Try feature scaling, a different kernel (SVM), or increase training data.",
            )
        return None

    def rule_r05_low_estimators(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        model_name = config.get("model", {}).get("name", "")
        n_estimators = config.get("model", {}).get("hyperparameters", {}).get("n_estimators")
        if "RandomForest" in model_name and n_estimators is not None and n_estimators < 50:
            return Recommendation(
                ruleId="R-05",
                message="Low tree count detected. Increase n_estimators to >= 100 for more stable predictions.",
            )
        return None

    def rule_r06_high_rmse(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        rmse = metrics.get("regressionMetrics", {}).get("rmse")
        target_column = config.get("targetColumn")
        std = None
        summary_stats = analysis.get("summaryStats", {})
        if target_column in summary_stats:
            std = summary_stats[target_column].get("std")
        if rmse is not None and std is not None and rmse > std:
            return Recommendation(
                ruleId="R-06",
                message="High RMSE relative to target spread. Consider feature engineering or non-linear models.",
            )
        return None

    def rule_r07_small_dataset(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        row_count = analysis.get("rowCount")
        if row_count is not None and row_count < 500:
            return Recommendation(
                ruleId="R-07",
                message="Small dataset detected. Accuracy may be unreliable. Collect more data or use cross-validation.",
            )
        return None

    def rule_r08_no_scaling_for_svm_knn(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> Optional[Recommendation]:
        scaling = config.get("preprocessing", {}).get("scaling", "none")
        model_name = config.get("model", {}).get("name", "")
        if scaling == "none" and model_name in {"SVM", "KNN"}:
            return Recommendation(
                ruleId="R-08",
                message="SVM and KNN are sensitive to feature scale. Apply StandardScaler or MinMaxScaler.",
            )
        return None
