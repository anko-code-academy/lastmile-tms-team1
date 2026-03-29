"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { VehicleForm } from "@/components/vehicles/vehicle-form";
import { useVehicle, useUpdateVehicle } from "@/lib/hooks/use-vehicles";
import { VehicleType } from "@/types/vehicle";
import { useParams } from "next/navigation";

export default function EditVehiclePage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const { data: vehicle, isLoading } = useVehicle(id);
  const updateVehicle = useUpdateVehicle();

  const handleSubmit = async (values: {
    registrationPlate: string;
    type: VehicleType;
    parcelCapacity: number;
    weightCapacityKg: number;
    depotId?: string | null;
  }) => {
    try {
      await updateVehicle.mutateAsync({ id, ...values });
      toast.success("Vehicle updated successfully");
      router.push(`/vehicles/${id}`);
    } catch {
      toast.error("Failed to update vehicle");
    }
  };

  if (isLoading) {
    return <div className="p-6">Loading...</div>;
  }

  if (!vehicle) {
    return <div className="p-6">Vehicle not found</div>;
  }

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href={`/vehicles/${id}`}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Edit Vehicle</h1>
          <p className="text-muted-foreground">
            Update vehicle {vehicle.registrationPlate}
          </p>
        </div>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Vehicle Information</CardTitle>
        </CardHeader>
        <CardContent>
          <VehicleForm
            defaultValues={{
              registrationPlate: vehicle.registrationPlate,
              type: vehicle.type as VehicleType,
              parcelCapacity: vehicle.parcelCapacity,
              weightCapacityKg: vehicle.weightCapacityKg,
              depotId: vehicle.depotId,
            }}
            onSubmit={handleSubmit}
            isSubmitting={updateVehicle.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
