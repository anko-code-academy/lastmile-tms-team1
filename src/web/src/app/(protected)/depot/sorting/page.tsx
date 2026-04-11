"use client";

import { useState, useCallback } from "react";
import { ParcelScanForm } from "@/components/depot-scan/parcel-scan-form";
import { DepotSelector } from "@/components/depot-scan/depot-selector";
import { scanConfigs } from "@/lib/depot-scan-configs";
import { useScanParcel } from "@/hooks/use-depot-scan";
import { ParcelStatus } from "@/graphql/generated/graphql";
import type { ScanResult } from "@/types/depot-scan";

export default function SortingPage() {
  const [depotId, setDepotId] = useState<string | null>(null);
  const config = scanConfigs.sorting;
  const scanParcelMutation = useScanParcel();

  const handleScan = useCallback(
    async (trackingNumber: string): Promise<ScanResult> => {
      const result = await scanParcelMutation.mutateAsync({
        trackingNumber,
        newStatus: ParcelStatus.Sorted,
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
    [scanParcelMutation, config.targetStatus]
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Sorting Scan</h1>
        <p className="text-muted-foreground">
          Scan parcels with status <strong>Received at Depot</strong> to transition them to <strong>Sorted</strong> and assign them to zones.
        </p>
      </div>

      <div>
        <label className="text-sm font-medium mb-1 block">Depot</label>
        <DepotSelector value={depotId} onChange={setDepotId} />
      </div>

      {depotId && (
        <ParcelScanForm
          config={config}
          expectedTrackingNumbers={[]}
          onScan={handleScan}
        />
      )}
    </div>
  );
}
