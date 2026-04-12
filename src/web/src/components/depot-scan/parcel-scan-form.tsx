"use client";

import { useReducer, useCallback } from "react";
import type { ScanOperationConfig, ScannedParcelEntry, ScanResult } from "@/types/depot-scan";
import { ScanInput } from "./scan-input";
import { ScanSummary } from "./scan-summary";
import { ScanList } from "./scan-list";

interface ParcelScanFormProps {
  config: ScanOperationConfig;
  expectedTrackingNumbers: string[];
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

export function ParcelScanForm({
  config,
  expectedTrackingNumbers,
  onScan,
  contextSelector,
  contextValue,
}: ParcelScanFormProps) {
  const [state, dispatch] = useReducer(scanReducer, {
    entries: [],
    isProcessing: false,
  });

  const scannedCount = state.entries.filter((e) => e.status === "success").length;
  const totalExpected = expectedTrackingNumbers.length;

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

      // Check duplicate
      if (state.entries.some((e) => e.trackingNumber === trackingNumber)) {
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
    [state.isProcessing, state.entries, onScan]
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

          <ScanList entries={state.entries} />
        </>
      )}
    </div>
  );
}
