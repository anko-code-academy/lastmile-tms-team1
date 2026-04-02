"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { depotKeys, userKeys } from "@/lib/query-key-factory";
import * as depotsService from "@/services/depots.service";
import { toast } from "sonner";

export function useDepots() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: depotKeys.list(),
    queryFn: () => depotsService.fetchDepots(session!.user.accessToken),
    enabled: !!session?.user?.accessToken,
  });
}

export function useDepot(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: depotKeys.detail(id),
    queryFn: () => depotsService.fetchDepot(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateDepot() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof depotsService.createDepot>[1]) =>
      depotsService.createDepot(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: depotKeys.all });
      toast.success("Depot created successfully");
    },
    onError: () => {
      toast.error("Failed to create depot");
    },
  });
}

export function useUpdateDepot() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof depotsService.updateDepot>[1]) =>
      depotsService.updateDepot(session!.user.accessToken, input),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: depotKeys.all });
      queryClient.invalidateQueries({ queryKey: depotKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: userKeys.all });
      toast.success("Depot updated successfully");
    },
    onError: () => {
      toast.error("Failed to update depot");
    },
  });
}

export function useDeleteDepot() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => depotsService.deleteDepot(session!.user.accessToken, id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: depotKeys.all });
      queryClient.invalidateQueries({ queryKey: userKeys.all });
      toast.success("Depot deleted successfully");
    },
    onError: () => {
      toast.error("Failed to delete depot");
    },
  });
}
