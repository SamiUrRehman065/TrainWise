# SOFTWARE REQUIREMENTS SPECIFICATION
## TrainWise — Web-Based ML Training Platform

| Field | Value |
|---|---|
| Document Version | 1.0 |
| Status | Final Draft |
| Date | May 10, 2026 |
| Department | Department of Computer Science |
| University | Muhammad Ali Jinnah University |
| Course | Final Year Project |
| Classification | Internal Use Only |

---

## Document Revision History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | Apr 15, 2026 | Project Team | Initial draft — core architecture and FR outline |
| 0.5 | Apr 28, 2026 | Project Team | Added NFRs, database schema, API endpoint specs |
| 0.9 | May 05, 2026 | Project Team | Added use cases, glossary, risk assessment, expanded FR-7 |
| 1.0 | May 10, 2026 | Project Team | Final review — all gaps addressed, approved for submission |

---

## 1. Introduction

### 1.1 Purpose
This Software Requirements Specification (SRS) defines the complete functional and non-functional requirements for TrainWise, a web-based machine learning training platform. It serves as a binding reference for developers, testers, and Final Year Project evaluators throughout the project lifecycle.

### 1.2 Scope
TrainWise provides an end-to-end classical ML workflow accessible via a web browser. Users can upload structured datasets, configure preprocessing steps, select and train ML models, visualize evaluation metrics, and view intelligent improvement recommendations. The system is designed for local deployment and is built on three tiers: a Blazor WebAssembly frontend, an ASP.NET Core Web API backend, and a Python FastAPI microservice for ML operations.

### 1.3 Intended Audience
- Final Year Project evaluators and academic supervisors
- Software developers implementing the system
- QA testers responsible for validation
- Project stakeholders reviewing scope and progress

### 1.4 Definitions, Acronyms & Abbreviations
Refer to Section 12 (Glossary) for a complete list of technical terms used in this document.

### 1.5 Document Conventions
- Functional requirements are labelled **FR-N** (e.g., FR-1, FR-2)
- Non-functional requirements are labelled **NFR-N**
- API endpoints are written as `HTTP METHOD /path`
- All data sizes are expressed in rows × columns unless otherwise stated

### 1.6 References
- ASP.NET Core 8 Documentation — https://docs.microsoft.com/aspnet/core
- FastAPI Documentation — https://fastapi.tiangolo.com
- Blazor WebAssembly Documentation — https://docs.microsoft.com/blazor
- scikit-learn Documentation — https://scikit-learn.org/stable
- Plotly.js Documentation — https://plotly.com/javascript

---

## 2. System Overview

### 2.1 Architecture
TrainWise follows a three-tier microservice architecture:

| Tier | Technology | Responsibility |
|---|---|---|
| Frontend | Blazor WebAssembly + Plotly.js (JS Interop) | User interface, file upload, chart rendering |
| Backend API | ASP.NET Core 8 Web API | Orchestration, file storage, experiment history |
| ML Service | Python 3.11 + FastAPI | Dataset analysis, model training, recommendations |
| Database | SQL Server 2022 (local) | Users, datasets, experiment metadata |
| File Storage | Local disk (server filesystem) | Raw CSV/XLSX dataset files |

### 2.2 High-Level Workflow
1. User registers/logs in (basic session authentication)
2. User uploads a CSV or XLSX dataset file
3. Backend stores the file and records metadata in SQL Server
4. ML Service analyses the dataset and returns a summary
5. User selects preprocessing options (encoding, scaling, null handling)
6. User selects a model type and configures hyperparameters
7. ML Service trains the model and returns evaluation metrics
8. Frontend displays metrics, charts, and rule-based recommendations
9. User optionally retrains with different settings
10. All experiment runs are persisted in SQL Server for history review

### 2.3 System Boundaries
TrainWise is scoped to classical supervised ML only. Deep learning, unsupervised clustering, and time-series forecasting are explicitly out of scope for version 1.0. The system operates entirely on-premises; no cloud infrastructure or external API calls are made.

---

## 3. Use Cases

### 3.1 Actors

| Actor | Description |
|---|---|
| End User (Data Scientist) | Primary actor. Uploads data, configures pipelines, reviews results. |
| System (Backend API) | Orchestrates requests between frontend and ML service. |
| ML Service (FastAPI) | Analyses data, trains models, generates recommendations. |
| Database (SQL Server) | Persists metadata, experiment history, and user sessions. |

### 3.2 Use Case Summary Table

| UC-ID | Use Case Name | Actor | Related FR |
|---|---|---|---|
| UC-01 | Upload Dataset | End User | FR-1 |
| UC-02 | Analyse Dataset | End User, ML Service | FR-2 |
| UC-03 | Configure Preprocessing | End User | FR-3 |
| UC-04 | Select & Train Model | End User, ML Service | FR-4, FR-5 |
| UC-05 | View Results & Charts | End User | FR-6 |
| UC-06 | Receive Recommendations | End User, ML Service | FR-7 |
| UC-07 | Retrain with New Config | End User | FR-8 |
| UC-08 | View Experiment History | End User | FR-9 |

### 3.3 Detailed Use Cases

#### UC-01: Upload Dataset

| Field | Details |
|---|---|
| Primary Actor | End User |
| Preconditions | User is logged in; file is CSV or XLSX ≤50 MB |
| Main Flow | 1. User navigates to Upload page. 2. User selects a local file. 3. System validates format and file size. 4. System stores file and records metadata. 5. System displays upload confirmation with dataset ID. |
| Alternate Flow | If format is invalid → system displays error: "Only CSV/XLSX supported." |
| Postconditions | Dataset is stored; DatasetId is generated; user is redirected to Analysis view. |

#### UC-04: Select & Train Model

| Field | Details |
|---|---|
| Primary Actor | End User |
| Preconditions | Dataset uploaded; preprocessing config selected |
| Main Flow | 1. User selects task type (Classification / Regression). 2. System shows available models for that task. 3. User picks a model and optionally sets hyperparameters. 4. User sets train/test split ratio. 5. User submits training request. 6. Backend forwards request to ML Service. 7. ML Service trains model and returns metrics JSON. 8. Frontend renders metrics and charts. |
| Alternate Flow | If training exceeds 90 s → system returns timeout error and prompts user to reduce dataset size. |
| Postconditions | Experiment record saved; metrics and charts displayed. |

---

## 4. Functional Requirements

### FR-1: Dataset Upload

| Attribute | Detail |
|---|---|
| ID | FR-1 |
| Priority | High |
| Status | Mandatory |
| Description | The system shall allow authenticated users to upload structured datasets. |
| Accepted Formats | CSV (.csv), Excel (.xlsx) |
| Maximum File Size | 50 MB |
| Validation | File extension check; MIME type check; row count > 0; at least 2 columns |
| Storage | File saved to `/data/uploads/{userId}/{datasetId}/` on the server filesystem |
| Error Handling | Invalid format → HTTP 400 with message. File too large → HTTP 413. |

### FR-2: Dataset Analysis

| Attribute | Detail |
|---|---|
| ID | FR-2 |
| Priority | High |
| Status | Mandatory |
| Description | After upload, the ML Service shall automatically analyse the dataset. |
| Trigger | Automatically on successful upload via `POST /analyze` |
| Performance | Must complete within 10 seconds for datasets up to 10,000 rows × 50 columns |

**Analysis Output:**
- Row and column counts
- Column data types (numerical / categorical / datetime)
- Missing value count and percentage per column
- Summary statistics (mean, median, std, min, max) for numerical columns
- Class distribution for target column
- Pairwise Pearson correlation matrix
- Dataset imbalance ratio (if classification task detected)

### FR-3: Preprocessing Options

| Attribute | Detail |
|---|---|
| ID | FR-3 |
| Priority | High |
| Status | Mandatory |
| Description | User shall be able to configure a preprocessing pipeline before training. |
| Null Handling | Drop rows with nulls; Fill with mean (numerical); Fill with median (numerical); Fill with mode (categorical) |
| Encoding | One-hot encoding (nominal); Label encoding (ordinal) |
| Scaling | StandardScaler (z-score); MinMaxScaler (0–1 normalization) |
| Class Imbalance | SMOTE oversampling (classification tasks only, requires imbalance ratio > 1.5:1) |
| Column Selection | User can exclude irrelevant columns before training |

### FR-4: Model Selection

| Task | Available Models |
|---|---|
| Classification | Logistic Regression, Random Forest Classifier, Support Vector Machine (SVM), K-Nearest Neighbours (KNN) |
| Regression | Linear Regression, Random Forest Regressor, SVR (Support Vector Regressor) |
| Configurable Hyperparameters | n_estimators (Random Forest); C and kernel (SVM); n_neighbors (KNN); max_iter (Logistic Regression) |

### FR-5: Model Training & Evaluation

| Attribute | Detail |
|---|---|
| ID | FR-5 |
| Priority | High |
| Status | Mandatory |
| Train/Test Split | User selects ratio: 70/30, 75/25, or 80/20 (default: 80/20) |
| Cross-Validation | Optional 5-fold CV (user toggle) |
| Classification Metrics | Accuracy, Precision (macro), Recall (macro), F1-Score (macro), Confusion Matrix |
| Regression Metrics | R² Score, RMSE, MAE |
| Output Format | JSON response (see Section 6.3 for schema) |
| Persistence | All metrics saved to Experiments table in SQL Server |

### FR-6: Visualization

| Chart | Description | Library |
|---|---|---|
| Accuracy Bar Chart | Compares metrics across multiple training runs | Plotly.js |
| Confusion Matrix Heatmap | Grid of actual vs predicted classes with colour intensity | Plotly.js |
| Feature Importance Chart | Horizontal bar chart of top-N features (Random Forest only) | Plotly.js |
| Correlation Heatmap | Full pairwise correlation matrix of numerical columns | Plotly.js |
| Class Distribution Pie Chart | Target class balance visualisation | Plotly.js |

### FR-7: Intelligent Recommendation Engine

The system shall apply a rule-based engine after every training run to surface actionable improvement suggestions. Each rule has a condition and a corresponding recommendation message displayed to the user.

| Rule ID | Condition | Recommendation Shown to User |
|---|---|---|
| R-01 | Missing value percentage > 20% in any column | "High missing data detected in [column]. Consider dropping this column or using model-based imputation." |
| R-02 | Class imbalance ratio > 2:1 and SMOTE not applied | "Dataset imbalance detected. Enable SMOTE oversampling to improve minority-class recall." |
| R-03 | Accuracy > 98% and cross-validation not enabled | "Near-perfect accuracy may indicate overfitting. Enable 5-fold cross-validation to verify." |
| R-04 | F1-Score < 0.60 | "Low F1-Score. Try feature scaling, a different kernel (SVM), or increase training data." |
| R-05 | Random Forest selected and n_estimators < 50 | "Low tree count detected. Increase n_estimators to ≥ 100 for more stable predictions." |
| R-06 | RMSE > 1 standard deviation of target column | "High RMSE relative to target spread. Consider feature engineering or non-linear models." |
| R-07 | Dataset rows < 500 | "Small dataset detected. Accuracy may be unreliable. Collect more data or use cross-validation." |
| R-08 | No scaling applied and model is SVM or KNN | "SVM and KNN are sensitive to feature scale. Apply StandardScaler or MinMaxScaler." |

### FR-8: Retraining

| Attribute | Detail |
|---|---|
| ID | FR-8 |
| Priority | Medium |
| Status | Mandatory |
| Description | User can modify any preprocessing or model setting and resubmit for training. |
| Behaviour | Each retrain creates a new Experiment record; previous runs are preserved. |
| UI | 'Retrain' button on results page pre-populates the config form with current settings. |

### FR-9: Experiment History

| Attribute | Detail |
|---|---|
| ID | FR-9 |
| Priority | Medium |
| Status | Mandatory |
| Description | All training runs are saved and viewable in an Experiment History table. |
| Displayed Columns | Experiment ID, Dataset Name, Model, Key Metrics, Preprocessing Config, Date/Time |
| Actions | View full metrics; compare two experiments side-by-side; delete experiment |

---

## 5. Non-Functional Requirements

| ID | Category | Requirement | Measure / Acceptance Criterion |
|---|---|---|---|
| NFR-1 | Performance | Dataset analysis shall complete within defined time bounds. | ≤10 s for ≤10k rows × 50 cols; ≤30 s for ≤50k rows × 100 cols; ≤60 s for ≤100k rows |
| NFR-2 | Performance | Model training shall complete within defined time bounds. | Small (<5k rows): ≤15 s; Medium (5k–50k rows): ≤60 s; Large (50k–100k rows): ≤120 s |
| NFR-3 | Usability | UI shall follow a linear step-by-step wizard pattern. | User completes full pipeline in ≤10 clicks from upload to results |
| NFR-4 | Usability | All error messages shall be human-readable with corrective guidance. | No raw stack traces displayed to the user under any circumstance |
| NFR-5 | Reliability | ML Service downtime shall not crash the frontend. | Frontend gracefully shows 'ML Service unavailable' banner; status code 503 |
| NFR-6 | Reliability | Invalid or corrupt dataset files shall be handled without server crash. | System returns HTTP 422 with a descriptive message; no unhandled exceptions in logs |
| NFR-7 | Security | File upload endpoint shall validate and sanitize all inputs. | File size ≤50 MB enforced; MIME type checked; filename sanitized; path traversal blocked |
| NFR-8 | Security | Basic session authentication required for all routes. | Unauthenticated requests return HTTP 401; sessions expire after 8 hours of inactivity |
| NFR-9 | Maintainability | ML Service shall be independently deployable and restartable. | Restart of FastAPI service does not require restart of ASP.NET Core API |
| NFR-10 | Scalability | Architecture shall support future addition of new models without core refactoring. | New model added via single model class + registration in model factory (Open/Closed Principle) |

> **Authentication Risk Note:** Version 1.0 implements only session-based authentication and is designed for local network deployment only. There is no JWT, HTTPS enforcement, or role-based access control in this version. This is a known, accepted risk documented in Section 11.

---

## 6. External Interface Requirements

### 6.1 Frontend → Backend API Endpoints (ASP.NET Core)

| Method | Endpoint | Request Body | Response | Description |
|---|---|---|---|---|
| POST | `/api/auth/login` | `{ username, password }` | `{ sessionToken, userId }` | Authenticate user |
| POST | `/api/dataset/upload` | `multipart/form-data (file)` | `{ datasetId, fileName, rowCount }` | Upload dataset |
| GET | `/api/dataset/{id}/summary` | — | DatasetSummary JSON | Get analysis results |
| POST | `/api/train` | TrainRequest JSON | `{ experimentId, metrics }` | Submit training job |
| GET | `/api/experiment/{id}` | — | ExperimentDetail JSON | Fetch experiment details |
| GET | `/api/experiment/history` | `?userId=&page=&pageSize=` | ExperimentList JSON | List all experiments |
| DELETE | `/api/experiment/{id}` | — | `{ success: true }` | Delete experiment record |

### 6.2 Backend API → ML Service Endpoints (FastAPI)

| Method | Endpoint | Request Body | Response | Description |
|---|---|---|---|---|
| POST | `/analyze` | `{ filePath, datasetId }` | DatasetSummary JSON | Run dataset analysis |
| POST | `/train` | TrainRequest JSON | TrainResult JSON | Train model and return metrics |
| POST | `/recommend` | `{ metrics, config }` | Recommendation[] JSON | Get rule-based recommendations |
| GET | `/health` | — | `{ status: "ok" }` | ML Service health check |

### 6.3 JSON Schemas

#### TrainRequest — `POST /api/train`

```json
{
  "datasetId": "string",
  "targetColumn": "string",
  "taskType": "classification | regression",
  "preprocessing": {
    "nullStrategy": "drop | mean | median | mode",
    "encoding": "onehot | label",
    "scaling": "standard | minmax | none",
    "applySmote": false,
    "excludeColumns": ["string"]
  },
  "model": {
    "name": "LogisticRegression | RandomForest | SVM | KNN | LinearRegression | RandomForestRegressor | SVR",
    "hyperparameters": {
      "n_estimators": 100,
      "C": 1.0,
      "kernel": "rbf",
      "n_neighbors": 5
    }
  },
  "trainTestSplit": 0.8,
  "crossValidation": false
}
```

#### TrainResult — Response from `/train`

```json
{
  "experimentId": "string",
  "modelName": "string",
  "taskType": "classification | regression",
  "classificationMetrics": {
    "accuracy": 0.94,
    "precision": 0.93,
    "recall": 0.92,
    "f1Score": 0.925,
    "confusionMatrix": [[50, 3], [4, 43]]
  },
  "regressionMetrics": {
    "r2Score": 0.87,
    "rmse": 4.23,
    "mae": 3.11
  },
  "featureImportances": { "feature_name": 0.42 },
  "trainingDurationSeconds": 8.4,
  "recommendations": [
    { "ruleId": "R-02", "message": "Dataset imbalance detected. Enable SMOTE oversampling to improve minority-class recall." }
  ]
}
```

### 6.4 Frontend Charts (Blazor JS Interop)
Plotly.js is loaded via CDN and invoked from Blazor components through JS Interop. Each chart is rendered in a designated `<div>` element with a unique ID. The Blazor component passes serialized JSON data to a JavaScript wrapper function that calls `Plotly.newPlot()`.

---

## 7. Database Requirements (SQL Server)

### 7.1 Entity Relationship Overview
`Users (1) → (*) Datasets (1) → (*) Experiments`

Each Experiment belongs to one Dataset and stores all configuration and metrics as JSON columns for flexibility.

### 7.2 Table Definitions

#### Table: Users

| Column | Type | Constraints | Description |
|---|---|---|---|
| UserId | UNIQUEIDENTIFIER | PK, DEFAULT NEWID() | Unique user identifier |
| Username | NVARCHAR(100) | NOT NULL, UNIQUE | Login username |
| PasswordHash | NVARCHAR(256) | NOT NULL | BCrypt hashed password |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Account creation timestamp |
| LastLoginAt | DATETIME2 | NULL | Last successful login timestamp |

#### Table: Datasets

| Column | Type | Constraints | Description |
|---|---|---|---|
| DatasetId | UNIQUEIDENTIFIER | PK, DEFAULT NEWID() | Unique dataset identifier |
| UserId | UNIQUEIDENTIFIER | FK → Users.UserId | Owning user |
| FileName | NVARCHAR(255) | NOT NULL | Original filename |
| FilePath | NVARCHAR(512) | NOT NULL | Server-side storage path |
| RowCount | INT | NOT NULL | Number of data rows |
| ColumnCount | INT | NOT NULL | Number of columns |
| UploadedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Upload timestamp |
| AnalysisSummaryJson | NVARCHAR(MAX) | NULL | Cached analysis result JSON |

#### Table: Experiments

| Column | Type | Constraints | Description |
|---|---|---|---|
| ExperimentId | UNIQUEIDENTIFIER | PK, DEFAULT NEWID() | Unique experiment identifier |
| DatasetId | UNIQUEIDENTIFIER | FK → Datasets.DatasetId | Source dataset |
| ModelName | NVARCHAR(100) | NOT NULL | Model class name |
| TaskType | NVARCHAR(20) | NOT NULL | 'classification' or 'regression' |
| PreprocessingJson | NVARCHAR(MAX) | NOT NULL | Serialized preprocessing config |
| HyperparametersJson | NVARCHAR(MAX) | NULL | Serialized hyperparameter config |
| MetricsJson | NVARCHAR(MAX) | NOT NULL | Serialized evaluation metrics |
| TrainingDurationSec | FLOAT | NULL | Wall-clock training time |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETDATE() | Experiment run timestamp |

---

## 8. Assumptions & Constraints

### 8.1 Assumptions
- All users have a web browser capable of running Blazor WebAssembly (Chrome 80+, Edge 80+, Firefox 75+)
- The host machine meets minimum hardware requirements: 8 GB RAM, 4-core CPU, 20 GB free disk space
- SQL Server 2019 or later is installed and accessible on the local machine
- Python 3.10+ and required packages (scikit-learn, pandas, imbalanced-learn, fastapi, uvicorn) are pre-installed
- Datasets contain structured tabular data with a clear target column
- All users are trusted internal users (local network deployment only)

### 8.2 Constraints
- Only CSV and XLSX file formats are supported. JSON, SQL dumps, and Parquet files are not in scope
- Maximum supported dataset size: 100,000 rows × 200 columns
- The system must run entirely on-premises; no internet connectivity required for core functionality
- SQL Server (not SQLite or PostgreSQL) is the required RDBMS for this version
- Deep learning, neural networks, and unsupervised learning are out of scope for version 1.0

---

## 9. Risk Assessment

| Risk ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R-01 | Blazor + Plotly.js interop complexity causes rendering bugs | Medium | High | Prototype charts early; maintain fallback HTML table view |
| R-02 | FastAPI ↔ ASP.NET Core network latency on large datasets | Medium | Medium | Implement async polling with progress indicator; add 90-second timeout |
| R-03 | No authentication in MVP exposes experiment data to all local users | Low | Medium | Documented as known risk; session auth added; JWT planned for v1.1 |
| R-04 | SMOTE not applied but recommended — user confusion | Low | Low | UI shows clear warning banner; SMOTE toggle is prominently placed |
| R-05 | SQL Server licensing or setup issues on evaluator machine | Low | Medium | Provide Docker Compose setup as alternative; include setup guide |
| R-06 | scikit-learn version mismatch breaks serialized models | Low | High | Pin all Python package versions in requirements.txt; use virtual environment |
| R-07 | Dataset with > 100k rows causes memory overflow in ML Service | Medium | High | Enforce 100k row hard limit at upload; return HTTP 413 with message |

---

## 10. Future Enhancements (Post-MVP)

| Enhancement | Description | Target Version |
|---|---|---|
| JWT Authentication | Replace session auth with stateless JWT tokens and RBAC | v1.1 |
| Model Artifact Storage | Serialize and persist trained model files for later inference | v1.1 |
| PDF Report Export | Generate downloadable PDF summary of any experiment run | v1.2 |
| Auto Model Comparison Leaderboard | Train all available models on one click and rank by primary metric | v1.2 |
| SHAP Explainability | Integrate SHAP values for feature contribution analysis (global + local) | v2.0 |
| Cloud Deployment | Deploy to Azure (App Service + Azure SQL + Azure Container Instances) | v2.0 |
| Deep Learning Support | Integrate Keras/TensorFlow for simple feedforward networks | v2.0 |
| Unsupervised Learning | K-Means clustering, PCA visualization | v2.1 |

---

## 11. Known Issues & Accepted Limitations

| Issue ID | Description | Accepted? | Notes |
|---|---|---|---|
| KI-01 | No HTTPS enforcement — traffic is plaintext on local network | Yes | Local-only deployment; to be addressed in cloud version |
| KI-02 | No file encryption at rest for uploaded datasets | Yes | Local trust model; disk encryption left to OS/admin |
| KI-03 | Session tokens stored in memory; lost on server restart | Yes | Known trade-off for simplicity; JWT planned for v1.1 |
| KI-04 | Blazor WASM initial load time may be 3–5 seconds on first visit | Yes | AOT compilation or lazy loading to be explored in v1.1 |
| KI-05 | SMOTE is in recommendations but preprocessing config is not auto-applied | Yes | User must manually enable SMOTE toggle based on recommendation |

---

## 12. Glossary

| Term | Definition |
|---|---|
| API | Application Programming Interface — a set of rules that allow software components to communicate. |
| ASP.NET Core | An open-source, cross-platform web framework by Microsoft for building APIs and web apps. |
| Blazor WebAssembly | A client-side web framework by Microsoft that runs C# in the browser via WebAssembly. |
| CORS | Cross-Origin Resource Sharing — HTTP mechanism that controls which origins can access a resource. |
| CSV | Comma-Separated Values — a plain-text file format for tabular data. |
| F1-Score | Harmonic mean of Precision and Recall; useful for imbalanced classification problems. |
| FastAPI | A modern Python web framework for building APIs with automatic OpenAPI documentation. |
| Feature Importance | A score indicating how useful or valuable a feature is in constructing the decision trees in a model. |
| JS Interop | JavaScript Interop — mechanism allowing Blazor components to call JavaScript functions and vice versa. |
| KNN | K-Nearest Neighbours — a non-parametric classification/regression algorithm. |
| Label Encoding | Converts categorical text values to integer labels (e.g., 'cat' → 0, 'dog' → 1). |
| MAE | Mean Absolute Error — average of absolute differences between predicted and actual values. |
| MinMaxScaler | Scales features to a fixed range [0, 1] by subtracting min and dividing by range. |
| MIME Type | Media type identifier — used to tell browsers the type of uploaded file content. |
| ML | Machine Learning — a subfield of AI where systems learn patterns from data. |
| One-Hot Encoding | Converts a categorical column into N binary columns, one per unique category. |
| Plotly.js | An open-source JavaScript charting library for interactive visualizations. |
| R² Score | Coefficient of Determination — proportion of variance explained by the model (1.0 = perfect). |
| RMSE | Root Mean Squared Error — square root of average squared differences between predictions and actuals. |
| Rule-Based Engine | A deterministic decision system that fires pre-defined if-condition → recommendation rules. |
| SHAP | SHapley Additive exPlanations — a game-theory-based approach to explain individual model predictions. |
| SMOTE | Synthetic Minority Oversampling Technique — generates synthetic samples for minority classes to address imbalance. |
| SQL Server | Microsoft's relational database management system used for structured data storage. |
| SRS | Software Requirements Specification — a document describing the intended purpose, capabilities, and constraints of a software system. |
| StandardScaler | Scales features to have zero mean and unit variance (z-score normalization). |
| SVM | Support Vector Machine — a supervised learning model that finds the optimal separating hyperplane. |
| XLSX | Microsoft Excel Open XML Spreadsheet format. |

---

*Document End — TrainWise SRS v1.0 — Confidential / Internal Use Only*
