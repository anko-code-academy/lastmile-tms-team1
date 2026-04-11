"use client";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface RouteParcel {
  trackingNumber: string;
  status: string;
}

interface RouteParcelsTableProps {
  parcels: RouteParcel[];
  targetStatus: string;
}

function formatStatus(status: string): string {
  return status
    .replace(/_/g, " ")
    .toLowerCase()
    .replace(/\b\w/g, (c) => c.toUpperCase());
}

function getStatusClasses(status: string, targetStatus: string): string {
  if (status === targetStatus) {
    return "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100";
  }
  return "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-100";
}

export function RouteParcelsTable({ parcels, targetStatus }: RouteParcelsTableProps) {
  const sorted = [...parcels].sort((a, b) => {
    if (a.status === targetStatus && b.status !== targetStatus) return -1;
    if (a.status !== targetStatus && b.status === targetStatus) return 1;
    return a.trackingNumber.localeCompare(b.trackingNumber);
  });

  const doneCount = parcels.filter((p) => p.status === targetStatus).length;

  return (
    <div className="space-y-2">
      <div className="text-sm text-muted-foreground">
        {doneCount} / {parcels.length} parcels {formatStatus(targetStatus).toLowerCase()}
      </div>
      <div className="rounded-md border overflow-auto">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Tracking #</TableHead>
              <TableHead>Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {sorted.map((parcel) => (
              <TableRow key={parcel.trackingNumber}>
                <TableCell className="font-mono text-sm">{parcel.trackingNumber}</TableCell>
                <TableCell>
                  <span
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${getStatusClasses(parcel.status, targetStatus)}`}
                  >
                    {formatStatus(parcel.status)}
                  </span>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
