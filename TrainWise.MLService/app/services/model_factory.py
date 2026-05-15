from typing import Callable, Dict, Any
from sklearn.linear_model import LogisticRegression, LinearRegression
from sklearn.ensemble import RandomForestClassifier, RandomForestRegressor
from sklearn.svm import SVC, SVR
from sklearn.neighbors import KNeighborsClassifier

class ModelFactory:
    @classmethod
    def create(cls, name: str, task_type: str = "classification", **hyperparameters: Any) -> Any:
        # Task-aware mappings
        if name == "RandomForest":
            model_cls = RandomForestClassifier if task_type == "classification" else RandomForestRegressor
        elif name == "SVM":
            model_cls = SVC if task_type == "classification" else SVR
            # Ensure SVM has probability=True for ROC/PR curves
            if task_type == "classification" and "probability" not in hyperparameters:
                hyperparameters["probability"] = True
        elif name == "LogisticRegression":
            if task_type == "regression":
                model_cls = LinearRegression
            else:
                model_cls = LogisticRegression
        elif name == "KNN":
            model_cls = KNeighborsClassifier # Add Regressor if needed
        elif name == "LinearRegression":
            model_cls = LinearRegression
        elif name == "RandomForestRegressor":
            model_cls = RandomForestRegressor
        elif name == "SVR":
            model_cls = SVR
        else:
            raise ValueError(f"Unsupported model: {name}")
            
        # Filter hyperparameters to only those supported by the model class
        import inspect
        sig = inspect.signature(model_cls)
        valid_params = {
            k: v for k, v in hyperparameters.items() 
            if k in sig.parameters
        }
        
        return model_cls(**valid_params)
