"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { parcelKeys, routeKeys } from "@/lib/query-key-factory";
import * as depotScanService from "@/services/depot-scan.service";
import type { ScanParcelCommandInput } from "@/graphql/generated/graphql";

export function useScanParcel() {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: ScanParcelCommandInput) =>
      depotScanService.scanParcel(session!.user.accessToken, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
    },
  });
}
