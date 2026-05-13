from typing import Literal, List, Optional
from pydantic import BaseModel, Field

class PreprocessingConfig(BaseModel):
    nullStrategy: Literal["drop", "mean", "median", "mode"]
    encoding: Literal["onehot", "label"]
    scaling: Literal["standard", "minmax", "none"]
    applySmote: bool = False
    excludeColumns: List[str] = Field(default_factory=list)

class ModelHyperparameters(BaseModel):
    n_estimators: Optional[int] = None
    C: Optional[float] = None
    kernel: Optional[str] = None
    n_neighbors: Optional[int] = None
    max_iter: Optional[int] = None

class ModelConfig(BaseModel):
    name: Literal[
        "LogisticRegression",
        "RandomForest",
        "SVM",
        "KNN",
        "LinearRegression",
        "RandomForestRegressor",
        "SVR",
    ]
    hyperparameters: ModelHyperparameters = Field(default_factory=ModelHyperparameters)

class TrainRequest(BaseModel):
    datasetId: str
    targetColumn: str
    taskType: Literal["classification", "regression"]
    preprocessing: PreprocessingConfig
    model: ModelConfig
    trainTestSplit: float = 0.8
    crossValidation: bool = False
    filePath: Optional[str] = None
