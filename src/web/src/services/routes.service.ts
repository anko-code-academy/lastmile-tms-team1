import { print } from "graphql";
import { apiFetch } from "@/lib/api";
import {
  GetRoutesDocument,
  GetRouteDocument,
  GetAvailableDriversDocument,
  CreateRouteDocument,
  UpdateRouteDocument,
  DeleteRouteDocument,
  ChangeRouteStatusDocument,
  AssignDriverToRouteDocument,
  AddParcelsToRouteDocument,
  AutoAssignParcelsByZoneDocument,
  RemoveParcelsFromRouteDocument,
  ReorderRouteStopsDocument,
  OptimizeRouteStopOrderDocument,
  type GetRoutesQuery,
  type GetRouteQuery,
  type GetAvailableDriversQuery,
  type CreateRouteMutation,
  type UpdateRouteMutation,
  type ChangeRouteStatusMutation,
  type AssignDriverToRouteMutation,
  type AddParcelsToRouteMutation,
  type AutoAssignParcelsByZoneMutation,
  type RemoveParcelsFromRouteMutation,
  type ReorderRouteStopsMutation,
  type OptimizeRouteStopOrderMutation,
  type CreateRouteCommandInput,
  type UpdateRouteCommandInput,
  type AddParcelsToRouteCommandInput,
  type AutoAssignParcelsByZoneCommandInput,
  type RemoveParcelsFromRouteCommandInput,
  type ReorderRouteStopsCommandInput,
  type OptimizeRouteStopOrderCommandInput,
  type RouteStatus,
} from "@/graphql/generated/graphql";

export interface FetchRoutesFilters {
  status?: RouteStatus;
  date?: string;
  first?: number;
  after?: string;
  order?: Record<string, string>[];
}

export type RouteListItem = NonNullable<NonNullable<GetRoutesQuery["routes"]>["nodes"]>[number];

export async function fetchRoutes(
  token: string,
  filters?: FetchRoutesFilters
): Promise<GetRoutesQuery["routes"]> {
  const where: Record<string, unknown> = {};
  if (filters?.status) {
    where.status = { eq: filters.status };
  }
  if (filters?.date) {
    const dayStart = new Date(filters.date);
    dayStart.setHours(0, 0, 0, 0);
    const dayEnd = new Date(dayStart);
    dayEnd.setDate(dayEnd.getDate() + 1);
    where.plannedStartTime = { gte: dayStart.toISOString(), lt: dayEnd.toISOString() };
  }

  const response = await apiFetch<{ data: GetRoutesQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetRoutesDocument),
      variables: {
        where: Object.keys(where).length > 0 ? where : undefined,
        order: filters?.order,
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
): Promise<GetRouteQuery["route"]> {
  const response = await apiFetch<{ data: GetRouteQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetRouteDocument),
      variables: { id },
    }),
  });
  return response.data.route;
}

export async function fetchAvailableDrivers(
  token: string,
  date: string
): Promise<GetAvailableDriversQuery["availableDrivers"]> {
  const response = await apiFetch<{ data: GetAvailableDriversQuery }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(GetAvailableDriversDocument),
      variables: { date: date.includes("T") ? date : `${date}T00:00:00.000Z` },
    }),
  });
  return response.data.availableDrivers;
}

export async function createRoute(
  token: string,
  input: CreateRouteCommandInput
): Promise<CreateRouteMutation["createRoute"]> {
  const response = await apiFetch<{ data: CreateRouteMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(CreateRouteDocument),
      variables: { input },
    }),
  });
  return response.data.createRoute;
}

export async function updateRoute(
  token: string,
  input: UpdateRouteCommandInput
): Promise<UpdateRouteMutation["updateRoute"]> {
  const response = await apiFetch<{ data: UpdateRouteMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(UpdateRouteDocument),
      variables: { input },
    }),
  });
  return response.data.updateRoute;
}

export async function deleteRoute(
  token: string,
  id: string
): Promise<boolean> {
  const response = await apiFetch<{ data: { deleteRoute: boolean } }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(DeleteRouteDocument),
      variables: { id },
    }),
  });
  return response.data.deleteRoute;
}

export async function changeRouteStatus(
  token: string,
  id: string,
  newStatus: RouteStatus
): Promise<ChangeRouteStatusMutation["changeRouteStatus"]> {
  const response = await apiFetch<{ data: ChangeRouteStatusMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(ChangeRouteStatusDocument),
      variables: { id, newStatus },
    }),
  });
  return response.data.changeRouteStatus;
}

export async function assignDriverToRoute(
  token: string,
  routeId: string,
  driverId: string | null
): Promise<AssignDriverToRouteMutation["assignDriverToRoute"]> {
  const response = await apiFetch<{ data: AssignDriverToRouteMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(AssignDriverToRouteDocument),
      variables: { routeId, driverId },
    }),
  });
  return response.data.assignDriverToRoute;
}

export async function addParcelsToRoute(
  token: string,
  input: AddParcelsToRouteCommandInput
): Promise<AddParcelsToRouteMutation["addParcelsToRoute"]> {
  const response = await apiFetch<{ data: AddParcelsToRouteMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(AddParcelsToRouteDocument),
      variables: { input },
    }),
  });
  return response.data.addParcelsToRoute;
}

export async function autoAssignParcelsByZone(
  token: string,
  input: AutoAssignParcelsByZoneCommandInput
): Promise<AutoAssignParcelsByZoneMutation["autoAssignParcelsByZone"]> {
  const response = await apiFetch<{ data: AutoAssignParcelsByZoneMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(AutoAssignParcelsByZoneDocument),
      variables: { input },
    }),
  });
  return response.data.autoAssignParcelsByZone;
}

export async function removeParcelsFromRoute(
  token: string,
  input: RemoveParcelsFromRouteCommandInput
): Promise<RemoveParcelsFromRouteMutation["removeParcelsFromRoute"]> {
  const response = await apiFetch<{ data: RemoveParcelsFromRouteMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(RemoveParcelsFromRouteDocument),
      variables: { input },
    }),
  });
  return response.data.removeParcelsFromRoute;
}

export async function reorderRouteStops(
  token: string,
  input: ReorderRouteStopsCommandInput
): Promise<ReorderRouteStopsMutation["reorderRouteStops"]> {
  const response = await apiFetch<{ data: ReorderRouteStopsMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(ReorderRouteStopsDocument),
      variables: { input },
    }),
  });
  return response.data.reorderRouteStops;
}

export async function optimizeRouteStopOrder(
  token: string,
  input: OptimizeRouteStopOrderCommandInput
): Promise<OptimizeRouteStopOrderMutation["optimizeRouteStopOrder"]> {
  const response = await apiFetch<{ data: OptimizeRouteStopOrderMutation }>("/api/graphql", {
    method: "POST",
    token,
    body: JSON.stringify({
      query: print(OptimizeRouteStopOrderDocument),
      variables: { input },
    }),
  });
  return response.data.optimizeRouteStopOrder;
}
