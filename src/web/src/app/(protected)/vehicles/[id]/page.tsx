"use client";

import Link from "next/link";
import { ArrowLeft, Pencil } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useVehicle, useVehicleHistory, useChangeVehicleStatus } from "@/hooks/use-vehicles";
import { VehicleStatus } from "@/types/vehicle";
import { useState } from "react";
import { useParams } from "next/navigation";
import { toast } from "sonner";

function getStatusBadgeVariant(status: VehicleStatus) {
  switch (status) {
    case VehicleStatus.AVAILABLE:
      return "success";
    case VehicleStatus.IN_USE:
      return "default";
    case VehicleStatus.MAINTENANCE:
      return "warning";
    case VehicleStatus.RETIRED:
      return "secondary";
    default:
      return "outline";
  }
}

export default function VehicleDetailPage() {
  const params = useParams();
  const id = params.id as string;
  const [newStatus, setNewStatus] = useState<string>("");

  const { data: vehicle, isLoading: vehicleLoading } = useVehicle(id);
  const { data: history, isLoading: historyLoading } = useVehicleHistory(id);
  const changeStatus = useChangeVehicleStatus();

  const handleStatusChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newStatus || newStatus === vehicle?.status) return;

    try {
      await changeStatus.mutateAsync({ id, newStatus: newStatus as VehicleStatus });
      toast.success("Vehicle status updated");
      setNewStatus("");
    } catch {
      toast.error("Failed to update vehicle status");
    }
  };

  if (vehicleLoading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!vehicle) {
    return <div className="p-6">Vehicle not found</div>;
  }

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/vehicles">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold">{vehicle.registrationPlate}</h1>
          <p className="text-muted-foreground">Vehicle Details</p>
        </div>
        <Link href={`/vehicles/${id}/edit`}>
          <Button>
            <Pencil className="h-4 w-4 mr-2" />
            Edit
          </Button>
        </Link>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Vehicle Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Type</p>
                <p>{vehicle.type}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Status</p>
                <Badge variant={getStatusBadgeVariant(vehicle.status)}>
                  {vehicle.status}
                </Badge>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Parcel Capacity
                </p>
                <p>{vehicle.parcelCapacity}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Weight Capacity
                </p>
                <p>{vehicle.weightCapacityKg} kg</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Depot</p>
                <p>{vehicle.depot?.name ?? "Not assigned"}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">
                  Created At
                </p>
                <p>{new Date(vehicle.createdAt).toLocaleDateString()}</p>
              </div>
            </div>

            <div className="pt-4 border-t">
              <p className="text-sm font-medium text-muted-foreground mb-2">
                Change Status
              </p>
              <form onSubmit={handleStatusChange} className="flex gap-2">
                <select
                  className="flex w-[180px] items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                  value={newStatus}
                  onChange={(e) => setNewStatus(e.target.value)}
                >
                  <option value="">Select new status</option>
                  {Object.values(VehicleStatus)
                    .filter((s) => s !== vehicle.status)
                    .map((status) => (
                      <option key={status} value={status}>
                        {status}
                      </option>
                    ))}
                </select>
                <button
                  type="submit"
                  className="px-4 py-2 bg-primary text-primary-foreground rounded-md text-sm font-medium disabled:opacity-50"
                  disabled={!newStatus || changeStatus.isPending}
                >
                  {changeStatus.isPending ? "Updating..." : "Update"}
                </button>
              </form>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Vehicle History</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {historyLoading ? (
              <p className="text-muted-foreground">Loading...</p>
            ) : history ? (
              <>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">
                      Total Mileage
                    </p>
                    <p>{history.totalMileageKm} km</p>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">
                      Routes Completed
                    </p>
                    <p>{history.totalRoutesCompleted}</p>
                  </div>
                </div>

                {history.routes.length > 0 && (
                  <div className="pt-4 border-t">
                    <p className="text-sm font-medium text-muted-foreground mb-2">
                      Recent Routes
                    </p>
                    <div className="space-y-2">
                      {history.routes.slice(0, 5).map((route) => (
                        <div
                          key={route.routeId}
                          className="flex justify-between text-sm"
                        >
                          <span>{route.routeName}</span>
                          <span className="text-muted-foreground">
                            {route.parcelCount} parcels
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </>
            ) : (
              <p className="text-muted-foreground">No history available</p>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
