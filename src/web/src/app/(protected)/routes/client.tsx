"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { RouteStatus } from "@/graphql/generated/graphql";
import { RouteTable } from "@/components/routes/route-table";
import { useRoutes, useDeleteRoute } from "@/hooks/use-routes";
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

export function RouteListClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isDeleting, setIsDeleting] = useState(false);

  const statusParam = searchParams.get("status") as RouteStatus | null;
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE);
  const [cursor, setCursor] = useState<string | undefined>(undefined);
  const [history, setHistory] = useState<string[]>([]);

  const resetPagination = () => {
    setCursor(undefined);
    setHistory([]);
  };

  const filters = {
    status: statusParam ?? undefined,
    first: pageSize,
    after: cursor,
  };

  const { data, isLoading, error } = useRoutes(filters);
  const routes = data?.nodes ?? [];
  const pageInfo = data?.pageInfo;
  const totalCount = data?.totalCount ?? 0;

  const deleteRouteMutation = useDeleteRoute();

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

      <RouteTable data={routes} onDelete={handleDelete} isDeleting={isDeleting} />

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
