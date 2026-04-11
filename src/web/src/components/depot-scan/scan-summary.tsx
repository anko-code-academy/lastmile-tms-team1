"use client";

interface ScanSummaryProps {
  total: number;
  scanned: number;
  remaining: number;
}

export function ScanSummary({ total, scanned, remaining }: ScanSummaryProps) {
  const percentage = total > 0 ? Math.round((scanned / total) * 100) : 0;

  return (
    <div className="space-y-2">
      <div className="flex justify-between text-sm text-muted-foreground">
        <span>Scanned: {scanned}</span>
        <span>Remaining: {remaining}</span>
        <span>Total: {total}</span>
      </div>
      <div className="h-2 rounded-full bg-muted overflow-hidden">
        <div
          className="h-full rounded-full bg-primary transition-all duration-300"
          style={{ width: `${percentage}%` }}
        />
      </div>
      <p className="text-xs text-muted-foreground text-right">{percentage}%</p>
    </div>
  );
}
