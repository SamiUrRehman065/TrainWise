from typing import Dict, List, Optional
from pydantic import BaseModel

class MissingValueInfo(BaseModel):
    count: int
    percentage: float

class SummaryStats(BaseModel):
    mean: float
    median: float
    std: float
    min: float
    max: float

class DatasetSummary(BaseModel):
    datasetId: str
    rowCount: int
    columnCount: int
    columnTypes: Dict[str, str]
    missingValues: Dict[str, MissingValueInfo]
    summaryStats: Dict[str, SummaryStats]
    classDistribution: Optional[Dict[str, int]] = None
    correlationMatrix: List[List[float]]
    imbalanceRatio: Optional[float] = None
