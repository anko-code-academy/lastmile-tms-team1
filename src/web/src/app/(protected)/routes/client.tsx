"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";
import { RouteStatus } from "@/types/route";
import { RouteTable } from "@/components/routes/route-table";
import { useRoutes, useDeleteRoute } from "@/lib/hooks/use-routes";

export function RouteListClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isDeleting, setIsDeleting] = useState(false);

  const statusParam = searchParams.get("status") as RouteStatus | null;
  const { data: routes = [], isLoading, error } = useRoutes(statusParam ?? undefined);
  const deleteRoute = useDeleteRoute();

  const handleDelete = async (id: string) => {
    setIsDeleting(true);
    try {
      await deleteRoute.mutateAsync(id);
      toast.success("Route deleted successfully");
    } catch {
      toast.error("Failed to delete route");
    } finally {
      setIsDeleting(false);
    }
  };

  const handleStatusFilter = (value: string) => {
    if (value === "all") {
      router.push("/routes");
    } else {
      router.push(`/routes?status=${value}`);
    }
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
          <option value={RouteStatus.PLANNED}>Planned</option>
          <option value={RouteStatus.IN_PROGRESS}>In Progress</option>
          <option value={RouteStatus.COMPLETED}>Completed</option>
          <option value={RouteStatus.CANCELLED}>Cancelled</option>
        </select>
      </div>

      <RouteTable data={routes} onDelete={handleDelete} isDeleting={isDeleting} />
    </div>
  );
}
