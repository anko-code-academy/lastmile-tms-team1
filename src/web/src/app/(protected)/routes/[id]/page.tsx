"use client";

import Link from "next/link";
import { ArrowLeft, Pencil } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useRoute } from "@/lib/hooks/use-routes";
import { RouteStatus } from "@/types/route";
import { useParams } from "next/navigation";

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
  const id = params.id as string;

  const { data: route, isLoading } = useRoute(id);

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
                <p>{route.vehiclePlate ?? "Not assigned"}</p>
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
