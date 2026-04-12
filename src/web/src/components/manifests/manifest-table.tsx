"use client";

import { useState } from "react";
import Link from "next/link";
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  createColumnHelper,
  type SortingState,
} from "@tanstack/react-table";
import { ArrowUpDown, Eye } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  ManifestStatus,
  ManifestItemStatus,
  type GetManifestsQuery,
} from "@/graphql/generated/graphql";

type ManifestNode = NonNullable<NonNullable<GetManifestsQuery["manifests"]>["nodes"]>[number];

const columnHelper = createColumnHelper<ManifestNode>();

function getStatusBadgeVariant(status: ManifestStatus) {
  switch (status) {
    case ManifestStatus.Open:
      return "default";
    case ManifestStatus.Receiving:
      return "warning";
    case ManifestStatus.Completed:
      return "success";
    default:
      return "outline";
  }
}

function formatDate(date: string | null | undefined) {
  if (!date) return "\u2014";
  return new Date(date).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

interface ManifestTableProps {
  data: ManifestNode[];
}

export function ManifestTable({ data }: ManifestTableProps) {
  const [sorting, setSorting] = useState<SortingState>([]);

  const columns = [
    columnHelper.accessor("name", {
      header: ({ column }) => (
        <Button
          variant="ghost"
          onClick={() => column.toggleSorting(column.getIsSorted() === "asc")}
        >
          Name
          <ArrowUpDown className="ml-2 h-4 w-4" />
        </Button>
      ),
      cell: (info) => info.getValue(),
    }),
    columnHelper.accessor("status", {
      header: "Status",
      cell: (info) => (
        <Badge variant={getStatusBadgeVariant(info.getValue())}>
          {info.getValue()}
        </Badge>
      ),
    }),
    columnHelper.accessor("depot", {
      header: "Depot",
      cell: (info) => info.getValue()?.name ?? "Unknown",
    }),
    columnHelper.display({
      id: "expected",
      header: "Expected",
      cell: ({ row }) =>
        row.original.items.filter((i) => i.status === ManifestItemStatus.Expected).length,
    }),
    columnHelper.display({
      id: "received",
      header: "Received",
      cell: ({ row }) =>
        row.original.items.filter((i) => i.status === ManifestItemStatus.Received).length,
    }),
    columnHelper.display({
      id: "unexpected",
      header: "Unexpected",
      cell: ({ row }) =>
        row.original.items.filter((i) => i.status === ManifestItemStatus.Unexpected).length,
    }),
    columnHelper.display({
      id: "missing",
      header: "Missing",
      cell: ({ row }) =>
        row.original.items.filter((i) => i.status === ManifestItemStatus.Missing).length,
    }),
    columnHelper.accessor("startedAt", {
      header: "Started At",
      cell: (info) => formatDate(info.getValue()),
    }),
    columnHelper.accessor("completedAt", {
      header: "Completed At",
      cell: (info) => formatDate(info.getValue()),
    }),
    columnHelper.display({
      id: "actions",
      header: "Actions",
      cell: ({ row }) => (
        <Button variant="ghost" size="icon-sm" asChild>
          <Link href={`/depot/inbound?manifestId=${row.original.id}`}>
            <Eye className="h-4 w-4" />
          </Link>
        </Button>
      ),
    }),
  ];

  const table = useReactTable({
    data,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
  });

  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead key={header.id}>
                  {header.isPlaceholder
                    ? null
                    : flexRender(
                        header.column.columnDef.header,
                        header.getContext()
                      )}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {table.getRowModel().rows?.length ? (
            table.getRowModel().rows.map((row) => (
              <TableRow key={row.id}>
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </TableCell>
                ))}
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell colSpan={columns.length} className="h-24 text-center">
                No manifests found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
}
