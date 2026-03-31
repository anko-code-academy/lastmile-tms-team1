"use client";

import Link from "next/link";
import { ArrowLeft, Pencil } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useRoute, useChangeRouteStatus } from "@/hooks/use-routes";
import { RouteStatus } from "@/types/route";
import { useParams, useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";

function getStatusBadgeVariant(status: RouteStatus) {
  switch (status) {
    case RouteStatus.PLANNED:
      return "default";
    case RouteStatus.IN_PROGRESS:
      return "warning";
    case RouteStatus.COMPLETED:
      return "success";
    case RouteStatus.CANCELLED:
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

  const handleStatusChange = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newStatus || newStatus === route?.status) return;

    try {
      await changeStatus.mutateAsync({ id, newStatus: newStatus as RouteStatus });
      toast.success("Route status updated");
      setNewStatus("");
      router.refresh();
    } catch {
      toast.error("Failed to update route status");
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
                <p>{route.totalParcelCount}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Total Distance</p>
                <p>{route.totalDistanceKm} km</p>
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
      </div>
    </div>
  );
}
