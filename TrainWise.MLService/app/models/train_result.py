from typing import Any, Dict, List, Optional, Literal
from pydantic import BaseModel, Field
from app.models.recommendation import Recommendation

class ClassificationMetrics(BaseModel):
    accuracy: float
    precision: float
    recall: float
    f1Score: float
    confusionMatrix: List[List[int]]
    classLabels: Optional[List[str]] = None
    trainAccuracy: Optional[float] = None
    trainF1Score: Optional[float] = None

class RegressionMetrics(BaseModel):
    r2Score: float
    rmse: float
    mae: float
    mse: Optional[float] = None
    trainR2: Optional[float] = None
    trainRmse: Optional[float] = None

class TrainResult(BaseModel):
    experimentId: str
    modelName: str
    taskType: Literal["classification", "regression"]
    classificationMetrics: Optional[ClassificationMetrics] = None
    regressionMetrics: Optional[RegressionMetrics] = None
    featureImportances: Optional[Dict[str, float]] = None
    trainingDurationSeconds: float
    recommendations: List[Recommendation] = Field(default_factory=list)
    charts: Optional[Dict[str, Any]] = None
    crossValidationScores: Optional[List[float]] = None
    crossValidationMean: Optional[float] = None
