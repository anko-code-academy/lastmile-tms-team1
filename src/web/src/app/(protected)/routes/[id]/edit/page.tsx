"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Plus } from "lucide-react";
import { toast } from "sonner";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RouteForm } from "@/components/routes/route-form";
import { useRoute, useUpdateRoute, useAddParcelsToRoute } from "@/hooks/use-routes";
import { useParcels } from "@/hooks/use-parcels";
import { useParams } from "next/navigation";
import { RouteStatus } from "@/graphql/generated/graphql";
import type { ColumnKey } from "@/components/parcel/column-registry";

const PARCEL_PICK_COLUMNS: ColumnKey[] = [
  "trackingNumber",
  "status",
  "recipientStreet",
  "recipientCity",
  "zone",
];

export default function EditRoutePage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  const [selectedParcelIds, setSelectedParcelIds] = useState<Set<string>>(new Set());

  const { data: route, isLoading } = useRoute(id);
  const updateRoute = useUpdateRoute();
  const addParcels = useAddParcelsToRoute();

  const isDraft = route?.status === RouteStatus.Draft;

  const { data: parcelsData, isLoading: parcelsLoading } = useParcels({
    zoneId: route?.zoneId ?? undefined,
    columns: PARCEL_PICK_COLUMNS,
    first: 100,
  });

  // Exclude parcels already on this route
  const assignedParcelIds = new Set(
    route?.routeStops?.flatMap((s) => s.parcels.map((p) => p.id)) ?? [],
  );
  const availableParcels = (parcelsData?.nodes ?? []).filter(
    (p) => !assignedParcelIds.has(p.id),
  );

  const handleSubmit = async (values: {
    name: string;
    plannedStartTime: string;
    zoneId?: string | null;
    vehicleId?: string | null;
    driverId?: string | null;
  }) => {
    try {
      const plannedStartTime = new Date(values.plannedStartTime).toISOString();
      await updateRoute.mutateAsync({ id, ...values, plannedStartTime });
      router.push(`/routes/${id}`);
    } catch {
      toast.error("Failed to update route");
    }
  };

  const handleAddParcels = async () => {
    if (selectedParcelIds.size === 0) return;
    try {
      await addParcels.mutateAsync({
        routeId: id,
        parcelIds: [...selectedParcelIds],
      });
      setSelectedParcelIds(new Set());
    } catch {
      // handled by hook
    }
  };

  const toggleParcel = (parcelId: string) => {
    setSelectedParcelIds((prev) => {
      const next = new Set(prev);
      if (next.has(parcelId)) next.delete(parcelId);
      else next.add(parcelId);
      return next;
    });
  };

  const toggleAll = () => {
    if (selectedParcelIds.size === availableParcels.length) {
      setSelectedParcelIds(new Set());
    } else {
      setSelectedParcelIds(new Set(availableParcels.map((p) => p.id)));
    }
  };

  if (isLoading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!route) {
    return <div className="p-6">Route not found</div>;
  }

  const formatDateTimeLocal = (isoString: string) => {
    const date = new Date(isoString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href={`/routes/${id}`}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Edit Route</h1>
          <p className="text-muted-foreground">
            Update route {route.name}
          </p>
        </div>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Route Information</CardTitle>
        </CardHeader>
        <CardContent>
          <RouteForm
            defaultValues={{
              name: route.name,
              plannedStartTime: formatDateTimeLocal(route.plannedStartTime),
              zoneId: route.zoneId,
              vehicleId: route.vehicleId,
              driverId: route.driverId,
            }}
            onSubmit={handleSubmit}
            isSubmitting={updateRoute.isPending}
          />
        </CardContent>
      </Card>

      {isDraft && (
        <Card className="max-w-2xl mt-6">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Add Parcels</CardTitle>
            <Button
              size="sm"
              onClick={handleAddParcels}
              disabled={selectedParcelIds.size === 0 || addParcels.isPending}
            >
              <Plus className="h-4 w-4 mr-1" />
              {addParcels.isPending
                ? "Adding..."
                : selectedParcelIds.size > 0
                  ? `Add ${selectedParcelIds.size} Selected`
                  : "Add Selected"}
            </Button>
          </CardHeader>
          <CardContent>
            {parcelsLoading ? (
              <p className="text-muted-foreground">Loading parcels...</p>
            ) : availableParcels.length === 0 ? (
              <p className="text-muted-foreground">
                No available parcels
                {route.zoneId ? ` in zone ${route.zone?.name ?? ""}` : ""}.
              </p>
            ) : (
              <div className="border rounded-md">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b bg-muted/50">
                      <th className="p-2 text-left w-10">
                        <input
                          type="checkbox"
                          checked={
                            availableParcels.length > 0 &&
                            selectedParcelIds.size === availableParcels.length
                          }
                          onChange={toggleAll}
                        />
                      </th>
                      <th className="p-2 text-left">Tracking #</th>
                      <th className="p-2 text-left">Status</th>
                      <th className="p-2 text-left">Address</th>
                      {!route.zoneId && <th className="p-2 text-left">Zone</th>}
                    </tr>
                  </thead>
                  <tbody>
                    {availableParcels.map((parcel) => (
                      <tr
                        key={parcel.id}
                        className={`border-b last:border-0 ${selectedParcelIds.has(parcel.id) ? "bg-primary/5" : "hover:bg-muted/30"}`}
                      >
                        <td className="p-2">
                          <input
                            type="checkbox"
                            checked={selectedParcelIds.has(parcel.id)}
                            onChange={() => toggleParcel(parcel.id)}
                          />
                        </td>
                        <td className="p-2 font-mono text-xs">
                          {parcel.trackingNumber}
                        </td>
                        <td className="p-2">{(parcel.status as string)?.replace("_", " ")}</td>
                        <td className="p-2">
                          {parcel.recipientAddress?.street1}
                          {parcel.recipientAddress?.city &&
                            `, ${parcel.recipientAddress.city}`}
                        </td>
                        {!route.zoneId && (
                          <td className="p-2">{parcel.zone?.name ?? "\u2014"}</td>
                        )}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
