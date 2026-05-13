# TrainWise - Future Enhancements & Roadmap

## Planned Features (Post-MVP)

This document outlines planned features and enhancements for TrainWise. These are prioritized based on user impact and technical feasibility.

---

## **1. Experiment Versioning**
**Category:** ML Management  
**Priority:** High  
**Effort:** Medium

### Description
Allow users to maintain multiple versions of model trainings for the same dataset. Enable side-by-side comparison of results across versions.

### Implementation
- Add `ExperimentVersion` table to track version history
- UI to display version timeline
- Rollback functionality to restore previous experiment configuration
- Version comparison view (metrics, hyperparameters)

### User Benefit
- Track model evolution over time
- Understand what changes improved/degraded performance
- Easily revert to previous best model

---

## **2. Experiment Tags/Notes**
**Category:** Experiment Organization  
**Priority:** High  
**Effort:** Low

### Description
Add custom labels and notes to experiments for better organization and documentation.

### Implementation
- Add `Tags` and `Notes` fields to Experiment model
- Tag management UI (create, filter by tags)
- Search/filter experiments by tag or note
- Tagging suggestions based on model name and metrics

### User Benefit
- Better experiment organization ("production", "test", "best_v2", etc.)
- Self-documenting analysis (notes on why experiment was run)
- Quick identification of important experiments

---

## **3. Dataset Versioning**
**Category:** Data Management  
**Priority:** Medium  
**Effort:** High

### Description
Track dataset changes, maintain version history, and enable rollback to previous versions.

### Implementation
- Add `DatasetVersion` table to track changes
- Git-like versioning system with snapshots
- Diff view to see what changed between versions
- Clone/branch datasets for experimentation
- Changelog UI showing modifications

### User Benefit
- Audit trail of dataset changes
- Compare model performance across data versions
- Easily test impact of data cleaning/preprocessing

---

## **4. Alerts on Training**
**Category:** Notifications  
**Priority:** Medium  
**Effort:** Low

### Description
Notify users when training completes, fails, or shows unusual metrics.

### Implementation
- Add in-app notification center
- Email alerts (optional)
- Webhooks for external integrations
- Alert rules (timeout, low accuracy, high loss, etc.)
- Notification preferences per user

### User Benefit
- Don't need to manually check if training finished
- Get alerted to problems early
- Better productivity (start another task while training runs)

---

## **5. Export Results**
**Category:** Reporting  
**Priority:** Medium  
**Effort:** Low

### Description
Download experiment metrics, configs, and results in multiple formats.

### Implementation
- Export to CSV (metrics + hyperparameters)
- Export to PDF (visual report with charts)
- Export to JSON (full experiment data)
- Batch export multiple experiments
- Email report delivery

### User Benefit
- Share results with stakeholders
- Integrate with external reporting tools
- Archive results for compliance

---

## **6. Compare Experiments**
**Category:** Analysis  
**Priority:** High  
**Effort:** Medium

### Description
Side-by-side view of different model trainings with metric comparison and visualization.

### Implementation
- Multi-select experiments view
- Comparison table (metrics, hyperparameters, preprocessing)
- Overlay charts (accuracy, loss, F1 score, etc.)
- Highlight best/worst performers
- Statistical significance test (if possible)

### User Benefit
- Quickly identify best model configuration
- Understand impact of hyperparameter changes
- Improve model iteratively with data-driven decisions

---

## **7. Batch Preprocessing**
**Category:** Data Processing  
**Priority:** Medium  
**Effort:** High

### Description
Apply preprocessing to multiple datasets simultaneously or queue preprocessing jobs.

### Implementation
- Batch upload interface
- Queue system for preprocessing jobs
- Parallel execution (within resource limits)
- Progress tracking dashboard
- Batch result export

### User Benefit
- Save time on repetitive preprocessing
- Handle large dataset collections
- Better resource utilization

---

## **8. Custom Hyperparameters**
**Category:** ML Tuning  
**Priority:** High  
**Effort:** Medium

### Description
Allow users to define and tune custom hyperparameters per model training experiment.

### Implementation
- Add hyperparameter definition UI
- Support for different parameter types (numeric, categorical, boolean)
- Parameter validation and constraints
- Grid search / random search for hyperparameter tuning
- Best hyperparameters recommendation

### User Benefit
- Fine-tune models for specific use cases
- Explore parameter impact systematically
- Reach higher model accuracy with optimal settings

---

## **9. Model Performance Benchmarks**
**Category:** Analytics  
**Priority:** Medium  
**Effort:** Medium

### Description
Track best models, compare against baselines, and maintain a leaderboard of top performers.

### Implementation
- Leaderboard UI (top models by metric)
- Baseline establishment and tracking
- Performance timeline charts
- Model registry with metadata
- Benchmark comparison to public datasets

### User Benefit
- Quick overview of best model performance
- Understand performance trends
- Benchmark against standards and previous runs
- Share best models across team

---

## Implementation Timeline

**Sprint 1-2 (Priority: High Impact, Low Effort)**
- Experiment Tags/Notes (#2)
- Alerts on Training (#4)
- Export Results (#5)

**Sprint 3-4 (Priority: High Impact, Medium Effort)**
- Experiment Versioning (#1)
- Compare Experiments (#6)
- Custom Hyperparameters (#8)

**Sprint 5-6 (Nice to Have, Higher Effort)**
- Dataset Versioning (#3)
- Batch Preprocessing (#7)
- Model Performance Benchmarks (#9)

---

## NOT Planned (For Future Consideration)
- **Quota Management (#10)** — User/team storage & experiment limits
  - Defer until multi-tenant enterprise features are prioritized
  - Consider when SaaS deployment is planned

---

## Tech Debt & Improvements
- [ ] Replace plaintext auth with OAuth/JWT
- [ ] Implement EF Core migrations properly (currently manual SQL)
- [ ] Add comprehensive API tests
- [ ] Improve ML service error handling
- [ ] Add request logging/tracing
- [ ] Implement proper soft-delete for datasets/experiments
- [ ] Add database indexing for performance
- [ ] Implement pagination for large result sets

