"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { parcelKeys } from "@/lib/query-key-factory";
import * as parcelsService from "@/services/parcels.service";
import { toast } from "sonner";

export function useParcels(filters?: parcelsService.FetchParcelsFilters) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: parcelKeys.list(filters),
    queryFn: () => parcelsService.fetchParcels(session!.user.accessToken, filters),
    enabled: !!session?.user?.accessToken,
  });
}

export function useParcel(trackingNumber: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: parcelKeys.detail(trackingNumber),
    queryFn: () => parcelsService.fetchParcel(session!.user.accessToken, trackingNumber),
    enabled: !!session?.user?.accessToken && !!trackingNumber,
  });
}

export function useCreateParcel() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: Parameters<typeof parcelsService.createParcel>[1]) =>
      parcelsService.createParcel(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
      toast.success("Parcel registered successfully");
    },
    onError: () => {
      toast.error("Failed to register parcel");
    },
  });
}
