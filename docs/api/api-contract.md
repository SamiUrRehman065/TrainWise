# API Contract

## Frontend to Backend API

| Method | Endpoint | Request Body | Response | Description |
|---|---|---|---|---|
| POST | /api/auth/login | { username, password } | { sessionToken, userId } | Authenticate user |
| POST | /api/auth/signup | { username, password, email } | { sessionToken, userId } | Create new user account |
| GET | /api/dataset | - | DatasetList JSON | List user's datasets |
| POST | /api/dataset/upload | multipart/form-data (file) | { datasetId, fileName, rowCount } | Upload dataset |
| GET | /api/dataset/{id}/summary | - | DatasetSummary JSON | Get analysis results |
| DELETE | /api/dataset/{id} | - | { success: true } | Delete dataset |
| GET | /api/dataset/manage/stats | - | StorageStats JSON | Get user's storage statistics |
| GET | /api/dataset/manage/training-history | - | TrainingHistory JSON | List user's all experiments |
| POST | /api/dataset/manage/archive | ?daysOld=30 | { archivedCount } | Archive unused datasets |
| GET | /api/dataset/manage/storage-mode | - | { mode: "disk" \| "sql" } | Check current storage backend |
| GET | /api/training/models | - | ModelList JSON | Get available ML models |
| POST | /api/training/train | TrainRequest JSON | { experimentId, metrics } | Submit training job |
| GET | /api/experiments | ?userId=&page=&pageSize= | ExperimentList JSON | List experiments |
| GET | /api/experiments/{id} | - | ExperimentDetail JSON | Fetch experiment details |
| GET | /api/experiments/compare | ?id1=&id2= | Comparison JSON | Compare two experiments side-by-side |
| DELETE | /api/experiments/{id} | - | { success: true } | Delete experiment |

## Backend API to ML Service

| Method | Endpoint | Request Body | Response | Description |
|---|---|---|---|---|
| POST | /analyze | { filePath, datasetId } | DatasetSummary JSON | Run dataset analysis |
| POST | /train | TrainRequest JSON | TrainResult JSON | Train model |
| POST | /recommend | { metrics, config } | Recommendation[] JSON | Rule-based recommendations |
| GET | /health | - | { status: "ok" } | ML service health check |
