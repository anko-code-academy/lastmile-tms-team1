"use client";

import { useState, useCallback, useMemo } from "react";
import { ParcelScanForm } from "@/components/depot-scan/parcel-scan-form";
import { DepotSelector } from "@/components/depot-scan/depot-selector";
import { RouteParcelsTable } from "@/components/depot-scan/route-parcels-table";
import { scanConfigs } from "@/lib/depot-scan-configs";
import { useScanParcel } from "@/hooks/use-depot-scan";
import { useRoute } from "@/hooks/use-routes";
import { ParcelStatus } from "@/graphql/generated/graphql";
import type { ScanResult } from "@/types/depot-scan";

export default function LoadoutPage() {
  const [depotId, setDepotId] = useState<string | null>(null);
  const [routeId, setRouteId] = useState<string>("");
  const config = scanConfigs.loadout;
  const scanParcelMutation = useScanParcel();
  const { data: route } = useRoute(routeId);

  const routeParcels = useMemo(() => {
    if (!route?.routeStops) return [];
    return route.routeStops.flatMap((stop) =>
      stop.parcels.map((p) => ({ trackingNumber: p.trackingNumber, status: p.status as string }))
    );
  }, [route]);

  const expectedTrackingNumbers = useMemo(
    () => routeParcels.map((p) => p.trackingNumber),
    [routeParcels]
  );

  const handleScan = useCallback(
    async (trackingNumber: string): Promise<ScanResult> => {
      const result = await scanParcelMutation.mutateAsync({
        trackingNumber,
        newStatus: ParcelStatus.Loaded,
        routeId: routeId || undefined,
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
    [scanParcelMutation, routeId]
  );

  const contextSelector = (
    <div>
      <label className="text-sm font-medium mb-1 block">Route</label>
      <input
        type="text"
        value={routeId}
        onChange={(e) => setRouteId(e.target.value)}
        placeholder="Enter route ID..."
        className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
      />
    </div>
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Load-Out Scan</h1>
        <p className="text-muted-foreground">
          Scan parcels with status <strong>Staged</strong> to transition them to <strong>Loaded</strong> as they are loaded onto the vehicle.
        </p>
      </div>

      <div>
        <label className="text-sm font-medium mb-1 block">Depot</label>
        <DepotSelector value={depotId} onChange={setDepotId} />
      </div>

      {depotId && (
        <ParcelScanForm
          config={config}
          expectedTrackingNumbers={expectedTrackingNumbers}
          onScan={handleScan}
          contextSelector={contextSelector}
          contextValue={routeId}
        />
      )}

      {routeId && route && (
        <RouteParcelsTable parcels={routeParcels} targetStatus={ParcelStatus.Loaded} />
      )}
    </div>
  );
}
