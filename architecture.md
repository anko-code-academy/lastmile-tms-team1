# LastMile TMS - Application Architecture

> GraphQL | Clean Architecture | .NET 10
>
> April 6, 2026

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Backend Architecture](#2-backend-architecture)
3. [Frontend Architecture (Web)](#3-frontend-architecture-web)
4. [Infrastructure & Deployment](#4-infrastructure--deployment)
5. [Technology Stack](#5-technology-stack)
6. [Known Issues & Inconsistencies](#6-known-issues--inconsistencies)

---

## 1. Architecture Overview

Monorepo with two apps: **backend** (.NET API), **web** (Next.js dispatcher UI).

- GraphQL is the primary domain API
- MediatR handles commands (mutations) only
- All reads are served by HotChocolate directly through `IAppDbContext`
- No external mapping libraries -- all object construction is inline

---

## 2. Backend Architecture

### 2.1 Solution Layout

```
src/backend/
  src/
    LastMile.TMS.Api/              -- Transport, GraphQL schema, composition root
    LastMile.TMS.Application/      -- Commands, validators, DTOs
    LastMile.TMS.Domain/           -- Entities, enums, domain services, business rules
    LastMile.TMS.Infrastructure/   -- External services & adapters
    LastMile.TMS.Persistence/      -- EF Core DbContext, configurations, migrations
  tests/
    LastMile.TMS.Api.Tests/              -- GraphQL & REST contract tests
    LastMile.TMS.Api.IntegrationTests/   -- Integration tests (WebApplicationFactory + Testcontainers)
    LastMile.TMS.Application.Tests/      -- Handlers, validators
    LastMile.TMS.Architecture.Tests/     -- Dependency & convention rules
    LastMile.TMS.Domain.Tests/           -- Pure domain behavior

src/web/                            -- Next.js 16 dispatcher UI (App Router)
```

### 2.2 Dependency Rules

```
Api ────────> Application ────────> Domain (no dependencies)
 │                 ^                    ^
 ├──> Infrastructure ───────────────────┘
 └──> Persistence ──────────────────────┘
```

- **Api** depends on Application for commands/DTOs and on `IAppDbContext` for query resolvers
- **Application** depends on abstractions (`IAppDbContext`, `ICurrentUserService`), not transport or persistence
- **Infrastructure** and **Persistence** implement inward-facing abstractions
- **Domain** has no project dependencies (framework-independent)

### 2.3 Architecture Tests

Enforced by `ArchitectureTests.cs`:

| Layer | Must NOT reference |
|---|---|
| Domain | Application, Infrastructure, Persistence, Api |
| Application | Infrastructure, Persistence, Api |
| Infrastructure | Persistence, Api |
| Persistence | Infrastructure, Api |

### 2.4 Request Flows

#### Mutations (Commands via MediatR)

```
GraphQL mutation
  -> resolver in *Mutations.cs
  -> constructs DTO inline from input parameters
  -> IMediator.Send(command)
  -> ValidationBehavior<TRequest, TResponse> (FluentValidation)
  -> *CommandHandler.Handle()
  -> constructs/updates Entity inline from DTO
  -> IAppDbContext persists to PostgreSQL
  -> Entity/DTO returned, serialized via *Types.cs
```

#### Queries (DbContext via HotChocolate)

```
GraphQL query
  -> resolver in *Queries.cs
  -> accesses AppDbContext directly
  -> returns IQueryable<TEntity>
  -> HotChocolate [UseProjection] / [UseFiltering] / [UseSorting]
  -> EF Core translates to SQL
  -> response shaped by EntityObjectType<T>
```

**Key rules for queries:**
- All reads go through `IAppDbContext` in resolvers -- no MediatR, no read services
- Resolvers return `IQueryable<TEntity>` and let HotChocolate drive SQL shaping
- No `.Select()` DTO projection, no `.Include()` in query resolvers

#### Object Construction

No mapping libraries. All construction is inline.

**Resolver -> Command (Input to DTO):**

```csharp
var result = await mediator.Send(new CreateDepotCommand(
    input.Name,
    input.Address,
    input.OperatingHours,
    input.IsActive));
```

**Handler -> Entity (DTO to Entity):**

```csharp
var depot = new Depot
{
    Name = command.Name,
    IsActive = command.IsActive,
    Address = new Address { Street1 = command.Address.Street1, ... }
};
context.Depots.Add(depot);
```

**Handler -> Update (DTO to existing Entity):**

```csharp
depot.Name = command.Name;
depot.IsActive = command.IsActive;
depot.Address.Street1 = command.Address.Street1;
```

### 2.5 Layers

#### Api Layer

Transport and composition root. `Program.cs` wires DI, configures OpenIddict, Hangfire, Serilog, and applies migrations at startup.

**Feature file structure:**

```
Api/GraphQL/Extensions/<Feature>/
  *Queries.cs     -- Query resolvers (AppDbContext + IQueryable)
  *Mutations.cs   -- Mutation resolvers (IMediator + inline DTO construction)
  *Types.cs       -- Output types, filter types, sort types (optional)
Api/GraphQL/Inputs/
  *Inputs.cs      -- GraphQL input types (when needed)
Api/GraphQL/Common/
  Query.cs                      -- Root query type
  Mutation.cs                   -- Root mutation type
  ErrorFilter.cs                -- General error filter (logs + trace IDs)
  DomainExceptionErrorFilter.cs -- Domain exception translation (ValidationException, KeyNotFoundException, etc.)
```

**Authorization patterns:**

| Role | Access |
|---|---|
| Admin | All domains, user management |
| Operations Manager | Depots, Zones, Vehicles, Routes, Drivers |
| Warehouse Operator | Parcels (create, view) |
| Dispatcher | Routes (view) |

**Rules:**
- Query resolvers access `AppDbContext` directly, return `IQueryable<TEntity>`
- Mutation resolvers construct DTOs inline, dispatch via `IMediator`
- Middleware attributes processed bottom-to-top by HotChocolate
- Omit files a feature doesn't need

#### Application Layer

Commands only. No query handlers, no read services.

```
Application/<Feature>/
  Commands/
    <UseCase>/
      *Command.cs           -- MediatR request record
      *CommandHandler.cs    -- Inline entity construction, persistence
      *CommandValidator.cs  -- FluentValidation (optional)
  Common/
    Behaviors/ValidationBehavior.cs
    Interfaces/IAppDbContext.cs
    Interfaces/ICurrentUserService.cs
    Interfaces/IDbSeeder.cs
    Interfaces/IEmailService.cs
    Interfaces/IGeocodingService.cs
    Interfaces/ILabelService.cs
    Interfaces/ITokenRevocationService.cs
```

**Standard command shapes:**

```csharp
record CreateXCommand(...) : IRequest<XResult>;
record UpdateXCommand(Guid Id, ...) : IRequest<XResult>;
record DeleteXCommand(Guid Id) : IRequest<bool>;
record ChangeXStatusCommand(Guid Id, XStatus NewStatus) : IRequest<XDto>;
```

**Pipeline behavior:**

```
ValidationBehavior<TRequest, TResponse>
  -> resolves all IValidator<T> for the request
  -> runs validators in parallel
  -> throws ValidationException on failure
  -> otherwise delegates to next handler
```

#### Domain Layer

Framework-independent. No MediatR, no EF Core, no GraphQL.

```
Domain/
  Common/
    BaseEntity.cs              -- Id (Guid, UUIDv7)
    BaseAuditableEntity.cs     -- CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy, IsDeleted, DeletedAt, DeletedBy
    IBaseAuditableEntity.cs
    IDomainEvent.cs
    IHasDomainEvents.cs
    OperatingHours.cs
  Entities/
    User.cs, Role.cs, Permission.cs, RolePermission.cs
    Depot.cs, Address.cs, Zone.cs
    Driver.cs, ShiftSchedule.cs, DayOff.cs
    Vehicle.cs, VehicleJourney.cs
    Route.cs
    Parcel.cs, ParcelContentItem.cs, ParcelWatcher.cs
    DeliveryConfirmation.cs, TrackingEvent.cs
  Enums/
    ParcelStatus, RouteStatus, VehicleStatus, VehicleType
    ServiceType, ParcelType
    WeightUnit, DimensionUnit
    UserStatus, EventType, ExceptionReason
    PermissionModule, PermissionScope
  Services/
    DeliveryDateCalculator.cs   -- IDeliveryDateCalculator singleton
```

**Entity base hierarchy:**

```
BaseEntity
  └── BaseAuditableEntity
        ├── Depot, Zone, Vehicle, Route, Parcel, Driver
        ├── ShiftSchedule, DayOff, VehicleJourney
        ├── Address, DeliveryConfirmation, TrackingEvent
        └── ParcelContentItem, ParcelWatcher

IdentityUser<Guid> + IBaseAuditableEntity
  └── User

IdentityRole<Guid> + IBaseAuditableEntity
  └── Role
```

**Domain behavior:**
- `Parcel` -- state machine (`CanTransitionTo`/`TransitionTo`), factory method `Create()`, tracking number generation (`LM-yyMMdd-XXXXXX`)
- `Route` -- state machine (Planned -> InProgress -> Completed/Cancelled), timestamps `ActualStartTime`/`ActualEndTime`
- `Vehicle` -- state machine (Available -> InUse/Maintenance/Retired), `AssignToRoute(parcelCount)`, `ReleaseFromRoute()`, `Update()` factory
- `User` -- `Activate()`/`Deactivate()`/`Suspend()`, `Create()` factory with email validation, `UpdateName/Phone/Email`, `IsSystemAdmin` protection
- `Zone` -- `SetBoundaryFromGeoJson()` parses GeoJSON Polygon via NetTopologySuite
- `Role` -- static factory methods for all five standard roles

#### Persistence Layer

```
Persistence/
  AppDbContext.cs            -- IdentityDbContext<User, Role, Guid>, DbSets, audit stamping on SaveChanges, soft-delete query filter
  DependencyInjection.cs     -- DbContext, Identity, OpenIddict registration
  Configurations/            -- One IEntityTypeConfiguration<T> per entity (auto-discovered)
  Migrations/                -- 24+ migration files
```

**DbContext features:**
- Soft delete: global query filter `HasQueryFilter(e => !e.IsDeleted)` on all auditable entities
- Audit stamping: `SaveChangesAsync` auto-sets `CreatedAt`/`CreatedBy`/`LastModifiedAt`/`LastModifiedBy`/`DeletedAt`/`DeletedBy`
- Spatial: PostGIS extension + NetTopologySuite for zone boundaries and address geometries
- Full-text search: NpgsqlTsVector columns on Parcel (`RecipientNameSearchVector`, `AddressSearchVector`) for PostgreSQL text search

#### Infrastructure Layer

```
Infrastructure/
  DependencyInjection.cs
  Services/
    CurrentUserService.cs           -- JWT claim extraction (UserId, UserName from HttpContext)
    NominatimGeocodingService.cs    -- Address -> Point (lat/lon, SRID 4326)
    SendGridEmailService.cs         -- Password reset + user created emails
    TokenRevocationService.cs       -- Revokes OpenIddict tokens on user deactivation
    LabelService.cs                 -- QuestPDF labels, ZXing barcodes/QR codes, ZPL
    DbSeeder.cs                     -- IHostedService: seeds roles, admin user, permissions
  Options/
    EmailOptions.cs
    TestingOptions.cs
```

### 2.6 Feature Domains

#### Depots

| Layer | File | Purpose |
|---|---|---|
| **API** | `DepotQueries.cs` | `GetDepots` (paginated list), `GetDepot` (by ID) via AppDbContext |
| **API** | `DepotMutations.cs` | `CreateDepot`, `UpdateDepot`, `DeleteDepot` |
| **API** | `DepotTypes.cs` | `DepotType`, `AddressType`, filter/sort types |
| **APP** | `Commands/CreateDepot/*` | Command + Handler (geocodes address, defaults shift schedules to Mon-Fri 9-17) + Validator |
| **APP** | `Commands/UpdateDepot/*` | Command + Handler (syncs shift schedules: add/update/remove) + Validator |
| **APP** | `Commands/DeleteDepot/*` | Command + Handler (soft delete) |
| **DOM** | `Entities/Depot.cs` | Name, Address (FK), IsActive, Zones, ShiftSchedules |
| **DOM** | `Entities/Address.cs` | Street1/2, City, State, PostalCode, CountryCode, Geometry, contact info |
| **PERS** | `DepotConfiguration.cs` | EF Core FluentAPI, owns Address |

#### Drivers

| Layer | File | Purpose |
|---|---|---|
| **API** | `DriverQueries.cs` | `GetDrivers` (paginated list), `GetDriver` (by ID) via AppDbContext |
| **API** | `DriverMutations.cs` | `CreateDriver`, `UpdateDriver`, `DeleteDriver` |
| **API** | `DriverTypes.cs` | `DriverType` output type |
| **API** | `Inputs/DriverInputs.cs` | `CreateDriverInput`, `UpdateDriverInput`, `ShiftScheduleInput`, `DayOffInput` |
| **APP** | `Commands/CreateDriver/*` | Command + Handler (resolves User by email, validates Driver role) + Validator |
| **APP** | `Commands/UpdateDriver/*` | Command + Handler (syncs shift schedules + days off) + Validator |
| **APP** | `Commands/DeleteDriver/*` | Command + Handler (soft delete) |
| **DOM** | `Entities/Driver.cs` | LicenseNumber, LicenseExpiryDate, Photo, UserId (FK to User), ShiftSchedules, DaysOff |
| **DOM** | `Entities/ShiftSchedule.cs` | DayOfWeek, OpenTime, CloseTime (linked to Depot or Driver) |
| **DOM** | `Entities/DayOff.cs` | Date, Reason, Type (Full/Half) |
| **PERS** | `DriverConfiguration.cs` | EF Core FluentAPI |

**Driver creation rules:**
- Email must correspond to an existing User with the Driver role
- User must not already have a Driver profile
- License expiry date must be in the future

#### Parcels

| Layer | File | Purpose |
|---|---|---|
| **API** | `ParcelQueries.cs` | `GetParcels` (paginated, full-text search on recipient + address), `GetParcel` (by ID), `GetParcelByTrackingNumber` |
| **API** | `ParcelMutations.cs` | `CreateParcel` |
| **API** | `Inputs/ParcelInputs.cs` | `CreateParcelInput`, `ParcelAddressInput` |
| **APP** | `Commands/CreateParcel/*` | Command + Handler (geocodes both addresses, auto-assigns zone via geocoding, calculates delivery date) + Validator |
| **DOM** | `Entities/Parcel.cs` | TrackingNumber (auto-generated `LM-yyMMdd-XXXXXX`), Status (state machine), ServiceType, ShipperAddress, RecipientAddress, Weight, Dimensions, ZoneId |
| **DOM** | `Entities/ParcelContentItem.cs` | Description, Quantity, Value |
| **DOM** | `Entities/TrackingEvent.cs` | EventType, Description, Location, Timestamp |
| **DOM** | `Entities/DeliveryConfirmation.cs` | SignaturePhoto, ConfirmedBy, DeliveredAt |
| **PERS** | `ParcelConfiguration.cs` | EF Core FluentAPI, tsvector columns for search |

**Parcel status state machine:**

```
Registered -> ReceivedAtDepot -> Sorted -> Staged -> Loaded -> OutForDelivery -> Delivered
                                                                      |              |
                                                                      v              v
                                                                FailedAttempt -----+
                                                                      |
                                                                      v
                                                              ReturnedToDepot
```

#### Routes

| Layer | File | Purpose |
|---|---|---|
| **API** | `RouteQueries.cs` | `GetRoutes` (list, no paging), `GetRoute` (by ID) via AppDbContext |
| **API** | `RouteMutations.cs` | `CreateRoute`, `UpdateRoute`, `DeleteRoute`, `ChangeRouteStatus` |
| **APP** | `Commands/CreateRoute/*` | Command + Handler (validates vehicle not retired) + Validator |
| **APP** | `Commands/UpdateRoute/*` | Command + Handler (vehicle assignment for InProgress routes) + Validator |
| **APP** | `Commands/DeleteRoute/*` | Command + Handler |
| **APP** | `Commands/ChangeRouteStatus/*` | Command + Handler (manages VehicleJourney, assigns/releases vehicle) |
| **DOM** | `Entities/Route.cs` | Name, Status (state machine), PlannedStartTime, ActualStartTime/EndTime, TotalDistanceKm, TotalParcelCount, VehicleId |
| **DOM** | `Entities/VehicleJourney.cs` | VehicleId, RouteId, StartMileageKm, EndMileageKm, DistanceKm (computed) |
| **PERS** | `RouteConfiguration.cs` | EF Core FluentAPI |

**Route status state machine:**

```
Planned -> InProgress -> Completed
   |          |
   v          v
Cancelled  Cancelled
```

#### Users

> Most complex domain -- 6 commands

| Layer | File | Purpose |
|---|---|---|
| **API** | `UserManagementQueries.cs` | `GetUsers` (paginated, Admin only), `GetUser` (by ID), `GetUserManagementLookups` (roles, active depots, active zones) |
| **API** | `UserManagementMutations.cs` | `CreateUser`, `UpdateUser`, `DeactivateUser`, `ActivateUser`, `ResetPassword`, `CompletePasswordReset` |
| **API** | `UserManagementTypes.cs` | `UserManagementUserType`, `UserRoleType` |
| **APP** | `Commands/CreateUser/*` | Creates user via Identity UserManager, assigns role |
| **APP** | `Commands/UpdateUser/*` | Updates profile, role (with Identity sync), zone/depot assignments |
| **APP** | `Commands/DeactivateUser/*` | Sets status to Inactive, immediate lockout, revokes tokens (protects SystemAdmin) |
| **APP** | `Commands/ActivateUser/*` | Sets status to Active, removes lockout (protects SystemAdmin) |
| **APP** | `Commands/ResetPassword/*` | Generates reset token, sends email (always returns true for security) |
| **APP** | `Commands/CompletePasswordReset/*` | Validates token, resets password |
| **APP** | `Common/SystemAdminProtection.cs` | `ThrowIfSystemAdmin(User)` -- prevents modification of system admin |
| **APP** | `Validation/UserManagementRules.cs` | Validates depot/zone assignment consistency |
| **DOM** | `Entities/User.cs` | Extends IdentityUser: FirstName, LastName, IsActive, IsSystemAdmin, Status, DepotId, ZoneId, RoleId |
| **DOM** | `Entities/Role.cs` | Extends IdentityRole: Description, 5 static roles (Admin, OperationsManager, Dispatcher, WarehouseOperator, Driver) |
| **DOM** | `Entities/Permission.cs` | Name, Module, Scope |
| **DOM** | `Entities/RolePermission.cs` | RoleId, PermissionId |
| **PERS** | `ApplicationUserConfiguration.cs` | EF Core FluentAPI |

**Standard roles:**

| Role | Description |
|---|---|
| Admin | Full system access |
| Operations Manager | Manage operations and dispatch |
| Dispatcher | Assign and track deliveries |
| Warehouse Operator | Manage parcel intake and sorting |
| Driver | Deliver parcels to customers |

#### Vehicles

| Layer | File | Purpose |
|---|---|---|
| **API** | `VehicleQueries.cs` | `GetVehicles` (list, no paging), `GetVehicle` (by ID), `GetVehicleHistory` (aggregated mileage + route history) |
| **API** | `VehicleMutations.cs` | `CreateVehicle`, `UpdateVehicle`, `DeleteVehicle`, `ChangeVehicleStatus` |
| **APP** | `Commands/CreateVehicle/*` | Command + Handler (factory method) + Validator |
| **APP** | `Commands/UpdateVehicle/*` | Command + Handler (domain Update method) + Validator |
| **APP** | `Commands/DeleteVehicle/*` | Command + Handler (hard delete) |
| **APP** | `Commands/ChangeVehicleStatus/*` | Command + Handler (domain TransitionTo method) |
| **DOM** | `Entities/Vehicle.cs` | RegistrationPlate, Type, ParcelCapacity, WeightCapacityKg, Status (state machine), DepotId |
| **PERS** | `VehicleConfiguration.cs` | EF Core FluentAPI |

**Vehicle status state machine:**

```
Available <-> InUse
   |    ^      |
   v    |      v
Maintenance    Retired
```

#### Zones

| Layer | File | Purpose |
|---|---|---|
| **API** | `ZoneQueries.cs` | `GetZones` (paginated list), `GetZone` (by ID) via AppDbContext |
| **API** | `ZoneMutations.cs` | `CreateZone`, `UpdateZone`, `DeleteZone` |
| **API** | `ZoneTypes.cs` | `ZoneType` output type, filter/sort types |
| **APP** | `Commands/CreateZone/*` | Command + Handler (validates depot, parses GeoJSON -> Polygon) + Validator |
| **APP** | `Commands/UpdateZone/*` | Command + Handler + Validator |
| **APP** | `Commands/DeleteZone/*` | Command + Handler (hard delete) |
| **DOM** | `Entities/Zone.cs` | Name, BoundaryGeometry (PostGIS Polygon), IsActive, DepotId |
| **PERS** | `ZoneConfiguration.cs` | EF Core FluentAPI, spatial index |

### 2.7 Dependency Injection

```csharp
builder.Services
    .AddApplication()       // MediatR (commands only), FluentValidation, ValidationBehavior, DeliveryDateCalculator
    .AddInfrastructure()    // CurrentUser, geocoding, email, label, token revocation, SendGrid, OpenIddict server
    .AddPersistence()       // DbContext (PostgreSQL), Identity, OpenIddict core, DbSeeder
    .AddIdentity<User, Role>()  // Password policy, unique emails
    .AddOpenIddict()        // Password + refresh token flow, JWT validation
    .AddLastMileApi();      // HotChocolate GraphQL server, all Types/Queries/Mutations, error filters, Hangfire, Redis
```

| Layer | Registers |
|---|---|
| **Application** | MediatR (commands only), FluentValidation validators (assembly scan), `ValidationBehavior`, `IDeliveryDateCalculator` (singleton) |
| **Infrastructure** | `ICurrentUserService`, `IGeocodingService` (Nominatim), `IEmailService` (SendGrid), `ILabelService` (QuestPDF + ZXing), `ITokenRevocationService`, OpenIddict server |
| **Persistence** | `AppDbContext` (PostgreSQL + PostGIS), ASP.NET Identity stores, OpenIddict core (EF), `IDbSeeder`, global soft-delete filter |
| **Api** | HotChocolate GraphQL server (projections, filtering, sorting, spatial), all domain Query/Mutation/Type extensions, `ErrorFilter`, `DomainExceptionErrorFilter`, Hangfire (PostgreSQL storage), Redis cache, Serilog, SignalR, Swagger |

---

## 3. Frontend Architecture (Web)

### 3.1 Data Flow

```
src/graphql/documents/<feature>.graphql    -- Query/mutation definitions
        | (npm run codegen)
        v
src/graphql/generated/index.ts             -- Generated TS types + TypedDocumentNode
        |
        v
src/services/<feature>.service.ts          -- GraphQL call wrappers (graphql-request)
        |
        v
src/hooks/use-<feature>.ts                 -- TanStack Query hooks
        |
        v
src/components/<feature>/*.tsx             -- React components
```

**Note:** The codegen pipeline (`npm run codegen`) generates types from `.graphql` documents, but the current codebase uses hand-written types in `src/lib/graphql/types.ts` and `src/types/`. See section 6.9 for details.

### 3.2 GraphQL Operations

| Domain | Queries | Mutations |
|---|---|---|
| Depots | GetDepots, GetDepot | CreateDepot, UpdateDepot, DeleteDepot |
| Drivers | GetDrivers, GetDriver | CreateDriver, UpdateDriver, DeleteDriver |
| Parcels | GetParcels, GetParcel, GetParcelByTrackingNumber | CreateParcel |
| Routes | GetRoutes, GetRoute | CreateRoute, UpdateRoute, DeleteRoute, ChangeRouteStatus |
| Users | GetUsers, GetUser, GetUserManagementLookups | CreateUser, UpdateUser, ActivateUser, DeactivateUser, ResetPassword, CompletePasswordReset |
| Vehicles | GetVehicles, GetVehicle, GetVehicleHistory | CreateVehicle, UpdateVehicle, DeleteVehicle, ChangeVehicleStatus |
| Zones | GetZones, GetZone | CreateZone, UpdateZone, DeleteZone |

### 3.3 Routing Structure

```
app/
  page.tsx                           -- Root redirect
  layout.tsx                         -- Root layout
  (auth)/
    login/page.tsx                   -- Login page
  (protected)/
    layout.tsx                       -- Auth-guarded layout with sidebar
    dashboard/page.tsx               -- Dashboard
    depots/
      page.tsx                       -- Depot list
      new/page.tsx                   -- Create depot
      [id]/page.tsx                  -- Depot detail
    drivers/
      page.tsx                       -- Driver list
      new/page.tsx                   -- Create driver
      [id]/page.tsx                  -- Driver detail
    parcels/
      page.tsx                       -- Parcel list
      new/page.tsx                   -- Create parcel
      [trackingNumber]/page.tsx      -- Parcel detail by tracking number
    routes/
      page.tsx                       -- Route list
      new/page.tsx                   -- Create route
      [id]/page.tsx                  -- Route detail
      [id]/edit/page.tsx             -- Edit route
    users/
      page.tsx                       -- User list
    vehicles/
      page.tsx                       -- Vehicle list
      new/page.tsx                   -- Create vehicle
      [id]/page.tsx                  -- Vehicle detail
      [id]/edit/page.tsx             -- Edit vehicle
    zones/
      page.tsx                       -- Zone list
      new/page.tsx                   -- Create zone
      [id]/page.tsx                  -- Zone detail
```

### 3.4 Key Patterns

- **Auth**: NextAuth v5 with credentials provider (OAuth2 password grant to OpenIddict `/connect/token`), JWT strategy with 7-day max age, automatic token refresh
- **Query key factories** for structured cache management
- **Auth gating** via `enabled: !!session`
- **Cascade invalidation** on mutations (e.g., creating a route invalidates vehicles + parcels)
- **Toast metadata** on mutations for automatic notifications (sonner)
- **Mock data** via `NEXT_PUBLIC_USE_MOCK_DATA=true` -- lazy-loaded, operation-name-based response mapping
- **Components never call services directly** -- always through TanStack Query hooks
- **Forms**: react-hook-form + zod validation
- **Maps**: Mapbox GL + `@mapbox/mapbox-gl-draw` for zone boundary editing
- **Real-time**: SignalR via `@microsoft/signalr`
- **Tables**: `@tanstack/react-table` headless components
- **Charts**: recharts for dashboard
- **UI**: shadcn/ui + radix-ui primitives + lucide-react icons

---

## 4. Infrastructure & Deployment

### 4.1 Docker Compose

| Service | Image | Purpose | Port |
|---|---|---|---|
| **caddy** | `caddy:2-alpine` | Reverse proxy, routing | 80 |
| **api** | Custom (.NET 10) | Backend API | 8080 (internal) |
| **web** | Custom (Node 20) | Next.js dispatcher UI | 3000 (internal) |
| **postgres** | `postgis/postgis:17-3.5` | Database + spatial | 5432 |
| **redis** | `redis:7-alpine` | Cache | 6379 (internal) |
| **pgbouncer** | `edoburu/pgbouncer` | Connection pooling (transaction mode, 200 max clients) | -- |
| **seq** | `datalust/seq` | Structured log aggregation | 80 (via caddy) |
| **pgadmin** | `dpage/pgadmin4` | DB admin (dev only, `COMPOSE_PROFILES=dev`) | 5050 |

### 4.2 Caddy Routing

| Path | Target |
|---|---|
| `/api/auth/*` | Next.js (NextAuth) |
| `/api/*` | .NET API (strips prefix) |
| `/connect/*` | .NET API (OpenIddict) |
| `/hubs/*` | .NET API (SignalR) |
| `/swagger/*` | .NET API (Swagger UI, dev only) |
| `/hangfire*` | .NET API (Hangfire dashboard) |
| `/graphql*` | .NET API (HotChocolate) |
| `/seq/*` | Seq (strips prefix) |
| Default | Next.js |

### 4.3 CI/CD (GitHub Actions)

| Workflow | Trigger | Steps |
|---|---|---|
| **backend.yml** | Push/PR to main (backend changes) | .NET 10 restore, build, test (with PostgreSQL service) |
| **web.yml** | Push/PR to main (web changes) | Node 20 install, lint, build |

---

## 5. Technology Stack

### Backend

| Concern | Technology |
|---|---|
| Runtime | .NET 10 (`net10.0`) |
| GraphQL | HotChocolate 16 (projection, filtering, sorting, spatial) |
| ORM | Entity Framework Core 10 + Npgsql + PostgreSQL 17 + PostGIS 3.5 |
| Commands | MediatR 14 |
| Validation | FluentValidation 12 (pipeline behavior) |
| Auth | OpenIddict 7 (OAuth2 password + refresh token) + ASP.NET Identity |
| Background Jobs | Hangfire (PostgreSQL storage) |
| Cache | Redis 7 (StackExchange) |
| Spatial | NetTopologySuite + PostGIS |
| Geocoding | Nominatim API |
| PDF/Labels | QuestPDF, ZXing.Net, SkiaSharp |
| Email | SendGrid |
| SMS | Twilio |
| Logging | Serilog (console + Seq) |

### Web Frontend

| Concern | Technology |
|---|---|
| Framework | Next.js 16 (App Router) |
| Language | TypeScript |
| Auth | NextAuth v5 (credentials, JWT) |
| Data Fetching | TanStack Query 5, graphql-request |
| GraphQL | graphql + @graphql-codegen |
| Forms | react-hook-form + zod |
| UI | shadcn/ui, radix-ui, lucide-react |
| Maps | Mapbox GL + mapbox-gl-draw |
| Charts | recharts |
| Tables | @tanstack/react-table |
| Notifications | sonner |
| Real-time | @microsoft/signalr |
| Testing | Playwright (E2E), Vitest (unit) |

### Infrastructure

| Concern | Technology |
|---|---|
| Containers | Docker Compose |
| Reverse Proxy | Caddy 2 |
| Database | PostgreSQL 17 + PostGIS 3.5 |
| Connection Pooling | PgBouncer |
| Cache | Redis 7 |
| Logging | Seq |
| CI/CD | GitHub Actions |

---

## 6. Known Issues & Inconsistencies

### 6.1 `IAppDbContext` missing `Permission` and `RolePermission` DbSets

**Location**: `src/backend/src/LastMile.TMS.Application/Common/Interfaces/IAppDbContext.cs`

`AppDbContext` exposes `DbSet<Permission>` and `DbSet<RolePermission>` (`AppDbContext.cs:27-28`), but the `IAppDbContext` interface does not declare them. This breaks the abstraction -- code that depends on `IAppDbContext` cannot access permissions. Query resolvers currently use `AppDbContext` directly, so this works at runtime but is a layering violation.

**Fix**: Add `DbSet<Permission> Permissions { get; }` and `DbSet<RolePermission> RolePermissions { get; }` to `IAppDbContext`.

---

### 6.2 Missing `[UsePaging]` on `GetRoutes` and `GetVehicles`

**Location**: `RouteQuery.cs:13-19`, `VehicleQuery.cs:15-19`

Five other collection queries (Depots, Zones, Drivers, Parcels, Users) all have `[UsePaging(IncludeTotalCount = true)]`. Routes and Vehicles do not. This means:
- Unbounded result sets (performance risk at scale)
- The frontend must handle different response shapes for Routes/Vehicles vs. other domains

**Fix**: Add `[UsePaging(IncludeTotalCount = true)]` to `GetRoutes` and `GetVehicles`. Note: this changes the GraphQL response shape from `[Route]` to a connection type `{ edges { node } pageInfo { ... } totalCount }` and will require corresponding frontend changes in the services and query hooks.

---

### 6.3 Parcel table name is singular

**Location**: `src/backend/src/LastMile.TMS.Persistence/Configurations/ParcelConfiguration.cs:12`

```csharp
builder.ToTable("Parcel");
```

All other entity configurations use plural form: `Drivers`, `Depots`, `Zones`, `Roles`, `Users`, `ShiftSchedules`, `DaysOff`, `Addresses`, `Permissions`, `RolePermissions`. This violates the project convention documented in `CLAUDE.md`: *"SQL table names: plural form of the entity class name"*.

**Fix**: Change to `builder.ToTable("Parcels")`. Requires a new EF Core migration to rename the table.

---

### 6.4 Missing `ToTable()` calls in 7 configurations

**Location**: `src/backend/src/LastMile.TMS.Persistence/Configurations/`

The following configurations do not call `builder.ToTable(...)`:
- `VehicleConfiguration.cs`
- `RouteConfiguration.cs`
- `VehicleJourneyConfiguration.cs`
- `DeliveryConfirmationConfiguration.cs`
- `ParcelContentItemConfiguration.cs`
- `ParcelWatcherConfiguration.cs`
- `TrackingEventConfiguration.cs`

EF Core defaults to the DbSet property name (pluralized), which is correct, but inconsistent with 11 other configurations that explicitly call `ToTable()`. This makes the convention ambiguous for future developers.

**Fix**: Add explicit `ToTable()` calls to all 7 configurations to match the established pattern.

---

### 6.5 Missing `parcels.graphql` document

**Location**: `src/web/src/graphql/documents/`

Every other domain has a `.graphql` document file for code generation. Parcels does not. The parcel frontend uses dynamic query building instead, bypassing the GraphQL code-generation pipeline used by all other domains.

**Fix**: Create `src/web/src/graphql/documents/parcels.graphql` with the parcel operations and regenerate types.

---

### 6.6 `IDeliveryDateCalculator` registered in Application DI

**Location**: `src/backend/src/LastMile.TMS.Application/DependencyInjection.cs:17`

```csharp
services.AddSingleton<Domain.Services.IDeliveryDateCalculator, Domain.Services.DeliveryDateCalculator>();
```

Both the interface and implementation live in `Domain/Services/DeliveryDateCalculator.cs`, but the registration is in the Application layer's `DependencyInjection.cs`. This creates a dependency from Application to concrete Domain types (rather than just abstractions). Minor layering violation.

**Fix**: Either move the interface to `Application/Common/Interfaces/` (and keep implementation in Infrastructure), or move the registration to `Infrastructure/DependencyInjection.cs`.

---

### 6.7 Inconsistent mutation parameter style

**Location**: `RouteMutation.cs`, `VehicleMutation.cs` vs. `DepotMutation.cs`, `ZoneMutation.cs`, `DriverMutation.cs`

Routes and Vehicles accept individual scalar parameters in mutations:
```csharp
public async Task<RouteDto> CreateRoute(string name, DateTime plannedStartTime, ...)
```

Depots, Zones, and Drivers accept structured input types:
```csharp
public async Task<DepotResult> CreateDepot(CreateDepotInput input, ...)
```

Both approaches work and HotChocolate handles them correctly, but the inconsistency means:
- Different calling conventions in the GraphQL schema
- `AddLastMileApi()` registers input types for some domains but not others

**Fix**: Add input types for Routes/Vehicles to match the rest of the codebase. The structured input-type pattern (`CreateDepotInput`) is preferred over positional scalar parameters because it eliminates argument-ordering mistakes -- callers identify fields by name, not position. This is especially important for mutations with 3+ parameters where similar-typed arguments (e.g., multiple `string` or `DateTime` fields) are easy to swap. Frontend updates required for Routes/Vehicles.

---

### 6.8 No explicit GraphQL output types for Routes and Vehicles

**Location**: `src/backend/src/LastMile.TMS.Api/GraphQL/Extensions/Route/`, `Vehicle/`

Depots, Zones, and Drivers define explicit `EntityObjectType<T>` subclasses (e.g., `DepotType`, `ZoneType`, `DriverType`) for fine-grained control over field exposure, filtering, and sorting. Routes and Vehicles have no `*Type.cs` files -- they rely entirely on HotChocolate schema inference from their DTO classes.

**Fix**: Add `RouteType.cs` and `VehicleType.cs` with explicit field configurations, filter types, and sort types. Lower priority since schema inference works correctly for simple DTOs.

---

### 6.9 GraphQL codegen output is unused -- all types are hand-written

**Location**: `src/web/src/graphql/generated/` (4 generated files), `src/web/src/graphql/documents/` (6 `.graphql` files)

The project has a full GraphQL code-generation pipeline (`@graphql-codegen/cli` + `client-preset`) configured to produce TypeScript types from `.graphql` documents. The generated files exist in `src/graphql/generated/` and export `TypedDocumentNode`-based types. However, **nothing in the codebase imports from them** -- zero references across all `.ts`/`.tsx` files.

Instead, every service and component uses hand-written types from three locations:
- `src/web/src/lib/graphql/types.ts` -- Depots, Zones, Drivers, Parcels (DTOs, enums, inputs)
- `src/web/src/types/vehicle.ts` -- Vehicles
- `src/web/src/types/route.ts` -- Routes
- `src/web/src/types/user.ts` -- Users
- `src/web/src/types/parcel.ts` -- Parcels

Services also use raw template-string queries (not the `gql` tag or `TypedDocumentNode`) and call `apiFetch` directly rather than a generated GraphQL client. The `.graphql` document files in `src/graphql/documents/` are similarly disconnected from the runtime code.

**Fix**: Either wire up the codegen pipeline properly (use generated types, `gql` tag, and `TypedDocumentNode` in services + TanStack Query), or remove the dead codegen infrastructure (`generated/`, `documents/`, `@graphql-codegen` deps, `npm run codegen` script) and rely on the hand-written types consistently.

**Note on dynamic queries (parcels list)**: The parcels page has a column picker that lets users toggle ~28 columns on/off, producing a dynamic GraphQL selection set at runtime. This is handled by `build-parcels-query.ts` + `column-registry.ts`, which construct the query string from the selected column's `graphqlFields` mappings.

GraphQL codegen requires **static** `.graphql` documents known at build time -- it cannot type queries that don't exist until runtime. The standard codegen-compatible alternative is `@include`/`@skip` directives:

```graphql
query GetParcels($showStatus: Boolean!, $showWeight: Boolean!, ...) {
  parcels { nodes {
    id
    status @include(if: $showStatus)
    weight @include(if: $showWeight)
  } }
}
```

This works with codegen but is a poor fit for column pickers:
- **Verbose**: 28 columns means 28+ `@include` directives and boolean variables
- **Nested fields**: Columns like `recipientCity` come from `recipientAddress { city }` -- you'd need per-sub-field booleans or include the entire parent object
- **No real type gain**: Every field becomes optional in the generated type, identical to the current hand-written `ParcelSummaryDto`

The dynamic builder approach (`build-parcels-query.ts`) is the preferred solution when dealing with runtime column selection. Codegen is better suited for operations with fixed selection sets (detail views, create/update mutations, simple lists). A hybrid approach is recommended: codegen for static operations, dynamic builders for column-pickable lists.
