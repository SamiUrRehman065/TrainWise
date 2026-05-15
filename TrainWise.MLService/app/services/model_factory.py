from typing import Callable, Dict, Any
from sklearn.linear_model import LogisticRegression, LinearRegression, Ridge, Lasso
from sklearn.ensemble import RandomForestClassifier, RandomForestRegressor, GradientBoostingClassifier, GradientBoostingRegressor, AdaBoostClassifier, AdaBoostRegressor
from sklearn.svm import SVC, SVR
from sklearn.neighbors import KNeighborsClassifier, KNeighborsRegressor
from sklearn.tree import DecisionTreeClassifier, DecisionTreeRegressor
from sklearn.naive_bayes import GaussianNB

class ModelFactory:
    @classmethod
    def create(cls, name: str, task_type: str = "classification", **hyperparameters: Any) -> Any:
        # Task-aware mappings
        if name == "RandomForest":
            model_cls = RandomForestClassifier if task_type == "classification" else RandomForestRegressor
        elif name == "SVM":
            model_cls = SVC if task_type == "classification" else SVR
            if task_type == "classification" and "probability" not in hyperparameters:
                hyperparameters["probability"] = True
        elif name == "LogisticRegression":
            model_cls = LogisticRegression if task_type == "classification" else LinearRegression
        elif name == "KNN":
            model_cls = KNeighborsClassifier if task_type == "classification" else KNeighborsRegressor
        elif name == "DecisionTree":
            model_cls = DecisionTreeClassifier if task_type == "classification" else DecisionTreeRegressor
        elif name == "GradientBoosting":
            model_cls = GradientBoostingClassifier if task_type == "classification" else GradientBoostingRegressor
        elif name == "AdaBoost":
            model_cls = AdaBoostClassifier if task_type == "classification" else AdaBoostRegressor
        elif name == "NaiveBayes":
            model_cls = GaussianNB # Regression doesn't apply to NB typically
        elif name == "Ridge":
            model_cls = Ridge
        elif name == "Lasso":
            model_cls = Lasso
        elif name == "LinearRegression":
            model_cls = LinearRegression
        elif name == "RandomForestRegressor":
            model_cls = RandomForestRegressor
        elif name == "SVR":
            model_cls = SVR
        else:
            raise ValueError(f"Unsupported model: {name}")
            
        # Filter hyperparameters
        import inspect
        sig = inspect.signature(model_cls)
        valid_params = {
            k: v for k, v in hyperparameters.items() 
            if k in sig.parameters
        }
        
        return model_cls(**valid_params)
