"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { manifestKeys, parcelKeys } from "@/lib/query-key-factory";
import * as manifestService from "@/services/manifest.service";
import { toast } from "sonner";

export function useManifests(filters?: manifestService.FetchManifestsFilters) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: manifestKeys.list(filters),
    queryFn: () => manifestService.fetchManifests(session!.user.accessToken, filters),
    enabled: !!session?.user?.accessToken,
  });
}

export function useManifest(id: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: manifestKeys.detail(id),
    queryFn: () => manifestService.fetchManifest(session!.user.accessToken, id),
    enabled: !!session?.user?.accessToken && !!id,
  });
}

export function useCreateManifest() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof manifestService.createManifest>[1]) =>
      manifestService.createManifest(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: manifestKeys.all });
      toast.success("Manifest created successfully");
    },
    onError: () => {
      toast.error("Failed to create manifest");
    },
  });
}

export function useReceiveParcel() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof manifestService.receiveParcel>[1]) =>
      manifestService.receiveParcel(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: manifestKeys.all });
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
    },
  });
}

export function useCompleteManifestReceiving() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof manifestService.completeManifestReceiving>[1]) =>
      manifestService.completeManifestReceiving(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: manifestKeys.all });
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
      toast.success("Receiving session completed");
    },
    onError: () => {
      toast.error("Failed to complete receiving session");
    },
  });
}
