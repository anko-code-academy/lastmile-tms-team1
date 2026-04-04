"use client";

import { useState } from "react";
import Link from "next/link";
import { useParcels } from "@/hooks/use-parcels";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ParcelStatus } from "@/lib/graphql/types";
import { Package, Plus } from "lucide-react";

function formatStatus(status: ParcelStatus): string {
  return status.replace(/_/g, " ").toLowerCase().replace(/^\w/, (c) => c.toUpperCase());
}

function getStatusVariant(status: ParcelStatus): "default" | "secondary" | "success" | "warning" | "destructive" {
  switch (status) {
    case ParcelStatus.REGISTERED:
      return "secondary";
    case ParcelStatus.RECEIVED_AT_DEPOT:
      return "secondary";
    case ParcelStatus.SORTED:
      return "secondary";
    case ParcelStatus.STAGED:
      return "secondary";
    case ParcelStatus.LOADED:
      return "secondary";
    case ParcelStatus.OUT_FOR_DELIVERY:
      return "warning";
    case ParcelStatus.DELIVERED:
      return "success";
    case ParcelStatus.FAILED_ATTEMPT:
      return "destructive";
    case ParcelStatus.RETURNED_TO_DEPOT:
      return "destructive";
    case ParcelStatus.CANCELLED:
      return "destructive";
    case ParcelStatus.EXCEPTION:
      return "destructive";
    default:
      return "secondary";
  }
}

export function ParcelList() {
  const { data: parcels, isLoading } = useParcels();
  const [search, setSearch] = useState("");

  const filtered = parcels?.filter((p) =>
    p.trackingNumber.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Parcels</CardTitle>
            <CardDescription>Manage registered parcels</CardDescription>
          </div>
          <Link href="/parcels/new">
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Register Parcel
            </Button>
          </Link>
        </div>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center gap-4">
          <Input
            placeholder="Search by tracking number..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="max-w-sm"
          />
        </div>

        {isLoading ? (
          <div className="text-center py-8 text-muted-foreground">Loading parcels...</div>
        ) : filtered?.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            {search ? "No parcels match your search." : "No parcels registered yet."}
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Tracking Number</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Service Type</TableHead>
                <TableHead>Created</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered?.map((parcel) => (
                <TableRow key={parcel.id}>
                  <TableCell>
                    <Link
                      href={`/parcels/${parcel.id}`}
                      className="font-medium hover:underline"
                    >
                      {parcel.trackingNumber}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <Badge variant={getStatusVariant(parcel.status)}>
                      {formatStatus(parcel.status)}
                    </Badge>
                  </TableCell>
                  <TableCell>{parcel.serviceType}</TableCell>
                  <TableCell>
                    {new Date(parcel.createdAt).toLocaleDateString()}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </CardContent>
    </Card>
  );
}
