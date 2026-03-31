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
import { useDepots, useDeleteDepot } from "@/hooks/use-depots";

export function DepotList() {
  const router = useRouter();
  const { data: depots, isLoading, error } = useDepots();
  const deleteDepot = useDeleteDepot();

  async function handleDelete(id: string) {
    if (!confirm("Are you sure you want to delete this depot?")) return;

    try {
      await deleteDepot.mutateAsync(id);
    } catch (err) {
      console.error(err);
    }
  }

  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Loading depots...
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-destructive">
          Error loading depots
        </CardContent>
      </Card>
    );
  }

  const depotList = depots ?? [];

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Depots</CardTitle>
            <CardDescription>Manage your depot locations</CardDescription>
          </div>
          <Button onClick={() => router.push("/depots/new")}>
            <Plus className="size-4" />
            Add Depot
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {depotList.length === 0 ? (
          <div className="py-8 text-center text-muted-foreground">
            No depots found. Create your first depot to get started.
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Address</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created</TableHead>
                <TableHead className="w-[100px]">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {depotList.map((depot) => (
                <TableRow key={depot.id}>
                  <TableCell className="font-medium">{depot.name}</TableCell>
                  <TableCell className="text-muted-foreground">
                    {depot.address
                      ? `${depot.address.city}, ${depot.address.state} ${depot.address.postalCode}`
                      : "-"}
                  </TableCell>
                  <TableCell>
                    <span
                      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${
                        depot.isActive
                          ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100"
                          : "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-400"
                      }`}
                    >
                      {depot.isActive ? "Active" : "Inactive"}
                    </span>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {new Date(depot.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    <div className="flex items-center gap-1">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        onClick={() => router.push(`/depots/${depot.id}`)}
                      >
                        <Pencil className="size-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        onClick={() => handleDelete(depot.id)}
                        disabled={deleteDepot.isPending}
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
