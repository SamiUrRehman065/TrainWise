# TrainWise Full Stack Testing Guide

## ✅ Frontend UI Implementation - COMPLETE

All 12 major features and 15+ pages/components have been created with modern, secure design.

### What Was Built

**Authentication Pages:**
- ✅ Login page with demo credentials hint and keyboard Enter support
- ✅ Signup page with full validation (username/password/confirmation)
- ✅ TopNav component with user menu and logout

**Dashboard & Account:**
- ✅ Stats page (dashboard) showing workspace overview
- ✅ Profile page with user account info and token preview
- ✅ Settings page with 4-tab interface (General/Notifications/Security/Storage)

**Dataset Management:**
- ✅ Datasets list page with grid view, pagination, filtering by size/date
- ✅ Dataset detail page showing columns, preview, and related experiments

**Training & Experiments:**
- ✅ Training page with dataset dropdown, task selection, auto-populated target columns
- ✅ Experiment history page with card-based UI and metrics preview
- ✅ Experiment detail page with full metrics, hyperparameters, export button
- ✅ Experiment comparison page for side-by-side analysis

**Utility Components:**
- ✅ Pagination component (Previous/Next with page info)
- ✅ FilterBar component (reusable filtering)
- ✅ Breadcrumbs component (consistent navigation)
- ✅ StorageVisualization component (quota display)
- ✅ FormFeedback component (styled alerts)

**Security:**
- ✅ BaseApiClient with Bearer token injection
- ✅ 401 Unauthorized handling (redirects to login)
- ✅ SessionState centralized token management
- ✅ All pages check authentication before rendering

**Exports & Utilities:**
- ✅ export.js - File download, CSV/JSON export functions
- ✅ Copy to clipboard utility
- ✅ MainLayout with JS import

### Design System
- Font: Space Grotesk (body), Fraunces (headings)
- Colors: Teal primary (#0f766e), Amber accent (#f59e0b)
- Spacing: 8px grid, 14px border-radius
- Responsive: Grid layouts, mobile media queries
- Interactions: Hover effects, transitions, loading spinners

---

## 🚀 How to Run

### Prerequisites
- Docker & Docker Compose installed
- .NET 8 SDK installed
- Python 3.11 with scikit-learn installed
- SQL Server 2022 or LocalDB available

### Step 1: Start All Services

```bash
cd "c:\Users\samiu\OneDrive - Higher Education Commission\Documents\TrainWise"
docker compose up -d
```

This starts:
- **API** (ASP.NET Core 8) on http://localhost:5000
- **Web** (Blazor WASM) on http://localhost:5002
- **ML Service** (FastAPI) on http://localhost:8000
- **Database** (SQL Server 2022) on localhost:1433

### Step 2: Verify Services are Running

```bash
# Check container status
docker compose ps

# Expected output:
# Container        Status
# trainwise-api    Up
# trainwise-web    Up
# trainwise-ml     Up
# trainwise-db     Up
```

### Step 3: Access the Application

Open browser and navigate to: **http://localhost:5002**

You should see:
- Modern landing page with "Sign In" and "Create Account" buttons
- Feature cards (Smart Datasets, ML Training, Experiment Tracking)
- No console errors in browser DevTools

---

## 📝 Testing Workflow

### Test 1: Authentication Flow
1. Click "Sign In" → `/login` page loads
2. See "Demo: admin / demo_pass" hint box
3. Enter username: `admin`, password: `demo_pass`
4. Click Sign In or press Enter
5. Should redirect to `/datasets` (dashboard)
6. See user avatar in top nav with dropdown menu

✅ **Expected**: Successful login, session token stored, user menu appears

### Test 2: Navigation
1. In top nav, verify these links work:
   - 📊 Datasets → `/datasets`
   - 📈 Training → `/training`
   - 🤖 History → `/experiments`
   - 📉 Profile → `/user/profile`
   - Settings icon → `/user/settings`
2. Breadcrumbs appear on detail pages
3. Click logo to go home

✅ **Expected**: All pages load without errors, navigation is smooth

### Test 3: Dataset Management
1. Go to `/datasets`
2. See "Upload Dataset" button
3. Mock datasets appear (from API GET /api/dataset)
4. Click "View" on any dataset → `/datasets/{id}` loads
5. See columns, preview, related experiments
6. Click "Train" → goes to `/training` with dataset pre-selected

✅ **Expected**: Dataset list loads, detail page shows all info, links work

### Test 4: Training
1. Go to `/training`
2. Select a dataset from dropdown (loads from GET /api/dataset)
3. Dataset preview card shows rows/columns
4. Select Task Type → models filter by type
5. Select Target Column → auto-populated from dataset summary
6. Fill in other fields (leave as defaults)
7. Click "Start Training"
8. Show success card with experiment ID

✅ **Expected**: Training page loads all data, POST /api/train succeeds, success message appears

### Test 5: Experiment History
1. Go to `/experiments`
2. See grid of experiment cards with:
   - Model name
   - Task type tag
   - Creation date
   - Metrics preview (accuracy, F1, duration)
3. Pagination at bottom (if >12 experiments)
4. Click "Details" on any experiment → `/experiments/{id}` loads

✅ **Expected**: History shows all experiments, pagination works, detail page loads

### Test 6: Experiment Details
1. On `/experiments/{id}`, see:
   - Configuration section (model, task type, dataset, created date)
   - Performance metrics (accuracy %, F1, duration, status badge)
   - Hyperparameters table with all model settings
   - Export button
2. Click "Export" → download JSON file with experiment results
3. Click "Compare" → goes to `/experiments/compare`

✅ **Expected**: All details load, export function works, JSON file downloads

### Test 7: Experiment Comparison
1. On `/experiments/compare`
2. Select two experiments from dropdowns
3. See comparison table with metrics
4. Positive/negative highlighting shows metric differences
5. Export comparison button works

✅ **Expected**: Comparison loads, highlighting works, export succeeds

### Test 8: Profile & Settings
1. Click user avatar in top nav
2. Select "Profile" → `/user/profile` shows:
   - Avatar with first letter
   - Username
   - Token preview (first 20 chars)
   - Workspace statistics (via GET /api/dataset/manage/stats)
3. Select "Settings" → `/user/settings` shows:
   - 4 tabs (General, Notifications, Security, Storage)
   - Preference controls
   - Danger zone for destructive actions

✅ **Expected**: Profile loads user data, Settings tabs work, no console errors

### Test 9: Security & Token Management
1. Open Browser DevTools → Network tab
2. On any page, check network requests:
   - All requests to `/api/*` have `Authorization: Bearer <token>` header
   - Token should be in cookies or localStorage
3. Go to Settings and look for option to "Log Out"
4. Click Logout → session cleared, redirect to `/login`

✅ **Expected**: Bearer tokens present in all API calls, logout works

### Test 10: Error Handling
1. Open Network tab in DevTools
2. Go to a page that makes API calls
3. Right-click any `api/` request → Block URL
4. Refresh page
5. See error message displayed (if from FormFeedback component)
6. Browser console should show errors being logged
7. Check that 401 responses trigger logout and redirect to `/login`

✅ **Expected**: Graceful error handling, no crash, user is informed

---

## 🔗 API Endpoints Validation

All 12 backend endpoints should be accessible through the frontend:

| Method | Endpoint | Frontend Page | Status |
|--------|----------|---------------|--------|
| POST | /api/auth/login | Login.razor | Exposed ✅ |
| POST | /api/auth/signup | Signup.razor | Exposed ✅ |
| GET | /api/dataset | Train.razor, Datasets.razor | Exposed ✅ |
| GET | /api/dataset/{id}/summary | Train.razor | Exposed ✅ |
| POST | /api/dataset/upload | Upload.razor | Exposed ✅ |
| DELETE | /api/dataset/{id} | Datasets.razor | Exposed ✅ |
| GET | /api/dataset/manage/stats | Stats.razor, Profile.razor | Exposed ✅ |
| POST | /api/train | Train.razor | Exposed ✅ |
| GET | /api/experiments/{id} | ExperimentDetail.razor | Exposed ✅ |
| GET | /api/experiments/history | History.razor | Exposed ✅ |
| GET | /api/experiments/compare | CompareExperiments.razor | Exposed ✅ |
| DELETE | /api/experiments/{id} | History.razor (via button) | Exposed ✅ |

---

## 📊 File Structure Verification

### Pages Created/Updated
```
Pages/
├── Auth/
│   ├── Login.razor ✅
│   └── Signup.razor ✅
├── Datasets/
│   ├── Datasets.razor ✅
│   ├── DatasetDetail.razor ✅
│   └── (existing: Summary.razor, Upload.razor)
├── Experiments/
│   ├── History.razor ✅ (updated with breadcrumbs)
│   ├── ExperimentDetail.razor ✅
│   └── CompareExperiments.razor ✅
├── User/
│   ├── Profile.razor ✅
│   └── Settings.razor ✅
├── Training/
│   └── Train.razor ✅ (enhanced with dropdowns)
├── Stats.razor ✅
└── Index.razor ✅ (modern hero layout)
```

### Components Created
```
Components/
├── Navigation/
│   └── TopNav.razor ✅
├── Common/
│   ├── Pagination.razor ✅
│   ├── FilterBar.razor ✅
│   ├── Breadcrumbs.razor ✅
│   ├── StorageVisualization.razor ✅
│   └── FormFeedback.razor ✅
└── Layout/
    └── MainLayout.razor ✅ (updated)
```

### Services
```
Services/
├── Api/
│   ├── BaseApiClient.cs ✅
│   ├── AuthApi.cs
│   ├── DatasetApi.cs
│   ├── TrainingApi.cs
│   ├── ExperimentApi.cs
│   └── ApiClient.cs (updated with header logic)
└── State/
    └── SessionState.cs
```

### JS Utilities
```
wwwroot/
└── js/
    └── export.js ✅
```

---

## 🐛 Troubleshooting

### Issue: Page shows "Please sign in"
**Solution**: Session token may be missing. Try logout and re-login at `/login`

### Issue: API calls get 401 Unauthorized
**Solution**: Check that Bearer token is being sent in Authorization header. See Network tab in DevTools.

### Issue: Dataset dropdown is empty
**Solution**: Ensure SQL Server is running and has dataset records. Check: `docker compose logs api`

### Issue: Training doesn't complete
**Solution**: Check ML Service is running: `docker compose logs ml-service`
Also verify database is receiving experiment results.

### Issue: Export button doesn't download file
**Solution**: Ensure `export.js` is loaded. Check Network tab for any 404 errors on `/js/export.js`

### Issue: Mobile layout broken
**Solution**: Browser window may be too small. Check CSS media queries in component files.

---

## 📈 Success Criteria

All of the following should be ✅ true before deployment:

- [ ] Page loads at http://localhost:5002 without console errors
- [ ] Login flow works (admin/demo_pass)
- [ ] All pages render with modern design (no broken layouts)
- [ ] All API calls have Bearer token in Authorization header
- [ ] 401 responses redirect to login
- [ ] Dataset dropdown shows data from GET /api/dataset
- [ ] Training POST request succeeds
- [ ] Experiment history shows results
- [ ] Export button downloads JSON file
- [ ] Pagination works (if >12 items per page)
- [ ] Breadcrumbs appear on detail pages
- [ ] User avatar and menu work
- [ ] Settings tabs are functional
- [ ] No unhandled Promise rejections in console
- [ ] Mobile layout is responsive (test at 375px width)

---

## 🎯 Next Steps After Testing

1. **If all tests pass**: Application is ready for deployment
2. **If some tests fail**:
   - Check browser console for error messages
   - Check `docker compose logs` for backend errors
   - Verify SQL Server has test data
   - Ensure all required packages are installed

3. **Performance optimization** (optional):
   - Minify CSS/JS
   - Implement lazy loading for large grids
   - Add caching headers
   - Profile with Chrome DevTools

4. **Security hardening** (optional):
   - Move demo credentials to environment variables
   - Add HTTPS/TLS
   - Implement CSRF protection
   - Add rate limiting on auth endpoints

---

**Session 5 Complete**: All frontend UI pages with modern design, security, and proper API integration ready for end-to-end testing.
