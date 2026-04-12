"use client";

import { useState } from "react";
import Link from "next/link";
import { useParcel, useTrackingEvents, useChangeParcelStatus } from "@/hooks/use-parcels";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ParcelStatus } from "@/graphql/generated/graphql";
import { Printer, Plus, Pencil, XCircle, ChevronRight, ArrowRight } from "lucide-react";
import { ParcelStatusTimeline } from "./parcel-status-timeline";
import { ParcelAuditLogTable } from "./parcel-audit-log-table";
import { EditParcelDialog } from "./edit-parcel-dialog";
import { CancelParcelDialog } from "./cancel-parcel-dialog";

const NEXT_STATUSES: Record<ParcelStatus, ParcelStatus[]> = {
  [ParcelStatus.Registered]: [ParcelStatus.ReceivedAtDepot],
  [ParcelStatus.ReceivedAtDepot]: [ParcelStatus.Sorted],
  [ParcelStatus.Sorted]: [ParcelStatus.Staged],
  [ParcelStatus.Staged]: [ParcelStatus.Loaded],
  [ParcelStatus.Loaded]: [ParcelStatus.OutForDelivery],
  [ParcelStatus.OutForDelivery]: [ParcelStatus.Delivered, ParcelStatus.FailedAttempt],
  [ParcelStatus.FailedAttempt]: [ParcelStatus.OutForDelivery, ParcelStatus.ReturnedToDepot],
  [ParcelStatus.ReturnedToDepot]: [],
  [ParcelStatus.Delivered]: [],
  [ParcelStatus.Cancelled]: [],
  [ParcelStatus.Exception]: [ParcelStatus.ReturnedToDepot],
};

function formatStatus(status: ParcelStatus): string {
  return status.replace(/_/g, " ").toLowerCase().replace(/^\w/, (c) => c.toUpperCase());
}

function getStatusVariant(status: ParcelStatus): "default" | "secondary" | "success" | "warning" | "destructive" {
  switch (status) {
    case ParcelStatus.Registered:
      return "secondary";
    case ParcelStatus.OutForDelivery:
      return "warning";
    case ParcelStatus.Delivered:
      return "success";
    case ParcelStatus.FailedAttempt:
    case ParcelStatus.ReturnedToDepot:
    case ParcelStatus.Cancelled:
    case ParcelStatus.Exception:
      return "destructive";
    default:
      return "secondary";
  }
}

const CANCELLABLE_STATUSES = new Set([
  ParcelStatus.Registered,
  ParcelStatus.ReceivedAtDepot,
  ParcelStatus.Sorted,
  ParcelStatus.Staged,
]);

interface ParcelDetailProps {
  trackingNumber: string;
}

export function ParcelDetail({ trackingNumber }: ParcelDetailProps) {
  const { data: parcel, isLoading } = useParcel(trackingNumber);
  const { data: trackingEventsData } = useTrackingEvents(parcel?.id ? String(parcel.id) : "");
  const [editOpen, setEditOpen] = useState(false);
  const [cancelOpen, setCancelOpen] = useState(false);
  const changeStatus = useChangeParcelStatus();

  const events = trackingEventsData?.pages.flatMap((page) => page?.nodes ?? []) ?? [];

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-1 text-sm text-muted-foreground">
          <Link href="/parcels" className="hover:underline">Parcels</Link>
          <ChevronRight className="h-3 w-3" />
          <span>Loading...</span>
        </div>
        <div className="text-center py-8 text-muted-foreground">Loading parcel...</div>
      </div>
    );
  }

  if (!parcel) {
    return (
      <div className="space-y-4">
        <div className="flex items-center gap-1 text-sm text-muted-foreground">
          <Link href="/parcels" className="hover:underline">Parcels</Link>
          <ChevronRight className="h-3 w-3" />
          <span>Not Found</span>
        </div>
        <div className="text-center py-8 text-muted-foreground">Parcel not found.</div>
      </div>
    );
  }

  const canEditOrCancel = CANCELLABLE_STATUSES.has(parcel.status);
  const nextStatuses = NEXT_STATUSES[parcel.status] ?? [];

  function handleStatusChange(newStatus: ParcelStatus) {
    changeStatus.mutate({ id: parcel!.id, newStatus });
  }

  function reprintLabel() {
    window.open(`/api/labels/${parcel!.id}/pdf`, "_blank");
  }

  return (
    <div className="p-6 space-y-4">
      {/* Breadcrumb */}
      <div className="flex items-center gap-1 text-sm text-muted-foreground">
        <Link href="/parcels" className="hover:underline">Parcels</Link>
        <ChevronRight className="h-3 w-3" />
        <span className="text-foreground font-medium">{parcel.trackingNumber}</span>
      </div>

      {/* Header with status + actions */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-xl font-semibold font-mono">{parcel.trackingNumber}</h1>
          <Badge variant={getStatusVariant(parcel.status)}>{formatStatus(parcel.status)}</Badge>
          {parcel.parcelType && (
            <Badge variant="outline">{parcel.parcelType}</Badge>
          )}
          <span className="text-sm text-muted-foreground">
            Created {new Date(parcel.createdAt).toLocaleString()}
          </span>
        </div>
        <div className="flex items-center gap-2">
          {canEditOrCancel && (
            <>
              <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
                <Pencil className="h-4 w-4 mr-1" />
                Edit
              </Button>
              <Button variant="outline" size="sm" onClick={() => setCancelOpen(true)} className="text-destructive hover:text-destructive">
                <XCircle className="h-4 w-4 mr-1" />
                Cancel
              </Button>
            </>
          )}
          {nextStatuses.length > 0 && (
            <div className="relative inline-flex items-center">
              <ArrowRight className="h-4 w-4 absolute left-2.5 pointer-events-none" />
              <select
                className="flex h-8 items-center gap-1 rounded-md border border-input bg-background px-3 pl-8 text-sm font-medium ring-offset-background hover:bg-accent hover:text-accent-foreground disabled:cursor-not-allowed disabled:opacity-50 appearance-none cursor-pointer"
                value=""
                onChange={(e) => e.target.value && handleStatusChange(e.target.value as ParcelStatus)}
                disabled={changeStatus.isPending}
              >
                <option value="" disabled>
                  {changeStatus.isPending ? "Changing..." : "Change Status"}
                </option>
                {nextStatuses.map((s) => (
                  <option key={s} value={s}>
                    {formatStatus(s)}
                  </option>
                ))}
              </select>
            </div>
          )}
          <Button variant="outline" size="sm" onClick={reprintLabel}>
            <Printer className="h-4 w-4 mr-1" />
            Reprint
          </Button>
          <Link
            href="/parcels/new"
            className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            <Plus className="size-4" />
            New Parcel
          </Link>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {/* Sender */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Shipper Address</CardTitle>
          </CardHeader>
          <CardContent className="space-y-1 text-sm">
            {parcel.shipperAddress.companyName && (
              <p className="font-medium">{parcel.shipperAddress.companyName}</p>
            )}
            {parcel.shipperAddress.contactName && (
              <p>{parcel.shipperAddress.contactName}</p>
            )}
            <p>{parcel.shipperAddress.street1}</p>
            {parcel.shipperAddress.street2 && <p>{parcel.shipperAddress.street2}</p>}
            <p>{parcel.shipperAddress.city}, {parcel.shipperAddress.state} {parcel.shipperAddress.postalCode}</p>
            <p>{parcel.shipperAddress.countryCode}</p>
            {parcel.shipperAddress.phone && <p className="text-muted-foreground">{parcel.shipperAddress.phone}</p>}
            {parcel.shipperAddress.email && <p className="text-muted-foreground">{parcel.shipperAddress.email}</p>}
          </CardContent>
        </Card>

        {/* Recipient + zone */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Recipient Address</CardTitle>
          </CardHeader>
          <CardContent className="space-y-1 text-sm">
            {parcel.recipientAddress.companyName && (
              <p className="font-medium">{parcel.recipientAddress.companyName}</p>
            )}
            {parcel.recipientAddress.contactName && (
              <p>{parcel.recipientAddress.contactName}</p>
            )}
            <p>{parcel.recipientAddress.street1}</p>
            {parcel.recipientAddress.street2 && <p>{parcel.recipientAddress.street2}</p>}
            <p>{parcel.recipientAddress.city}, {parcel.recipientAddress.state} {parcel.recipientAddress.postalCode}</p>
            <p>{parcel.recipientAddress.countryCode}</p>
            {parcel.recipientAddress.phone && <p className="text-muted-foreground">{parcel.recipientAddress.phone}</p>}
            {parcel.recipientAddress.email && <p className="text-muted-foreground">{parcel.recipientAddress.email}</p>}
            {parcel.zone && (
              <p className="text-muted-foreground mt-2">Delivery Zone: <span className="text-foreground">{parcel.zone.name}</span></p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Package Details */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Package Details</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
            <div>
              <span className="text-muted-foreground">Weight</span>
              <p>{parcel.weight} {parcel.weightUnit}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Dimensions</span>
              <p>{parcel.length} x {parcel.width} x {parcel.height} {parcel.dimensionUnit}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Declared Value</span>
              <p>{parcel.currency} {parcel.declaredValue}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Service Type</span>
              <p>{parcel.serviceType}</p>
            </div>
            {parcel.estimatedDeliveryDate && (
              <div>
                <span className="text-muted-foreground">Est. Delivery</span>
                <p>{new Date(parcel.estimatedDeliveryDate).toLocaleDateString()}</p>
              </div>
            )}
            {parcel.actualDeliveryDate && (
              <div>
                <span className="text-muted-foreground">Actual Delivery</span>
                <p>{new Date(parcel.actualDeliveryDate).toLocaleString()}</p>
              </div>
            )}
            {parcel.deliveryAttempts > 0 && (
              <div>
                <span className="text-muted-foreground">Delivery Attempts</span>
                <p>{parcel.deliveryAttempts}</p>
              </div>
            )}
          </div>
          {parcel.bin && (
            <div className="mt-4 pt-4 border-t text-sm">
              <span className="text-muted-foreground">Bin: </span>
              <span>{parcel.bin.label}</span>
              <span className="text-muted-foreground ml-2">Aisle: </span>
              <span>{parcel.bin.aisle.label} ({parcel.bin.aisle.name})</span>
              <span className="text-muted-foreground ml-2">Depot: </span>
              <span>{parcel.bin.zone.depot.name}</span>
            </div>
          )}
          {parcel.description && (
            <div className="mt-4 pt-4 border-t text-sm">
              <span className="text-muted-foreground">Description: </span>
              <span>{parcel.description}</span>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Tracking Timeline */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Tracking Timeline</CardTitle>
        </CardHeader>
        <CardContent>
          <ParcelStatusTimeline events={events} />
        </CardContent>
      </Card>

      {/* Route & Delivery Info */}
      {parcel.routeStop && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Route & Delivery Info</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
              {parcel.routeStop.route && (
                <div>
                  <span className="text-muted-foreground">Route</span>
                  <p>
                    <Link
                      href={`/routes/${parcel.routeStop.route.id}`}
                      className="text-primary hover:underline"
                    >
                      {parcel.routeStop.route.name}
                    </Link>
                  </p>
                </div>
              )}
              {parcel.routeStop.route && (
                <div>
                  <span className="text-muted-foreground">Route Status</span>
                  <p>{parcel.routeStop.route.status.replace(/_/g, " ").toLowerCase().replace(/^\w/, (c) => c.toUpperCase())}</p>
                </div>
              )}
              {parcel.routeStop.route?.plannedStartTime && (
                <div>
                  <span className="text-muted-foreground">Planned Start</span>
                  <p>{new Date(parcel.routeStop.route.plannedStartTime).toLocaleString()}</p>
                </div>
              )}
              {parcel.routeStop.route?.driver && (
                <div>
                  <span className="text-muted-foreground">Driver</span>
                  <p>{parcel.routeStop.route.driver.user.firstName} {parcel.routeStop.route.driver.user.lastName}</p>
                </div>
              )}
              {parcel.routeStop.route?.vehicle && (
                <div>
                  <span className="text-muted-foreground">Vehicle</span>
                  <p>{parcel.routeStop.route.vehicle.registrationPlate}</p>
                </div>
              )}
              <div>
                <span className="text-muted-foreground">Stop Sequence</span>
                <p>#{parcel.routeStop.sequenceNumber}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Stop Status</span>
                <p>{parcel.routeStop.status.replace(/_/g, " ").toLowerCase().replace(/^\w/, (c) => c.toUpperCase())}</p>
              </div>
              {parcel.routeStop.arrivalTime && (
                <div>
                  <span className="text-muted-foreground">Arrival</span>
                  <p>{new Date(parcel.routeStop.arrivalTime).toLocaleString()}</p>
                </div>
              )}
              {parcel.routeStop.departureTime && (
                <div>
                  <span className="text-muted-foreground">Departure</span>
                  <p>{new Date(parcel.routeStop.departureTime).toLocaleString()}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {/* Proof of Delivery */}
      {parcel.deliveryConfirmation && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Proof of Delivery</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
              {parcel.deliveryConfirmation.receivedBy && (
                <div>
                  <span className="text-muted-foreground">Received By</span>
                  <p>{parcel.deliveryConfirmation.receivedBy}</p>
                </div>
              )}
              {parcel.deliveryConfirmation.deliveryLocation && (
                <div>
                  <span className="text-muted-foreground">Delivery Location</span>
                  <p>{parcel.deliveryConfirmation.deliveryLocation}</p>
                </div>
              )}
              <div>
                <span className="text-muted-foreground">Delivered At</span>
                <p>{new Date(parcel.deliveryConfirmation.deliveredAt).toLocaleString()}</p>
              </div>
            </div>
            {(parcel.deliveryConfirmation.signatureImage || parcel.deliveryConfirmation.photo) && (
              <div className="mt-4 pt-4 border-t flex gap-4">
                {parcel.deliveryConfirmation.signatureImage && (
                  <div>
                    <span className="text-sm text-muted-foreground">Signature</span>
                    <img
                      src={parcel.deliveryConfirmation.signatureImage}
                      alt="Delivery signature"
                      className="mt-1 max-h-24 border rounded"
                    />
                  </div>
                )}
                {parcel.deliveryConfirmation.photo && (
                  <div>
                    <span className="text-sm text-muted-foreground">Delivery Photo</span>
                    <img
                      src={parcel.deliveryConfirmation.photo}
                      alt="Delivery photo"
                      className="mt-1 max-h-24 border rounded"
                    />
                  </div>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Change History */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Change History</CardTitle>
        </CardHeader>
        <CardContent>
          <ParcelAuditLogTable parcelId={String(parcel.id)} />
        </CardContent>
      </Card>

      {/* Notes */}
      {parcel.notes && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Notes</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm whitespace-pre-wrap">{parcel.notes}</p>
          </CardContent>
        </Card>
      )}

      {/* Dialogs */}
      <EditParcelDialog
        parcel={parcel}
        open={editOpen}
        onOpenChange={setEditOpen}
      />
      <CancelParcelDialog
        parcelId={String(parcel.id)}
        trackingNumber={parcel.trackingNumber}
        open={cancelOpen}
        onOpenChange={setCancelOpen}
      />
    </div>
  );
}
