import numpy as np
from scipy import stats
from sklearn.metrics import confusion_matrix, roc_curve, auc, precision_recall_curve, average_precision_score

class ChartBuilder:
    @staticmethod
    def build_confusion_matrix(y_true, y_pred, labels):
        cm = confusion_matrix(y_true, y_pred)
        return {
            "Matrix": cm.tolist(),
            "XLabels": [str(label) for label in labels],
            "YLabels": [str(label) for label in labels]
        }

    @staticmethod
    def build_roc_curve(y_true, y_probs):
        y_true = np.array(y_true)
        y_probs = np.array(y_probs)
        
        # Binary classification only for ROC
        if len(np.unique(y_true)) != 2 or y_probs.shape[1] < 2:
            return None
            
        probs = y_probs[:, 1]
        fpr, tpr, _ = roc_curve(y_true, probs)
        roc_auc = auc(fpr, tpr)
        
        return {
            "Fpr": fpr.tolist(),
            "Tpr": tpr.tolist(),
            "Auc": round(float(roc_auc), 4),
            "Baseline": [0, 1]
        }

    @staticmethod
    def build_precision_recall_curve(y_true, y_probs):
        y_true = np.array(y_true)
        y_probs = np.array(y_probs)
        
        # Binary classification only for PR Curve
        if len(np.unique(y_true)) != 2 or y_probs.shape[1] < 2:
            return None
            
        probs = y_probs[:, 1]
        precision, recall, _ = precision_recall_curve(y_true, probs)
        avg_precision = average_precision_score(y_true, probs)
        
        return {
            "Precision": precision.tolist(),
            "Recall": recall.tolist(),
            "AvgPrecision": round(float(avg_precision), 4)
        }

    @staticmethod
    def build_feature_importance(model, feature_names):
        importance = None
        if hasattr(model, 'feature_importances_'):
            importance = model.feature_importances_
        elif hasattr(model, 'coef_'):
            importance = np.abs(model.coef_[0]) if len(model.coef_.shape) > 1 else np.abs(model.coef_)
        
        if importance is None: return None
        feat_imp = sorted(zip(feature_names, importance.tolist()), key=lambda x: x[1], reverse=True)[:20]
        return {
            "Names": [x[0] for x in feat_imp],
            "Values": [round(float(x[1]), 5) for x in feat_imp]
        }

    @staticmethod
    def build_actual_vs_predicted(y_true, y_pred):
        y_true, y_pred = np.array(y_true), np.array(y_pred)
        min_v, max_v = float(min(y_true.min(), y_pred.min())), float(max(y_true.max(), y_pred.max()))
        return {
            "Actual": y_true.tolist(),
            "Predicted": y_pred.tolist(),
            "Reference": [min_v, max_v]
        }

    @staticmethod
    def build_residuals_scatter(y_true, y_pred):
        y_true, y_pred = np.array(y_true), np.array(y_pred)
        return {
            "Predicted": y_pred.tolist(),
            "Residuals": (y_true - y_pred).tolist(),
            "ZeroLine": [float(y_pred.min()), float(y_pred.max())]
        }

    @staticmethod
    def build_residuals_distribution(y_true, y_pred):
        res = np.array(y_true) - np.array(y_pred)
        return {
            "Residuals": res.tolist(),
            "Mean": round(float(np.mean(res)), 4),
            "Std": round(float(np.std(res)), 4)
        }

    # --- NEW ENHANCEMENTS ---

    @staticmethod
    def build_probability_distribution(y_probs):
        """
        Builds a histogram of predicted probabilities to show model confidence.
        """
        y_probs = np.array(y_probs)
        confidences = np.max(y_probs, axis=1) if len(y_probs.shape) > 1 else y_probs
        return {
            "Confidences": confidences.tolist(),
            "Bins": 20
        }

    @staticmethod
    def build_qq_plot(y_true, y_pred):
        """
        Builds a Q-Q plot for residuals to check for normality.
        """
        residuals = np.array(y_true) - np.array(y_pred)
        # Standardize residuals
        std_res = (residuals - np.mean(residuals)) / np.std(residuals)
        osm, osr = stats.probplot(std_res, dist="norm")
        return {
            "Theoretical": osm[0].tolist(),
            "Sample": osm[1].tolist(),
            "Line": [float(np.min(osm[0])), float(np.max(osm[0]))]
        }

    @staticmethod
    def build_class_comparison(y_true, y_pred, labels):
        """
        Compares actual vs predicted class frequencies.
        """
        y_true_list = list(y_true)
        y_pred_list = list(y_pred)
        
        # Count occurrences for each actual label value
        actual_counts = []
        pred_counts = []
        
        for i, label in enumerate(labels):
            # If y was label encoded, it contains 0, 1, 2...
            # If not, it contains the original values.
            # In both cases, the index 'i' or the 'label' value itself 
            # should match the values in the lists.
            count_true = y_true_list.count(i) if isinstance(y_true_list[0], (int, np.integer)) else y_true_list.count(label)
            count_pred = y_pred_list.count(i) if isinstance(y_pred_list[0], (int, np.integer)) else y_pred_list.count(label)
            
            actual_counts.append(count_true)
            pred_counts.append(count_pred)

        return {
            "Labels": [str(l) for l in labels],
            "ActualCounts": actual_counts,
            "PredictedCounts": pred_counts
        }

    @staticmethod
    def build_cv_metrics(cv_scores):
        """
        Visualizes individual fold performance if cross-validation was used.
        """
        if not cv_scores: return None
        return {
            "Folds": [f"Fold {i+1}" for i in range(len(cv_scores))],
            "Scores": [round(float(s), 4) for s in cv_scores],
            "Mean": round(float(np.mean(cv_scores)), 4)
        }
