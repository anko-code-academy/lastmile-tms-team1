# Plan: Route Management UI (LMTT1-XX)

## Context
Vehicle Management (LMTT1-15) requires assigning vehicles to routes. Backend has Route entity but no GraphQL layer yet.

## Backend Requirements (prerequisite for UI)

### 1. Create RouteDto
**File:** `src/backend/src/LastMile.TMS.Application/Features/Routes/RouteDto.cs`
```csharp
public class RouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RouteStatus Status { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public int TotalParcelCount { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehiclePlate { get; set; } // for display
}

public class RouteSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RouteStatus Status { get; set; }
    public DateTime PlannedStartTime { get; set; }
    public Guid? VehicleId { get; set; }
}
```

### 2. Add Route Commands
**Files:**
- `CreateRouteCommand.cs` / `CreateRouteCommandHandler.cs`
- `UpdateRouteCommand.cs` / `UpdateRouteCommandHandler.cs`
- `DeleteRouteCommand.cs` / `DeleteRouteCommandHandler.cs`
- `CreateRouteCommandValidator.cs`
- `UpdateRouteCommandValidator.cs`

### 3. Add GraphQL Queries and Mutations
**File:** `src/backend/src/LastMile.TMS.Api/GraphQL/RouteQuery.cs` (or add to existing Query.cs)
```csharp
[Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
public async Task<IReadOnlyList<RouteSummaryDto>> GetRoutes(AppDbContext context)
[Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
public async Task<RouteDto?> GetRoute(AppDbContext context, Guid id)
```

**File:** `src/backend/src/LastMile.TMS.Api/GraphQL/RouteMutation.cs`
```csharp
[Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
public async Task<RouteDto> CreateRoute(...)
[Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
public async Task<RouteDto> UpdateRoute(...)
[Authorize(Roles = [Role.RoleNames.Admin, Role.RoleNames.OperationsManager])]
public async Task<bool> DeleteRoute(...)
```

## Frontend Requirements

### 1. Add Route Operations
**File:** `src/web/src/lib/operations/routes.ts`
- GET_ROUTES, GET_ROUTE queries
- CREATE_ROUTE, UPDATE_ROUTE, DELETE_ROUTE mutations
- RouteStatus enum matching backend (PLANNED, IN_PROGRESS, COMPLETED, CANCELLED)

### 2. Add Route Hooks
**File:** `src/web/src/lib/hooks/use-routes.ts`
- useRoutes, useRoute queries
- useCreateRoute, useUpdateRoute, useDeleteRoute mutations

### 3. Add Route Types
**File:** `src/web/src/types/route.ts`
```typescript
export enum RouteStatus {
  PLANNED = "PLANNED",
  IN_PROGRESS = "IN_PROGRESS",
  COMPLETED = "COMPLETED",
  CANCELLED = "CANCELLED",
}

export interface RouteSummary { ... }
export interface Route { ... }
```

### 4. Create Route List Page
**File:** `src/web/src/app/(protected)/routes/page.tsx`
- Table with routes: Name, Status, Planned Start, Vehicle, Actions
- Filter by status
- "Add Route" button

### 5. Create Route Detail Page
**File:** `src/web/src/app/(protected)/routes/[id]/page.tsx`
- Route info display
- Assigned vehicle info
- Status indicator

### 6. Create Route Form (Create/Edit)
**File:** `src/web/src/components/routes/route-form.tsx`
- Fields: Name, PlannedStartTime, TotalDistanceKm, TotalParcelCount, VehicleId (select)
- Vehicle selection dropdown (requires GetVehicles query)
- Validation

### 7. Create Route Pages
- `src/web/src/app/(protected)/routes/new/page.tsx`
- `src/web/src/app/(protected)/routes/[id]/edit/page.tsx`

### 8. Update Sidebar Navigation
**File:** `src/web/src/app/(protected)/layout.tsx`
- Add Routes link (same roles as Vehicles)

## Verification
1. Backend: `dotnet build` passes
2. Frontend: `npm run build` passes
3. Navigate to /routes
4. Create a new route with vehicle assignment
5. View route details with assigned vehicle
6. Edit route to change vehicle
7. Delete route
