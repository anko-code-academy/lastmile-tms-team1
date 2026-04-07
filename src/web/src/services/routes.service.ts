import { apiFetch } from "@/lib/api";
import {
  type CreateRouteCommandInput,
  type UpdateRouteCommandInput,
  type RouteDto,
  type Route,
  type RoutesConnection,
  type AvailableDriverDto,
  type RouteStatus,
} from "@/graphql/generated/graphql";

export interface FetchRoutesFilters {
  status?: RouteStatus;
  first?: number;
  after?: string;
}

const GET_ROUTES_QUERY = `
  query GetRoutes($where: RouteFilterInput, $order: [RouteSortInput!], $first: Int, $after: String) {
    routes(where: $where, order: $order, first: $first, after: $after) {
      nodes {
        id
        name
        status
        plannedStartTime
        vehicleId
        vehicle {
          registrationPlate
        }
        driverId
        driver {
          user {
            firstName
            lastName
          }
        }
      }
      pageInfo {
        hasNextPage
        hasPreviousPage
        startCursor
        endCursor
      }
      totalCount
    }
  }
`;

const GET_ROUTE_QUERY = `
  query GetRoute($id: UUID!) {
    route(id: $id) {
      id
      name
      status
      plannedStartTime
      actualStartTime
      actualEndTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehicle {
        registrationPlate
      }
      driverId
      driver {
        user {
          firstName
          lastName
        }
      }
      createdAt
    }
  }
`;

const GET_AVAILABLE_DRIVERS_QUERY = `
  query GetAvailableDrivers($date: DateTime!) {
    availableDrivers(date: $date) {
      id
      name
      shift {
        openTime
        closeTime
      }
      assignedRoutes {
        id
        name
        status
      }
    }
  }
`;

const CREATE_ROUTE_MUTATION = `
  mutation CreateRoute($input: CreateRouteCommandInput!) {
    createRoute(input: $input) {
      id
      name
      status
      plannedStartTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      driverId
      driverName
      createdAt
    }
  }
`;

const UPDATE_ROUTE_MUTATION = `
  mutation UpdateRoute($input: UpdateRouteCommandInput!) {
    updateRoute(input: $input) {
      id
      name
      status
      plannedStartTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      driverId
      driverName
      createdAt
    }
  }
`;

const DELETE_ROUTE_MUTATION = `
  mutation DeleteRoute($id: UUID!) {
    deleteRoute(id: $id)
  }
`;

const CHANGE_ROUTE_STATUS_MUTATION = `
  mutation ChangeRouteStatus($id: UUID!, $newStatus: RouteStatus!) {
    changeRouteStatus(id: $id, newStatus: $newStatus) {
      id
      name
      status
      plannedStartTime
      actualStartTime
      actualEndTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      driverId
      driverName
      createdAt
    }
  }
`;

const ASSIGN_DRIVER_TO_ROUTE_MUTATION = `
  mutation AssignDriverToRoute($routeId: UUID!, $driverId: UUID) {
    assignDriverToRoute(routeId: $routeId, driverId: $driverId) {
      id
      name
      status
      driverId
      driverName
      vehicleId
      vehiclePlate
    }
  }
`;

interface RoutesResponse {
  routes: RoutesConnection;
}

interface RouteResponse {
  route: Route | null;
}

interface AvailableDriversResponse {
  availableDrivers: AvailableDriverDto[];
}

export async function fetchRoutes(
  token: string,
  filters?: FetchRoutesFilters
): Promise<RoutesConnection> {
  const response = await apiFetch<{ data: RoutesResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_ROUTES_QUERY,
      variables: {
        where: filters?.status ? { status: { eq: filters.status } } : undefined,
        first: filters?.first ?? 25,
        after: filters?.after || null,
      },
    }),
  });
  return response.data.routes;
}

export async function fetchRoute(
  token: string,
  id: string
): Promise<Route | null> {
  const response = await apiFetch<{ data: RouteResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_ROUTE_QUERY,
      variables: { id },
    }),
  });
  return response.data.route;
}

export async function fetchAvailableDrivers(
  token: string,
  date: string
): Promise<AvailableDriverDto[]> {
  const response = await apiFetch<{ data: AvailableDriversResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_AVAILABLE_DRIVERS_QUERY,
      variables: { date: date.includes("T") ? date : `${date}T00:00:00.000Z` },
    }),
  });
  return response.data.availableDrivers;
}

export async function createRoute(
  token: string,
  input: CreateRouteCommandInput
): Promise<RouteDto> {
  const response = await apiFetch<{ data: { createRoute: RouteDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: CREATE_ROUTE_MUTATION,
        variables: { input },
      }),
    }
  );
  return response.data.createRoute;
}

export async function updateRoute(
  token: string,
  input: UpdateRouteCommandInput
): Promise<RouteDto> {
  const response = await apiFetch<{ data: { updateRoute: RouteDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: UPDATE_ROUTE_MUTATION,
        variables: { input },
      }),
    }
  );
  return response.data.updateRoute;
}

export async function deleteRoute(
  token: string,
  id: string
): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteRoute: boolean } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: DELETE_ROUTE_MUTATION,
        variables: { id },
      }),
    }
  );
  return response.data.deleteRoute;
}

export async function changeRouteStatus(
  token: string,
  id: string,
  newStatus: RouteStatus
): Promise<RouteDto> {
  const response = await apiFetch<{ data: { changeRouteStatus: RouteDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: CHANGE_ROUTE_STATUS_MUTATION,
        variables: { id, newStatus },
      }),
    }
  );
  return response.data.changeRouteStatus;
}

export async function assignDriverToRoute(
  token: string,
  routeId: string,
  driverId: string | null
): Promise<RouteDto> {
  const response = await apiFetch<{ data: { assignDriverToRoute: RouteDto } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: ASSIGN_DRIVER_TO_ROUTE_MUTATION,
        variables: { routeId, driverId },
      }),
    }
  );
  return response.data.assignDriverToRoute;
}
