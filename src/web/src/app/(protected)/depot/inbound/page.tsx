"use client";

import { useState, useCallback } from "react";
import { ParcelScanForm } from "@/components/depot-scan/parcel-scan-form";
import { DepotSelector } from "@/components/depot-scan/depot-selector";
import { scanConfigs } from "@/lib/depot-scan-configs";
import { useScanParcel } from "@/hooks/use-depot-scan";
import { useManifests, useReceiveParcel, useCompleteManifestReceiving, useCreateManifest } from "@/hooks/use-manifests";
import { ManifestStatus, ParcelStatus } from "@/graphql/generated/graphql";
import type { ScanResult } from "@/types/depot-scan";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";

export default function InboundPage() {
  const [depotId, setDepotId] = useState<string | null>(null);
  const [manifestId, setManifestId] = useState<string | null>(null);
  const config = scanConfigs.inbound;

  const scanParcelMutation = useScanParcel();
  const receiveParcelMutation = useReceiveParcel();
  const completeMutation = useCompleteManifestReceiving();
  const createManifestMutation = useCreateManifest();
  const manifestsQuery = useManifests(
    depotId ? { depotId, status: [ManifestStatus.Open, ManifestStatus.Receiving] } : undefined
  );

  const expectedTrackingNumbers = manifestsQuery.data?.nodes?.flatMap(
    (m) => m.items?.map((i) => i.trackingNumber).filter((t): t is string => !!t) ?? []
  ) ?? [];

  const handleScan = useCallback(
    async (trackingNumber: string): Promise<ScanResult> => {
      if (manifestId) {
        const result = await receiveParcelMutation.mutateAsync({
          manifestId,
          trackingNumber,
        });
        return {
          trackingNumber: result.trackingNumber,
          previousStatus: "REGISTERED",
          newStatus: result.newStatus,
          zoneName: null,
          binLabel: null,
          routeName: null,
        };
      }
      // Fallback to generic scan if no manifest selected
      const result = await scanParcelMutation.mutateAsync({
        trackingNumber,
        newStatus: ParcelStatus.ReceivedAtDepot,
      });
      return {
        trackingNumber: result.trackingNumber,
        previousStatus: result.previousStatus,
        newStatus: result.newStatus,
        zoneName: result.zoneName,
        binLabel: result.binLabel,
        routeName: result.routeName,
      };
    },
    [manifestId, receiveParcelMutation, scanParcelMutation, config.targetStatus]
  );

  const handleCreateManifest = async () => {
    const name = prompt("Enter manifest name:");
    if (!name || !depotId) return;

    const trackingNumbersStr = prompt("Enter tracking numbers (one per line):");
    if (!trackingNumbersStr) return;

    const trackingNumbers = trackingNumbersStr
      .split("\n")
      .map((t) => t.trim())
      .filter(Boolean);

    const result = await createManifestMutation.mutateAsync({
      name,
      depotId,
      trackingNumbers,
    });
    await manifestsQuery.refetch();
    setManifestId(result.id);
    toast.success(`Created manifest with ${trackingNumbers.length} items`);
  };

  const handleEndSession = async () => {
    if (!manifestId) return;
    await completeMutation.mutateAsync({ manifestId });
    setManifestId(null);
  };

  const contextSelector = (
    <div className="space-y-3">
      <div className="flex gap-2 items-end">
        <div className="flex-1">
          <label className="text-sm font-medium mb-1 block">Manifest</label>
          <select
            value={manifestId ?? ""}
            onChange={(e) => setManifestId(e.target.value || null)}
            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
          >
            <option value="">No manifest (free scan)</option>
            {manifestsQuery.data?.nodes?.map((m) => (
              <option key={m.id} value={m.id}>
                {m.name} ({m.status})
              </option>
            ))}
          </select>
        </div>
        <Button variant="outline" onClick={handleCreateManifest} disabled={!depotId}>
          New Manifest
        </Button>
        {manifestId && (
          <Button variant="destructive" onClick={handleEndSession}>
            End Session
          </Button>
        )}
      </div>
    </div>
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Inbound Receiving</h1>
        <p className="text-muted-foreground">
          Scan parcels with status <strong>Registered</strong> to transition them to <strong>Received at Depot</strong> against a manifest.
        </p>
      </div>

      <div>
        <label className="text-sm font-medium mb-1 block">Depot</label>
        <DepotSelector value={depotId} onChange={setDepotId} />
      </div>

      {depotId && (
        <ParcelScanForm
          config={config}
          expectedTrackingNumbers={manifestId ? expectedTrackingNumbers : []}
          onScan={handleScan}
          contextSelector={contextSelector}
          contextValue={depotId}
        />
      )}
    </div>
  );
}
