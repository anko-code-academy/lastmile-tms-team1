"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { graphqlFetch } from "@/lib/graphql";
import {
  GET_ROUTES,
  GET_ROUTE,
  CREATE_ROUTE,
  UPDATE_ROUTE,
  DELETE_ROUTE,
  CHANGE_ROUTE_STATUS,
} from "@/lib/operations/routes";
import { Route, RouteSummary, RouteStatus } from "@/types/route";

interface RoutesResponse {
  routes: RouteSummary[];
}

interface RouteResponse {
  route: Route | null;
}

interface CreateRouteResponse {
  createRoute: Route;
}

interface UpdateRouteResponse {
  updateRoute: Route;
}

interface DeleteRouteResponse {
  deleteRoute: boolean;
}

interface ChangeRouteStatusResponse {
  changeRouteStatus: Route;
}

export function useRoutes(status?: RouteStatus) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["routes", status],
    queryFn: async () => {
      const data = await graphqlFetch<RoutesResponse>(
        GET_ROUTES,
        status ? { where: { status } } : {},
        session?.user?.accessToken
      );
      return data.routes;
    },
    enabled: !!session?.user?.accessToken,
  });
}

export function useRoute(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["route", id],
    queryFn: async () => {
      const data = await graphqlFetch<RouteResponse>(
        GET_ROUTE,
        { id },
        session?.user?.accessToken
      );
      return data.route;
    },
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: {
      name: string;
      plannedStartTime: string;
      totalDistanceKm: number;
      totalParcelCount: number;
      vehicleId?: string | null;
    }) => {
      return graphqlFetch<CreateRouteResponse>(
        CREATE_ROUTE,
        {
          name: input.name,
          plannedStartTime: input.plannedStartTime,
          totalDistanceKm: input.totalDistanceKm,
          totalParcelCount: input.totalParcelCount,
          vehicleId: input.vehicleId || null,
        },
        session?.user?.accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["routes"] });
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
    },
  });
}

export function useUpdateRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: {
      id: string;
      name: string;
      plannedStartTime: string;
      totalDistanceKm: number;
      totalParcelCount: number;
      vehicleId?: string | null;
    }) => {
      return graphqlFetch<UpdateRouteResponse>(
        UPDATE_ROUTE,
        {
          id: input.id,
          name: input.name,
          plannedStartTime: input.plannedStartTime,
          totalDistanceKm: input.totalDistanceKm,
          totalParcelCount: input.totalParcelCount,
          vehicleId: input.vehicleId || null,
        },
        session?.user?.accessToken
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["routes"] });
      queryClient.invalidateQueries({ queryKey: ["route", variables.id] });
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
    },
  });
}

export function useDeleteRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      return graphqlFetch<DeleteRouteResponse>(
        DELETE_ROUTE,
        { id },
        session?.user?.accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["routes"] });
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
    },
  });
}

export function useChangeRouteStatus() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, newStatus }: { id: string; newStatus: RouteStatus }) => {
      return graphqlFetch<ChangeRouteStatusResponse>(
        CHANGE_ROUTE_STATUS,
        { id, newStatus },
        session?.user?.accessToken
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["routes"] });
      queryClient.invalidateQueries({ queryKey: ["route", variables.id] });
      // Vehicle status may change when route completes
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
      queryClient.invalidateQueries({ queryKey: ["vehicle"] });
    },
    onSettled: () => {
      // Ensure routes and vehicles are always refetched
      queryClient.invalidateQueries({ queryKey: ["routes"] });
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
    },
  });
}
