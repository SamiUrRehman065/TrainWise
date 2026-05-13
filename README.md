# TrainWise - Web-Based ML Training Platform

![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-512BD4?logo=blazor)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8-512BD4?logo=dotnet)
![FastAPI](https://img.shields.io/badge/FastAPI-0.111-009688?logo=fastapi)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)
![Python](https://img.shields.io/badge/Python-3.11-3776AB?logo=python)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)

## What is TrainWise
TrainWise is a full-stack, web-based platform for classical machine learning workflows. It allows data scientists and students to upload tabular datasets, configure smart preprocessing steps, and train state-of-the-art models without leaving their browsers. 

Designed with a microservices architecture, it separates the heavy-lifting of Machine Learning (Python/FastAPI) from the enterprise-grade REST API (C#/.NET) and the interactive user interface (Blazor WebAssembly). It is the perfect tool for quickly establishing baselines, running rapid experiments, and comparing model feature importances visually.

## ✨ Key Features

- 📂 **Smart Dataset Management**: Upload CSV/XLSX files with automatic deduplication (SHA-256 hash checking) and user-scoped storage management.
- ⚙️ **Configurable Preprocessing**: Automatically handles null values (mean/median/mode), applies categorical encodings (One-hot/Factorize), drops high-cardinality noise, and applies SMOTE for imbalanced classes.
- 🤖 **Multi-Model Training**: Train Logistic Regression, Decision Trees, Random Forests, XGBoost, and more with a single click.
- 📊 **Interactive Analytics**: View confusion matrices, precision-recall graphs, and metrics formatted beautifully via Plotly.js integrations.
- ⚖️ **Side-by-Side Comparison**: Deep-dive comparison UI for multiple experiments, calculating percentage differences and visual bar charts for feature importances.
- 📦 **Data Export**: Export your models' hyperparameter configs and metrics seamlessly to JSON.

## 🏗️ Architecture

```text
┌─────────────────────────────────────────────────────────┐
│                 FRONTEND LAYER                           │
│         Blazor WebAssembly + Plotly.js                  │
│  - User Registration & Login                            │
│  - Dataset Upload & Management                          │
│  - Interactive Training UI & Comparisons                │
└────────────────────┬────────────────────────────────────┘
                     │ HTTPS (dev: HTTP)
                     ↓
┌─────────────────────────────────────────────────────────┐
│                 BACKEND API LAYER                        │
│         ASP.NET Core 8 Web API + EF Core               │
│  - Authentication & Authorization                       │
│  - Dataset Deduplication & Archival                    │
│  - Experiment Orchestration                            │
└────────────────────┬────────────────────────────────────┘
          │                          │
    (REST API)              (Async Jobs)
          │                          │
          ↓                          ↓
┌─────────────────────┐    ┌─────────────────────┐
│   SQL SERVER 2022   │    │  PYTHON FASTAPI ML  │
│   Local/Docker      │    │  Service            │
│                     │    │  - Analysis         │
│ Tables:             │    │  - Preprocessing    │
│ - Users             │    │  - Training         │
│ - Datasets          │    │  - Feature Weights  │
│ - Experiments       │    │                     │
└─────────────────────┘    └─────────────────────┘
```

## 🛠️ Prerequisites
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Python 3.11](https://www.python.org/downloads/)
- SQL Server 2022 (LocalDB or Developer edition)
- Git

## 🚀 Quick Start (Local Run)

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/TrainWise.git
   cd TrainWise
   ```

2. **Setup the Database**
   Ensure SQL Server is running. The API is configured to use `.\SQLEXPRESS` by default. If using EF Core Migrations, run them against `TrainWise.API`.

3. **Start the ML Service**
   ```bash
   cd TrainWise.MLService
   python -m venv .venv
   source .venv/Scripts/activate # or .venv\Scripts\activate on Windows
   pip install -r requirements.txt
   uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
   ```

4. **Start the .NET Web API**
   ```bash
   cd ../TrainWise.API
   dotnet run
   # Runs on http://localhost:5000
   ```

5. **Start the Blazor Frontend**
   ```bash
   cd ../TrainWise.Web
   dotnet run
   # Runs on http://localhost:5002
   ```

## 🔐 Environment Variables
Configure these in the `appsettings.json` of your API project:

| Variable | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `MLService:BaseUrl` | Base URL for FastAPI (default: `http://localhost:8000`) |
| `Upload:MaxFileSizeMb` | Upload size limit (default: `50`) |
| `Upload:StorageMode` | Either `"disk"` or `"sql"` blob storage |

## 📡 Core API Reference
*(See `docs/api-contract.md` for the full list)*

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Authenticate user |
| `POST` | `/api/auth/signup` | Register user |
| `POST` | `/api/dataset/upload` | Upload new dataset |
| `POST` | `/api/training/train` | Execute ML training pipeline |
| `GET` | `/api/experiments/compare` | Compare two models |

## 🔮 Roadmap / Future Enhancements
- **v1.1**: JWT authentication replacement for current session-based auth.
- **v1.2**: Support for exporting PDF training reports.
- **v2.0**: Advanced Cloud Deployment (Azure), SHAP explainability, and Model saving/serving for live inference.

## 📄 License
This project is licensed under the MIT License.
