# Sprint Plan

| Sprint | Weeks | Goal | Deliverables | FR/NFR Covered | Definition of Done |
|---|---|---|---|---|---|
| Sprint 1 | Weeks 1-2 | Foundation and data ingestion | Repo setup, DB schema, login flow, dataset upload + analysis trigger | FR-1, FR-2, NFR-7, NFR-8, NFR-4 | User can log in and upload a dataset, analysis summary returns |
| Sprint 2 | Weeks 3-4 | Training pipeline | Preprocessing config UI, model selection, training API + ML training pipeline | FR-3, FR-4, FR-5, NFR-1, NFR-2 | Train request returns metrics and saves experiment |
| Sprint 3 | Weeks 5-6 | Visualization and recommendations | Plotly charts, recommendation engine, retrain flow | FR-6, FR-7, FR-8, NFR-3, NFR-10 | User can retrain and see charts and recommendations |
| Sprint 4 | Weeks 7-8 | History, reliability, deployment | Experiment history, error handling, docker compose, docs | FR-9, NFR-5, NFR-6, NFR-9 | History works, services deploy cleanly, docs complete |

## Risks and Mitigations

| Sprint | Risk (Section 9) | Mitigation |
|---|---|---|
| Sprint 1 | R-05 SQL Server setup issues | Provide Docker Compose alternative and setup guide |
| Sprint 1 | R-06 scikit-learn version mismatch | Pin versions in requirements.txt and document venv |
| Sprint 2 | R-02 API to ML latency on large datasets | Add async call handling and timeout guard |
| Sprint 2 | R-07 large datasets causing memory overflow | Enforce upload row limits and return 413 |
| Sprint 3 | R-01 Blazor and Plotly interop bugs | Prototype charts early and add fallback table view |
| Sprint 3 | R-04 SMOTE not applied but recommended | Add clear UI banner and toggle near preprocessing |
| Sprint 4 | R-03 limited auth exposure | Document known limitation and scope to local network |
| Sprint 4 | R-02 service latency under load | Add health check and retry/backoff in API |
