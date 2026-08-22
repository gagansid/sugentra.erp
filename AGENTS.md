# Sugentra ERP — Agent Instructions

Modular-monolith ERP API (.NET 8, C#, Dapper — no EF Core). Full architecture rationale and roadmap live in [docs/plan.md](docs/plan.md) — read it for "why", this file is only "how to work here". Per-module, session-by-session implementation history (what was built, in what order, with what caveats) lives in `docs/modules/<name>.md` (e.g. [docs/modules/identity.md](docs/modules/identity.md), [docs/modules/settings.md](docs/modules/settings.md), [docs/modules/master-data.md](docs/modules/master-data.md), [docs/modules/ui.md](docs/modules/ui.md)) — check the relevant file there for implementation-level context before starting work on an existing module.

## Build / Test
```
dotnet build Sugentra.ERP.sln
dotnet test tests/Sugentra.ERP.Tests/Sugentra.ERP.Tests.csproj
dotnet run --project src/Sugentra.ERP.Api
dotnet run --project src/Sugentra.ERP.Migrator   # applies DbUp scripts to SQL Server
```
No EF Core migrations — schema changes are hand-written, versioned SQL scripts under `src/Sugentra.ERP.Migrator/Scripts/<Module>/NNNN_Description.sql`, applied by the Migrator (DbUp) in filename order. Never edit an already-applied script; add a new numbered one.

## Module Structure (per module under `src/Sugentra.ERP.Api/Modules/<Name>/`)
`Controllers/ → UseCases/ → Repositories/ + Services/`, plus separate `Queries/` for reads, `Entities/`, `Dtos/`.

- **Write path**: Controller → UseCase (business rules, one class per entity with multiple methods, e.g. `UserUseCase.CreateAsync/UpdateAsync`) → Repository (Dapper CRUD only) + module Services + Shared services (audit log, hashing).
- **Read path**: Controller → Query (raw Dapper, joins/paging, projects directly to Dtos) — bypasses UseCase and Repository entirely; reads have no business rules to protect.
- Simple reference data (Currencies, UoM, etc.) can skip a bespoke UseCase and use the generic `Repository<TEntity>` (in [Shared/Persistence/Repository.cs](src/Sugentra.ERP.Api/Shared/Persistence/Repository.cs)) directly via `CrudUseCase<T>`.
- Entities inherit `BaseAuditableEntity` and are tagged `[Table("Identity_Users")]`-style for the generic repository's table-name resolution.
- Each module exposes one extension method for DI wiring, e.g. `AddIdentityModule()` in [IdentityModuleExtensions.cs](src/Sugentra.ERP.Api/Modules/Identity/IdentityModuleExtensions.cs), called from `Program.cs`. Controllers use attribute routing so no `MapXEndpoints()` is needed.

## Module Isolation (enforced, not just convention)
A class under `Modules/X/**` must never reference another module's namespace directly — only `Modules/X/**` and `Shared/**`. Cross-module calls go through an interface in `Shared/Contracts/`, implemented by the owning module. This is enforced automatically by [tests/Sugentra.ERP.Tests/Architecture/ModuleBoundaryTests.cs](tests/Sugentra.ERP.Tests/Architecture/ModuleBoundaryTests.cs) (NetArchTest) — run the test suite after adding cross-module code; a build-breaking failure there means the boundary was violated.

## Conventions
- Primary keys: `BIGINT IDENTITY`, not GUID.
- Every table has standard audit columns: `CreatedAt/By`, `UpdatedAt/By`, `IsDeleted`, `DeletedAt/By`. Repositories auto-filter `WHERE IsDeleted = 0` and populate audit columns on write — don't do this manually in UseCases.
- SQL table/column naming: `<Module>_<Entity>` (e.g. `Identity_Users`, `Setting_AuditLogs`). `Identity` and `Settings` are separate modules — Identity owns Users/Roles/Permissions/Auth; Settings owns pure reference/config data only.
- Validation via FluentValidation. Password hashing via BCrypt (`IPasswordHasher`). No MediatR/CQRS framework — "Queries" here means the folder convention above, not a mediator pattern.
- API responses use the `ApiResponse<T>` envelope ([Shared/Common/ApiResponse.cs](src/Sugentra.ERP.Api/Shared/Common/ApiResponse.cs)); validation failures return 422 via the `InvalidModelStateResponseFactory` configured in `Program.cs`, not the ASP.NET default 400.
- Audit logging: call `IAuditLogService.LogAsync(...)` explicitly from UseCase methods after Insert/Update/Delete — it is not automatic.
- Permission checks: `[Authorize(Policy = "PermissionCode")]` on controller actions, resolved dynamically via `PermissionPolicyProvider`/`PermissionAuthorizationHandler` in [Shared/Auth/PermissionAuthorization.cs](src/Sugentra.ERP.Api/Shared/Auth/PermissionAuthorization.cs) (no per-permission policy registration needed). JWT Bearer auth is wired in `Program.cs`; permissions are resolved once at login by `PermissionResolverService` (role grants ∪ allow-overrides − deny-overrides, **deny always wins**) and embedded as `"permission"` claims in the access token — authorization checks read claims only, zero DB query per request. 401 (not authenticated) and 403 (authenticated, missing permission) both return the standard `ApiResponse` envelope via `JwtBearerEvents.OnChallenge`/`OnForbidden`, never an empty body.

## Adding a New Module
1. Create `Modules/<Name>/{Controllers,UseCases,Services,Repositories,Queries,Entities,Dtos}`.
2. Add SQL scripts under `Sugentra.ERP.Migrator/Scripts/<Name>/`.
3. Add `Add<Name>Module()` extension method, register it in `Program.cs`.
4. Run the architecture test to confirm no cross-module leakage.
