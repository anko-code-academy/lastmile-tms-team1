"use client";

import Link from "next/link";
import { ArrowLeft, Pencil, Plus, Trash2, Zap, ArrowUpDown } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  useRoute,
  useChangeRouteStatus,
  useAutoAssignParcelsByZone,
  useRemoveParcelsFromRoute,
  useOptimizeRouteStopOrder,
} from "@/hooks/use-routes";
import { RouteStatus, RouteStopStatus } from "@/graphql/generated/graphql";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";

function getStatusBadgeVariant(status: RouteStatus) {
  switch (status) {
    case RouteStatus.Draft:
      return "default";
    case RouteStatus.InProgress:
      return "warning";
    case RouteStatus.Completed:
      return "success";
    default:
      return "outline";
  }
}

function getStopStatusBadgeVariant(status: RouteStopStatus) {
  switch (status) {
    case RouteStopStatus.Pending:
      return "outline";
    case RouteStopStatus.Arrived:
      return "warning";
    case RouteStopStatus.Completed:
      return "success";
    case RouteStopStatus.Skipped:
      return "secondary";
    default:
      return "outline";
  }
}

export default function RouteDetailPage() {
  const params = useParams();
  const router = useRouter();
  const id = params.id as string;
  const [newStatus, setNewStatus] = useState<string>("");

  const { data: route, isLoading } = useRoute(id);
  const changeStatus = useChangeRouteStatus();
  const autoAssign = useAutoAssignParcelsByZone();
  const removeParcels = useRemoveParcelsFromRoute();
  const optimizeStops = useOptimizeRouteStopOrder();

  const handleStatusChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newStatus || newStatus === route?.status) return;

    try {
      await changeStatus.mutateAsync({ id, newStatus: newStatus as RouteStatus });
      setNewStatus("");
      router.refresh();
    } catch {
      toast.error("Failed to update route status");
    }
  };

  const handleAutoAssign = async () => {
    try {
      await autoAssign.mutateAsync(id);
    } catch {
      // error toast handled by hook
    }
  };

  const handleOptimizeStops = async () => {
    try {
      await optimizeStops.mutateAsync(id);
    } catch {
      // error toast handled by hook
    }
  };

  const handleRemoveParcel = async (parcelId: string) => {
    try {
      await removeParcels.mutateAsync({ routeId: id, parcelIds: [parcelId] });
    } catch {
      // error toast handled by hook
    }
  };

  if (isLoading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!route) {
    return <div className="p-6">Route not found</div>;
  }

  const plannedStart = new Date(route.plannedStartTime);
  const actualStart = route.actualStartTime ? new Date(route.actualStartTime) : null;
  const actualEnd = route.actualEndTime ? new Date(route.actualEndTime) : null;
  const isDraft = route.status === RouteStatus.Draft;

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/routes">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div className="flex-1">
          <h1 className="text-3xl font-bold">{route.name}</h1>
          <p className="text-muted-foreground">Route Details</p>
        </div>
        <Link href={`/routes/${id}/edit`}>
          <Button>
            <Pencil className="h-4 w-4 mr-2" />
            Edit
          </Button>
        </Link>
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Route Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Status</p>
                <Badge variant={getStatusBadgeVariant(route.status)}>
                  {route.status.replace("_", " ")}
                </Badge>
                <form onSubmit={handleStatusChange} className="flex gap-2 mt-2">
                  <select
                    className="flex w-[160px] items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                    value={newStatus}
                    onChange={(e) => setNewStatus(e.target.value)}
                  >
                    <option value="">Change status</option>
                    {Object.values(RouteStatus)
                      .filter((s) => s !== route.status)
                      .map((status) => (
                        <option key={status} value={status}>
                          {status.replace("_", " ")}
                        </option>
                      ))}
                  </select>
                  <button
                    type="submit"
                    className="px-3 py-2 bg-primary text-primary-foreground rounded-md text-sm font-medium disabled:opacity-50"
                    disabled={!newStatus || changeStatus.isPending}
                  >
                    {changeStatus.isPending ? "..." : "Update"}
                  </button>
                </form>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Parcels</p>
                <p className="text-2xl font-bold">{route.totalParcelCount}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Stops</p>
                <p className="text-2xl font-bold">{route.routeStops.length}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Zone</p>
                <p>{route.zone?.name ?? "Not assigned"}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Distance</p>
                <p>{route.totalDistanceKm > 0 ? `${route.totalDistanceKm} km` : "—"}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Vehicle</p>
                <p>{route.vehicle?.registrationPlate ?? "Not assigned"}</p>
              </div>
            </div>

            <div className="pt-4 border-t">
              <h3 className="text-sm font-medium mb-3">Schedule</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Planned Start</p>
                  <p>{plannedStart.toLocaleDateString()}</p>
                  <p className="text-sm">{plannedStart.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</p>
                </div>
                {actualStart && (
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Actual Start</p>
                    <p>{actualStart.toLocaleDateString()}</p>
                    <p className="text-sm">{actualStart.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</p>
                  </div>
                )}
                {actualEnd && (
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">Actual End</p>
                    <p>{actualEnd.toLocaleDateString()}</p>
                    <p className="text-sm">{actualEnd.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</p>
                  </div>
                )}
              </div>
            </div>

            <div className="pt-4 border-t">
              <p className="text-sm font-medium text-muted-foreground">
                Created At
              </p>
              <p>{new Date(route.createdAt).toLocaleDateString()}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Stops ({route.routeStops.length})</CardTitle>
            {isDraft && (
              <div className="flex gap-2">
                {route.routeStops.length >= 2 && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleOptimizeStops}
                    disabled={optimizeStops.isPending}
                  >
                    <ArrowUpDown className="h-4 w-4 mr-1" />
                    {optimizeStops.isPending ? "Optimizing..." : "Optimize Order"}
                  </Button>
                )}
                {route.zoneId && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={handleAutoAssign}
                    disabled={autoAssign.isPending}
                  >
                    <Zap className="h-4 w-4 mr-1" />
                    {autoAssign.isPending ? "Assigning..." : "Auto-Assign by Zone"}
                  </Button>
                )}
                <Button variant="outline" size="sm" asChild>
                  <Link href={`/routes/${id}/edit`}>
                    <Plus className="h-4 w-4 mr-1" />
                    Add Parcels
                  </Link>
                </Button>
              </div>
            )}
          </CardHeader>
          <CardContent>
            {route.routeStops.length === 0 ? (
              <div className="text-center py-8 text-muted-foreground">
                <p>No stops yet.</p>
                {isDraft && (
                  <p className="text-sm mt-1">
                    Add parcels to create stops, or auto-assign by zone.
                  </p>
                )}
              </div>
            ) : (
              <div className="space-y-4">
                {route.routeStops
                  .sort((a, b) => a.sequenceNumber - b.sequenceNumber)
                  .map((stop) => (
                    <div
                      key={stop.id}
                      className="rounded-lg border p-4 space-y-2"
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span className="flex items-center justify-center w-6 h-6 rounded-full bg-primary text-primary-foreground text-xs font-bold">
                            {stop.sequenceNumber}
                          </span>
                          <span className="font-medium">{stop.street1}</span>
                        </div>
                        <Badge variant={getStopStatusBadgeVariant(stop.status)}>
                          {stop.status.replace("_", " ")}
                        </Badge>
                      </div>

                      {stop.parcels.length > 0 && (
                        <div className="ml-8 space-y-1">
                          {stop.parcels.map((parcel) => (
                            <div
                              key={parcel.id}
                              className="flex items-center justify-between text-sm py-1 px-2 rounded bg-muted/50"
                            >
                              <div className="flex items-center gap-2">
                                <Link href={`/parcels/${parcel.trackingNumber}`} className="font-mono text-xs hover:underline">
                                  {parcel.trackingNumber}
                                </Link>
                                <Badge variant="outline" className="text-xs">
                                  {parcel.status.replace("_", " ")}
                                </Badge>
                              </div>
                              {isDraft && (
                                <Button
                                  variant="ghost"
                                  size="icon-sm"
                                  onClick={() => handleRemoveParcel(parcel.id)}
                                  disabled={removeParcels.isPending}
                                >
                                  <Trash2 className="h-3 w-3 text-destructive" />
                                </Button>
                              )}
                            </div>
                          ))}
                        </div>
                      )}

                      {stop.estimatedServiceMinutes > 0 && (
                        <p className="ml-8 text-xs text-muted-foreground">
                          Est. service: {stop.estimatedServiceMinutes} min
                        </p>
                      )}
                    </div>
                  ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
