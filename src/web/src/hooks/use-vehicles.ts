"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { vehicleKeys } from "@/lib/query-key-factory";
import * as vehiclesService from "@/services/vehicles.service";
import type { VehicleStatus } from "@/types/vehicle";
import { toast } from "sonner";

export function useVehicles(status?: VehicleStatus) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: vehicleKeys.list(status ? { where: { status } } : undefined),
    queryFn: () => vehiclesService.fetchVehicles(session!.user.accessToken, status),
    enabled: !!session?.user?.accessToken,
  });
}

export function useVehicle(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: vehicleKeys.detail(id),
    queryFn: () => vehiclesService.fetchVehicle(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useVehicleHistory(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: vehicleKeys.history(id),
    queryFn: () =>
      vehiclesService.fetchVehicleHistory(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateVehicle() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      registrationPlate: string;
      type: string;
      parcelCapacity: number;
      weightCapacityKg: number;
      depotId?: string | null;
    }) => vehiclesService.createVehicle(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      toast.success("Vehicle created successfully");
    },
    onError: () => {
      toast.error("Failed to create vehicle");
    },
  });
}

export function useUpdateVehicle() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      id: string;
      registrationPlate: string;
      type: string;
      parcelCapacity: number;
      weightCapacityKg: number;
      depotId?: string | null;
    }) => vehiclesService.updateVehicle(session!.user.accessToken, input),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.detail(variables.id) });
      toast.success("Vehicle updated successfully");
    },
    onError: () => {
      toast.error("Failed to update vehicle");
    },
  });
}

export function useDeleteVehicle() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) =>
      vehiclesService.deleteVehicle(session!.user.accessToken, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      toast.success("Vehicle deleted successfully");
    },
    onError: () => {
      toast.error("Failed to delete vehicle");
    },
  });
}

export function useChangeVehicleStatus() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, newStatus }: { id: string; newStatus: VehicleStatus }) =>
      vehiclesService.changeVehicleStatus(session!.user.accessToken, id, newStatus),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.detail(variables.id) });
      toast.success("Vehicle status updated successfully");
    },
    onError: () => {
      toast.error("Failed to update vehicle status");
    },
  });
}
