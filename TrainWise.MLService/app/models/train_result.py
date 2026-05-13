from typing import Dict, List, Optional, Literal
from pydantic import BaseModel, Field
from app.models.recommendation import Recommendation

class ClassificationMetrics(BaseModel):
    accuracy: float
    precision: float
    recall: float
    f1Score: float
    confusionMatrix: List[List[int]]

class RegressionMetrics(BaseModel):
    r2Score: float
    rmse: float
    mae: float

class TrainResult(BaseModel):
    experimentId: str
    modelName: str
    taskType: Literal["classification", "regression"]
    classificationMetrics: Optional[ClassificationMetrics] = None
    regressionMetrics: Optional[RegressionMetrics] = None
    featureImportances: Optional[Dict[str, float]] = None
    trainingDurationSeconds: float
    recommendations: List[Recommendation] = Field(default_factory=list)
    crossValidationScores: Optional[List[float]] = None
    crossValidationMean: Optional[float] = None
