"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { VehicleForm } from "@/components/vehicles/vehicle-form";
import { useCreateVehicle } from "@/hooks/use-vehicles";
import { VehicleType } from "@/types/vehicle";

export default function NewVehiclePage() {
  const router = useRouter();
  const createVehicle = useCreateVehicle();

  const handleSubmit = async (values: {
    registrationPlate: string;
    type: VehicleType;
    parcelCapacity: number;
    weightCapacityKg: number;
    depotId?: string | null;
  }) => {
    try {
      await createVehicle.mutateAsync(values);
      toast.success("Vehicle created successfully");
      router.push("/vehicles");
    } catch {
      toast.error("Failed to create vehicle");
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/vehicles">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Add Vehicle</h1>
          <p className="text-muted-foreground">
            Create a new vehicle in your fleet
          </p>
        </div>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Vehicle Information</CardTitle>
        </CardHeader>
        <CardContent>
          <VehicleForm
            onSubmit={handleSubmit}
            isSubmitting={createVehicle.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
