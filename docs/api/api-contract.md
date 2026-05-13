# API Contract

## Frontend to Backend API

| Method | Endpoint | Request Body | Response | Description |
|---|---|---|---|---|
| POST | /api/auth/login | { username, password } | { sessionToken, userId } | Authenticate user |
| POST | /api/dataset/upload | multipart/form-data (file) | { datasetId, fileName, rowCount } | Upload dataset |
| GET | /api/dataset/{id}/summary | - | DatasetSummary JSON | Get analysis results |
| POST | /api/train | TrainRequest JSON | { experimentId, metrics } | Submit training job |
| GET | /api/experiment/{id} | - | ExperimentDetail JSON | Fetch experiment details |
| GET | /api/experiment/history | ?userId=&page=&pageSize= | ExperimentList JSON | List experiments |
| DELETE | /api/experiment/{id} | - | { success: true } | Delete experiment |

## Backend API to ML Service

| Method | Endpoint | Request Body | Response | Description |
|---|---|---|---|---|
| POST | /analyze | { filePath, datasetId } | DatasetSummary JSON | Run dataset analysis |
| POST | /train | TrainRequest JSON | TrainResult JSON | Train model |
| POST | /recommend | { metrics, config } | Recommendation[] JSON | Rule-based recommendations |
| GET | /health | - | { status: "ok" } | ML service health check |
