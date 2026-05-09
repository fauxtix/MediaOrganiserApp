# MediaOrganiserApp – Technical Debt Issue Drafts

## 1. Audit Web API authentication and authorization pipeline
**Priority:** Critical

**Description:**  
Review the ASP.NET Core middleware pipeline in `MediaOrganizerApp.WebApi/Program.cs` to confirm authentication is configured correctly. The current setup visibly includes `UseAuthorization()` and a custom `ApiKeyMiddleware`, but `UseAuthentication()` is not present in the startup pipeline excerpt reviewed. Because the application includes user management, roles, and admin-only capabilities, this should be verified immediately.

**Why this matters:**  
If authentication is incomplete or misordered, protected endpoints may not enforce identity and role checks as intended.

**Suggested outcome:**  
- Confirm intended auth model
- Add `UseAuthentication()` if required
- Validate `[Authorize]` behavior across controllers
- Add integration tests for admin/user-protected endpoints

---

## 2. Refactor oversized backend controllers
**Priority:** High

**Description:**  
Split large controllers such as `MediaController.cs`, `PublicationsController.cs`, and `AiController.cs` into smaller, purpose-specific controllers or handler-backed endpoints. These controllers appear to own too many responsibilities, making them harder to test, maintain, and safely modify.

**Why this matters:**  
Large controllers increase regression risk and blur boundaries between transport concerns and business logic.

**Suggested outcome:**  
- Keep controllers thin
- Move orchestration into application services
- Group endpoints by workflow/subdomain
- Improve unit and integration testability

---

## 3. Refactor oversized Blazor API wrapper services
**Priority:** High

**Description:**  
Review and split large UI-facing wrapper classes such as `PublicacaoService.cs` and `MediaServiceWrapper.cs`. These appear to aggregate multiple workflows into broad client services, which can make frontend maintenance more difficult.

**Why this matters:**  
Large wrapper services tend to create unnecessary coupling between pages and a wide API surface.

**Suggested outcome:**  
- Split wrappers by workflow
- Introduce narrower interfaces
- Separate list/search, CRUD, maintenance, and enrichment operations

---

## 4. Restrict production CORS configuration
**Priority:** High

**Description:**  
Replace the permissive `AllowAnyOrigin/AllowAnyMethod/AllowAnyHeader` policy with environment-specific CORS configuration. The current setup is convenient for development but too broad for production use.

**Why this matters:**  
Overly permissive CORS increases exposure to unintended client access patterns.

**Suggested outcome:**  
- Restrict allowed origins in production
- Keep permissive behavior only for development if necessary
- Document allowed frontend origins in configuration

---

## 5. Clarify architecture boundaries and remove structural ambiguity
**Priority:** Medium

**Description:**  
Document and simplify the repository’s active architectural structure. The codebase currently appears to contain both generic layered folders (`Application`, `Domain`, `Infrastructure`) and explicit solution projects (`MediaOrganizerApp.Application`, `MediaOrganizerApp.Persistence`, etc.), which may indicate an unfinished refactor or overlapping patterns.

**Why this matters:**  
Contributors may be unsure where new code should live or which layer is authoritative.

**Suggested outcome:**  
- Document intended architecture
- Mark legacy/transitional folders clearly
- Remove or archive unused code paths where possible

---

## 6. Standardize naming conventions across the codebase
**Priority:** Medium

**Description:**  
Define and apply naming conventions consistently across repositories, projects, folders, and code identifiers. The repo currently mixes:
- `MediaOrganiser` and `MediaOrganizer`
- Portuguese domain names such as `Publicacao`, `Autores`, `Editoras`
- English names such as `Media`, `Users`, `Dashboard`

**Why this matters:**  
Naming inconsistency increases cognitive overhead and makes discovery/refactoring harder.

**Suggested outcome:**  
- Define a naming policy
- Apply it to new code first
- Plan incremental refactors for heavily used areas

---

## 7. Centralize external metadata enrichment strategy
**Priority:** Medium

**Description:**  
Unify the logic that coordinates external metadata providers such as Google Books, Wikidata, Wikipedia, Last.fm, music metadata providers, and AI fallback services.

**Why this matters:**  
Without a common orchestration strategy, integrations can diverge in retry behavior, error handling, normalization, and fallback logic.

**Suggested outcome:**  
- Create a shared enrichment pipeline abstraction
- Define provider priority and fallback rules
- Normalize all external data into internal DTOs
- Centralize retry, timeout, and logging behavior

---

## 8. Rationalize AI feature boundaries
**Priority:** Medium

**Description:**  
Review and simplify how AI capabilities are split across `AiController`, `ConversationsController`, prompt templates, and specialized AI services. The current design is powerful, but likely harder to reason about than necessary.

**Why this matters:**  
Overlapping AI responsibilities can lead to duplicated logic and inconsistent behavior between chat and one-off enrichment flows.

**Suggested outcome:**  
- Separate chat, translation, author enrichment, book enrichment, and media enrichment clearly
- Document ownership of each AI workflow
- Centralize shared AI execution policies

---

## 9. Improve configuration governance and startup validation
**Priority:** Medium

**Description:**  
Create a clearer configuration strategy for required settings such as SQL connection strings, Gmail/SMTP values, AI provider settings, feature flags, and external API credentials.

**Why this matters:**  
As configuration surface area grows, environment drift and missing values become more likely.

**Suggested outcome:**  
- Add startup validation for required config
- Publish a configuration matrix
- Provide example settings templates
- Distinguish required from optional settings

---

## 10. Refresh README and architecture documentation
**Priority:** Medium

**Description:**  
Update repository documentation so it accurately reflects the current project structure, runtime model, setup instructions, and feature set. Existing docs appear to contain some inconsistency and duplication.

**Why this matters:**  
Documentation drift slows onboarding and increases setup errors.

**Suggested outcome:**  
- Remove duplicate sections
- Confirm actual frontend hosting model
- Add architecture overview and project map
- Document key feature modules and integrations

---

## 11. Normalize SignalR hub semantics
**Priority:** Medium-Low

**Description:**  
Review SignalR hub naming and routing to ensure each hub clearly represents its purpose. The current mapping suggests at least one possible semantic mismatch, with `/chat-progress` mapped to `GoogleBooksProgressHub`.

**Why this matters:**  
Naming drift makes real-time flows harder to understand and maintain.

**Suggested outcome:**  
- Verify whether hub reuse is intentional
- Rename or split hubs where appropriate
- Document event contracts for each hub

---

## 12. Add architectural and integration safety checks
**Priority:** Medium-Low

**Description:**  
Strengthen automated verification around architecture boundaries, configuration, and protected endpoint behavior.

**Why this matters:**  
As the application grows, manual enforcement of architectural intent becomes unreliable.

**Suggested outcome:**  
- Add integration tests for auth-protected routes
- Add startup validation tests
- Add architecture/boundary checks where practical
- Add smoke tests for major publication/media/AI flows
