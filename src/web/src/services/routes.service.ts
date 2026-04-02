import { apiFetch } from "@/lib/api";
import type { Route, RouteSummary, RouteStatus } from "@/types/route";

const GET_ROUTES_QUERY = `
  query GetRoutes($where: RouteFilterInput) {
    routes(where: $where) {
      id
      name
      status
      plannedStartTime
      vehicleId
      vehicle {
        registrationPlate
      }
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
      createdAt
    }
  }
`;

const CREATE_ROUTE_MUTATION = `
  mutation CreateRoute(
    $name: String!
    $plannedStartTime: DateTime!
    $totalDistanceKm: Decimal!
    $totalParcelCount: Int!
    $vehicleId: UUID
  ) {
    createRoute(
      name: $name
      plannedStartTime: $plannedStartTime
      totalDistanceKm: $totalDistanceKm
      totalParcelCount: $totalParcelCount
      vehicleId: $vehicleId
    ) {
      id
      name
      status
      plannedStartTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
      createdAt
    }
  }
`;

const UPDATE_ROUTE_MUTATION = `
  mutation UpdateRoute(
    $id: UUID!
    $name: String!
    $plannedStartTime: DateTime!
    $totalDistanceKm: Decimal!
    $totalParcelCount: Int!
    $vehicleId: UUID
  ) {
    updateRoute(
      id: $id
      name: $name
      plannedStartTime: $plannedStartTime
      totalDistanceKm: $totalDistanceKm
      totalParcelCount: $totalParcelCount
      vehicleId: $vehicleId
    ) {
      id
      name
      status
      plannedStartTime
      totalDistanceKm
      totalParcelCount
      vehicleId
      vehiclePlate
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
      createdAt
    }
  }
`;

interface RoutesResponse {
  routes: RouteSummary[];
}

interface RouteResponse {
  route: Route | null;
}

export async function fetchRoutes(
  token: string,
  status?: RouteStatus
): Promise<RouteSummary[]> {
  const response = await apiFetch<{ data: RoutesResponse }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: GET_ROUTES_QUERY,
      variables: status ? { where: { status } } : {},
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

export async function createRoute(
  token: string,
  input: {
    name: string;
    plannedStartTime: string;
    totalDistanceKm: number;
    totalParcelCount: number;
    vehicleId?: string | null;
  }
): Promise<Route> {
  const response = await apiFetch<{ data: { createRoute: Route } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: CREATE_ROUTE_MUTATION,
        variables: {
          name: input.name,
          plannedStartTime: input.plannedStartTime,
          totalDistanceKm: input.totalDistanceKm,
          totalParcelCount: input.totalParcelCount,
          vehicleId: input.vehicleId || null,
        },
      }),
    }
  );
  return response.data.createRoute;
}

export async function updateRoute(
  token: string,
  input: {
    id: string;
    name: string;
    plannedStartTime: string;
    totalDistanceKm: number;
    totalParcelCount: number;
    vehicleId?: string | null;
  }
): Promise<Route> {
  const response = await apiFetch<{ data: { updateRoute: Route } }>(
    "/api/graphql",
    {
      method: "POST",
      token,
      body: JSON.stringify({
        query: UPDATE_ROUTE_MUTATION,
        variables: {
          id: input.id,
          name: input.name,
          plannedStartTime: input.plannedStartTime,
          totalDistanceKm: input.totalDistanceKm,
          totalParcelCount: input.totalParcelCount,
          vehicleId: input.vehicleId || null,
        },
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
): Promise<Route> {
  const response = await apiFetch<{ data: { changeRouteStatus: Route } }>(
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
