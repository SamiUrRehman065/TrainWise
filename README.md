# TrainWise - Web-Based ML Training Platform

![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8-512BD4?logo=dotnet)
![FastAPI](https://img.shields.io/badge/FastAPI-0.111-009688?logo=fastapi)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)
![Python](https://img.shields.io/badge/Python-3.11-3776AB?logo=python)

## What is TrainWise
TrainWise is a web-based platform for classical machine learning workflows. Users upload tabular datasets, configure preprocessing, select models, and train them without leaving the browser.

The system provides evaluation metrics, interactive charts, and rule-based recommendations to improve model quality. It is designed for local deployment and Final Year Project evaluation.

## Architecture Diagram
```
[Blazor WebAssembly]
         |
         v
[ASP.NET Core API] <-> [SQL Server 2022]
         |
         v
[FastAPI ML Service]
```

## Prerequisites
- .NET SDK 8.0
- Python 3.11
- SQL Server 2022 (LocalDB or Developer edition)
- Git
- Docker Desktop (optional, for containerized setup)

## Quick Start
1. Clone the repository.
2. Create the SQL Server database using the schema script.
3. Run the FastAPI ML service.
4. Run the ASP.NET Core API.
5. Run the Blazor WebAssembly frontend.

## Environment Variables
| Variable | Description | Example |
|---|---|---|
| ConnectionStrings__DefaultConnection | SQL Server connection string | Server=localhost,1433;Database=TrainWiseDb;User Id=sa;Password=YourPass!;TrustServerCertificate=True |
| MLService__BaseUrl | Base URL for FastAPI | http://localhost:8000 |
| Auth__SessionTimeoutHours | Session expiry window | 8 |
| Upload__MaxFileSizeMb | Upload size limit | 50 |
| Upload__StoragePath | Dataset storage path | /data/uploads |
| ASPNETCORE_URLS | API listener URLs | http://localhost:5000 |

## Project Structure
```
TrainWise/
├── TrainWise.API/
├── TrainWise.Web/
├── TrainWise.MLService/
├── TrainWise.Database/
├── docs/
├── docker/
└── .github/
```

## API Reference
| Method | Endpoint | Description |
|---|---|---|
| POST | /api/auth/login | Authenticate user |
| POST | /api/dataset/upload | Upload dataset |
| GET | /api/dataset/{id}/summary | Fetch analysis summary |
| POST | /api/train | Train model |
| GET | /api/experiment/{id} | Get experiment details |
| GET | /api/experiment/history | List experiment history |
| DELETE | /api/experiment/{id} | Delete experiment |

## Known Limitations
- No HTTPS enforcement in v1.0 (local deployment only)
- Uploaded datasets are not encrypted at rest
- Session tokens are stored in memory and lost on server restart
- Initial Blazor WASM load time may be 3 to 5 seconds
- SMOTE recommendations require manual user action

## Roadmap
- JWT authentication and RBAC (v1.1)
- Model artifact storage for inference (v1.1)
- PDF report export (v1.2)
- Auto model comparison leaderboard (v1.2)
- SHAP explainability (v2.0)
- Cloud deployment to Azure (v2.0)
- Deep learning support (v2.0)
- Unsupervised learning (v2.1)

## Contributing
Contributions are welcome. Please open an issue to discuss changes before submitting a PR.

## License
MIT License (placeholder)
