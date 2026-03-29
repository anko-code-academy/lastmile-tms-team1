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
import { useGraphQuery, useGraphMutation } from "@/hooks/use-graphql";
import { GET_DRIVERS_QUERY } from "@/lib/graphql/queries/driver";
import { DELETE_DRIVER_MUTATION } from "@/lib/graphql/mutations/driver";
import type { DriverSummaryDto } from "@/lib/graphql/types";
import { toast } from "sonner";

interface DriversResponse {
  drivers: {
    nodes: DriverSummaryDto[];
  };
}

export function DriverList() {
  const router = useRouter();
  const { data, isLoading, error } = useGraphQuery<DriversResponse, null>({
    queryKey: ["drivers"],
    query: GET_DRIVERS_QUERY,
  });

  const deleteMutation = useGraphMutation<{ deleteDriver: boolean }, { id: string }>({
    mutation: DELETE_DRIVER_MUTATION,
    invalidateKeys: ["drivers"],
  });

  async function handleDelete(id: string) {
    if (!confirm("Are you sure you want to delete this driver?")) return;

    try {
      await deleteMutation.mutateAsync({ id });
      toast.success("Driver deleted successfully");
    } catch (err) {
      toast.error("Failed to delete driver");
      console.error(err);
    }
  }

  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Loading drivers...
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-destructive">
          Error loading drivers
        </CardContent>
      </Card>
    );
  }

  const drivers = data?.drivers?.nodes ?? [];

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Drivers</CardTitle>
            <CardDescription>Manage driver profiles with license info, zone assignment, and availability</CardDescription>
          </div>
          <Button onClick={() => router.push("/drivers/new")}>
            <Plus className="size-4" />
            Add Driver
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {drivers.length === 0 ? (
          <div className="py-8 text-center text-muted-foreground">
            No drivers found. Create your first driver to get started.
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Contact</TableHead>
                <TableHead>Zone</TableHead>
                <TableHead>Depot</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created</TableHead>
                <TableHead className="w-[100px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {drivers.map((driver) => (
                <TableRow key={driver.id}>
                  <TableCell className="font-medium">
                    {driver.firstName} {driver.lastName}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    <div>{driver.email}</div>
                    <div className="text-xs">{driver.phone}</div>
                  </TableCell>
                  <TableCell>{driver.zone?.name ?? "-"}</TableCell>
                  <TableCell>{driver.depot?.name ?? "-"}</TableCell>
                  <TableCell>
                    <span
                      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                        driver.isActive
                          ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100"
                          : "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-400"
                      }`}
                    >
                      {driver.isActive ? "Active" : "Inactive"}
                    </span>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {new Date(driver.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        onClick={() => router.push(`/drivers/${driver.id}`)}
                      >
                        <Pencil className="size-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        onClick={() => handleDelete(driver.id)}
                        disabled={deleteMutation.isPending}
                      >
                        <Trash2 className="size-4 text-destructive" />
                      </Button>
                    </div>
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
