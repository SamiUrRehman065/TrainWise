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
            self.rule_r04_low_f1,
            self.rule_r05_low_estimators,
            self.rule_r06_high_rmse,
            self.rule_r07_small_dataset,
            self.rule_r08_no_scaling_for_svm_knn,
            self.rule_r09_good_f1,
            self.rule_r10_balanced_precision_recall,
            self.rule_r11_smote_successful,
            self.rule_r12_scaling_applied,
            self.rule_r13_no_missing_values,
            self.rule_r14_overfitting,
            self.rule_r15_underfitting,
            self.rule_r16_regression_tradeoffs,
            self.rule_r17_optimal_variance,
            self.rule_r18_optimal_bias,
        ]
        recommendations: List[Recommendation] = []
        for rule in rules:
            result = rule(metrics, config, analysis)
            if result is not None:
                recommendations.append(result)
        return recommendations

    def rule_r01_missing_values(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        missing_values = analysis.get("missingValues", {})
        for column, info in missing_values.items():
            pct = (info or {}).get("percentage")
            if pct is not None and pct > 20:
                return Recommendation(
                    ruleId="R-01",
                    severity="warning",
                    message=f"High missing data detected in column '{column}' ({pct:.1f}% missing).",
                    action="Consider dropping this column or using model-based imputation (e.g., KNNImputer)."
                )
        return None

    def rule_r02_class_imbalance(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        imbalance_ratio = analysis.get("imbalanceRatio")
        apply_smote = config.get("preprocessing", {}).get("applySmote", False)
        if imbalance_ratio is not None and imbalance_ratio > 2 and not apply_smote:
            return Recommendation(
                ruleId="R-02",
                severity="warning",
                message=f"Dataset imbalance detected (ratio: {imbalance_ratio:.2f}).",
                action="Enable SMOTE oversampling during training to improve minority-class recall."
            )
        return None

    def rule_r14_overfitting(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        if "classificationMetrics" in metrics:
            train_acc = metrics["classificationMetrics"].get("trainAccuracy")
            test_acc = metrics["classificationMetrics"].get("accuracy")
            if train_acc is not None and test_acc is not None:
                if train_acc - test_acc > 0.10:
                    return Recommendation(
                        ruleId="R-14",
                        severity="critical",
                        message=f"High Variance (Overfitting) detected. Train Accuracy ({train_acc:.1%}) is significantly higher than Test Accuracy ({test_acc:.1%}).",
                        action="The model is memorizing the training data instead of generalizing. Try simplifying the model, reducing tree depth, adding regularization, or increasing training data."
                    )
        elif "regressionMetrics" in metrics:
            train_r2 = metrics["regressionMetrics"].get("trainR2")
            test_r2 = metrics["regressionMetrics"].get("r2Score")
            if train_r2 is not None and test_r2 is not None:
                if train_r2 - test_r2 > 0.15:
                    return Recommendation(
                        ruleId="R-14",
                        severity="critical",
                        message=f"High Variance (Overfitting) detected. Train R² ({train_r2:.2f}) is much higher than Test R² ({test_r2:.2f}).",
                        action="The model is fitting to noise in the training set. Add regularization (e.g. Ridge/Lasso), prune trees, or gather more data."
                    )
        return None

    def rule_r15_underfitting(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        if "classificationMetrics" in metrics:
            train_acc = metrics["classificationMetrics"].get("trainAccuracy")
            test_acc = metrics["classificationMetrics"].get("accuracy")
            if train_acc is not None and test_acc is not None:
                if train_acc < 0.60 and test_acc < 0.60:
                    return Recommendation(
                        ruleId="R-15",
                        severity="warning",
                        message=f"High Bias (Underfitting) detected. Both Train ({train_acc:.1%}) and Test ({test_acc:.1%}) accuracies are low.",
                        action="The model is too simple to capture the underlying patterns. Try using a more complex model (e.g., Random Forest instead of Logistic Regression), engineering new features, or reducing regularization."
                    )
        elif "regressionMetrics" in metrics:
            train_r2 = metrics["regressionMetrics"].get("trainR2")
            test_r2 = metrics["regressionMetrics"].get("r2Score")
            if train_r2 is not None and test_r2 is not None:
                if train_r2 < 0.40 and test_r2 < 0.40:
                    return Recommendation(
                        ruleId="R-15",
                        severity="warning",
                        message=f"High Bias (Underfitting) detected. Both Train ({train_r2:.2f}) and Test ({test_r2:.2f}) R² scores are low.",
                        action="The model cannot capture the variance in the target variable. Consider non-linear models (e.g., Polynomial features, SVR, or Tree-based models)."
                    )
        return None

    def rule_r17_optimal_variance(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        if "classificationMetrics" in metrics:
            train_acc = metrics["classificationMetrics"].get("trainAccuracy")
            test_acc = metrics["classificationMetrics"].get("accuracy")
            if train_acc is not None and test_acc is not None:
                diff = train_acc - test_acc
                # Optimal variance: difference is very small (e.g., < 0.03) and test accuracy is decent
                if 0 <= diff <= 0.03 and test_acc > 0.70:
                    return Recommendation(
                        ruleId="R-17",
                        severity="success",
                        message=f"Optimal Variance (No Overfitting). Train Accuracy ({train_acc:.1%}) and Test Accuracy ({test_acc:.1%}) are very close.",
                        action="Your model generalizes exceptionally well to unseen data. The complexity is perfectly balanced."
                    )
        elif "regressionMetrics" in metrics:
            train_r2 = metrics["regressionMetrics"].get("trainR2")
            test_r2 = metrics["regressionMetrics"].get("r2Score")
            if train_r2 is not None and test_r2 is not None:
                diff = train_r2 - test_r2
                if 0 <= diff <= 0.05 and test_r2 > 0.60:
                    return Recommendation(
                        ruleId="R-17",
                        severity="success",
                        message=f"Optimal Variance (No Overfitting). Train R² ({train_r2:.2f}) and Test R² ({test_r2:.2f}) are very close.",
                        action="Your model generalizes exceptionally well to unseen data. Regularization and feature complexity are well-tuned."
                    )
        return None

    def rule_r18_optimal_bias(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        if "classificationMetrics" in metrics:
            train_acc = metrics["classificationMetrics"].get("trainAccuracy")
            test_acc = metrics["classificationMetrics"].get("accuracy")
            if train_acc is not None and test_acc is not None:
                if train_acc > 0.85 and test_acc > 0.80:
                    return Recommendation(
                        ruleId="R-18",
                        severity="success",
                        message=f"Optimal Bias (No Underfitting). Both Train ({train_acc:.1%}) and Test ({test_acc:.1%}) accuracies are strong.",
                        action="Your model has sufficient complexity to capture the underlying patterns in your dataset. Excellent feature selection!"
                    )
        elif "regressionMetrics" in metrics:
            train_r2 = metrics["regressionMetrics"].get("trainR2")
            test_r2 = metrics["regressionMetrics"].get("r2Score")
            if train_r2 is not None and test_r2 is not None:
                if train_r2 > 0.80 and test_r2 > 0.75:
                    return Recommendation(
                        ruleId="R-18",
                        severity="success",
                        message=f"Optimal Bias (No Underfitting). Both Train ({train_r2:.2f}) and Test ({test_r2:.2f}) R² scores are strong.",
                        action="Your model is successfully capturing the variance in the target variable without being too simplistic."
                    )
        return None

    def rule_r16_regression_tradeoffs(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        if "regressionMetrics" in metrics:
            rmse = metrics["regressionMetrics"].get("rmse")
            mae = metrics["regressionMetrics"].get("mae")
            if rmse is not None and mae is not None:
                if rmse > mae * 1.5:
                    return Recommendation(
                        ruleId="R-16",
                        severity="warning",
                        message=f"RMSE ({rmse:.2f}) is significantly higher than MAE ({mae:.2f}).",
                        action="RMSE severely penalizes large errors, meaning your model is making a few very large mistakes (likely due to outliers). Consider robust scaling, handling outliers in preprocessing, or using a robust model like HuberRegressor."
                    )
        return None

    def rule_r04_low_f1(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        f1 = metrics.get("classificationMetrics", {}).get("f1Score")
        if f1 is not None and f1 < 0.60:
            return Recommendation(
                ruleId="R-04",
                severity="critical",
                message=f"Low F1-score ({f1:.2f}). The model is struggling with this configuration.",
                action="Try feature scaling, switch to a different model (e.g., SVM), or increase training data."
            )
        return None

    def rule_r05_low_estimators(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        model_name = config.get("model", {}).get("name", "")
        n_estimators = config.get("model", {}).get("hyperparameters", {}).get("n_estimators")
        if "RandomForest" in model_name and n_estimators is not None and n_estimators < 50:
            return Recommendation(
                ruleId="R-05",
                severity="info",
                message=f"Low tree count ({n_estimators} estimators) detected for Random Forest.",
                action="Increase n_estimators to at least 100 for more stable predictions."
            )
        return None

    def rule_r06_high_rmse(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
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
                severity="critical",
                message="High RMSE relative to target spread.",
                action="Consider feature engineering or non-linear models."
            )
        return None

    def rule_r07_small_dataset(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        row_count = analysis.get("rowCount")
        if row_count is not None and row_count < 500:
            return Recommendation(
                ruleId="R-07",
                severity="info",
                message=f"Small dataset detected ({row_count} rows). Model accuracy may be unreliable.",
                action="Collect more training data or use cross-validation to get reliable estimates."
            )
        return None

    def rule_r08_no_scaling_for_svm_knn(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        scaling = config.get("preprocessing", {}).get("scaling", "none")
        model_name = config.get("model", {}).get("name", "")
        if scaling == "none" and model_name in {"SVM", "KNN", "LogisticRegression"}:
            return Recommendation(
                ruleId="R-08",
                severity="warning",
                message=f"{model_name} is sensitive to feature scale, but no scaling was applied.",
                action="Enable StandardScaler or MinMaxScaler preprocessing to improve performance."
            )
        return None

    def rule_r09_good_f1(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        f1 = metrics.get("classificationMetrics", {}).get("f1Score")
        if f1 is not None and 0.85 <= f1 <= 0.98:
            return Recommendation(
                ruleId="R-09",
                severity="success",
                message=f"Excellent F1-Score ({f1:.2f}). The model has successfully found a strong balance between precision and recall.",
                action="The current feature set and model configuration are working exceptionally well."
            )
        return None

    def rule_r10_balanced_precision_recall(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        precision = metrics.get("classificationMetrics", {}).get("precision")
        recall = metrics.get("classificationMetrics", {}).get("recall")
        if precision is not None and recall is not None and precision > 0.6 and recall > 0.6:
            diff = abs(precision - recall)
            if diff < 0.05:
                return Recommendation(
                    ruleId="R-10",
                    severity="success",
                    message=f"Balanced Precision ({precision:.2f}) and Recall ({recall:.2f}).",
                    action="The model shows no significant bias towards false positives or false negatives."
                )
        return None

    def rule_r11_smote_successful(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        imbalance_ratio = analysis.get("imbalanceRatio")
        apply_smote = config.get("preprocessing", {}).get("applySmote", False)
        f1 = metrics.get("classificationMetrics", {}).get("f1Score")
        if imbalance_ratio is not None and imbalance_ratio > 2 and apply_smote and f1 is not None and f1 > 0.75:
            return Recommendation(
                ruleId="R-11",
                severity="success",
                message="SMOTE was effectively applied to the imbalanced dataset.",
                action="Synthetic oversampling successfully helped the model learn the minority classes without sacrificing overall performance."
            )
        return None

    def rule_r12_scaling_applied(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        scaling = config.get("preprocessing", {}).get("scaling", "none")
        model_name = config.get("model", {}).get("name", "")
        if scaling != "none" and model_name in {"SVM", "KNN", "LogisticRegression"}:
            return Recommendation(
                ruleId="R-12",
                severity="success",
                message=f"Proper feature scaling ({scaling}) was applied for {model_name}.",
                action="This ensures distance metrics and gradient descent compute correctly, maximizing model accuracy."
            )
        return None

    def rule_r13_no_missing_values(
        self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]
    ) -> Optional[Recommendation]:
        missing_values = analysis.get("missingValues", {})
        total_missing = sum((info or {}).get("count", 0) for info in missing_values.values())
        if total_missing == 0:
            return Recommendation(
                ruleId="R-13",
                severity="success",
                message="The dataset is perfectly clean with no missing values.",
                action="No imputation was necessary. High quality data leads to more reliable models."
            )
        return None
