from pydantic import BaseModel

class Recommendation(BaseModel):
    ruleId: str
    message: str
