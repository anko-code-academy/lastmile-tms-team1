"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";
import { VehicleStatus } from "@/types/vehicle";
import { VehicleTable } from "@/components/vehicles/vehicle-table";
import { useVehicles, useDeleteVehicle } from "@/lib/hooks/use-vehicles";

export function VehicleListClient() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isDeleting, setIsDeleting] = useState(false);

  const statusParam = searchParams.get("status") as VehicleStatus | null;
  const { data: vehicles = [], isLoading, error } = useVehicles(statusParam ?? undefined);
  const deleteVehicle = useDeleteVehicle();

  const handleDelete = async (id: string) => {
    setIsDeleting(true);
    try {
      await deleteVehicle.mutateAsync(id);
      toast.success("Vehicle deleted successfully");
    } catch {
      toast.error("Failed to delete vehicle");
    } finally {
      setIsDeleting(false);
    }
  };

  const handleStatusFilter = (value: string) => {
    if (value === "all") {
      router.push("/vehicles");
    } else {
      router.push(`/vehicles?status=${value}`);
    }
  };

  if (isLoading) {
    return <div className="py-8 text-center text-muted-foreground">Loading vehicles...</div>;
  }

  if (error) {
    return <div className="py-8 text-center text-destructive">Failed to load vehicles</div>;
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
          <option value={VehicleStatus.AVAILABLE}>Available</option>
          <option value={VehicleStatus.IN_USE}>In Use</option>
          <option value={VehicleStatus.MAINTENANCE}>Maintenance</option>
          <option value={VehicleStatus.RETIRED}>Retired</option>
        </select>
      </div>

      <VehicleTable data={vehicles} onDelete={handleDelete} isDeleting={isDeleting} />
    </div>
  );
}
