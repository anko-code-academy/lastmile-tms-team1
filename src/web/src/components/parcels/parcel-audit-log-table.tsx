"use client";

import { useParcelAuditLogs } from "@/hooks/use-parcels";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";

interface ParcelAuditLogTableProps {
  parcelId: string;
}

export function ParcelAuditLogTable({ parcelId }: ParcelAuditLogTableProps) {
  const { data, isLoading, fetchNextPage, hasNextPage, isFetchingNextPage } =
    useParcelAuditLogs(parcelId);

  if (isLoading) {
    return <div className="text-center py-4 text-muted-foreground">Loading audit log...</div>;
  }

  const nodes = data?.pages.flatMap((page) => page?.nodes ?? []) ?? [];

  if (nodes.length === 0) {
    return (
      <div className="text-center py-4 text-muted-foreground">
        No changes recorded.
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Field</TableHead>
            <TableHead>Old Value</TableHead>
            <TableHead>New Value</TableHead>
            <TableHead>Changed By</TableHead>
            <TableHead>Timestamp</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {nodes.map((log) => (
            <TableRow key={String(log.id)}>
              <TableCell className="font-medium">{log.propertyName}</TableCell>
              <TableCell className="text-muted-foreground">{log.oldValue ?? "—"}</TableCell>
              <TableCell>{log.newValue ?? "—"}</TableCell>
              <TableCell>{log.changedBy}</TableCell>
              <TableCell className="text-muted-foreground">
                {new Date(log.createdAt).toLocaleString()}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      {hasNextPage && (
        <div className="flex justify-center">
          <Button
            variant="outline"
            size="sm"
            onClick={() => fetchNextPage()}
            disabled={isFetchingNextPage}
          >
            {isFetchingNextPage ? "Loading..." : "Load more"}
          </Button>
        </div>
      )}
    </div>
  );
}
