from typing import Dict, List, Optional
from pydantic import BaseModel

class CategoryFreq(BaseModel):
    category: str
    count: int

class FeatureProfile(BaseModel):
    name: str
    type: str
    isTarget: bool
    isCandidateId: bool
    isConstant: bool
    stats: Dict[str, float]
    relevanceScore: float = 0.0
    uniqueCount: int
    uniqueRatio: float
    missingCount: int
    missingPercentage: float
    outlierCount: int
    outlierPercentage: float
    histogramValues: Optional[List[float]] = None
    histogramLabels: Optional[List[str]] = None
    topCategories: Optional[List[CategoryFreq]] = None
    alerts: List[str] = []
    recommendations: List[str] = []

class DatasetOverview(BaseModel):
    rowCount: int
    columnCount: int
    sizeBytes: int
    duplicateRows: int
    duplicatePercentage: float
    totalCells: int
    missingCells: int
    missingPercentage: float
    typeDistribution: Dict[str, int]
    targetColumn: str
    problemType: str
    sparsityScore: float

class FeatureCorrelation(BaseModel):
    feature: str
    correlation: float

class CorrelationAnalysis(BaseModel):
    columns: List[str]
    matrix: List[List[float]]
    highCorrelations: List[FeatureCorrelation]

class MissingValueAnalysis(BaseModel):
    missingByColumn: Dict[str, int]
    patterns: List[str]
    recommendations: List[str]

class TargetAnalysis(BaseModel):
    name: str
    type: str
    stats: Dict[str, float]
    distribution: Optional[Dict[str, int]] = None
    insights: List[str]

class DataQualityIssue(BaseModel):
    severity: str
    type: str
    message: str
    column: Optional[str] = None

class DataQualityReport(BaseModel):
    overallScore: float
    riskLevel: str
    issues: List[DataQualityIssue]

class MlReadinessReport(BaseModel):
    isReady: bool
    requiredPreprocessing: List[str]
    suitableModels: List[str]
    actionableSteps: List[str]

class DatasetIntelligence(BaseModel):
    datasetId: str
    overview: DatasetOverview
    features: List[FeatureProfile]
    correlations: CorrelationAnalysis
    missingValues: MissingValueAnalysis
    targetAnalysis: Optional[TargetAnalysis]
    qualityReport: DataQualityReport
    mlReadiness: MlReadinessReport
