# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

### Backend (.NET 10)
```bash
cd src/backend
dotnet restore
dotnet build --no-restore                          # debug build
dotnet build --no-restore --configuration Release  # release build
dotnet test --no-build                             # run all tests
dotnet test --no-build --filter "FullyQualifiedName~ClassName.MethodName"  # single test
dotnet run --project src/LastMile.TMS.Api           # run API locally
```

### Web (Next.js)
```bash
cd src/web
npm ci
npm run lint
npm run build
npm run dev    # dev server on port 3000
```

### Full Stack (Docker)
```bash
docker compose up --build   # everything at http://localhost
```

## Documentation

- read documentation, don't invent stuff, look at the examples in the documentation to understand how things work.
- use the `context7` MCP tool to fetch up-to-date HotChocolate documentation before writing query/mutation code.
- when you encounter inconsistency or an issue, document it in the `architecture.md` file.

## Architecture

See `architecture.md` in the repo root for full architecture documentation.

### GraphQL / HotChocolate — Coding Rules

**Do NOT use MediatR for queries.** Queries are direct HotChocolate resolvers using `AppDbContext` directly.

Middleware attribute order matters — HotChocolate processes them bottom-to-top:

**Collection queries (with pagination):**
```csharp
[Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
[UsePaging(IncludeTotalCount = true)]
[UseProjection]
[UseFiltering]
[UseSorting]
public IQueryable<Zone> GetZones([Service] AppDbContext context)
    => context.Zones.AsNoTracking();
```

**Single-item queries:**
```csharp
[Authorize(Roles = new[] { Role.RoleNames.Admin, Role.RoleNames.OperationsManager })]
[UseSingleOrDefault]
[UseProjection]
public IQueryable<Zone> GetZone(Guid id, [Service] AppDbContext context)
    => context.Zones.AsNoTracking().Where(z => z.Id == id);
```

Reference implementations: `src/backend/src/LastMile.TMS.Api/GraphQL/Extensions/Zone/ZoneQuery.cs`, `DepotQuery.cs`.

**Mutations use MediatR** — commands via `IRequest<T>` / `IRequestHandler<T, TResponse>` in `Application/Features/{Entity}/Commands/`. The `ValidationBehavior` pipeline auto-applies FluentValidation.

```csharp
public async Task<ZoneResult> CreateZone(CreateZoneCommand input, [Service] IMediator mediator)
    => await mediator.Send(input);
```

## TDD — Red, Green, Refactor

All features and bug fixes must follow the TDD cycle:

1. **Red**: Write a failing test first that defines the expected behavior. Build and confirm the test fails.
2. **Green**: Write the minimum code to make the test pass. No more than what the test requires.
3. **Refactor**: Clean up the implementation while keeping all tests green. Remove duplication, improve naming, simplify.

- Never write production code without a failing test driving it.
- Run the relevant test(s) after each step to verify the cycle: fail → pass → pass.
- Test projects mirror source projects: `Domain.Tests`, `Application.Tests`, `Api.Tests`, `Architecture.Tests`.
- Backend uses xUnit + FluentAssertions + NSubstitute. Api.Tests uses `WebApplicationFactory` for integration tests.

## Core Rules

- Read `architecture.md` before starting any coding task.
- Do not introduce structure or dependencies that conflict with architecture docs.
- If code and architecture docs diverge, call out the mismatch and fix one or the other intentionally.
- Update `architecture.md` when architectural rules or maintained structure changes.
- Pagination is always done through backend, using hot chocolate middleware `[UsePaging]`.
- Preserve the project vocabulary: `depots`, `drivers`, `parcels`, `routes`, `users`, `vehicles`, `zones`.
- Keep transport and composition layers thin.
- Prefer small, focused files with clear responsibilities over large mixed modules.
- Update tests together with code changes.
- Run lint and tsc to make sure there are no errors.
- Do not introduce a second hand-maintained GraphQL schema mirror in frontend types without a reason.

### New vs Existing Features

- **New features must be designed without known issues.** If a new frontend page is developed, it should use codegen when reasonable (see Frontend Checklist).
- **Existing features that work must be left as-is.** Do not refactor working code to adopt a new pattern unless the feature is being actively changed. The parcels list, for example, uses a dynamic query builder because of its column picker -- this is the correct approach and should not be retrofitted to codegen.

## Backend Checklist

When adding or changing a backend feature:

1. Model the use case in `Application` -- command record, handler, validator, DTOs.
2. Choose the read path: direct `IQueryable<TEntity>` via `AppDbContext` (queries) or MediatR handler (only if needed).
3. Organize `Application/<Feature>/` using:
   - `Commands/<UseCase>/` -- command, handler, validator
   - `DTOs/` -- request/response DTOs
   - optional `Services/` or `Support/`
4. Organize `Api/GraphQL/Extensions/<Feature>/` using:
   - `*Queries.cs` -- direct `AppDbContext` resolvers
   - `*Mutations.cs` -- thin resolvers delegating to `IMediator`
   - `*Types.cs` -- output types, filter types, sort types (when needed)
   - `*Inputs.cs` -- input types (when needed, see `Api/GraphQL/Inputs/`)
5. Keep GraphQL resolvers thin -- no business logic, no direct `DbContext` writes.
6. Add or update tests in the owning backend test project.

## Frontend Checklist

When adding or changing a frontend feature:

1. Add or update `.graphql` operations in `src/web/src/graphql/documents/`.
2. Run `cd src/web && npm run codegen`.
3. Put request orchestration in `src/web/src/services/<domain>.service.ts`.
4. Put React Query hooks in `src/web/src/hooks/use-<domain>.ts`.
5. Keep route files thin under `src/web/src/app/`.
6. Put domain UI in `src/web/src/components/<domain>/`.
7. Use local `src/web/src/types/*` only for UI/request models that add value beyond raw GraphQL transport types.
8. Add or update tests close to the owning layer.

**Exception -- dynamic column selection:** For pages with a user-facing column picker (like the parcels list), use the dynamic query builder pattern (`column-registry.ts` + `build-<feature>-query.ts`) instead of codegen. See `architecture.md` section 6.9 for the tradeoff details.

## Key Rules

- Do not call backend services directly from components; go through query hooks.
- Do not put business logic in route files.
- Do not edit generated GraphQL artifacts by hand.
- Run `npm run codegen` after backend GraphQL contract changes before touching frontend transport code.

## Code Style

- Avoid unnecessary reordering of lines if the file was commited before with different order, like sorting imports alphabetically.
- Use latest stable versions (preview version 16 of hot chocolate is an exception)
- C#: 4-space indent, nullable references enabled, implicit usings, file-scoped namespaces, C# 12+ features (primary constructors, collection expressions),
- TypeScript: 2-space indent
- Line endings: LF (`.editorconfig` at root)
- TFM: `net10.0` (preview SDK)
- SQL table names: plural form of the entity class name (`DayOff` → `DaysOff`), matching EF Core `DbSet<T>` convention; applies to explicit `builder.ToTable(...)` calls too
