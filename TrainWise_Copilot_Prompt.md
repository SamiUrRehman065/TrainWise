# GitHub Copilot — TrainWise Project Planning Prompt

> **How to use:**
> 1. Place both files in your VS Code project root:
>    - `TrainWise_SRS_v1.0.md`  ← Copilot reads this
>    - `TrainWise_Copilot_Prompt.md` ← this file
> 2. Open **GitHub Copilot Chat** → switch to **Agent mode**
> 3. Copy everything after the divider below and paste it into Copilot Chat

---

---

## PROMPT — PASTE EVERYTHING BELOW INTO COPILOT CHAT

---

@workspace

You are acting as a **senior software architect** for a Final Year Project called **TrainWise** — a web-based machine learning training platform.

The file `TrainWise_SRS_v1.0.md` is in this workspace. **Read it fully and completely before doing anything else.** It contains 12 sections covering architecture, 9 functional requirements, 10 non-functional requirements, all API contracts, database schema, use cases, risks, and a glossary.

Complete every task below **in order**. Do not skip or summarise any task.

---

## TASK 1 — Confirm You Read the SRS

After reading `TrainWise_SRS_v1.0.md`, output a confirmation paragraph that includes:
- What TrainWise does (one sentence)
- The three tiers and their technologies
- Total count of Functional Requirements (FR-1 to FR-9) and Non-Functional Requirements (NFR-1 to NFR-10)
- The three database tables and their relationships

Do not move to Task 2 until this is done.

---

## TASK 2 — Complete Project Folder Structure

Generate the **full folder and file tree** for the entire TrainWise solution. Rules:

- Solution root folder: `TrainWise/`
- Three projects inside:
  - `TrainWise.Web/` — Blazor WebAssembly frontend
  - `TrainWise.API/` — ASP.NET Core 8 Web API backend
  - `TrainWise.MLService/` — Python FastAPI ML microservice
- Additional top-level folders:
  - `TrainWise.Database/` — SQL scripts and migrations
  - `docs/` — SRS, diagrams, and planning docs
  - `docker/` — Dockerfiles per service + docker-compose.yml
  - `.github/workflows/` — CI/CD pipeline stubs

**Requirements for the tree:**
- Go **at least 4 levels deep** for each project
- Show **actual file names** (not just folder names), including `.cs`, `.razor`, `.py`, `.json`, `.sql`, `.yml` files
- Add a `# comment` beside every file and folder explaining its purpose
- Every FR (FR-1 to FR-9) must be traceable to at least one file in the tree

Output as a **fenced code block** in tree format:

```
TrainWise/
├── TrainWise.API/
│   ├── Controllers/
│   │   └── DatasetController.cs   # FR-1: handles dataset upload and summary
│   ...
```

---

## TASK 3 — File Responsibility Map

Create a markdown table mapping every major file to its role:

| File Path | Project | Layer | Responsibility | Related FR/NFR |
|---|---|---|---|---|

Every FR-1 through FR-9 and NFR-1 through NFR-10 must appear in at least one row.

---

## TASK 4 — Technology Stack Table

Confirm every technology choice with a justification table:

| Component | Technology | Version | Why This Choice | Alternative Considered |
|---|---|---|---|---|

Must cover: Frontend framework, Backend framework, ML framework, ORM (EF Core), HTTP client (Blazor→API), HTTP client (API→FastAPI), Chart library, Auth method, JSON serialization, C# package manager, Python package manager, Containerization tool.

---

## TASK 5 — Database Setup (SQL + EF Core)

Based on Section 7 of the SRS, produce:

**5a. SQL Server CREATE TABLE script** — all 3 tables (Users, Datasets, Experiments) with exact column names, types, constraints, PKs, FKs, and DEFAULT values as defined in the SRS.

**5b. EF Core model classes** in C# — one class per table, placed in `TrainWise.API/Data/Models/`. Include data annotations and navigation properties.

**5c. DbContext class** (`AppDbContext.cs`) — with all three `DbSet<T>` properties and `OnModelCreating` with Fluent API configuration matching the SRS schema.

**5d. Migration command** — the exact `dotnet ef` command to run after setup, including the migration name.

---

## TASK 6 — Full API Contract + Controller Signatures

**6a.** For every endpoint in SRS Sections 6.1 and 6.2, produce a complete table:

| Endpoint | Method | Auth Required | Request Body/Params | Success Response | Error Codes | Related FR |
|---|---|---|---|---|---|---|

**6b.** For these 3 endpoints, write the full C# controller method signature with XML doc comments:
- `POST /api/dataset/upload`
- `POST /api/train`
- `POST /analyze` (FastAPI)

Each signature must include: route attribute, HTTP method attribute, `[Authorize]` if needed, typed parameters, return type, and a `/// <summary>` block.

---

## TASK 7 — Python ML Service Plan (FastAPI)

**7a. `requirements.txt`** — all packages with pinned versions:
fastapi, uvicorn, scikit-learn, pandas, numpy, imbalanced-learn, pydantic, python-multipart, httpx.

**7b. Pydantic models** for:
- `TrainRequest` — matching the JSON schema in SRS Section 6.3 exactly
- `TrainResult` — matching the JSON schema in SRS Section 6.3 exactly
- `DatasetSummary`
- `Recommendation`

**7c. Router skeleton** (`routers/train.py`) — function signatures + docstrings for `analyze()`, `train()`, `recommend()`, `health_check()`.

**7d. ModelFactory class** (`services/model_factory.py`) — maps model name strings (e.g. `"RandomForest"`) to scikit-learn instances. Must support the Open/Closed Principle (NFR-10): adding a new model requires only one line change.

**7e. RecommendationEngine class** (`services/recommendation_engine.py`) — implement all 8 rules from FR-7 (R-01 through R-08) as individual methods. Each method checks its condition and returns a `Recommendation` object or `None`.

---

## TASK 8 — Sprint Plan (8-Week FYP Roadmap)

Break the project into **4 sprints** (2 weeks each):

| Sprint | Weeks | Goal | Deliverables | FR/NFR Covered | Definition of Done |
|---|---|---|---|---|---|

After the table, list the **top 2 risks per sprint** with mitigations cross-referenced to SRS Section 9.

---

## TASK 9 — README.md

Generate the complete `README.md` for the project root with these exact sections:

1. **Project Title + Badge row** (Blazor, ASP.NET Core, FastAPI, SQL Server, Python)
2. **What is TrainWise** — 2-paragraph description
3. **Architecture Diagram** — ASCII art showing Frontend ↔ API ↔ ML Service ↔ DB with request flow arrows
4. **Prerequisites** — exact tools and versions to install
5. **Quick Start** — numbered steps: clone → DB setup → run ML Service → run API → run frontend
6. **Environment Variables** — table of all config values needed (connection strings, ports, etc.)
7. **Project Structure** — compact tree (2 levels deep)
8. **API Reference** — all 7 frontend→API endpoints in a table
9. **Known Limitations** — from SRS Section 11
10. **Roadmap** — from SRS Section 10
11. **Contributing** — brief placeholder
12. **License** — MIT placeholder

---

## TASK 10 — Config & Scaffolding Files

Generate these files completely:

**10a. `.gitignore`** — for a .NET 8 + Python + VS Code solution. Must include: `bin/`, `obj/`, `*.user`, `.vs/`, `__pycache__/`, `*.pyc`, `.env`, `*.db`, `node_modules/`, `/data/uploads/`, `wwwroot/dist/`, `*.log`.

**10b. `docker-compose.yml`** — orchestrates all 3 services:
- `trainwise-api` (ASP.NET Core, port 5000)
- `trainwise-ml` (FastAPI/uvicorn, port 8000)
- `trainwise-db` (SQL Server 2022, port 1433)
- Include environment variables, volume mounts, and `depends_on` links.

**10c. `TrainWise.sln`** — Visual Studio solution file referencing `TrainWise.API` and `TrainWise.Web` with correct project GUIDs.

**10d. `appsettings.json`** for `TrainWise.API` — include `ConnectionStrings:DefaultConnection`, `MLService:BaseUrl`, `Auth:SessionTimeoutHours`, `Upload:MaxFileSizeMb`, `Upload:StoragePath`.

---

## FORMATTING RULES

- Use `##` headers for each task number so I can navigate with the outline panel
- All code in fenced blocks with language tags: `sql`, `csharp`, `python`, `json`, `bash`, `yaml`, `text`
- All requested tables must be actual markdown tables — not bullet lists
- **Do not truncate.** Never write "// ... rest of implementation" or "etc." — always write the full output
- After all 10 tasks, add a **"Start Here"** section listing the first 5 files to create, in order, with one sentence explaining why each comes first

---

## PROMPT END
