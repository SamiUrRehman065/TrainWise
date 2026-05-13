from typing import Any
from fastapi import APIRouter
from pydantic import BaseModel
from app.models.dataset_summary import DatasetSummary
from app.models.train_request import TrainRequest
from app.models.train_result import TrainResult
from app.models.recommendation import Recommendation
from app.models.eda_report import EdaReport
from app.services.analyzer import analyze_dataset
from app.services.analyzer import generate_eda_report
from app.services.trainer import train_model
from app.services.recommendation_engine import RecommendationEngine

router = APIRouter()

class AnalyzeRequest(BaseModel):
    filePath: str
    datasetId: str

class RecommendRequest(BaseModel):
    metrics: dict[str, Any]
    config: dict[str, Any]
    analysis: dict[str, Any] | None = None

class EdaRequest(BaseModel):
    datasetId: str
    targetColumn: str | None = None

@router.post("/analyze", response_model=DatasetSummary)
async def analyze(request: AnalyzeRequest) -> DatasetSummary:
    """
    Analyze the dataset and return descriptive statistics.
    Triggered immediately after upload by the API.
    Returns DatasetSummary JSON.
    """
    return analyze_dataset(request.filePath, request.datasetId)

@router.post("/train", response_model=TrainResult)
async def train(request: TrainRequest) -> TrainResult:
    """
    Train the selected model and compute evaluation metrics.
    Supports classification and regression tasks.
    Returns TrainResult JSON.
    """
    return train_model(request)

@router.post("/recommend", response_model=list[Recommendation])
async def recommend(request: RecommendRequest) -> list[Recommendation]:
    """
    Generate rule-based recommendations after training.
    Uses metrics and preprocessing config to evaluate rules.
    Returns a list of Recommendation objects.
    """
    engine = RecommendationEngine()
    return engine.evaluate_all(request.metrics, request.config, request.analysis or {})

@router.get("/health")
async def health_check() -> dict:
    """
    Basic liveness check for the ML service.
    Returns status ok when healthy.
    """
    return {"status": "ok"}

@router.post("/eda", response_model=EdaReport)
async def eda(request: EdaRequest) -> EdaReport:
    """
    Generate deeper EDA insights for the selected dataset.
    """
    return generate_eda_report(request.datasetId, request.targetColumn)
