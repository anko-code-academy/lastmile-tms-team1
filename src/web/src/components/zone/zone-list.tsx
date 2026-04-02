"use client";

import { useRouter } from "next/navigation";
import { Pencil, Trash2, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useZones, useDeleteZone } from "@/hooks/use-zones";
import type { ZoneSummaryDto } from "@/lib/graphql/types";

export function ZoneList() {
  const router = useRouter();
  const { data: zones, isLoading, error } = useZones();
  const deleteZone = useDeleteZone();

  async function handleDelete(id: string) {
    if (!confirm("Are you sure you want to delete this zone?")) return;

    try {
      await deleteZone.mutateAsync(id);
    } catch (err) {
      console.error(err);
    }
  }

  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Loading zones...
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-destructive">
          Error loading zones
        </CardContent>
      </Card>
    );
  }

  const zoneList = zones ?? [];

  const zonesByDepot = zoneList.reduce(
    (acc, zone) => {
      const depotName = zone.depot?.name ?? "Unassigned";
      if (!acc[depotName]) {
        acc[depotName] = [];
      }
      acc[depotName]!.push(zone);
      return acc;
    },
    {} as Record<string, ZoneSummaryDto[]>
  );

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Zones</CardTitle>
            <CardDescription>Manage delivery zones</CardDescription>
          </div>
          <Button onClick={() => router.push("/zones/new")}>
            <Plus className="size-4" />
            Add Zone
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {zoneList.length === 0 ? (
          <div className="py-8 text-center text-muted-foreground">
            No zones found. Create your first zone to get started.
          </div>
        ) : (
          Object.entries(zonesByDepot).map(([depotName, depotZones]) => (
            <div key={depotName} className="mb-6 last:mb-0">
              <h3 className="mb-3 text-sm font-semibold text-muted-foreground">
                {depotName}
              </h3>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="w-[100px]">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {depotZones.map((zone) => (
                    <TableRow key={zone.id}>
                      <TableCell className="font-medium">{zone.name}</TableCell>
                      <TableCell>
                        <span
                          className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                            zone.isActive
                              ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100"
                              : "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-400"
                          }`}
                        >
                          {zone.isActive ? "Active" : "Inactive"}
                        </span>
                      </TableCell>
                      <TableCell className="text-muted-foreground">
                        {new Date(zone.createdAt).toLocaleDateString()}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1">
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            onClick={() => router.push(`/zones/${zone.id}`)}
                          >
                            <Pencil className="size-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            onClick={() => handleDelete(zone.id)}
                            disabled={deleteZone.isPending}
                          >
                            <Trash2 className="size-4 text-destructive" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ))
        )}
      </CardContent>
    </Card>
  );
}
