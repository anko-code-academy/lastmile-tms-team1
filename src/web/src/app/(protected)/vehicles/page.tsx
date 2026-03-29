import { Suspense } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { VehicleListClient } from "./client";

export default function VehiclesPage() {
  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">Vehicles</h1>
          <p className="text-muted-foreground">
            Manage your fleet of vehicles
          </p>
        </div>
        <Link href="/vehicles/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Add Vehicle
          </Button>
        </Link>
      </div>

      <Suspense fallback={<div className="py-8 text-center text-muted-foreground">Loading...</div>}>
        <VehicleListClient />
      </Suspense>
    </div>
  );
}
