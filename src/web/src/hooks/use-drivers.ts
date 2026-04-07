"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { driverKeys, routeKeys } from "@/lib/query-key-factory";
import * as driversService from "@/services/driver.service";
import { toast } from "sonner";

export function useDrivers() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: driverKeys.list(),
    queryFn: () => driversService.fetchDrivers(session!.user.accessToken),
    enabled: !!session?.user?.accessToken,
  });
}

export function useDriver(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: driverKeys.detail(id),
    queryFn: () => driversService.fetchDriver(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateDriver() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof driversService.createDriver>[1]) =>
      driversService.createDriver(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
    },
  });
}

export function useUpdateDriver() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof driversService.updateDriver>[1]) =>
      driversService.updateDriver(session!.user.accessToken, input),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
      queryClient.invalidateQueries({ queryKey: driverKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });
}

export function useDeleteDriver() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => driversService.deleteDriver(session!.user.accessToken, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
    },
  });
}
