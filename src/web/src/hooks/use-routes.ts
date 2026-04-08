"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { routeKeys, vehicleKeys, driverKeys } from "@/lib/query-key-factory";
import * as routesService from "@/services/routes.service";
import {
  RouteStatus,
  type CreateRouteCommandInput,
  type UpdateRouteCommandInput,
} from "@/graphql/generated/graphql";
import { toast } from "sonner";

export function useRoutes(filters?: routesService.FetchRoutesFilters) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: routeKeys.list(filters),
    queryFn: () => routesService.fetchRoutes(session!.user.accessToken, filters),
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

export function useAvailableDrivers(date: string | undefined) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: routeKeys.availableDrivers(date ?? ""),
    queryFn: () => routesService.fetchAvailableDrivers(session!.user.accessToken, date!),
    enabled: !!session?.user?.accessToken && !!date,
  });
}

export function useCreateRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateRouteCommandInput) =>
      routesService.createRoute(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
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
    mutationFn: (input: UpdateRouteCommandInput) =>
      routesService.updateRoute(session!.user.accessToken, input),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
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

export function useAssignDriverToRoute() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ routeId, driverId }: { routeId: string; driverId: string | null }) =>
      routesService.assignDriverToRoute(session!.user.accessToken, routeId, driverId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.detail(variables.routeId) });
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
      toast.success("Driver assigned successfully");
    },
    onError: () => {
      toast.error("Failed to assign driver");
    },
  });
}
