"use client";

import { useQuery, useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import { parcelKeys } from "@/lib/query-key-factory";
import * as parcelsService from "@/services/parcels.service";
import { toast } from "sonner";
import type {
  UpdateParcelCommandInput,
  CancelParcelCommandInput,
  ChangeParcelStatusCommandInput,
} from "@/graphql/generated/graphql";

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

export function useTrackingEvents(parcelId: string) {
  const { data: session } = useSession();

  return useInfiniteQuery({
    queryKey: parcelKeys.trackingEvents(parcelId),
    queryFn: ({ pageParam }) =>
      parcelsService.fetchTrackingEvents(session!.user.accessToken, parcelId, 50, pageParam),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage?.pageInfo?.hasNextPage ? lastPage.pageInfo.endCursor ?? undefined : undefined,
    enabled: !!session?.user?.accessToken && !!parcelId,
  });
}

export function useParcelAuditLogs(parcelId: string) {
  const { data: session } = useSession();

  return useInfiniteQuery({
    queryKey: parcelKeys.auditLogs(parcelId),
    queryFn: ({ pageParam }) =>
      parcelsService.fetchParcelAuditLogs(session!.user.accessToken, parcelId, 25, pageParam),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage?.pageInfo?.hasNextPage ? lastPage.pageInfo.endCursor ?? undefined : undefined,
    enabled: !!session?.user?.accessToken && !!parcelId,
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

export function useUpdateParcel() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateParcelCommandInput) =>
      parcelsService.updateParcel(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
      toast.success("Parcel updated successfully");
    },
    onError: () => {
      toast.error("Failed to update parcel");
    },
  });
}

export function useCancelParcel() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();
  const router = useRouter();

  return useMutation({
    mutationFn: (input: CancelParcelCommandInput) =>
      parcelsService.cancelParcel(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
      toast.success("Parcel cancelled");
      router.push("/parcels");
    },
    onError: () => {
      toast.error("Failed to cancel parcel");
    },
  });
}

export function useChangeParcelStatus() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: ChangeParcelStatusCommandInput) =>
      parcelsService.changeParcelStatus(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
      toast.success("Parcel status updated");
    },
    onError: () => {
      toast.error("Failed to update parcel status");
    },
  });
}
