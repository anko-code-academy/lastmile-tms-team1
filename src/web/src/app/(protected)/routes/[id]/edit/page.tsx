"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RouteForm } from "@/components/routes/route-form";
import { useRoute, useUpdateRoute } from "@/lib/hooks/use-routes";
import { useParams } from "next/navigation";

export default function EditRoutePage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: route, isLoading } = useRoute(id);
  const updateRoute = useUpdateRoute();

  const handleSubmit = async (values: {
    name: string;
    plannedStartTime: string;
    totalDistanceKm: number;
    totalParcelCount: number;
    vehicleId?: string | null;
  }) => {
    try {
      // Convert datetime-local format to ISO 8601 with UTC timezone
      const plannedStartTime = new Date(values.plannedStartTime).toISOString();
      await updateRoute.mutateAsync({ id, ...values, plannedStartTime });
      toast.success("Route updated successfully");
      router.push(`/routes/${id}`);
    } catch {
      toast.error("Failed to update route");
    }
  };

  if (isLoading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!route) {
    return <div className="p-6">Route not found</div>;
  }

  // Format datetime-local from ISO string
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
              totalDistanceKm: route.totalDistanceKm,
              totalParcelCount: route.totalParcelCount,
              vehicleId: route.vehicleId,
            }}
            onSubmit={handleSubmit}
            isSubmitting={updateRoute.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
