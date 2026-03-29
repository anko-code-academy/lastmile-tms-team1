"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { graphqlFetch } from "@/lib/graphql";
import {
  GET_VEHICLES,
  GET_VEHICLE,
  GET_VEHICLE_HISTORY,
  CREATE_VEHICLE,
  UPDATE_VEHICLE,
  DELETE_VEHICLE,
  CHANGE_VEHICLE_STATUS,
} from "@/lib/operations/vehicles";
import { Vehicle, VehicleSummary, VehicleHistory, VehicleStatus } from "@/types/vehicle";

interface VehiclesResponse {
  vehicles: VehicleSummary[];
}

interface VehicleResponse {
  vehicle: Vehicle | null;
}

interface VehicleHistoryResponse {
  vehicleHistory: VehicleHistory | null;
}

interface CreateVehicleResponse {
  createVehicle: Vehicle;
}

interface UpdateVehicleResponse {
  updateVehicle: Vehicle;
}

interface DeleteVehicleResponse {
  deleteVehicle: boolean;
}

interface ChangeStatusResponse {
  changeVehicleStatus: Vehicle;
}

export function useVehicles(status?: VehicleStatus) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["vehicles", status],
    queryFn: async () => {
      const data = await graphqlFetch<VehiclesResponse>(
        GET_VEHICLES,
        status ? { where: { status } } : {},
        session?.user?.accessToken
      );
      return data.vehicles;
    },
    enabled: !!session?.user?.accessToken,
  });
}

export function useVehicle(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["vehicle", id],
    queryFn: async () => {
      const data = await graphqlFetch<VehicleResponse>(
        GET_VEHICLE,
        { id },
        session?.user?.accessToken
      );
      return data.vehicle;
    },
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useVehicleHistory(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["vehicleHistory", id],
    queryFn: async () => {
      const data = await graphqlFetch<VehicleHistoryResponse>(
        GET_VEHICLE_HISTORY,
        { id },
        session?.user?.accessToken
      );
      return data.vehicleHistory;
    },
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateVehicle() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: {
      registrationPlate: string;
      type: string;
      parcelCapacity: number;
      weightCapacityKg: number;
      depotId?: string | null;
    }) => {
      return graphqlFetch<CreateVehicleResponse>(
        CREATE_VEHICLE,
        {
          registrationPlate: input.registrationPlate,
          type: input.type,
          parcelCapacity: input.parcelCapacity,
          weightCapacityKg: input.weightCapacityKg,
          depotId: input.depotId || null,
        },
        session?.user?.accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
    },
  });
}

export function useUpdateVehicle() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: {
      id: string;
      registrationPlate: string;
      type: string;
      parcelCapacity: number;
      weightCapacityKg: number;
      depotId?: string | null;
    }) => {
      return graphqlFetch<UpdateVehicleResponse>(
        UPDATE_VEHICLE,
        {
          id: input.id,
          registrationPlate: input.registrationPlate,
          type: input.type,
          parcelCapacity: input.parcelCapacity,
          weightCapacityKg: input.weightCapacityKg,
          depotId: input.depotId || null,
        },
        session?.user?.accessToken
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
      queryClient.invalidateQueries({ queryKey: ["vehicle", variables.id] });
    },
  });
}

export function useDeleteVehicle() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      return graphqlFetch<DeleteVehicleResponse>(
        DELETE_VEHICLE,
        { id },
        session?.user?.accessToken
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
    },
  });
}

export function useChangeVehicleStatus() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, newStatus }: { id: string; newStatus: VehicleStatus }) => {
      return graphqlFetch<ChangeStatusResponse>(
        CHANGE_VEHICLE_STATUS,
        { id, newStatus },
        session?.user?.accessToken
      );
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["vehicles"] });
      queryClient.invalidateQueries({ queryKey: ["vehicle", variables.id] });
    },
  });
}
