from typing import Callable, Dict, Any
from sklearn.linear_model import LogisticRegression, LinearRegression
from sklearn.ensemble import RandomForestClassifier, RandomForestRegressor
from sklearn.svm import SVC, SVR
from sklearn.neighbors import KNeighborsClassifier

class ModelFactory:
    _registry: Dict[str, Callable[..., Any]] = {
        "LogisticRegression": LogisticRegression,
        "RandomForest": RandomForestClassifier,
        "SVM": SVC,
        "KNN": KNeighborsClassifier,
        "LinearRegression": LinearRegression,
        "RandomForestRegressor": RandomForestRegressor,
        "SVR": SVR,
    }

    @classmethod
    def create(cls, name: str, **hyperparameters: Any) -> Any:
        if name not in cls._registry:
            raise ValueError(f"Unsupported model: {name}")
        
        # Ensure SVM has probability=True for ROC/PR curves
        if name == "SVM" and "probability" not in hyperparameters:
            hyperparameters["probability"] = True
            
        model_cls = cls._registry[name]
        return model_cls(**hyperparameters)
