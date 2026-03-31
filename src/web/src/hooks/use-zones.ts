"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { zoneKeys } from "@/lib/query-key-factory";
import * as zonesService from "@/services/zones.service";
import { toast } from "sonner";

export function useZones() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: zoneKeys.list(),
    queryFn: () => zonesService.fetchZones(session!.user.accessToken),
    enabled: !!session?.user?.accessToken,
  });
}

export function useZone(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: zoneKeys.detail(id),
    queryFn: () => zonesService.fetchZone(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateZone() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof zonesService.createZone>[1]) =>
      zonesService.createZone(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: zoneKeys.all });
      toast.success("Zone created successfully");
    },
    onError: () => {
      toast.error("Failed to create zone");
    },
  });
}

export function useUpdateZone() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof zonesService.updateZone>[1]) =>
      zonesService.updateZone(session!.user.accessToken, input),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: zoneKeys.all });
      queryClient.invalidateQueries({ queryKey: zoneKeys.detail(variables.id) });
      toast.success("Zone updated successfully");
    },
    onError: () => {
      toast.error("Failed to update zone");
    },
  });
}

export function useDeleteZone() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => zonesService.deleteZone(session!.user.accessToken, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: zoneKeys.all });
      toast.success("Zone deleted successfully");
    },
    onError: () => {
      toast.error("Failed to delete zone");
    },
  });
}
