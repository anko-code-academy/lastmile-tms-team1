"use client";

import { useEffect, useRef } from "react";
import type { ScannedParcelEntry } from "@/types/depot-scan";
import { ScanListItem } from "./scan-list-item";

interface ScanListProps {
  entries: ScannedParcelEntry[];
}

export function ScanList({ entries }: ScanListProps) {
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [entries.length]);

  if (entries.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        <p className="text-sm">No parcels scanned yet</p>
        <p className="text-xs mt-1">Scan a barcode to begin</p>
      </div>
    );
  }

  return (
    <div className="space-y-1.5 max-h-80 overflow-y-auto">
      {entries.map((entry, index) => (
        <ScanListItem key={`${entry.trackingNumber}-${index}`} entry={entry} />
      ))}
      <div ref={bottomRef} />
    </div>
  );
}
