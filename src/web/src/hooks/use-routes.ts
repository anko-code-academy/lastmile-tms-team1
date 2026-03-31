"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { routeKeys } from "@/lib/query-key-factory";
import { vehicleKeys } from "@/lib/query-key-factory";
import * as routesService from "@/services/routes.service";
import type { RouteStatus } from "@/types/route";
import { toast } from "sonner";

export function useRoutes(status?: RouteStatus) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: routeKeys.list(status ? { where: { status } } : undefined),
    queryFn: () => routesService.fetchRoutes(session!.user.accessToken, status),
    enabled: !!session?.user?.accessToken,
  });
}

export function useRoute(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: routeKeys.detail(id),
    queryFn: () => routesService.fetchRoute(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      name: string;
      plannedStartTime: string;
      totalDistanceKm: number;
      totalParcelCount: number;
      vehicleId?: string | null;
    }) => routesService.createRoute(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      toast.success("Route created successfully");
    },
    onError: () => {
      toast.error("Failed to create route");
    },
  });
}

export function useUpdateRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      id: string;
      name: string;
      plannedStartTime: string;
      totalDistanceKm: number;
      totalParcelCount: number;
      vehicleId?: string | null;
    }) => routesService.updateRoute(session!.user.accessToken, input),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      toast.success("Route updated successfully");
    },
    onError: () => {
      toast.error("Failed to update route");
    },
  });
}

export function useDeleteRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      routesService.deleteRoute(session!.user.accessToken, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      toast.success("Route deleted successfully");
    },
    onError: () => {
      toast.error("Failed to delete route");
    },
  });
}

export function useChangeRouteStatus() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, newStatus }: { id: string; newStatus: RouteStatus }) =>
      routesService.changeRouteStatus(session!.user.accessToken, id, newStatus),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      toast.success("Route status updated successfully");
    },
    onError: () => {
      toast.error("Failed to update route status");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
    },
  });
}
