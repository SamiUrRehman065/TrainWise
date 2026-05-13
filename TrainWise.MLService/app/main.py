from fastapi import FastAPI
from app.routers import train

app = FastAPI(title="TrainWise ML Service", version="1.0")
app.include_router(train.router)
