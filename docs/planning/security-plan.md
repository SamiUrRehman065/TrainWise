# TrainWise Security Remediation Plan

_Date: May 13, 2026_

## Implemented in this update
1. **CORS tightened**
   - Replaced wildcard origin + credentials with `Cors:AllowedOrigins` configuration.
   - Prevents permissive credentialed cross-origin requests.

2. **HTTPS redirection enabled**
   - Added `UseHttpsRedirection()` in API startup pipeline.

3. **Removed hardcoded frontend API URL**
   - Blazor now reads `ApiBaseUrl` from `wwwroot/appsettings.json`.

4. **Reduced browser token persistence risk**
   - Session token storage moved from `localStorage` to `sessionStorage`.

5. **Archive consistency fixed**
   - Archive operation now removes dataset rows from DB after storage deletion.

6. **Repo hygiene**
   - Added `.venv` patterns to `.gitignore`.

## Next priority actions (recommended)
1. Move SQL connection string secret to environment variables / secret manager for all environments.
2. Replace in-memory session store with Redis or SQL-backed distributed cache.
3. Protect ML service routes behind API-only network policy (private network binding / reverse proxy).
4. Eliminate raw `filePath` trust boundary by passing dataset IDs only, resolved server-side.
5. Refactor dataset upload path to go through `IDatasetStorageService` to avoid dual storage behavior.
6. Replace seed `demo` password with BCrypt hash and force password change on first login.
7. Add CI workflow for lint + build + tests, and expand API integration tests.
