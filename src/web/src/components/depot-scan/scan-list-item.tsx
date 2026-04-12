"use client";

import type { ScannedParcelEntry } from "@/types/depot-scan";
import { CheckCircle2, AlertCircle, AlertTriangle, XCircle } from "lucide-react";

interface ScanListItemProps {
  entry: ScannedParcelEntry;
}

const statusConfig = {
  success: { icon: CheckCircle2, color: "text-green-600", bg: "bg-green-50" },
  error: { icon: XCircle, color: "text-red-600", bg: "bg-red-50" },
  duplicate: { icon: AlertCircle, color: "text-amber-600", bg: "bg-amber-50" },
  unexpected: { icon: AlertTriangle, color: "text-amber-600", bg: "bg-amber-50" },
} as const;

export function ScanListItem({ entry }: ScanListItemProps) {
  const isException = entry.status === "success" && entry.newStatus === "EXCEPTION";
  const config = isException ? statusConfig.error : statusConfig[entry.status];
  const Icon = config.icon;

  return (
    <div className={`flex items-center gap-3 rounded-md border px-3 py-2 ${config.bg}`}>
      <Icon className={`h-4 w-4 shrink-0 ${config.color}`} />
      <div className="flex-1 min-w-0">
        <p className="font-mono text-sm truncate">{entry.trackingNumber}</p>
        {entry.zoneName && (
          <p className="text-xs text-muted-foreground">
            Zone: {entry.zoneName}
            {entry.binLabel && ` / Bin: ${entry.binLabel}`}
          </p>
        )}
        {entry.routeName && (
          <p className="text-xs text-muted-foreground">Route: {entry.routeName}</p>
        )}
      </div>
      <div className="text-right shrink-0">
        {isException && (
          <span className="text-xs text-red-600">
            {entry.previousStatus} → {entry.newStatus}
          </span>
        )}
        {entry.status === "success" && !isException && (
          <span className="text-xs text-green-600">
            {entry.previousStatus} → {entry.newStatus}
          </span>
        )}
        {entry.status === "error" && (
          <span className="text-xs text-red-600">{entry.errorMessage}</span>
        )}
        {entry.status === "duplicate" && (
          <span className="text-xs text-amber-600">Duplicate</span>
        )}
        {entry.status === "unexpected" && (
          <span className="text-xs text-amber-600">Unexpected</span>
        )}
      </div>
    </div>
  );
}
