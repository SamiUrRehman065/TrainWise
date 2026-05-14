# 🧠 TrainWise: Full-Stack ML Training Platform

## 📌 Overview
**TrainWise** is an enterprise-grade, web-based toolkit that performs:
* **Smart Dataset Management** (upload, analysis, deduplication)
* **Configurable Preprocessing** (null handling, encoding, scaling, SMOTE)
* **Multi-Model Training** (Logistic Regression, Random Forests, XGBoost, etc.)
* **Advanced Experiment Comparison** (side-by-side metric analytics)

Built using **C# (.NET 8)**, **Blazor WebAssembly**, **Python (FastAPI)**, and **SQL Server**, it provides a clean, responsive web interface for classical machine learning workflows.

This project demonstrates a **modular microservices app architecture**, clean separation of **frontend**, **REST API**, and **ML microservices**, and user-friendly interaction with interactive Plotly.js charts.

---

## ✨ Key Features

### 📂 Dataset Management
* Upload CSV/XLSX files with automatic deduplication (SHA-256 hash checking)
* Analyze datasets to extract feature types, counts, and null distributions

### ⚙️ Preprocessing Pipeline
* Automatically handles null values (mean/median/mode)
* Applies categorical encodings (One-hot/Factorize) and drops high-cardinality noise
* Applies SMOTE for imbalanced classes and various scalers (MinMax/Standard)

### 🤖 Multi-Model Training & Comparison
* Train multiple classical ML models with a single click
* Interactive analytics: view confusion matrices and precision-recall graphs via Plotly
* Side-by-side comparison UI for experiments, with visual bar charts for feature importances

### 🌐 Web Interface
* Built with **Blazor WebAssembly** for a fast, responsive SPA experience.
* **Premium UI**: Uses a custom **Deep Navy Glassmorphism** design system with smooth animations and high-contrast typography.
* Centralized dashboard and experiment tracking with a modern "Command Center" aesthetic.

---

## 🏗️ Architecture

```text
┌─────────────────────────────────────────────────────────┐
│                 FRONTEND LAYER                           │
│         Blazor WebAssembly + Plotly.js                  │
└────────────────────┬────────────────────────────────────┘
                     │ HTTPS (dev: HTTP)
                     ↓
┌─────────────────────────────────────────────────────────┐
│                 BACKEND API LAYER                        │
│         ASP.NET Core 8 Web API + EF Core               │
└────────────────────┬────────────────────────────────────┘
          │                          │
    (REST API)              (Async Jobs)
          │                          │
          ↓                          ↓
┌─────────────────────┐    ┌─────────────────────┐
│   SQL SERVER 2022   │    │  PYTHON FASTAPI ML  │
│   Local/Docker      │    │  Service            │
└─────────────────────┘    └─────────────────────┘
```

---

## 🧱 Project Structure

```text
TrainWise/
│
├── TrainWise.Web/                   # Blazor WebAssembly frontend
│   ├── Components/                  # Reusable UI elements
│   │   ├── Common/                  # Shared components (Breadcrumbs, Pagination)
│   │   ├── Layout/                  # MainLayout, App structure
│   │   └── Navigation/              # TopNav bar
│   ├── Pages/                       # Full-screen routable components
│   │   ├── Auth/                    # Login.razor, Signup.razor
│   │   ├── Datasets/                # Datasets.razor, DatasetDetail.razor
│   │   ├── Experiments/             # History.razor, CompareExperiments.razor
│   │   └── Training/                # Train.razor
│   ├── Services/                    # Blazor services
│   │   ├── Api/                     # Typed API clients (DatasetApi, TrainingApi)
│   │   └── State/                   # SessionState manager
│   └── wwwroot/                     # Static assets, CSS, Plotly integrations, JS scripts
│
├── TrainWise.API/                   # ASP.NET Core 8 backend API
│   ├── Controllers/                 # REST Endpoints (Auth, Datasets, Training)
│   ├── Data/                        # EF Core DbContext
│   │   └── Models/                  # DB Entities (User, Dataset, Experiment)
│   ├── DTOs/                        # Request/Response Data Transfer Objects
│   ├── Middleware/                  # Global Exception Handling Middleware
│   ├── Program.cs                   # Dependency Injection & Pipeline
│   └── Services/                    # Business Logic Layer
│       ├── Auth/                    # Session Store & Registration logic
│       ├── Background/              # Background jobs (Auto-archival service)
│       ├── Dataset/                 # File IO, Hash deduplication, Validation
│       └── Training/                # HTTP clients communicating with Python API
│
├── TrainWise.MLService/             # Python FastAPI microservice
│   ├── app/
│   │   ├── core/                    # FastAPI app setup
│   │   ├── models/                  # Pydantic schemas (TrainRequest, TrainResult)
│   │   ├── routers/                 # API routes (/analyze, /train, /recommend)
│   │   └── services/                
│   │       ├── trainer.py           # Scikit-learn pipelines & Pandas preprocessing
│   │       ├── model_factory.py     # Machine learning algorithm instantiations
│   │       └── recommender.py       # Rule-based dataset recommendations
│   └── requirements.txt             # Python package dependencies
│
├── TrainWise.Database/              # SQL scripts and tools
│   ├── schema/                      # Initial database creation SQL scripts
│   └── backfill/                    # Maintenance C# utility scripts
│
├── docs/                            # Project documentation
│   ├── api/                         # API Contracts and Postman collections
│   └── planning/                    # SRS, Sprint Plans, Architecture validations
├── docker-compose.yml               # Container orchestration (if available)
├── README.md                        # Project documentation
└── TrainWise.sln                    # Visual Studio Solution file
```

---

## 🧮 Module Breakdown

| Module | Purpose |
| ------ | ------- |
| `TrainWise.Web` | Blazor SPA frontend for all user interactions |
| `TrainWise.API` | Central API gateway handling auth, db storage, and routing to ML |
| `TrainWise.MLService` | Python microservice wrapping pandas and scikit-learn |
| `trainer.py` | Core ML logic for preprocessing, encoding, and fitting models |
| `CompareExperiments.razor` | Analytical UI for comparing feature importances |
| `AppDbContext.cs` | Entity Framework Core context for SQL Server integration |

---

## 🖥️ How It Works

### 🔍 Flow for Dataset Ingestion
1. User uploads a CSV/XLSX file via the Blazor frontend.
2. The .NET API calculates a SHA-256 hash to prevent duplicate storage.
3. The file is sent to the Python ML Service for deep schema analysis.
4. The dataset summary is saved to the SQL Server database.

### 🔍 Flow for Training
1. User selects a dataset, target column, and preprocessing options.
2. The .NET API forwards the configuration to the FastAPI ML service.
3. The Python service pre-processes the data, trains the chosen model (e.g. RandomForest), and extracts feature importances and metrics.
4. The .NET API saves the `TrainResult` and the Blazor UI displays interactive Plotly charts.

---

## 🖥️ Technologies Used

| Technology | Role |
| ---------- | ---- |
| C# & .NET 8 | Backend REST API & Blazor WebAssembly |
| Python 3.11 | ML microservice backend scripting |
| FastAPI | High-performance Python web framework |
| SQL Server 2022 | Relational database for users and experiments |
| Scikit-learn & Pandas | Data manipulation and classical ML training |
| Plotly.js | Interactive frontend data visualization |

---

## 🚀 How to Run Locally

1. Clone the repo:
   ```bash
   git clone https://github.com/SamiUrRehman065/TrainWise.git
   cd TrainWise
   ```

2. Setup the ML Service (Python):
   ```bash
   cd TrainWise.MLService
   python -m venv venv
   .\venv\Scripts\Activate.ps1   # Windows PowerShell
   pip install -r requirements.txt
   uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
   ```

3. Setup the .NET API & Database:
   Ensure SQL Server `.\SQLEXPRESS` is running.
   ```bash
   cd ../TrainWise.API
   dotnet run
   ```

4. Run the Blazor Frontend:
   ```bash
   cd ../TrainWise.Web
   dotnet run
   ```

5. Open the browser at the provided localhost link (usually `http://localhost:53351` or `http://localhost:5002`).

---

## 📡 Core API Reference

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Authenticate user |
| `POST` | `/api/dataset/upload` | Upload new dataset |
| `POST` | `/api/training/train` | Execute ML training pipeline |
| `GET` | `/api/experiments/compare` | Compare two models |

---

## 🔮 Roadmap / Future Enhancements
* **v1.1**: JWT authentication replacement for current session-based auth.
* **v1.2**: Support for exporting PDF training reports.
* **v2.0**: Advanced Cloud Deployment (Azure), SHAP explainability, and Model saving/serving for live inference.

---

## ⚠️ Notes
* TrainWise uses **session-based authentication** (in-memory) for the local MVP deployment.
* Designed for **classical machine learning** (Random Forests, Logistic Regression, etc.).
* CSV deduplication works across the entire workspace automatically.

---

## 🧑‍💻 Author

**Name:** Sami Ur Rehman  
**Location:** Karachi, Pakistan  
**GitHub:** [SamiUrRehman065](https://github.com/SamiUrRehman065)

---

## 🪞 Developer Reflection

### What I Learned
* How to architect a polyglot microservices system (.NET + Python).
* Seamless integration between Blazor WebAssembly and Javascript libraries like Plotly.js.
* Building complex machine learning preprocessing pipelines that handle real-world messy tabular data.
* Structuring robust REST APIs with Entity Framework Core and SQL Server.

### Challenges
* Preventing model "feature explosion" during one-hot encoding of high-cardinality categorical variables.
* Handling asynchronous communication and timeouts between the C# API and the Python ML service during long training runs.
* Synchronizing the database schemas across polyglot microservices.

### Solutions
* Implemented dynamic dropping of identifier columns (e.g., `id`, `name`) and high-cardinality features (>50 uniques) in the Python pipeline.
* Used task queues and robust error handling in the .NET API to gracefully manage long-running Fast API calls.
* Kept a strict JSON contract (`TrainRequest` and `TrainResult`) shared between C# records and Pydantic models.

---

## 🤝 Contributing

Contributions welcome! 🎉
Feel free to open issues or submit PRs with new features, bug fixes, or improvements.
