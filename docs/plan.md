# Plan: ERP Ekspor Rotan — Architecture FINALIZED + Phase 1 (Identity + Settings Modules)

> Architecture below (Modular Monolith, folder-per-module in a single project, `Identity` split from `Settings`) **supersedes** all earlier drafts. Locked in before implementation starts, per user request. Implementation will start with `User` (vertical proof-of-concept: Entity→Repository→Service→UseCase→Controller→test), then replicate the same pattern to Role/Permission/etc.

## Module Documentation Index
This file holds cross-cutting architecture: conventions, the module roadmap, and founding design decisions ("why"). Session-by-session implementation history for each module ("what was built, in what order, with what caveats") has been moved into per-module files under `docs/modules/` to keep this file from growing unbounded:
- [modules/identity.md](modules/identity.md) — Users, Roles, Permissions, join tables, JWT auth, lockout.
- [modules/settings.md](modules/settings.md) — Company Profile, Currencies, UoM, Document Numbering, Approval Matrix, Modules/Menus (System Administration group), Audit Log.
- [modules/master-data.md](modules/master-data.md) — Business Partners, Items, Bill of Materials, Price Lists.
- [modules/ui.md](modules/ui.md) — `Sugentra.ERP.UI` shared shell (layout, sidebar, dashboard/module hub, branding, login page).

**Convention going forward:** once a module's own session log would exceed roughly 100-150 lines here, give it a `docs/modules/<name>.md` file and only leave a short pointer in this file.

## UI List/Form Standard (mandatory for every new module page with a table)
Established from the Email Settings/Templates/History pages — apply to every new (and when touched, existing) list+form view pair:
1. Breadcrumbs via `~/Views/Shared/_Breadcrumb.cshtml` on every Index/Create/Edit/Detail view; the breadcrumb's first icon = the owning module's icon (not a generic one).
2. Index page's `<div class="card">` is full-width (no `col-md-*` wrapper) — only Create/Edit/Detail forms are constrained to `col-md-6`. Card header uses `d-flex justify-content-between align-items-center` with the title (`<h5 class="mb-0">`) and the add-data button on the same row.
3. Table uses `table table-sm table-hover`; fonts/row height/header follow this existing sizing — don't introduce new table classes/sizes.
4. Add a keyword-search filter row (`_ShowEntries` partial + `<form method="get">` with a single `keyword` input) whenever the table has filterable columns — filter should match every visible column, not just one.
5. Every table has exactly two fixed structural columns: `No` at the far left and `Actions` at the far right. Both get an explicit `style="width: ...px;"` (minimum enough to fit their content, e.g. `60px` for No, `110px` for a 2-icon Actions) and are center-aligned in both header and body (`class="text-center"` on `<th>`/`<td>`, row content centered via `justify-content-center`). Delete icon buttons must use the same Remix icon (`ri-delete-bin-line`) consistently across every table — never mix icons — and every delete action must show a confirm step (the standard `#delete{Entity}Modal` Yes/No modal, not a bare `onsubmit="confirm(...)"`).
6. Pagination via `Models.PagedResult<T>` + `_ShowEntries`/`_PagingFooter` partials, populated in-memory in the controller (`Skip`/`Take`), with `No` numbered per page (continuing across pages).
7. Create/Edit/Detail forms use the vertical small-form layout: `<div class="row mb-3"><label class="col-sm-3 col-form-label col-form-label-sm"></label><div class="col-sm-9"><input class="form-control form-control-sm"/></div></div>`, not `row g-4`/full-label-above layout.
8. Save/Cancel buttons sit bottom-right of the form (`<div class="mt-4 d-flex justify-content-end gap-2">`, Cancel then Save, both `btn-sm`) — never left-aligned/full-width. If a page has no Cancel action (e.g. a read-only Detail view), show Refresh + Back buttons instead, positioned top-right, aligned with the breadcrumb row (not bottom).
9. Any badge (status or otherwise) uses `bg-primary` as its "on/positive" color (e.g. Active = `bg-primary rounded-pill`) — only fall back to a different semantic color (success/danger/etc.) when the states aren't a simple boolean/positive-negative pair (e.g. Sent/Failed).
10. **Button naming/color standard (mandatory, latest revision):**
    - The "add new record" button on every Index page must be labeled **"Add Data"** (not "Add Currency"/"Add Item"/etc.) and use `btn btn-sm btn-primary`.
    - **Primary** (`btn-primary`): Add Data, Save, Submit, Filter.
    - **Danger** (`btn-danger`): Delete only — always paired with a uniform delete icon (`ri-delete-bin-line`) and a confirm dialog.
    - **`btn-label-dark waves-effect`**: Download, Export, Reset, Back, Refresh, Cancel.
    - Filter, Reset/Clear, Save, Submit, and Cancel buttons ALL carry the shared `.btn-filter-action` CSS class (min-width 70px, defined in `wwwroot/css/site.css`) so every action button in a filter row or a form footer is the same width — e.g. Filter = `btn btn-sm btn-primary btn-filter-action`, Reset = `btn btn-sm btn-label-dark waves-effect btn-filter-action`, Save = `btn btn-primary btn-sm btn-filter-action`, Cancel = `btn btn-label-dark waves-effect btn-sm btn-filter-action`. (Bootstrap modal dismiss "Cancel" buttons on confirm/create/edit modals are exempt — those stay as the modal's default secondary style since they're a close action, not a form Save/Cancel pair.)
    - Pagination's active page number (`.pagination .page-item.active .page-link`) uses the same dark color as `.btn-label-dark`/Cancel (`var(--bs-dark)` background), not Bootstrap's default primary — overridden globally in `wwwroot/css/site.css`.
11. **Responsive multi-field filter row standard** (established on Error Logs, then replicated to Audit Log): whenever a filter row has more than a simple single keyword input (selects/date-range/etc.), structure it as:
    - Split the filter row's fields (selects/search/date inputs) into their own `<form method="get" id="{page}FilterForm">`, separate from the `_ShowEntries` partial's own `<form>` — never nest a `<form>` inside another `<form>` (invalid HTML; browsers silently break it).
    - Fields row: `<div class="d-flex flex-wrap align-items-center gap-2 justify-content-end w-100 mb-2">` wrapping each field with a shared `filter-field` class plus a field-specific class (e.g. `filter-source`, `filter-search`, `filter-date`). A from/to date pair is grouped in its own `<div class="d-flex gap-2 filter-field filter-date-group">` so the two stay side-by-side even when stacked.
    - Scoped `<style>` block (per view, prefixed `.{page}-filter-form`): base rule `.filter-field { width: 100%; }` (mobile: one full-width field per row, so they always stack instead of overflowing/cramping) and `.filter-date { width: 50%; }` (date pairs share a row). At `@media (min-width: 576px)` override each field class to a fixed comfortable px width (e.g. 120–220px) and reset `.filter-date-group`/`.filter-actions`-equivalent wrapper back to `width: auto` (don't forget this — leaving a wrapper div's class in the base `filter-field` rule with no override left-aligns its children awkwardly on desktop).
    - Bottom row: `<div class="d-flex flex-row justify-content-between align-items-center gap-2">` containing the `_ShowEntries` partial on the left and the Filter/Clear buttons on the right — always `flex-row` (not `flex-column`/`flex-sm-row`) so "Show entries" and the action buttons stay on the same line at every viewport width, matching mobile's expected compact toolbar.
    - Filter/Clear buttons live in this bottom row, outside the fields `<form>`; use `<button type="submit" form="{page}FilterForm">` (the HTML5 `form` attribute) so they still submit the fields form despite being physically outside it. Give the buttons' wrapper a page-specific class (e.g. `.{page}-filter-form-actions .btn { width: 80px; max-width: 80px; }`, applied at all widths, not just desktop) so they never stretch full-width or grow disproportionately.
    - For date pickers needing a nicer UX than the bare native `<input type="date">`, swap to Flatpickr (`~/vendor/libs/flatpickr/flatpickr.{css,js}`, wired via `@@section VendorStyles`/`VendorScripts`) on a `type="text"` input with a placeholder (e.g. "From date") instead of a `to` label — initialize with `appendTo: document.body` in `@@section Scripts` (critical: Flatpickr's default `appendTo` is the input's own parent, which is inside the flex filter row — without this override, opening the calendar grows that flex line and visibly shifts the whole layout on focus).
12. **Standard small-control height = 34px** (`calc(1.375em + 0.883rem + calc(1px * 2))`), enforced globally in `wwwroot/css/site.css` for every `.form-control-sm`/`.form-select-sm`/`.btn-sm`/`input.form-control`, AND for `.bootstrap-select.form-control-sm .dropdown-toggle` (bootstrap-select's generated toggle `<button>` never carries the `.form-control-sm` class itself, so it needs its own explicit `min-height` rule to match plain inputs/selects on the same row — without it the toggle renders ~1.3px short). Any new small form control (input, native select, bootstrap-select, button) placed alongside others in a row must resolve to this same 34px so rows never look visually uneven.
    - **bootstrap-select specificity gotcha**: `.bootstrap-select.form-control-sm .dropdown-toggle` is a 3-class-selector rule. A narrower override like `.my-custom-group .dropdown-toggle` (2 classes) will **lose** to it even when both sides use `!important` — `!important` only wins ties on equal specificity, it doesn't itself outrank a more specific selector. Any per-component override touching the toggle's padding/etc. must match or exceed 3 class-selectors, e.g. `.my-custom-group .bootstrap-select.form-control-sm .dropdown-toggle { ... !important; }`.


Applied so far to: Email Settings, Email Templates, Email History (partial — Detail-only, Sent/Failed badge kept as-is), Modules, Menus, Currencies, Units of Measurement, Business Partners, Items, Price Lists, Bill of Materials (Index only — Create/Edit forms intentionally left untouched due to dynamic JS-driven line-item logic). Button-naming/color revision (point 10) applied app-wide: Add Data label + Reset/Clear/Cancel → `btn-label-dark waves-effect` + `.btn-filter-action` on Filter/Reset/Clear/Save/Submit/Cancel + dark pagination active color, across AuditLogs, Permissions, Roles, Roles/Permissions, Users (incl. Detail partials), EmailSettings, EmailTemplates, EmailHistory, Modules, Menus, Currencies, Units of Measurement, Business Partners, Items, Price Lists, Bill of Materials. Responsive multi-field filter row standard (point 11) applied to: Error Logs (`Views/ErrorLogs/Index.cshtml`, Flatpickr date pickers), Audit Log (`Views/AuditLogs/Index.cshtml`, native date inputs), and — with a new keyword + Status (Active/Inactive) filter combo — Items, Business Partners, Price Lists, Bill of Materials, Currencies, Units of Measurement (all 6 Master Data/Settings list pages now filter server-side on `isActive` in addition to `keyword`) — still to be retrofitted onto any other multi-field filter pages as they're touched.

## Context / Decisions (cumulative, latest wins)
- Frontend: NOT built yet — API-only for now.
- Database: SQL Server instance ALREADY provisioned (VPS/Cloud). Connection string supplied later via `appsettings.Development.json`/user secrets (not committed).
- Data access: Dapper (no EF Core). Schema is Database-First via versioned raw SQL scripts.
- Migrations: DbUp (NuGet: dbup-sqlserver) executed by a separate console project, tracks applied scripts in journal table.
- Solution/root namespace: `Sugentra.ERP`.
- Primary keys: BIGINT IDENTITY (not GUID).
- Password hashing: BCrypt.Net-Next. Validation: FluentValidation. No MediatR/CQRS.
- Standard audit/soft-delete columns on every table: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `IsDeleted`, `DeletedAt`, `DeletedBy`.
- Business model: Trading + Production (rotan mentah diproses jadi barang jadi). Finance/Accounting built in-house. HR & Payroll included in roadmap.
- Added table beyond original list: `Identity_UserRefreshTokens` (needed for JWT refresh flow) — still pending final user confirmation, default to include it.
- **Module naming: English** (Inventory, Finance, Procurement...) for folders/namespaces/code. Indonesian only for UI labels later.
- **Module isolation: folders within a single project** (NOT separate .csproj per module) — simplicity over compiler-enforced isolation. Guardrail: `NetArchTest.Rules` architecture test instead of the compiler.
- **Layer folder naming standardized to internationally recognized terms** (see table below) — `Features`→`Controllers`, `Managers`→`UseCases`, `Models`→split into top-level `Entities/` + `Dtos/`.
- **`Identity` module split from `Settings`** (decided): Users, Roles, Permissions, UserRoles, RolePermissions, UserPermissions, UserRefreshTokens, Auth (login/refresh/logout) move to their own `Modules/Identity/` with DB prefix `Identity_*`. `Settings` keeps pure configuration/reference data only: CompanyProfile, Warehouses, Currencies, UoM, Incoterms, Ports, DocumentNumberings, ApprovalMatrices — prefix stays `Setting_*`.
  - **Why split:** every other module depends on `User` (CreatedBy/UpdatedBy FK, permission checks) — that makes it a foundational module, architecturally different from "Settings" which is optional configuration only needed by specific transactional modules.
  - **Why "Identity" not "Authorization":** the module covers Authentication (login/JWT) + Authorization (Roles/Permissions) + User management — "Authorization" only describes one of the three. "Identity" is the standard industry term (ASP.NET Core Identity, Entra ID, IdentityServer, general IAM terminology).
  - UI sidebar still shows "⚙️ Settings > 1. User & Access Control" as a menu label — that's just navigation grouping, independent of the backend module name.
- `AuditLog` entity + repository physically lives in `Shared/Persistence` (not nested in a business module), since `Shared/Logging/AuditLogService` is called by EVERY module (including Identity itself) to write audit rows — `Shared` must never depend on `Modules/*`. Table stays `Setting_AuditLogs` (system-wide log, conceptually a "setting/system" concern). The read-only `AuditLogsController`/`AuditLogQuery` (for admins to browse logs) lives under `Modules/Settings/`.

## Layer Naming Standard (international convention, per module)
| Folder | Standard term / origin | Notes |
|---|---|---|
| `Controllers/` | ASP.NET Web API/MVC convention (worldwide standard) | Renamed from "Features" — this project groups by module first, not full vertical-slice-per-action, so "Controllers" is the accurate, unambiguous term. One controller per entity/area (`UsersController`, `RolesController`). |
| `UseCases/` | **Clean Architecture** (Robert C. Martin) — cross-language standard term also used in Android/iOS/Flutter Clean Architecture | Renamed from "Managers". "Manager" is avoided deliberately: widely flagged in naming-convention guides as a vague/God-class-prone suffix. One `UseCase` class per entity/aggregate with multiple methods (`UserUseCase.CreateAsync/UpdateAsync/DeactivateAsync`), not one class per action. |
| `Services/` | **Service Layer Pattern** (Martin Fowler, *Patterns of Enterprise Application Architecture*) | Unchanged — reusable domain logic shared by UseCases within the same module. |
| `Repositories/` | **Repository Pattern** (Fowler / Eric Evans DDD) | Unchanged — data access only. Interfaces co-located (`IUserRepository` next to `UserRepository`) or in a `Repositories/Interfaces/` subfolder if the list grows long. |
| `Queries/` | **CQRS** (Greg Young) | Unchanged — read-only, joined/paginated reads bypassing UseCase+Repository. |
| `Entities/` | **DDD Entity** (Eric Evans) | Split out of "Models" — DB-mapped POCOs, inherit `BaseAuditableEntity`. |
| `Dtos/` | **DTO — Data Transfer Object** (Fowler, PoEAA) | Split out of "Models" — request/response contracts + list-item projections. Universally recognized term, avoids the vague "Models" wrapper (which in ASP.NET MVC historically mixed both concerns). |

## FINALIZED Repository Structure (Modular Monolith, module-first folders)
```
SUGENTRA-ERP/
├── Sugentra.ERP.sln
├── src/
│   ├── Sugentra.ERP.Api/                       # single app project — hosts everything
│   │   ├── Modules/
│   │   │   ├── Identity/
│   │   │   │   ├── Controllers/                 # UsersController, RolesController, PermissionsController, AuthController...
│   │   │   │   ├── UseCases/                    # UserUseCase, RoleUseCase, PermissionUseCase, AuthUseCase...
│   │   │   │   ├── Services/                    # PermissionResolverService (role+override resolution, pure logic)
│   │   │   │   ├── Repositories/                # IUserRepository/UserRepository, IRoleRepository/RoleRepository, IPermissionRepository, IUserRefreshTokenRepository...
│   │   │   │   ├── Queries/                     # UserListQuery, RoleListQuery (paginated/joined reads)
│   │   │   │   ├── Entities/                    # User.cs, Role.cs, Permission.cs, UserRole.cs, RolePermission.cs, UserPermission.cs, UserRefreshToken.cs
│   │   │   │   └── Dtos/                        # CreateUserRequest, UserResponse, UserListItemDto, LoginRequest, TokenResponse...
│   │   │   ├── Settings/
│   │   │   │   ├── Controllers/                 # CompanyProfileController, WarehousesController, MasterParametersController, NumberingController, ApprovalMatrixController, AuditLogsController
│   │   │   │   ├── UseCases/                     # CompanyProfileUseCase, WarehouseUseCase, ApprovalMatrixUseCase (simple ref-data uses shared CrudUseCase<T> instead)
│   │   │   │   ├── Services/                     # DocumentNumberGeneratorService (wraps sp_Setting_DocumentNumbering_GetNext)
│   │   │   │   ├── Repositories/                 # ICompanyProfileRepository, IWarehouseRepository, ICurrencyRepository, IUnitOfMeasurementRepository, IIncotermRepository, IPortRepository, IDocumentNumberingRepository, IApprovalMatrixRepository
│   │   │   │   ├── Queries/                      # AuditLogQuery (reads Shared AuditLog table), WarehouseListQuery...
│   │   │   │   ├── Entities/                     # CompanyProfile.cs, Warehouse.cs, Currency.cs, UnitOfMeasurement.cs, Incoterm.cs, Port.cs, DocumentNumbering.cs, ApprovalMatrix.cs
│   │   │   │   └── Dtos/
│   │   │   ├── MasterData/{Controllers,UseCases,Services,Repositories,Queries,Entities,Dtos}
│   │   │   ├── Procurement/{...same shape...}
│   │   │   ├── Inventory/{...}
│   │   │   ├── Production/{...}
│   │   │   ├── Sales/{...}
│   │   │   ├── ExportDocuments/{...}
│   │   │   ├── Logistics/{...}
│   │   │   ├── QualityControl/{...}
│   │   │   ├── Finance/{...}
│   │   │   ├── HR/{...}
│   │   │   └── Reporting/{...}
│   │   ├── Shared/                               # cross-cutting, referenced by all modules
│   │   │   ├── Auth/            # JwtTokenService, PasswordHasher, PermissionPolicyProvider, PermissionAuthorizationHandler, ICurrentUserService
│   │   │   ├── Logging/         # IAuditLogService, AuditLogService, AuditLog entity + repository (Setting_AuditLogs)
│   │   │   ├── Persistence/     # IDbConnectionFactory, generic Repository<TEntity> base class
│   │   │   ├── Contracts/       # cross-module interfaces (e.g. IInventoryReservationService) — the ONLY way modules may call each other
│   │   │   ├── Common/          # BaseAuditableEntity, Result<T>, PagedResult<T>, exceptions, CrudUseCase<TEntity>
│   │   │   └── Middleware/      # global exception handling
│   │   ├── Program.cs           # calls builder.Services.AddIdentityModule(), AddSettingsModule(), app.MapIdentityEndpoints(), etc. per module
│   │   └── appsettings.json
│   └── Sugentra.ERP.Migrator/                    # separate console app (DbUp) — different runtime, must stay its own project
│       └── Scripts/
│           ├── Identity/000N_*.sql                # IDN_0001 Identity_Users ... IDN_000N Seed_SuperAdmin
│           └── Settings/000N_*.sql                # SET_0001 Setting_CompanyProfile ... SET_000N Setting_AuditLogs
└── tests/
    └── Sugentra.ERP.Tests/
        ├── Modules/<ModuleName>/                  # unit tests mirroring module folders (e.g. PermissionResolverServiceTests)
        └── Architecture/ModuleBoundaryTests.cs     # NetArchTest rule enforcing module isolation (see below)
```

## Module Internal Architecture (per-module layer responsibilities)
Two request paths per module — **Command path** (writes, has business rules) and **Query path** (reads, no business rules):

```
WRITE:  Controller (Controllers/) → UseCase (UseCases/) → IRepository (Repositories/) + Service (Services/) + Shared services (AuditLog/PasswordHasher/Jwt)
READ:   Controller (Controllers/) → Query (Queries/) → DB directly (Dapper, returns Dto, bypasses UseCase & Repository)
```

- **Controller** (`Controllers/`): HTTP concern only — routing, model binding, `[Authorize(Policy = "...")]`, calls one UseCase or Query, maps result to `ActionResult`. No business logic, no SQL.
- **UseCase** (`UseCases/`): orchestration + business rules for ONE entity/aggregate, not one class per action. E.g. `UserUseCase` has `CreateAsync`, `UpdateAsync`, `DeactivateAsync`, `AssignRolesAsync` methods. Validates via FluentValidation, enforces invariants (e.g. unique username), calls `IRepository` for persistence, calls `Services`/`Shared` for cross-cutting concerns (hashing, audit log, permission resolution), owns the transaction boundary. *Decision: one UseCase class per entity with multiple methods — not one class per action — to avoid class-explosion across 12 modules.*
- **Service** (`Services/`): reusable, mostly stateless domain logic used by one or more UseCases within the SAME module, not tied to a single entity's persistence (e.g. `PermissionResolverService`, `DocumentNumberGeneratorService` wrapping the `UPDLOCK` stored proc call). Different from `Shared/` services: module `Services/` are module-internal only; `Shared/` services are used by ALL modules.
- **IRepository / Repository** (`Repositories/`): data access ONLY — CRUD + simple filtered lookups needed by a UseCase (`GetByIdAsync`, `GetByUsernameAsync`, `ExistsByEmailAsync`). Parameterized Dapper queries, auto-filters `WHERE IsDeleted = 0`, auto-populates audit columns on write. No business logic. Interface (`IUserRepository`) lets UseCase depend on an abstraction — enables mocking in UseCase unit tests.
- **Query** (`Queries/`): read-only, often multi-table joins, pagination/filtering, projected straight into response Dtos (skips entity mapping entirely) — powers GET list/search endpoints. Called directly by Controller, no UseCase involved (a pure read has no business rule to protect).
- **Entities**: DB-mapped POCOs inheriting `BaseAuditableEntity`. **Dtos**: request/response contracts + list-item projections used by Queries.

**Reduce boilerplate across ~12 modules / 60+ entities:** define a generic `Repository<TEntity>` base class in `Shared/Persistence` implementing standard CRUD (Get by id, Add, Update, SoftDelete, GetAll) via Dapper + a `[Table("Identity_Users")]`-style attribute per entity for table name mapping. Per-entity repositories (`UserRepository : Repository<User>, IUserRepository`) only add specialized methods.

**Not every entity needs every layer** — for simple reference/master data with no business rules (Currencies, UoM, Incoterms, Ports), skip a bespoke UseCase: Controller can call the generic `Repository<T>` (via a thin shared `CrudUseCase<TEntity>` helper in `Shared/Common` that just adds audit logging around generic CRUD) directly. Reserve full bespoke UseCases for entities with real business rules (Users, Roles/Permissions, DocumentNumbering, ApprovalMatrix, Auth).

## Cross-Module Communication Pattern (keeping it clean)
**Rule: a module may NEVER reference another module's namespace directly (`Modules.Sales` must not import `Modules.Inventory.*`).** All cross-module calls go through an interface declared in `Shared/Contracts/`, implemented by the owning module, consumed via DI by the caller. This also applies to `Settings`/other modules needing `Identity` data (e.g. resolving a user's display name) — they depend on `Shared/Contracts/ICurrentUserService` or a dedicated `IUserLookupService`, never `Modules.Identity.Repositories` directly.

Example — Sales creating an Export Order needs to reserve stock in Inventory:
1. Declare `Shared/Contracts/IInventoryReservationService.cs`: `Task<bool> IsStockAvailableAsync(long itemId, decimal qty)`, `Task ReserveStockAsync(long itemId, decimal qty, string refNumber)`.
2. Implement it in `Modules/Inventory/Services/InventoryReservationService.cs : IInventoryReservationService` (uses Inventory's own repositories internally).
3. Inventory's `AddInventoryModule()` registers `services.AddScoped<IInventoryReservationService, InventoryReservationService>()`.
4. `Modules/Sales/UseCases/ExportOrderUseCase.cs` takes `IInventoryReservationService` as a constructor dependency — it only ever sees the interface type from `Shared.Contracts`, never `Modules.Inventory.*` directly.
5. `Sugentra.ERP.Tests/Architecture/ModuleBoundaryTests.cs` (NetArchTest) asserts: types in `Modules.*` must not depend on other `Modules.*` namespaces, except `Shared.*` — this fails the build if step 4 is ever done wrong.

**Exception (documented, doesn't violate the rule):** the **Reporting** module is read-only and cross-cutting by nature — it may query SQL Views spanning multiple modules' tables directly via its own `Queries/`, without ever referencing another module's C# namespace.

This keeps the codebase clean because: (a) each module's internal implementation stays private/opaque to other modules, (b) the only public surface between modules is a small, deliberate `Shared/Contracts` interface, (c) violations are caught automatically by a test instead of relying on code review discipline, (d) a module can later be extracted into its own microservice by keeping its `Contracts` interface as the API boundary.

### Module boundary rule (guardrail since there's no compiler enforcement)
- A class under `Modules/X/**` may only reference `Modules/X/**` and `Shared/**`. Never another module's namespace directly.
- Enforced automatically via a `NetArchTest.Rules` test in `Sugentra.ERP.Tests/Architecture/ModuleBoundaryTests.cs` that fails the build if any module namespace references another module namespace directly. Add this test early (Phase A) so violations are caught from day one.
- Each module exposes exactly two extension methods for composition root wiring: `AddXModule(IServiceCollection)` (Program.cs DI registration) and `MapXEndpoints(WebApplication)` (route mapping) — keeps `Program.cs` a short list of calls even at 12+ modules.

### Why this over full Clean Architecture / per-module projects
- Single project ⇒ fastest `dotnet build`/OmniSharp on M1 Air, simplest `.sln`, easiest to navigate for a small team.
- Adding a module = new `Modules/<Name>/{Controllers,UseCases,Services,Repositories,Queries,Entities,Dtos}` folder + `AddXModule()`/`MapXEndpoints()` calls in `Program.cs` + new SQL script folder in Migrator. Nothing else is touched.
- Trade-off accepted: no compiler-enforced isolation between modules — mitigated by the NetArchTest guardrail above.

## Full Module Roadmap (business model: Trading + Production, Finance in-house, HR included)
1. **Identity** — Users, Roles, Permissions, Auth (Phase 1 — detailed below)
2. **Settings** — Company Profile, Warehouses, Master Parameters, Numbering, Approval Matrix, Audit Logs (Phase 1 — detailed below)
3. **Master Data** — Business Partners (Customer LN, Supplier lokal), Items (grade rotan mentah/setengah jadi/jadi), BOM, Price Lists
4. **Procurement** — Purchase Requisition, PO ke supplier, Goods Receipt, Vendor Bills
5. **Inventory & Warehouse** — Stock raw/WIP/finished goods, Stock Mutation, Stock Opname, Batch/Grade tracking
6. **Production/Manufacturing** — Work Order, Production Planning, konsumsi BOM, Yield/Waste tracking
7. **Sales & Export Order** — Quotation, Sales Contract/Export Order, Proforma Invoice
8. **Export Documentation & Compliance** — Packing List, Commercial Invoice, B/L, Certificate of Origin, Phytosanitary, Fumigation, V-Legal/SVLK, PEB
9. **Logistics & Shipping** — Booking Container/Vessel, Shipping Instruction, Freight Forwarder, Shipment Tracking
10. **Quality Control** — Inspeksi bahan baku, QC produksi, QC pra-kirim, Certificate of Quality
11. **Finance & Accounting** — Chart of Accounts, GL, AR/AP, Cash/Bank, multi-currency, Tax (PPN/PPh), Costing/HPP
12. **HR & Payroll** — Employee Master, Attendance, Payroll, Leave
13. **Reporting & Dashboard/BI** — Cross-module report

Dependency notes: ALL modules depend on `Identity` (CreatedBy/UpdatedBy, permission checks). Procurement/Inventory/Production/Sales/Finance additionally depend on Master Data + Settings (numbering/approval). Export Documentation depends on Sales + Inventory + Logistics. Finance AR/AP triggered by Sales/Procurement transactions.

Module build order for future phases: Identity → Settings → Master Data → Inventory → Procurement → Production → Sales → Export Documentation → Logistics → Quality Control → Finance → HR → Reporting.

---

## PHASE 1 DETAIL: Identity + Settings Modules

### Permission Resolution Algorithm (critical business logic — lives in `Modules/Identity/Services/PermissionResolverService.cs`)
On login:
1. Fetch user's roles (`Identity_UserRoles`).
2. Union permissions from `Identity_RolePermissions` for those roles → base set.
3. Apply `Identity_UserPermissions` overrides: `IsAllowed = true` adds, `IsAllowed = false` (Deny) removes — **Deny always wins**, applied last.
4. Final permission codes embedded as claims inside JWT access token.
5. Api never re-queries DB for authorization — only reads claims (`Zero DB Query on Navigation`).
6. Endpoint protection via dynamic `IAuthorizationPolicyProvider` + custom `PermissionAuthorizationHandler`, so `[Authorize(Policy = "Create_Invoice")]` works without pre-registering every permission as a policy manually.

### Document Numbering (`Modules/Settings/`)
`Setting_DocumentNumberings`: DocumentType, Prefix, Suffix, NumberLength, ResetPeriod (Never/Yearly/Monthly), CurrentNumber, LastResetDate, FormatTemplate.
Next-number generation via SQL stored procedure `usp_Setting_DocumentNumbering_GetNext` using `UPDLOCK, HOLDLOCK` (concurrency-safe, not read-then-write in app code).

### Approval Matrix (`Modules/Settings/`)
`Setting_ApprovalMatrices`: DocumentType, MinAmount, MaxAmount, CurrencyId, ApprovalLevel/Sequence, ApproverRoleId. Phase 1 = metadata storage only; workflow engine execution is out of scope (future PO/Invoice modules consume this).

### Audit Logging (lives in `Shared/Logging`, table `Setting_AuditLogs`)
Explicit `IAuditLogService.LogAsync(tableName, recordId, action, oldValues, newValues, changedBy)` called from each UseCase method (in any module) after Insert/Update/Delete, writing JSON snapshots into `Setting_AuditLogs`.

### Steps

**Phase A — Solution Scaffolding**
1. `dotnet new sln -n Sugentra.ERP`; create `Sugentra.ERP.Api` (webapi, controllers), `Sugentra.ERP.Migrator` (console), `Sugentra.ERP.Tests` (xunit) under `src/`/`tests/`; add to sln. Tests project references Api.
2. Add NuGet: Dapper, Microsoft.Data.SqlClient, Microsoft.AspNetCore.Authentication.JwtBearer, Swashbuckle.AspNetCore, FluentValidation.AspNetCore, BCrypt.Net-Next (Api); dbup-sqlserver (Migrator); NetArchTest.Rules, xunit (Tests).
3. Create `Shared/Common/BaseAuditableEntity.cs` (Id, CreatedAt/By, UpdatedAt/By, IsDeleted, DeletedAt/By) and `Shared/Persistence/Repository<TEntity>` generic base.
4. Add `Sugentra.ERP.Tests/Architecture/ModuleBoundaryTests.cs` (NetArchTest rule) early so it catches violations from the first module onward.

**Phase B — Database Schema & Migrations** (*parallel with Phase A*)
5. Write versioned SQL DDL scripts in `Sugentra.ERP.Migrator/Scripts/Identity/`: Identity_Users → Identity_Roles → Identity_UserRoles → Identity_Permissions → Identity_RolePermissions → Identity_UserPermissions → Identity_UserRefreshTokens.
6. Write versioned SQL DDL scripts in `Sugentra.ERP.Migrator/Scripts/Settings/`: Setting_CompanyProfile → Setting_Warehouses → Setting_Currencies → Setting_UnitsOfMeasurement → Setting_Incoterms → Setting_Ports → Setting_DocumentNumberings → Setting_ApprovalMatrices → Setting_AuditLogs.
7. Write stored procedure `sp_Setting_DocumentNumbering_GetNext` (`UPDLOCK, HOLDLOCK`).
8. Write idempotent seed script: Super Admin role + full permission grants + one Super Admin user (BCrypt hash).
9. Wire `Sugentra.ERP.Migrator/Program.cs` to run DbUp `EnsureDatabase` + `DeployChanges` across both script folders in order (Identity before Settings, since Setting_AuditLogs.ChangedBy and other audit columns reference Identity_Users). *Depends on 5-8.*

**Phase C — Identity Module: start with User (vertical proof-of-concept)** (*depends on Phase A step 3*)
10. Create `Modules/Identity/Entities/User.cs` + `Modules/Identity/Dtos/{CreateUserRequest,UpdateUserRequest,UserResponse,UserListItemDto}.cs`.
11. Implement `Modules/Identity/Repositories/IUserRepository.cs` + `UserRepository.cs` (extends generic `Repository<User>`, adds `GetByUsernameAsync`, `ExistsByUsernameAsync`, `ExistsByEmailAsync`).
12. Implement Shared: `IPasswordHasher`/`PasswordHasher` (BCrypt), `ICurrentUserService`.
13. Implement `Modules/Identity/UseCases/UserUseCase.cs` (`CreateAsync`, `UpdateAsync`, `DeactivateAsync`) — validates uniqueness via repository, hashes password, persists, writes audit log via `IAuditLogService`.
14. Implement `Modules/Identity/Queries/UserListQuery.cs` (paginated list, no business logic).
15. Implement `Modules/Identity/Controllers/UsersController.cs` (CRUD endpoints, `[Authorize(Policy = "...")]` on writes).
16. **Verify User vertical slice end-to-end** (build + manual insert via Swagger against dev DB) before replicating pattern to Role/Permission/UserRole/RolePermission/UserPermission/UserRefreshToken/Auth.

**Phase D — Identity Module: Role, Permission, Auth** (*depends on Phase C pattern being verified*)
17. Repeat the same per-entity pattern (Entity→Dto→Repository→UseCase→Query→Controller) for `Role`, `Permission`, `UserRole`, `RolePermission`, `UserPermission`.
18. Implement `Modules/Identity/Services/PermissionResolverService.cs` (pure logic — role grant ∪ allow-override − deny-override).
19. Implement `Shared/Auth/JwtTokenService` (access + refresh token, permission claims) and `Modules/Identity/Repositories/IUserRefreshTokenRepository`.
20. Implement `Modules/Identity/UseCases/AuthUseCase.cs` (`LoginAsync`, `RefreshAsync`, `LogoutAsync`) using `PermissionResolverService` + `JwtTokenService` + `PasswordHasher`.
21. Implement `Modules/Identity/Controllers/AuthController.cs`, `RolesController.cs`, `PermissionsController.cs`.

**Phase E — Settings Module** (*can start in parallel with Phase D once Shared/Persistence exists*)
22. Repeat the per-entity pattern for `CompanyProfile`, `Warehouse` (bespoke UseCase) and `Currency`, `UnitOfMeasurement`, `Incoterm`, `Port` (simple ref-data via shared `CrudUseCase<TEntity>`).
23. Implement `Modules/Settings/Services/DocumentNumberGeneratorService.cs` + `Modules/Settings/UseCases/DocumentNumberingUseCase.cs`/`ApprovalMatrixUseCase.cs`.
24. Implement `Shared/Logging/AuditLog.cs` entity + repository + `AuditLogService`, and `Modules/Settings/Queries/AuditLogQuery.cs` + `Modules/Settings/Controllers/AuditLogsController.cs` (read-only).

**Phase F — Wiring & Verification** (*depends on all above*)
25. Configure `Program.cs`: `AddIdentityModule()`, `AddSettingsModule()`, JWT Bearer auth, dynamic `PermissionPolicyProvider` + `PermissionAuthorizationHandler`, Swagger w/ Bearer support, global exception middleware, CORS (future frontend origin via config).
26. Unit tests in `Sugentra.ERP.Tests/Modules/Identity/`: `PermissionResolverServiceTests` (role grant, allow-override, deny-override precedence); `Sugentra.ERP.Tests/Modules/Settings/`: document-number formatting; `ModuleBoundaryTests` passing.
27. Manual E2E via Swagger/Postman: run Migrator against real connection string → confirm all `Identity_*` + `Setting_*` tables + DbUp journal exist → login as seeded Super Admin (`/api/identity/auth/login`) → confirm JWT contains full permission claims → call protected endpoint without required permission (expect 403) and with it (expect 200) → create/update/soft-delete a `Setting_Warehouses` record → confirm `Setting_AuditLogs` entries created and record has `IsDeleted=1` (not physically removed).

### Relevant files (to be created)
- `Sugentra.ERP.sln`
- `src/Sugentra.ERP.Api/Shared/Common/BaseAuditableEntity.cs`, `CrudUseCase.cs`
- `src/Sugentra.ERP.Api/Shared/Persistence/Repository.cs`, `IDbConnectionFactory.cs`
- `src/Sugentra.ERP.Api/Shared/Auth/JwtTokenService.cs`, `PasswordHasher.cs`, `PermissionPolicyProvider.cs`, `PermissionAuthorizationHandler.cs`, `ICurrentUserService.cs`
- `src/Sugentra.ERP.Api/Shared/Logging/AuditLog.cs`, `AuditLogService.cs`
- `src/Sugentra.ERP.Api/Modules/Identity/Entities/*.cs`, `Dtos/*.cs`, `Repositories/*.cs`, `Services/PermissionResolverService.cs`, `UseCases/*.cs`, `Queries/*.cs`, `Controllers/*.cs`
- `src/Sugentra.ERP.Api/Modules/Settings/Entities/*.cs`, `Dtos/*.cs`, `Repositories/*.cs`, `Services/DocumentNumberGeneratorService.cs`, `UseCases/*.cs`, `Queries/*.cs`, `Controllers/*.cs`
- `src/Sugentra.ERP.Api/Program.cs`
- `src/Sugentra.ERP.Migrator/Scripts/Identity/000N_*.sql`
- `src/Sugentra.ERP.Migrator/Scripts/Settings/000N_*.sql`
- `tests/Sugentra.ERP.Tests/Modules/Identity/PermissionResolverServiceTests.cs`
- `tests/Sugentra.ERP.Tests/Architecture/ModuleBoundaryTests.cs`

### Verification
- `dotnet build` on solution succeeds.
- Migrator run completes without error against the provisioned SQL Server instance; all `Identity_*` (7 tables) + `Setting_*` (8 tables) + DbUp journal exist.
- Swagger UI loads at Api root; `/api/identity/auth/login` returns JWT with permission claims for seeded Super Admin.
- Authorization tests pass per Phase F step 27 (403 vs 200; deny-override precedence).
- `dotnet test` passes, including `ModuleBoundaryTests`.

## Further Considerations
1. Need actual SQL Server connection string (host/port/db/credentials) from the provisioned VPS/Cloud instance before Phase B step 9 can run for real.
2. Confirm `Identity_UserRefreshTokens` addition — recommended default: include it (Option A) vs. defer refresh flow and use short-lived access token only (Option B, simpler but worse UX).

## Stored Procedure Naming Convention
**Format:** `usp_{ModulePrefix}_{Entity}_{Action}[_{Qualifier}]`

- **`usp_` prefix, never `sp_`** — `sp_` is reserved by SQL Server for system procs (forces a master-DB lookup first, minor perf hit, risk of name collision). `usp_` ("user stored procedure") is the standard safe alternative.
- **`{ModulePrefix}`** matches the DB table prefix ALREADY established (`Identity_`, `Setting_`) — NOT the C# module folder name. Note the deliberate mismatch: C# folder is `Settings` (plural, matches sidebar menu) but DB/SP prefix is `Setting_` (singular, per original table convention) — always follow the DB prefix for SP names.
- **`{Entity}`** singular entity name matching the table's logical entity (`User`, `Role`, `DocumentNumbering`, `ApprovalMatrix`).
- **`{Action}`** from a fixed verb vocabulary only (below) — don't invent synonyms (no mixing Fetch/Retrieve/List/Get for the same concept).
- PascalCase throughout, consistent with table/column naming.
- Parameters: `@ColumnName` PascalCase matching the column name exactly, no Hungarian notation/abbreviations.

**Action vocabulary — maps 1:1 to the calling C# method, so SP name ↔ method name is always predictable:**

| C# method (Repository/Query) | Action verb | Example SP |
|---|---|---|
| `GetByIdAsync(id)` | `Get` | `usp_Identity_User_Get` |
| `GetByUsernameAsync(username)` | `GetBy{Field}` | `usp_Identity_User_GetByUsername` |
| `ExistsByUsernameAsync(username)` | `ExistsBy{Field}` | `usp_Identity_User_ExistsByUsername` |
| `GetPagedAsync(filter, page, size)` (Query) | `GetPaged` | `usp_Identity_User_GetPaged` |
| `SearchAsync(keyword)` (Query) | `Search` | `usp_Identity_User_Search` |
| `CreateAsync(entity)` | `Create` | `usp_Identity_User_Create` |
| `UpdateAsync(entity)` | `Update` | `usp_Identity_User_Update` |
| `SoftDeleteAsync(id)` | `SoftDelete` | `usp_Identity_User_SoftDelete` |
| `RestoreAsync(id)` | `Restore` | `usp_Identity_User_Restore` |
| `AssignRoleAsync(userId, roleId)` | `Assign{Related}` | `usp_Identity_User_AssignRole` |
| `RevokeRoleAsync(userId, roleId)` | `Revoke{Related}` | `usp_Identity_User_RevokeRole` |
| `GetNextNumberAsync(documentType)` | `GetNext` | `usp_Setting_DocumentNumbering_GetNext` |
| `SearchAuditTrailAsync(filter)` (Query, joins AuditLog+User) | `Search` | `usp_Setting_AuditLog_Search` |

**When to actually write a SP vs keep inline parameterized Dapper SQL (default is inline):** convert a Repository/Query method's SQL into a stored procedure only when ANY of:
1. ≥3 table joins or nested subqueries/CTEs.
2. Requires transactional/locking guarantees (e.g. `GetNext` needs `UPDLOCK, HOLDLOCK`).
3. Many optional filter parameters needing dynamic WHERE building (`WHERE (@Param IS NULL OR Column = @Param)` pattern).
4. Identical logic reused by more than one call-site/module.
5. Performance-critical hot path where cached execution plans matter.

Simple single-table CRUD (`Get`, `Create`, `Update`, `SoftDelete` on one table) stays as inline Dapper SQL in the Repository — no SP needed.

**File location:** SP scripts live alongside table DDL in the Migrator, in a `Procedures` subfolder per module, applied after that module's tables: `Sugentra.ERP.Migrator/Scripts/{Identity|Settings}/Procedures/usp_{Module}_{Entity}_{Action}.sql`.

## Identity module — implementation history moved out
Detailed session-by-session build log for the Identity module (User/Role/Permission vertical slices, join tables, `PermissionResolverService`, JWT issuing, schema-drift fix) now lives in **[docs/modules/identity.md](modules/identity.md)** — moved out of this file to keep `plan.md` focused on cross-cutting architecture. See that file for the full history through JWT auth being wired end-to-end.

## Dev convenience: auto-free port on `dotnet run` (this session)
`dotnet run --project src/Sugentra.ERP.Api` kept failing with `AddressInUseException` because a previous run was left listening (terminal closed without Ctrl+C). Fixed by adding dev-only logic directly in `Program.cs` (not a shell script, per explicit user request) — added at the very top after `builder = WebApplication.CreateBuilder(args)`, gated by `builder.Environment.IsDevelopment()`:
- `FreeDevelopmentPorts()` reads `ASPNETCORE_URLS` env var (set automatically by `dotnet run` from the active launch profile), extracts each port via `Uri`, calls `KillProcessListeningOnPort(port)` for each.
- `KillProcessListeningOnPort(port)` shells out to `lsof -ti tcp:{port}` (macOS/Linux only — guarded by `OperatingSystem.IsMacOS()/IsLinux()`, no-op elsewhere since this is a local dev convenience, never meant to run against a real prod port), parses PIDs from stdout, skips the current process id, force-kills (`Process.GetProcessById(pid).Kill(entireProcessTree: true)`) any other PID found, logs `"Freed port {port}: killed leftover process {pid}."` to console. Wrapped in try/catch — if `lsof` is unavailable or anything fails, silently no-ops and lets Kestrel surface the real bind error as before (non-fatal fallback).
- **Verified working live:** ran the Api twice back-to-back without ever manually killing anything in between — first run auto-killed 2 old leftover stray processes (601, 13402) from earlier in the session; second run automatically killed the first run's process (13939) and started cleanly both times. `dotnet build` — 0 warnings/errors.
- **Deliberately Development-only and platform-guarded** — this must never run in Production (would be dangerous to kill arbitrary processes on a real server) and only shells out on macOS/Linux (no Windows `netstat`/`Stop-Process` equivalent implemented, not needed for this all-macOS dev setup).

## Queries/ folder convention (applies to EVERY Query class, all modules)
- Each execution method in `Modules/<X>/Queries/*Query.cs` must be EXACTLY ONE database call: either
  (a) one stored procedure call (`CommandType.StoredProcedure`, only when SP threshold rule is met), or
  (b) one plain SQL SELECT (inline via Dapper).
- NEVER bundle multiple separate statements/round-trips in one method (e.g. do NOT run a COUNT query then a separate paged SELECT).
- For paged/list queries needing both a page of rows and a total count, use `COUNT(*) OVER()` as a window function in the single SELECT (see `UserListQuery.GetPagedSql` + `UserRepository.GetPagedAsync` as the reference implementation) — map first row's TotalCount (or 0 if no rows) into `PagedResult<T>.TotalCount`.
- This keeps Queries/ methods trivially readable as "one query in, one query out" and avoids N+1/multi-roundtrip patterns creeping into the read side.
- **Single source of truth for raw SQL text (decided this session):** any hardcoded SQL string specific to an entity (SELECT/UPDATE/DELETE/SP text) — even ones used by the Repository, not just reads — must be defined ONCE as a `public const string XxxSql` in that entity's Query class (e.g. `UserListQuery.GetByUsernameSql`, `ExistsByUsernameSql`, `ExistsByEmailSql`, `GetPagedSql`). Repository methods reference these constants instead of redefining the SQL inline, so if two places need the same statement they just call/reference the same constant — no duplication.
- **Query classes have NO execution/return methods at all (finalized this session):** e.g. `UserListQuery` is a `public static class` holding ONLY `public const string XxxSql` fields — no Dapper calls, no `async`/`return` methods, no constructor/DI (not registered in DI at all). ALL execution (opening connection, calling Dapper, mapping to DTO, returning `PagedResult<T>`) lives in the Repository (e.g. `UserRepository.GetPagedAsync` executes `UserListQuery.GetPagedSql` and returns `PagedResult<UserListItemDto>`). `IUserRepository`/`IXxxRepository` interface must declare these read methods too (e.g. `GetPagedAsync`).
- Consequence: `UseCase.GetPagedAsync` now delegates to **Repository**, not Query (e.g. `UserUseCase.GetPagedAsync` calls `userRepository.GetPagedAsync(...)`). Repository is now the ONLY thing UseCase talks to for both reads and writes — Query classes are pure SQL-text constant holders referenced only by Repository.
- Exception: the generic CRUD in `Shared/Persistence/Repository<T>` base class (Get/Add/Update/SoftDelete) builds SQL dynamically via reflection (table name + property list) and is shared across ALL entities — this stays as-is, NOT moved into per-entity Query classes, since it isn't per-entity hardcoded text.

## GetPaged filter refactor (decided this session — reference pattern for every future List/Search endpoint)
User asked to add optional filter params (Username/Email/FullName) to `GET /api/identity/users`, passed as a single JSON/query model, not loose primitives.
- **Request model:** `UserListRequest(string? Username = null, string? Email = null, string? FullName = null, int Page = 1, int PageSize = 20)` record added to `Dtos/UserDtos.cs`. Controller binds it as `[FromQuery] UserListRequest request` — ASP.NET Core binds record constructor params from query string automatically (works since .NET 7+), so the whole filter shape shows as one model in Swagger instead of loose query params.
- **Crossed the SP threshold rule:** 3 optional filters = "many optional filters needing dynamic WHERE building" (rule #3 in Stored Procedure Naming Convention section) → `GetPagedAsync` converted from inline SQL to stored procedure `usp_Identity_User_GetPaged` (matches the name already predicted in the Action Vocabulary table). SP uses the standard `WHERE (@Param IS NULL OR Column LIKE '%' + @Param + '%')` pattern + `COUNT(*) OVER()` for total count, one single SELECT inside the proc.
- **Files changed:** `src/Sugentra.ERP.Migrator/Scripts/Identity/Procedures/usp_Identity_User_GetPaged.sql` (NEW, not yet run against real DB); `UserListQuery.cs` — `GetPagedSql` const replaced with `GetPagedProcedureName = "usp_Identity_User_GetPaged"`; `UserRepository.GetPagedAsync(UserListRequest request)` now calls Dapper with `commandType: CommandType.StoredProcedure` passing `{ request.Username, request.Email, request.FullName, request.Page, request.PageSize }`; `UserUseCase.GetPagedAsync(UserListRequest request)`; `UsersController.GetPaged([FromQuery] UserListRequest request)`.
- **Reminder:** this new SP script has NOT been applied to any real database yet (Migrator still blocked on connection string, same as all other scripts) — only compiled/verified via `dotnet build`.
- **Pattern to replicate for every future List/Search endpoint with ≥3 optional filters:** define a `XxxListRequest` record in Dtos with nullable filter fields + Page/PageSize defaults → bind via `[FromQuery]` in Controller → write a `usp_{Module}_{Entity}_GetPaged`/`Search` SP with the `IS NULL OR` pattern → Query class holds only the SP name const → Repository executes it.

## DbConnection SP-call helper (decided this session — use for EVERY stored procedure call, all modules)
User asked if a helper was worth adding to avoid repeating `commandType: CommandType.StoredProcedure` boilerplate on every SP call, and separately tried an invalid shortcut (`"usp_Xxx @Param = @0, ..."` positional-style string as the "procedure name" constant) — **this does NOT work**: (1) when `CommandType.StoredProcedure` is used, the SQL/command text must be ONLY the bare procedure name — SQL Server can't parse an inline `@Param = value` list appended to it; (2) Dapper has no positional (`@0`, `@1`) parameter binding — it binds named params by matching object property names to `@ParamName` tokens via reflection; (3) `@@Username` (double `@`) collides with T-SQL system-function syntax (`@@ROWCOUNT` etc.) and is invalid as a regular parameter anyway.
- **Added `Shared/Persistence/DbConnectionExtensions.cs`** — extension methods on `IDbConnection`: `QueryStoredProcedureAsync<T>(procedureName, param?)`, `QuerySingleStoredProcedureAsync<T>(procedureName, param?)`, `ExecuteStoredProcedureAsync(procedureName, param?)` — each just wraps the matching Dapper method with `commandType: CommandType.StoredProcedure` baked in.
- **Usage:** Repository calls `connection.QueryStoredProcedureAsync<TRow>(XxxQuery.GetPagedProcedureName, new { ...filters, request.Page, request.PageSize })` — no more manual `commandType:` argument, and no `using System.Data;` needed in the Repository file since `CommandType` is now hidden inside the helper.
- **Apply this helper for every future stored-procedure call in any Repository across all modules** — never inline `commandType: CommandType.StoredProcedure` directly in a Repository method anymore.

## Full DbConnectionExtensions helper set (expanded this session — covers plain SQL too, not just SP)
User asked for a complete helper covering plain SELECT (single row / many rows), UPDATE/DELETE, and SP — framed as reducing SQL-injection risk. Note: the actual injection protection comes from Dapper's parameterized binding (`new { Username = username }` → `@Username` placeholder), which the codebase already used everywhere; the helper's real value is DRY/consistency + a single place to harden later (logging, timeouts, retry) — never string-concatenate SQL, always pass a param object through these helpers.
`Shared/Persistence/DbConnectionExtensions.cs` — full set of `IDbConnection` extension methods, ALL repositories must use these instead of calling `connection.QueryAsync`/`ExecuteAsync`/etc. or Dapper directly:
- `QuerySingleAsync<T>(sql, param?)` → plain SQL, one row (wraps `QuerySingleOrDefaultAsync`).
- `QueryListAsync<T>(sql, param?)` → plain SQL, many rows, returns `Task<List<T>>` (wraps `QueryAsync` + `.ToList()`).
- `QueryScalarAsync<T>(sql, param?)` → plain SQL, scalar (COUNT/EXISTS/`OUTPUT INSERTED.Id`), returns `Task<T>` (wraps `ExecuteScalarAsync`, null-forgiving `!` used internally to avoid a CS8619 nullability warning since T is unconstrained).
- `ExecuteCommandAsync(sql, param?)` → plain SQL INSERT/UPDATE/DELETE, returns affected row count (wraps `ExecuteAsync`).
- `QuerySingleStoredProcedureAsync<T>` / `QueryStoredProcedureAsync<T>` (returns `Task<List<T>>`) / `ExecuteStoredProcedureAsync` → same shapes but for stored procedures (`commandType: CommandType.StoredProcedure` baked in).
- **Applied everywhere already:** `Shared/Persistence/Repository<TEntity>` generic base (GetByIdAsync/GetAllAsync/AddAsync/UpdateAsync/SoftDeleteAsync), `UserRepository` (GetByUsernameAsync/ExistsByUsernameAsync/ExistsByEmailAsync/GetPagedAsync), `AuditLogService.LogAsync` — none of these call Dapper's `connection.*` methods directly anymore, all go through this helper class. `using Dapper;` was removed from files that no longer need it directly (only `DbConnectionExtensions.cs` itself imports Dapper now).
- **Rule going forward:** every new Repository method (any module) must call one of these 6 helpers — never call `connection.QueryAsync/ExecuteAsync/QuerySingleOrDefaultAsync/ExecuteScalarAsync` directly, and never build SQL by string concatenation/interpolation of user input (only static SQL text/column-name interpolation from reflection, like the generic base class does, is acceptable — actual VALUES/WHERE data always goes through the `param` object).
- **Rejected idea (do NOT implement): positional-argument helpers** — user proposed `connection.SingleOrDefault<T>(procName, val1, val2, val3, ...)` style (passing values positionally instead of a named param object) to shorten call sites further. **Explicitly rejected**: Dapper only binds parameters by matching object property names to `@ParamName` — there is no positional binding. Faking it would require reflecting the SP's `sys.parameters` metadata per call (extra DB round-trip) or hardcoding order, and if the SP's parameter order ever changes, every call site silently binds the WRONG value to the WRONG column with no compile error (e.g. Email value landing in FullName) — unacceptable correctness/data-integrity risk for an ERP system.
- **Better shortcut that IS safe and was applied:** when a request/DTO's public property names already match the SP's `@Parameter` names 1:1 (e.g. `UserListRequest.Username/Email/FullName/Page/PageSize` matching `usp_Identity_User_GetPaged`'s params), pass the object directly — NO `new { request.X, request.Y, ... }` wrapper needed at all. Dapper reflects the object's properties directly. E.g. `connection.QueryStoredProcedureAsync<UserPagedRow>(UserListQuery.GetPagedProcedureName, request)`. Only wrap in `new { }` when you need to rename/reshape properties to match different SQL param names than the source object has.

## Standard API Response Convention (applies to EVERY controller, all 12+ modules)
User explicitly asked to (1) route ALL Controller calls through `UseCase` (uniform, even reads — no more Controller→Query direct calls) and (2) standardize response shape.

**Read path changed:** `Controller → UseCase → Query` (UseCase now has a thin delegating method, e.g. `UserUseCase.GetPagedAsync` just calls `userListQuery.GetPagedAsync`). Query still does the actual efficient SQL projection — only the Controller's dependency surface changed to always be `UseCase` only, never `Query` directly. Apply this same delegation pattern for every future entity's List/Search query.

**Response standard (uniform envelope for both success AND error, revised this session — supersedes the original RFC 7807 approach below):**
- `ApiResponse<T>` (`Shared/Common/ApiResponse.cs`): `{ Success, Message, Data, Errors, Timestamp }` — wraps EVERY response, success or error alike (no more separate ProblemDetails shape for errors).
- `ApiControllerBase` (`Shared/Common/ApiControllerBase.cs`, has `[ApiController]`): all controllers must inherit this instead of `ControllerBase` directly. Helpers: `Success<T>(data, message?, statusCode?)` → 200 wrapped in `ApiResponse<T>`; `SuccessCreated<T>(location, data, message?)` → 201 with Location header, wrapped; `SuccessMessage(message?, statusCode?)` → for actions with nothing to return (Delete/Deactivate/Revoke acknowledgements) — named `SuccessMessage` (not an overload of `Success`) to avoid C# overload ambiguity between `Success<T>(T data,...)` and a no-data `Success(string? message,...)` when the argument is a string; `Failure(message, statusCode, errors?)` → same `ApiResponse<object?>` envelope with `Success=false`; `ValidationFailure(message, errors)` → `Failure(...)` at 422.
- `GlobalExceptionHandler` (`Shared/Middleware/GlobalExceptionHandler.cs`, implements .NET 8 `IExceptionHandler`): catches any unhandled exception, logs it, returns the SAME `ApiResponse<object?>` envelope (500, `Success=false`, generic message) — no longer emits ProblemDetails. Registered in `Program.cs` via `AddExceptionHandler<GlobalExceptionHandler>()` + `app.UseExceptionHandler()`. **`AddProblemDetails()` is still required in DI** even though ProblemDetails is no longer used for the response body — `UseExceptionHandler()` throws `InvalidOperationException` at startup without it (either `ExceptionHandlingPath`/`ExceptionHandler` or a registered `IProblemDetailsService` must exist for the middleware to construct).
- **Model-validation errors (automatic `[ApiController]` 400) also unified**: `Program.cs` configures `services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = ...)` to build the same `ApiResponse<object?>` envelope (422, `Errors` = dictionary of field→message array) instead of the default `ValidationProblemDetails`.
- Status code convention per action: Create success→201 (`SuccessCreated`, Location = `api/{module}/{resource}/{id}`), Create failure (duplicate/conflict)→409; Update success→200 via `Success(data, "X updated successfully.")`, Update failure (not found)→404; Delete/Deactivate/Revoke success→200 via `SuccessMessage("X deleted successfully.")` (**no longer `204 NoContent()`** — revised this session so delete/update acknowledgements always carry a message body, consistent with the rest of the envelope), Delete failure→404; Read/list→200 via `Success(pagedResult)`.
- `[ApiController]` attribute now lives ONLY on `ApiControllerBase` (removed from individual controllers).
- Applies to ALL future controllers across all modules — `UsersController`/`RolesController`/`PermissionsController` are the reference implementations to copy the pattern from.

## Email/Password/Username DataAnnotations validation (this session — CreateUserRequest reference pattern)
Added `System.ComponentModel.DataAnnotations` attributes directly on `CreateUserRequest`'s (and `UpdateUserRequest`'s Email) record primary-constructor parameters — validated automatically by `[ApiController]`'s model-binding pipeline before the UseCase is ever invoked, output through the unified `ApiResponse` 422 envelope above.
- **C# record gotcha (bit twice this session, now load-bearing rule): validation attributes on a record's primary-constructor parameters must NOT use the `[property: ...]` target.** Using `[property: Required, ...]` throws a runtime `InvalidOperationException` ("has validation metadata defined on property 'X' that will be ignored... must be associated with the constructor parameter") the moment ASP.NET Core tries to validate the model — NOT a compile error, only surfaces when an actual request hits the endpoint. Always write plain `[Required, StringLength(...), ...]` directly on the parameter (no attribute target specifier) for record types bound from `[FromBody]`.
- **Built-in `[EmailAddress]` is too weak, do not rely on it alone:** .NET's `EmailAddressAttribute.IsValid` only checks that there is exactly one `@` character, not at the first/last position — it does **not** require a domain or TLD, so `user@localhost` or `user@abc` incorrectly PASS. Fixed by using a custom `[RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ...)]` instead (requires at least one `.` after the `@`, i.e. a domain+extension) — **removed `[EmailAddress]` entirely** rather than stacking both attributes, since having both simultaneously produced two different (and once, accidentally identical-text) error messages for the same underlying failure when both fired at once; one attribute, one message, no ambiguity.
- **Final `CreateUserRequest` validation set** (`Modules/Identity/Dtos/UserDtos.cs`):
  - `Username`: `[Required, StringLength(100, MinimumLength = 3), RegularExpression(@"^[a-zA-Z0-9_.-]+$")]` — matches DB column max length (100) and restricts to safe characters.
  - `Email`: `[Required, StringLength(256), RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]` — matches DB column max length (256), requires real domain+extension.
  - `Password`: `[Required, MinLength(8)]` — no DB length constraint (only the BCrypt hash is stored), but a minimum is still enforced for basic security.
  - `FullName`: `[Required, StringLength(200, MinimumLength = 2)]` — matches DB column max length (200).
  - `PhoneNumber` (optional): `[Phone, StringLength(50)]`.
  - `EmployeeId` (optional): `[StringLength(50)]`.
  - `UpdateUserRequest.Email` uses the same Email pattern as above (kept in sync manually — no shared constant extracted yet, small enough duplication for now).
- **Reference/lesson for future entities:** always cross-check DataAnnotations `StringLength` max values against the actual DB column `NVARCHAR(N)` length in the Migrator `.sql` script (re-read the script, don't assume) so a request can never pass C# validation but still fail at the DB with a truncation error.

## Settings module — implementation history moved out
Detailed session-by-session build log for the Settings module (CompanyProfile, Warehouses, Currencies, UoM, Incoterms, Ports, Document Numbering, Approval Matrix, module hub decision, Setting_Modules/Setting_Menus for the System Administration group) now lives in **[docs/modules/settings.md](modules/settings.md)**. The superadmin credential note that used to live in this section has been relocated to [docs/modules/identity.md](modules/identity.md) — do not re-add credentials to this file.

## Phase F follow-up: RowVersion removed entirely (this session, part 5)
Not needed for now (no optimistic-concurrency use case yet) — removed from the audit-column convention globally rather than left half-used.

- `Shared/Common/BaseAuditableEntity.cs` — `RowVersion` property removed.
- `Shared/Persistence/Repository<TEntity>.ResolveWritableColumns()` — the special-case exclusion for `RowVersion` removed (no longer exists on the base entity).
- New idempotent DROP COLUMN scripts added (existing applied scripts left untouched, per convention): `Scripts/Identity/0011_Drop_RowVersion_Columns.sql` (6 Identity tables), `Scripts/Settings/0013_Drop_RowVersion_Columns.sql` (9 Settings tables). Applied to the live DB — "Upgrade successful".
- `AGENTS.md` and this plan's audit-column convention bullets updated to drop the `RowVersion` mention.
- Validated: `dotnet build` — 0 warnings/errors. Migrator ran clean against live DB.

## Phase F follow-up: audit log OldValues/NewValues now full JSON snapshots (this session, part 6)
Previously each UseCase built ad-hoc `"Field=value;Field2=value2"` strings (or `null`) for `OldValues`/`NewValues`. Changed to full `System.Text.Json` serialization of the entity/relation before and after the change — no schema change needed since `Setting_AuditLogs.OldValues`/`NewValues` were already `NVARCHAR(MAX)`.

- `CrudUseCase<TEntity>` (generic Settings CRUD): Create logs `JsonSerializer.Serialize(entity)` as `NewValues`; Update logs the pre-change and post-change serialized entity; Delete logs the pre-delete entity as `OldValues`.
- `RoleUseCase`/`PermissionUseCase`/`UserUseCase`: same pattern for Create/Update/SoftDelete. Relation actions (`AssignPermission`/`RevokePermission`/`AssignRole`/`RevokeRole`/`SetPermissionOverride`/`RemovePermissionOverride`) now log a small JSON object (e.g. `{"RoleId":1,"PermissionId":2}`) instead of an ad-hoc string, for consistent parseability.
- **Security fix along the way:** `User.PasswordHash` marked `[JsonIgnore]` so the hash is never written into an audit log JSON snapshot (it also wasn't exposed via `UserResponse` before, but this closes the same risk for the new full-entity serialization).
- Validated: `dotnet build` — 0 warnings/errors. Architecture test still passing.

## Master Data module — implementation history moved out
Detailed session-by-session build log for the Master Data module (Business Partners, Items, Bill of Materials, Price Lists) now lives in **[docs/modules/master-data.md](modules/master-data.md)**.

## Sugentra.ERP.UI — implementation history moved out
Detailed session-by-session build log for the `Sugentra.ERP.UI` shared shell (full MVC frontend scaffolding, dashboard/module hub, branding, dynamic sidebar, per-page UI polish rolled out app-wide) now lives in **[docs/modules/ui.md](modules/ui.md)**. Related Master Data sample-data-seed and Identity GET-by-id session notes have moved into [docs/modules/master-data.md](modules/master-data.md) and [docs/modules/identity.md](modules/identity.md) respectively.

## Disaster-recovery check: from-scratch migration was actually broken — found + fixed (this session)
User asked whether the whole DB (schema + data) could be rebuilt from scratch via the Migrator if it were ever lost. Rather than assume, this was tested empirically by pointing `SUGENTRA_ERP_CONNECTION_STRING` at a brand-new, disposable database and running the Migrator end-to-end. The answer was **no** — two real, previously-undiscovered bugs were found and fixed:

1. **Hardcoded `USE [Sugentra_ERP]` in 16 scripts silently redirected execution to the real dev DB.** `Scripts/Identity/0001_Identity_Users.sql` (and 15 others across Identity/Settings/MasterData) started with a literal `USE [Sugentra_ERP]\nGO` header. DbUp keeps one open connection for the whole run, so once the *first* script (0001) executed that `USE`, every subsequent script — even ones without their own `USE` — ran against the real `Sugentra_ERP` database instead of the intended fresh test database, regardless of what the connection string said. The brand-new test database itself stayed essentially empty. This was confirmed by finding 16 duplicate `SchemaVersionsJournal` rows in the real DB afterward (idempotent `IF NOT EXISTS`/`WHERE NOT EXISTS` guards meant no actual schema/data duplication occurred — only journal bookkeeping was affected, and the duplicates were cleaned up with a one-off `DELETE` keeping the lowest `Id` per `ScriptName`).
   - **Fix:** removed the `USE [Sugentra_ERP]\nGO` header from all 16 affected scripts (pure removal, no schema/logic change — DbUp's connection string already targets the correct database, making the `USE` both redundant and environment-unsafe). This is a rare, deliberate exception to "never edit an already-applied script," justified because it only strips a dangerous, non-semantic statement and doesn't touch any DDL/DML.
2. **`Scripts/Identity/0017_Add_IsBanned_To_Users.sql` had no idempotency guard.** On a fresh DB it ran twice in the same session (once via the `USE` hijack against the real DB, once for real against the actual target once bug #1 surfaced it) and failed with SQL error 2705 (duplicate column). Fixed by wrapping it in `IF COL_LENGTH('Identity_Users', 'IsBanned') IS NULL BEGIN ... END`, consistent with the guard style used elsewhere in the script set.
3. **Cross-module script ordering bug (pre-existing, unrelated to the above): `MasterData` scripts have FK dependencies on `Settings` reference data (Currencies, UnitsOfMeasurement), but DbUp's default embedded-script discovery sorts every script alphabetically by full resource name across all folders combined — `Identity` < `MasterData` < `Settings` — so `MasterData_Items` (which FKs to `Setting_UnitsOfMeasurement`) tried to run before `Settings` created that table, failing with error 1767 on a truly fresh database.**
   - **Fix:** `src/Sugentra.ERP.Migrator/Program.cs` now supplies a custom `ScriptModuleOrderComparer : IComparer<string>` via `.WithScriptNameComparer(...)` that forces module run order `Identity → Settings → MasterData` while preserving each module's own existing numeric script order. (First attempt built a pre-sorted `List<SqlScript>` and passed it via `.WithScripts(...)`, but DbUp's `UpgradeEngine.GetScriptsToExecuteInsideOperation()` always re-sorts whatever it's given via `configuration.ScriptSorter.Sort(..., configuration.ScriptNameComparer)` — confirmed by reading DbUp's source — so a custom `IComparer<string>` registered via `WithScriptNameComparer` is the only way to actually control final ordering.) Since script *names* never change, already-migrated environments (real dev DB) are completely unaffected — this only changes ordering for a from-scratch run.
- **Validated:** ran the Migrator twice more against fresh disposable test databases (`Sugentra_ERP_MigrationTest`, `Sugentra_ERP_MigrationTest2`) after each fix; the final run completed with all 19 Identity scripts, all 25 Settings scripts, and all 6 MasterData scripts executing in the corrected order, ending in `Upgrade successful` + automatic Super Admin seeding. Both disposable test databases were dropped afterward; the real dev DB's duplicate journal rows were cleaned up.
- **Answer given to user:** disaster recovery from scratch was **not** actually reliable before this session — it is now, and has been verified with an actual from-scratch run rather than just inspection.

## Script consolidation: CREATE + INSERT only, no ALTER/DROP/UPDATE/DELETE (this session)
Follow-up to the disaster-recovery check above. User asked to fold every incremental `ALTER TABLE`/column add-drop back into its originating `CREATE TABLE` script, and every `UPDATE`/`DELETE` against seed data back into its originating `INSERT` (using the final, correct values) — so a from-scratch migration executes **only** `CREATE`/`INSERT` statements (plus `CREATE OR ALTER PROCEDURE` for stored procs, which are inherently re-creatable and not schema/data patches).

**Identity module:**
- Removed `RowVersion ROWVERSION NOT NULL` from `0002`–`0007`'s `CREATE TABLE` statements (was dropped app-wide via a since-deleted `ALTER`).
- `IsBanned BIT NOT NULL DEFAULT (0)` added directly into `0001_Identity_Users.sql`'s `CREATE TABLE` (was added later via `ALTER`).
- Fixed `Module` values directly at their seed source instead of via a later `UPDATE`: `AuditLog_View` → `'AuditLog'` in `0009`; `Module_View`/`Module_Manage` → `'SystemAdministration'` in `0014`; `Menu_View`/`Menu_Manage` → `'SystemAdministration'` in `0015`.
- `Identity/Procedures/usp_Identity_User_GetPaged.sql` (canonical, DbUp-authoritative definition — it sorts alphabetically *after* the numbered scripts within the module, so a stale copy here silently reverts any patches applied by numbered scripts) rewritten to the final version: `@Status` parameter (Active/Inactive/Locked/Banned precedence), `IsBanned`/`LockoutEnd` columns, `GETDATE()`-based. This was a genuine **live latent bug** — the old file here would have silently broken `@Status`-filtered user queries on any fresh migration.
- Deleted (fully merged, now redundant): `0011_Drop_RowVersion_Columns.sql`, `0017_Add_IsBanned_To_Users.sql`, `0018_Update_UserGetPaged_Status_Filter.sql`, `0019_UserGetPaged_Use_GetDate.sql`. `0016_Fix_SystemModule_PermissionMapping.sql` kept but trimmed down to only its still-needed `UIKit_View` permission `INSERT` (its two `UPDATE`s were eliminated by fixing `0009`/`0014`/`0015` directly).

**Settings module:**
- Removed `RowVersion` from `0001`, `0003`–`0010`'s `CREATE TABLE` statements.
- `Setting_AuditLogs.ChangedBy` was already nullable in `0001` (the file had already converged with `0002`'s intent) — `0002_Setting_AuditLogs_ChangedBy_Nullable.sql` deleted as a genuine no-op.
- `Setting_Modules` (`0014`): added `Color`/`ColorOpacity`/`BackgroundColor`/`BackgroundOpacity`/`TextColor` columns directly into the `CREATE TABLE` (previously 5 separate guarded `ALTER`s in `0016`); its seed `INSERT` now carries final `Icon` (previously `UPDATE`d in `0015`) and final `Route` values (`/Workspace/{Code}` pattern — previously two generations of `UPDATE`, an intermediate `/Workspace/Index/{Code}` in `0019` then the final form in `0025`) directly in the `VALUES` list.
- `0021_Setting_Modules_SystemGroups.sql` (SystemAdministration/AuditLog/UIKit) seed `INSERT` now sets `IsActive = 1` and the final `Route` directly (previously `UPDATE`d in `0023` and `0024`).
- `Setting_CompanyProfile` seed (`0012`) now inserts the final `LogoUrl = '/uploads/branding/logo.png'` directly (previously two generations of `UPDATE` in `0017`/`0018`, only the second of which held the final value).
- `Settings/Procedures/0011_usp_Setting_DocumentNumbering_GetNext.sql` converted from `DROP PROCEDURE IF EXISTS` + `CREATE PROCEDURE` to `CREATE OR ALTER PROCEDURE` (its internal `UPDATE Setting_DocumentNumberings` is legitimate runtime business logic — number generation — not a migration patch, so it stays).
- Deleted (fully merged, now redundant): `0002`, `0013_Drop_RowVersion_Columns.sql`, `0015_Seed_ModuleIcons.sql`, `0016_Setting_Modules_Colors.sql`, `0017_Update_CompanyProfile_LogoUrl.sql`, `0018_Update_CompanyProfile_LogoUrl.sql`, `0019_Update_Module_Routes.sql`, `0023_Activate_SystemGroups.sql`, `0024_Setting_Modules_SystemGroup_Routes.sql`, `0025_Drop_Index_From_Module_Routes.sql` (despite its name, this was actually a further `Route` value `UPDATE`, not an index drop).

**MasterData module:** no changes needed — all 6 scripts (`0001`–`0006`) were already `CREATE TABLE`/`INSERT`-only with `IF NOT EXISTS`/`WHERE NOT EXISTS` guards.

- **Validated end-to-end:** built the solution clean (0 warnings/errors), ran `dotnet test` (architecture/module-boundary test still passes), then ran the full Migrator from scratch against a brand-new disposable database (`Sugentra_ERP_ConsolidationTest`) via a throwaway `Microsoft.Data.SqlClient` console helper (created and torn down in `/tmp`, not part of the repo) — confirmed 0 `RowVersion` columns remain anywhere, `Identity_Users.IsBanned` exists, all 5 `Setting_Modules` color columns exist, `CompanyProfile.LogoUrl`/`Setting_Modules.Route`/`Icon`/`IsActive` and all `Identity_Permissions.Module` values match the intended final state, `UIKit_View` permission exists, and Super Admin seeding completed. Test database dropped afterward, no residual test artifacts left in the repo.
- A repo-wide `grep` for `ALTER TABLE|DROP TABLE|DROP PROCEDURE|DROP COLUMN|UPDATE |DELETE ` across `Scripts/**` now returns only the single legitimate business-logic `UPDATE` inside `usp_Setting_DocumentNumbering_GetNext` — every other migration script is `CREATE`/`INSERT`-only.

**Renumbering follow-up:** deleting the redundant files above left gaps (`Identity` jumped `0010→0012`; `Settings` had gaps at `0002`, `0011`, `0013`, `0015`–`0019`, `0023`–`0025`). Renamed all numbered scripts to be contiguous: `Identity` is now `0001`–`0015`, `Settings` is now `0001`–`0014`. `Settings/Procedures/usp_Setting_DocumentNumbering_GetNext.sql` had its now-meaningless `0011` prefix dropped (standalone procedure file in its own subfolder, not part of the numbered sequence). `MasterData` was already contiguous (`0001`–`0006`), untouched.
- **Validated:** forced a clean rebuild of `Sugentra.ERP.Migrator` (embedded resource names are baked in at compile time, so a straight `dotnet build`/`dotnet run` without clearing `obj`/`bin` can silently run against stale embedded script names after a rename — had to `rm -rf obj bin` first to confirm the rename actually took effect) and re-ran the Migrator against another fresh disposable database (`Sugentra_ERP_RenumberTest`) — all scripts executed under their new names in the same correct order, ending in `Upgrade successful`. Test database dropped afterward.

## Menu-to-module grouping review + System Administration future scope (this session, discussion only — no code changed)
User asked for a senior-architect review of which existing menu (Users, Roles, Permissions, Business Partners, Items, BOM, Price Lists, Currencies, UoM, Modules, Menus, Audit Log, Tables) belongs in which of the 16 planned business modules, then a deeper critique of whether the current grouping is architecturally sound, then specifically what else typically belongs in **System Administration** going forward. Conclusions below, not yet implemented.

**Menu grouping confirmed correct as-is** (no schema/code change needed): Users/Roles/Permissions → Identity & Access; Business Partners/Items/BOM/Price Lists → Master Data; Currencies/UoM → Settings; Modules/Menus → System Administration (business/UI grouping — physically still coded under `Modules/Settings/**`, unchanged, per the existing documented trade-off above); Audit Log → its own cross-cutting group; Tables → UI Kit (pure style reference, not business data, see `TablesController.cs`).

**Architectural risks flagged during the critique (not yet fixed, revisit before the owning module is built):**
1. **`MasterData_PriceLists` is under-modeled** — flat `ItemId/CurrencyId/Price/EffectiveDate`, no `Type` (Sales vs Purchase) and no named list/customer-group concept. Before Sales or Procurement builds on top of this table, redesign to `PriceListHeader (Name, Type, CurrencyId, BusinessPartnerId?) → PriceListLines (ItemId, Price, EffectiveDate)` — otherwise the two modules will collide on the same flat table.
2. **`Setting_Currencies`/`Setting_UnitsOfMeasurement` are static lookups today** (no exchange-rate or conversion-factor data), which is why Settings is a defensible home for them right now. Rule going forward: if Finance adds exchange-rate history, that's a `Finance_ExchangeRates` (time-series, FK to `Setting_Currencies`) table owned by **Finance**, not a column added to `Setting_Currencies`. Same for UoM conversion factors → belongs to Master Data/Inventory (`MasterData_ItemUnitConversions`), not `Setting_UnitsOfMeasurement`.
3. **`MasterData_BillOfMaterials` is currently just a static recipe header** (`ItemId/Name/Description`, no versioning/routing/yield). Fine as Master Data today. Once Production needs BOM versioning/alternate BOM/routing, those attributes must live in a **Production-owned** table FK'd to the BOM header — do not add Production-specific columns directly onto `MasterData_BillOfMaterials`.
4. **`MasterData_BusinessPartners` is a unified Customer/Supplier/Both entity** (`PartnerType` string flag) — correct for this monolith today. If Sales needs `CreditLimit`/`SalesRepId` or Procurement needs `PaymentTermDays`/`VendorRating`, those must go in module-owned 1:1 extension tables (`Sales_CustomerProfile`, `Procurement_VendorProfile`), not as new columns on the core Master Data entity.
5. **General rule to document/enforce**: Settings = static configuration with no behavior/history attached; the moment a "setting" needs a time-series or module-specific business rule, ownership moves to the module that owns that rule.

**System Administration — proposed future scope** (in addition to existing Modules/Menus), prioritized for this ERP's actual context (single-tenant, on-prem SQL Server, export/trading company — explicitly NOT a multi-tenant SaaS, so tenant/license management is out of scope):
- **High priority:**
  - *System Parameters* — move currently-hardcoded `appsettings.json` values (`Jwt.AccessTokenExpiryMinutes`, `RefreshTokenExpiryDays`, `AccountLockout.MaxFailedAttempts`) into a DB-backed, admin-editable parameter table + memory cache with invalidation, so they're changeable without a redeploy.
  - *Session Management* — admin UI over the existing `Identity_UserRefreshTokens` table to view active sessions and force-logout a user (e.g. resigned employee, lost device). Lives in System Administration (operational/security concern), not Identity (account management).
  - *Integration / API Key Management* — this ERP will eventually integrate PEB/customs, freight forwarders, banks; need a place to manage external API keys/secrets, webhook endpoints, and integration request/response logs (distinct from `Setting_AuditLogs`, which is data-change level, not transport level).
  - *Scheduled Jobs* — admin visibility (status/history, e.g. `SystemAdmin_JobExecutionLogs`) over background tasks like daily FX rate fetch, document-number resets, approval-matrix reminders.
  - *Notification / Email Template Management* — externalize approval/low-stock/export-deadline notification templates instead of hardcoding strings in code.
- **Medium priority:**
  - *System/Error Log Viewer* — technical application logs (Serilog-style), distinct from the business-data-change `Setting_AuditLogs`.
  - *Data Import/Export Center* — generalize the per-page CSV import/export pattern (already built for Users) into a shared System Administration facility instead of duplicating it per module.
  - *Backup & Maintenance* — manual backup trigger, read-only view of the DbUp journal/schema-version table, maintenance-mode toggle.
- **Explicitly out of scope for now:** multi-tenant/company management, license key management, multi-language/localization — not relevant to a single-company on-prem deployment; revisit only if a genuine multi-branch/multi-entity requirement appears.

**Next logical step:** not yet started — candidates ranked by "cheapest to build on existing patterns first": System Parameters and Session Management (both reuse `GenericRepository`/`CrudUseCase`, no new architectural pattern needed) before Integration/Scheduled Jobs (need new infra: key storage/encryption, job runner). Ask the user which to implement first.

</content>
