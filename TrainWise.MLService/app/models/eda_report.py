from typing import Dict, List, Optional
from pydantic import BaseModel

class OutlierInfo(BaseModel):
    count: int
    percentage: float

class FeatureCorrelation(BaseModel):
    feature: str
    correlation: float

class EdaReport(BaseModel):
    datasetId: str
    targetColumn: Optional[str] = None
    numericColumns: List[str]
    categoricalColumns: List[str]
    duplicateRows: int
    duplicatePercentage: float
    memoryUsageMb: float
    outliersByColumn: Dict[str, OutlierInfo]
    topTargetCorrelations: List[FeatureCorrelation]
    targetDistribution: Optional[Dict[str, int]] = None
