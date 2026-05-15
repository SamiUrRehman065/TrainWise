from typing import Any, Dict, Optional, List
from app.models.recommendation import Recommendation

class RecommendationEngine:
    """
    Advanced context-aware recommendation engine.
    Uses hierarchical evaluation and cross-domain heuristics to provide 
    cohesive, expert-level feedback on ML training results.
    """
    
    def evaluate_all(
        self,
        metrics: Dict[str, Any],
        config: Dict[str, Any],
        analysis: Dict[str, Any],
    ) -> List[Recommendation]:
        recommendations: List[Recommendation] = []
        
        # 1. Data Quality Domain
        recommendations.extend(self._evaluate_data_quality(metrics, config, analysis))
        
        # 2. Model Health Domain (Bias/Variance)
        health = self._evaluate_model_health(metrics, config)
        if health: recommendations.append(health)
        
        # 3. Performance Insights (Precision/Recall/F1)
        recommendations.extend(self._evaluate_performance(metrics))
        
        # 4. Configuration Optimization
        recommendations.extend(self._evaluate_config(config))

        # 5. Visualization Insights
        recommendations.extend(self._evaluate_visualizations(metrics, config, analysis))
        
        return recommendations

    def _evaluate_visualizations(self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]) -> List[Recommendation]:
        recs = []
        task_type = config.get("taskType", "classification")
        target_analysis = analysis.get("targetAnalysis", {})
        classes = target_analysis.get("uniqueValuesCount", 0)
        
        if task_type == "regression":
            recs.append(Recommendation(
                ruleId="VI-01",
                severity="info",
                message="Regression Insights Active.",
                action="For regression tasks, ROC/PR curves are replaced by Residuals and Actual-vs-Predicted plots to better visualize prediction error."
            ))
        elif task_type == "classification":
            if classes > 2:
                recs.append(Recommendation(
                    ruleId="VI-02",
                    severity="info",
                    message="Multi-Class Visualization Active.",
                    action=f"Your dataset has {classes} classes. ROC and PR curves are calculated using 'Macro-average' logic to represent overall model health."
                ))
            else:
                recs.append(Recommendation(
                    ruleId="VI-03",
                    severity="success",
                    message="Binary Visualization Active.",
                    action="Standard ROC/PR curves generated for precise performance auditing of your binary target."
                ))
                
        return recs

    def _evaluate_data_quality(self, metrics: Dict[str, Any], config: Dict[str, Any], analysis: Dict[str, Any]) -> List[Recommendation]:
        recs = []
        overview = analysis.get("overview", {})
        features = analysis.get("features", [])
        feature_importance = metrics.get("featureImportances", {}) or {}
        
        # Missing Value Intelligence (Cross-referenced with Importance)
        found_missing = False
        for feat in features:
            pct = feat.get("missingPercentage", 0)
            if pct > 5:
                found_missing = True
                name = feat.get("name", "Unknown")
                importance = feature_importance.get(name, 0)
                
                severity = "warning"
                action = "Use simple imputation (mean/median)."
                
                if importance > 0.1:
                    severity = "critical"
                    action = f"CRITICAL: '{name}' is highly important but has {pct:.1f}% missing data. Standard imputation may bias results. Consider KNNImputer or manual investigation."
                elif pct > 40:
                    severity = "critical"
                    action = f"Severe data loss in '{name}'. Dropping this column might be safer than imputation."
                
                recs.append(Recommendation(
                    ruleId="DQ-01",
                    severity=severity,
                    message=f"Missing values detected in '{name}' ({pct:.1f}% missing).",
                    action=action
                ))

        # Imbalance Intelligence
        target_analysis = analysis.get("targetAnalysis", {})
        dist = target_analysis.get("distribution", {})
        if dist and len(dist) > 1:
            vals = list(dist.values())
            ratio = max(vals) / min(vals) if min(vals) > 0 else 100
            
            if ratio > 3:
                apply_smote = config.get("preprocessing", {}).get("applySmote", False)
                if not apply_smote:
                    recs.append(Recommendation(
                        ruleId="DQ-02",
                        severity="warning",
                        message=f"Significant class imbalance (Ratio {ratio:.1f}:1) detected.",
                        action="Enable SMOTE oversampling in preprocessing to improve minority-class performance."
                    ))
                else:
                    recs.append(Recommendation(
                        ruleId="DQ-03",
                        severity="success",
                        message="Class Imbalance Mitigated.",
                        action="SMOTE oversampling is active and helping the model learn minority classes."
                    ))

        # Overall Cleanliness
        if not found_missing and overview.get("missingPercentage", 0) == 0:
            recs.append(Recommendation(
                ruleId="DQ-00",
                severity="success",
                message="Dataset Integrity: Pristine.",
                action="No missing values or critical quality issues detected. High quality data leads to reliable models."
            ))

        return recs

    def _evaluate_model_health(self, metrics: Dict[str, Any], config: Dict[str, Any]) -> Optional[Recommendation]:
        task_type = config.get("taskType", "classification")
        
        if task_type == "classification":
            m = metrics.get("classificationMetrics", {})
            train = m.get("trainAccuracy")
            test = m.get("accuracy")
        else:
            m = metrics.get("regressionMetrics", {})
            train = m.get("trainR2")
            test = m.get("r2Score")

        if train is None or test is None: return None
        diff = train - test
        
        if diff > 0.12:
            return Recommendation(ruleId="MH-01", severity="critical", message="High Variance (Overfitting).", action="The model is memorizing training noise. Increase regularization or prune trees.")
        
        if train < 0.50:
            return Recommendation(ruleId="MH-02", severity="warning", message="High Bias (Underfitting).", action="Model is too simple. Try a more complex algorithm or feature engineering.")

        if abs(diff) < 0.04 and test > 0.80:
            return Recommendation(ruleId="MH-03", severity="success", message="Balanced Learning Curve.", action="The model generalizes exceptionally well to unseen data.")
            
        return None

    def _evaluate_performance(self, metrics: Dict[str, Any]) -> List[Recommendation]:
        recs = []
        cls = metrics.get("classificationMetrics", {})
        if cls:
            f1 = cls.get("f1Score", 0)
            precision = cls.get("precision", 0)
            recall = cls.get("recall", 0)
            
            if abs(precision - recall) > 0.15:
                direction = "Precision" if precision > recall else "Recall"
                recs.append(Recommendation(
                    ruleId="PR-01",
                    severity="warning",
                    message=f"Performance Skew: {direction} is dominant.",
                    action="There is a significant gap between Precision and Recall. Adjust the threshold if necessary."
                ))
            
            if f1 > 0.90:
                recs.append(Recommendation(ruleId="PR-02", severity="success", message=f"Strong consistency (F1: {f1:.2f}).", action="The model is highly reliable."))
        return recs

    def _evaluate_config(self, config: Dict[str, Any]) -> List[Recommendation]:
        recs = []
        model = config.get("model", {})
        name = model.get("name", "")
        prep = config.get("preprocessing", {})
        
        if name in ["SVM", "KNN", "LogisticRegression"] and prep.get("scaling", "none") == "none":
            recs.append(Recommendation(ruleId="CF-01", severity="critical", message=f"{name} requires scaling.", action="Distance-based models fail without normalization. Enable StandardScaler."))
                
        return recs
