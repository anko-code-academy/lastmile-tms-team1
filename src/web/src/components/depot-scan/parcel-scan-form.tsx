"use client";

import { useReducer, useCallback, useMemo } from "react";
import type { ScanOperationConfig, ScannedParcelEntry, ScanResult, ManifestScanItem } from "@/types/depot-scan";
import { ScanInput } from "./scan-input";
import { ScanSummary } from "./scan-summary";
import { ScanList } from "./scan-list";

interface ParcelScanFormProps {
  config: ScanOperationConfig;
  expectedTrackingNumbers?: string[];
  manifestItems?: ManifestScanItem[];
  onScan: (trackingNumber: string) => Promise<ScanResult>;
  contextSelector?: React.ReactNode;
  contextValue?: unknown;
  onComplete?: (scanned: ScannedParcelEntry[]) => void;
}

interface ScanState {
  entries: ScannedParcelEntry[];
  isProcessing: boolean;
}

type ScanAction =
  | { type: "START_PROCESSING" }
  | { type: "ADD_ENTRY"; entry: ScannedParcelEntry }
  | { type: "STOP_PROCESSING" };

function scanReducer(state: ScanState, action: ScanAction): ScanState {
  switch (action.type) {
    case "START_PROCESSING":
      return { ...state, isProcessing: true };
    case "ADD_ENTRY":
      return {
        ...state,
        isProcessing: false,
        entries: [action.entry, ...state.entries],
      };
    case "STOP_PROCESSING":
      return { ...state, isProcessing: false };
    default:
      return state;
  }
}

const TRACKING_NUMBER_PATTERN = /^LM-\d{6}-[A-Z0-9]{6}$/;

function toEntry(item: ManifestScanItem): ScannedParcelEntry {
  const isException = item.status === "UNEXPECTED" || item.status === "MISSING";
  return {
    trackingNumber: item.trackingNumber,
    status: "success",
    previousStatus: "REGISTERED",
    newStatus: isException ? "EXCEPTION" : "RECEIVED_AT_DEPOT",
    scannedAt: new Date(),
  };
}

export function ParcelScanForm({
  config,
  expectedTrackingNumbers,
  manifestItems,
  onScan,
  contextSelector,
  contextValue,
}: ParcelScanFormProps) {
  const [state, dispatch] = useReducer(scanReducer, {
    entries: [],
    isProcessing: false,
  });

  const preScannedEntries = useMemo(() => {
    if (!manifestItems) return [];
    return manifestItems
      .filter((i) => i.status !== "EXPECTED")
      .map(toEntry);
  }, [manifestItems]);

  // Dedup: local entries take priority over pre-scanned backend entries
  const localTrackingNumbers = useMemo(
    () => new Set(state.entries.map((e) => e.trackingNumber)),
    [state.entries]
  );

  const allEntries = useMemo(() => {
    const filteredPreScanned = preScannedEntries.filter(
      (e) => !localTrackingNumbers.has(e.trackingNumber)
    );
    return [...state.entries, ...filteredPreScanned];
  }, [state.entries, preScannedEntries, localTrackingNumbers]);

  const scannedCount = allEntries.filter((e) => e.status === "success").length;
  const totalExpected = manifestItems?.length ?? expectedTrackingNumbers?.length ?? 0;

  const scannedTrackingNumbers = useMemo(() => {
    if (manifestItems) {
      return new Set(manifestItems.filter((i) => i.status !== "EXPECTED").map((i) => i.trackingNumber));
    }
    return new Set<string>();
  }, [manifestItems]);

  const handleScan = useCallback(
    async (trackingNumber: string) => {
      if (state.isProcessing) return;

      if (!TRACKING_NUMBER_PATTERN.test(trackingNumber)) {
        dispatch({
          type: "ADD_ENTRY",
          entry: {
            trackingNumber,
            status: "error",
            errorMessage: "Invalid tracking number format",
            scannedAt: new Date(),
          },
        });
        return;
      }

      // Check duplicate against local entries and backend-scanned items
      if (state.entries.some((e) => e.trackingNumber === trackingNumber) || scannedTrackingNumbers.has(trackingNumber)) {
        dispatch({
          type: "ADD_ENTRY",
          entry: {
            trackingNumber,
            status: "duplicate",
            scannedAt: new Date(),
          },
        });
        return;
      }

      dispatch({ type: "START_PROCESSING" });

      try {
        const result = await onScan(trackingNumber);
        dispatch({
          type: "ADD_ENTRY",
          entry: {
            trackingNumber: result.trackingNumber,
            status: "success",
            previousStatus: result.previousStatus,
            newStatus: result.newStatus,
            zoneName: result.zoneName ?? undefined,
            binLabel: result.binLabel ?? undefined,
            routeName: result.routeName ?? undefined,
            scannedAt: new Date(),
          },
        });
      } catch (err) {
        dispatch({
          type: "ADD_ENTRY",
          entry: {
            trackingNumber,
            status: "error",
            errorMessage: err instanceof Error ? err.message : "Scan failed",
            scannedAt: new Date(),
          },
        });
      }
    },
    [state.isProcessing, state.entries, scannedTrackingNumbers, onScan]
  );

  const contextSelected = contextValue !== undefined && contextValue !== null && contextValue !== "";
  const canScan = !contextSelector || contextSelected;

  return (
    <div className="space-y-4">
      <div>
        <h2 className="text-lg font-semibold">{config.title}</h2>
        <p className="text-sm text-muted-foreground">{config.description}</p>
      </div>

      {contextSelector && <div>{contextSelector}</div>}

      {canScan && (
        <>
          <ScanInput
            placeholder={config.inputPlaceholder}
            disabled={state.isProcessing}
            onSubmit={handleScan}
          />

          {totalExpected > 0 && (
            <ScanSummary
              total={totalExpected}
              scanned={scannedCount}
              remaining={totalExpected - scannedCount}
            />
          )}

          <ScanList entries={allEntries} />
        </>
      )}
    </div>
  );
}
