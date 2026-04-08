"use client";

import Link from "next/link";
import { useParcel } from "@/hooks/use-parcels";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ParcelStatus } from "@/lib/graphql/types";
import { Printer, ArrowLeft, Plus } from "lucide-react";

function formatStatus(status: ParcelStatus): string {
  return status.replace(/_/g, " ").toLowerCase().replace(/^\w/, (c) => c.toUpperCase());
}

function getStatusVariant(status: ParcelStatus): "default" | "secondary" | "success" | "warning" | "destructive" {
  switch (status) {
    case ParcelStatus.REGISTERED:
      return "secondary";
    case ParcelStatus.OUT_FOR_DELIVERY:
      return "warning";
    case ParcelStatus.DELIVERED:
      return "success";
    case ParcelStatus.FAILED_ATTEMPT:
    case ParcelStatus.RETURNED_TO_DEPOT:
    case ParcelStatus.CANCELLED:
    case ParcelStatus.EXCEPTION:
      return "destructive";
    default:
      return "secondary";
  }
}

interface ParcelDetailProps {
  trackingNumber: string;
}

export function ParcelDetail({ trackingNumber }: ParcelDetailProps) {
  const { data: parcel, isLoading } = useParcel(trackingNumber);

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Link href="/parcels" className="flex items-center gap-2 text-muted-foreground hover:underline">
          <ArrowLeft className="h-4 w-4" /> Back to Parcels
        </Link>
        <div className="text-center py-8 text-muted-foreground">Loading parcel...</div>
      </div>
    );
  }

  if (!parcel) {
    return (
      <div className="space-y-4">
        <Link href="/parcels" className="flex items-center gap-2 text-muted-foreground hover:underline">
          <ArrowLeft className="h-4 w-4" /> Back to Parcels
        </Link>
        <div className="text-center py-8 text-muted-foreground">Parcel not found.</div>
      </div>
    );
  }

  function reprintLabel() {
    window.open(`/api/labels/${parcel!.id}/pdf`, "_blank");
  }

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <Link href="/parcels" className="flex items-center gap-2 text-muted-foreground hover:underline">
          <ArrowLeft className="h-4 w-4" /> Back to Parcels
        </Link>
        <div className="flex items-center gap-2">
          <Link
            href="/parcels/new"
            className="inline-flex items-center gap-1 rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            <Plus className="size-4" />
            New Parcel
          </Link>
          <Button onClick={reprintLabel}>
            <Printer className="h-4 w-4 mr-2" />
            Reprint Label
          </Button>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        {/* Tracking Info */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Tracking Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Tracking #</span>
              <span className="font-mono font-medium">{parcel.trackingNumber}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Status</span>
              <Badge variant={getStatusVariant(parcel.status)}>{formatStatus(parcel.status)}</Badge>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Service Type</span>
              <span>{parcel.serviceType}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Created</span>
              <span>{new Date(parcel.createdAt).toLocaleString()}</span>
            </div>
            {parcel.estimatedDeliveryDate && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Est. Delivery</span>
                <span>{new Date(parcel.estimatedDeliveryDate).toLocaleDateString()}</span>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Package Details */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Package Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Weight</span>
              <span>{parcel.weight} {parcel.weightUnit}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Dimensions</span>
              <span>{parcel.length} x {parcel.width} x {parcel.height} {parcel.dimensionUnit}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">Declared Value</span>
              <span>{parcel.currency} {parcel.declaredValue}</span>
            </div>
            {parcel.parcelType && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Parcel Type</span>
                <span>{parcel.parcelType}</span>
              </div>
            )}
            {parcel.zone && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Sort Zone</span>
                <span>{parcel.zone.name}</span>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Shipper Address */}
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

        {/* Recipient Address */}
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
          </CardContent>
        </Card>
      </div>

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
    </div>
  );
}
