"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState, useCallback } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import type { SortingState } from "@tanstack/react-table";
import { RouteStatus } from "@/graphql/generated/graphql";
import { RouteTable } from "@/components/routes/route-table";
import { RouteMap } from "@/components/routes/route-map";
import { useRoutes, useRoutesForMap, useDeleteRoute } from "@/hooks/use-routes";
import { useZones } from "@/hooks/use-zones";
import { useDepots } from "@/hooks/use-depots";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];
const DEFAULT_PAGE_SIZE = 25;

const SORT_FIELD_MAP: Record<string, string> = {
  name: "name",
  status: "status",
  plannedStartTime: "plannedStartTime",
  totalDistanceKm: "totalDistanceKm",
  totalParcelCount: "totalParcelCount",
};

function sortingToOrder(sorting: SortingState): Record<string, string>[] | undefined {
  if (sorting.length === 0) return undefined;
  return sorting
    .filter((s) => SORT_FIELD_MAP[s.id])
    .map((s) => ({ [SORT_FIELD_MAP[s.id]]: s.desc ? "DESC" : "ASC" }));
}

export function RouteListClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isDeleting, setIsDeleting] = useState(false);

  const statusParam = searchParams.get("status") as RouteStatus | null;
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [cursor, setCursor] = useState<string | undefined>(undefined);
  const [history, setHistory] = useState<string[]>([]);
  const [dateFilter, setDateFilter] = useState<string>("");
  const [sorting, setSorting] = useState<SortingState>([]);

  const resetPagination = () => {
    setCursor(undefined);
    setHistory([]);
  };

  const filters = {
    status: statusParam ?? undefined,
    date: dateFilter || undefined,
    first: pageSize,
    after: cursor,
    order: sortingToOrder(sorting),
  };

  const { data, isLoading, error } = useRoutes(filters);
  const routes = data?.nodes ?? [];
  const pageInfo = data?.pageInfo;
  const totalCount = data?.totalCount ?? 0;

  const deleteRouteMutation = useDeleteRoute();

  const { data: mapRoutes } = useRoutesForMap(dateFilter || undefined);
  const { data: zones } = useZones();
  const { data: depots } = useDepots();

  const handleDelete = async (id: string) => {
    setIsDeleting(true);
    try {
      await deleteRouteMutation.mutateAsync(id);
      toast.success("Route deleted successfully");
    } catch {
      toast.error("Failed to delete route");
    } finally {
      setIsDeleting(false);
    }
  };

  const handleStatusFilter = (value: string) => {
    resetPagination();
    if (value === "all") {
      router.push("/routes");
    } else {
      router.push(`/routes?status=${value}`);
    }
  };

  const handleDateFilter = (value: string) => {
    setDateFilter(value);
    resetPagination();
  };

  const handleSortingChange = useCallback((newSorting: SortingState) => {
    setSorting(newSorting);
    resetPagination();
  }, []);

  const handleNextPage = () => {
    if (pageInfo?.endCursor) {
      setHistory((prev) => [...prev, cursor ?? ""]);
      setCursor(pageInfo.endCursor);
    }
  };

  const handlePreviousPage = () => {
    const prev = history[history.length - 1];
    setHistory((h) => h.slice(0, -1));
    setCursor(prev || undefined);
  };

  if (isLoading) {
    return <div className="py-8 text-center text-muted-foreground">Loading routes...</div>;
  }

  if (error) {
    return <div className="py-8 text-center text-destructive">Failed to load routes</div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-4">
        <select
          className="flex w-[200px] items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          value={statusParam ?? "all"}
          onChange={(e) => handleStatusFilter(e.target.value)}
        >
          <option value="all">All Statuses</option>
          <option value={RouteStatus.Draft}>Draft</option>
          <option value={RouteStatus.InProgress}>In Progress</option>
          <option value={RouteStatus.Completed}>Completed</option>
        </select>

        <input
          type="date"
          className="flex w-[180px] items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          value={dateFilter}
          onChange={(e) => handleDateFilter(e.target.value)}
          placeholder="Filter by date"
        />
        {dateFilter && (
          <Button
            variant="ghost"
            size="sm"
            onClick={() => handleDateFilter("")}
          >
            Clear date
          </Button>
        )}

        <Select
          value={String(pageSize)}
          onValueChange={(value) => {
            setPageSize(Number(value));
            resetPagination();
          }}
        >
          <SelectTrigger className="inline-flex shrink-0 items-center justify-center text-sm font-medium border border-input bg-transparent rounded-md h-9 px-3 w-[120px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {PAGE_SIZE_OPTIONS.map((size) => (
              <SelectItem key={size} value={String(size)}>
                {size} / page
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {dateFilter && (
        <RouteMap
          routes={mapRoutes?.nodes?.filter((r): r is NonNullable<typeof r> => !!r) ?? []}
          zones={zones ?? []}
          depots={depots ?? []}
        />
      )}

      <RouteTable
        data={routes}
        onDelete={handleDelete}
        isDeleting={isDeleting}
        onSortingChange={handleSortingChange}
      />

      {totalCount > 0 && (
        <div className="flex items-center justify-between pt-4">
          <div className="text-sm text-muted-foreground">
            Showing {routes.length} of {totalCount}
          </div>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={handlePreviousPage}
              disabled={!pageInfo?.hasPreviousPage}
            >
              <ChevronLeft className="size-4 mr-1" />
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={handleNextPage}
              disabled={!pageInfo?.hasNextPage}
            >
              Next
              <ChevronRight className="size-4 ml-1" />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
