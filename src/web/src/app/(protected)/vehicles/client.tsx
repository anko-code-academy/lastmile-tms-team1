"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";
import { VehicleStatus } from "@/types/vehicle";
import { VehicleTable } from "@/components/vehicles/vehicle-table";
import { useVehicles, useDeleteVehicle } from "@/lib/hooks/use-vehicles";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

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
        <Select
          value={statusParam ?? "all"}
          onValueChange={handleStatusFilter}
        >
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="Filter by status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Statuses</SelectItem>
            <SelectItem value={VehicleStatus.Available}>Available</SelectItem>
            <SelectItem value={VehicleStatus.InUse}>In Use</SelectItem>
            <SelectItem value={VehicleStatus.Maintenance}>Maintenance</SelectItem>
            <SelectItem value={VehicleStatus.Retired}>Retired</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <VehicleTable data={vehicles} onDelete={handleDelete} isDeleting={isDeleting} />
    </div>
  );
}
