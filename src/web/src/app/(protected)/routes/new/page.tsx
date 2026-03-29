"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { RouteForm } from "@/components/routes/route-form";
import { useCreateRoute } from "@/lib/hooks/use-routes";

export default function NewRoutePage() {
  const router = useRouter();
  const createRoute = useCreateRoute();

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
      await createRoute.mutateAsync({ ...values, plannedStartTime });
      toast.success("Route created successfully");
      router.push("/routes");
    } catch {
      toast.error("Failed to create route");
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/routes">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Add Route</h1>
          <p className="text-muted-foreground">
            Create a new delivery route
          </p>
        </div>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Route Information</CardTitle>
        </CardHeader>
        <CardContent>
          <RouteForm
            onSubmit={handleSubmit}
            isSubmitting={createRoute.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
