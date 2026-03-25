import { Suspense } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { RouteListClient } from "./client";

export default function RoutesPage() {
  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">Routes</h1>
          <p className="text-muted-foreground">
            Manage delivery routes
          </p>
        </div>
        <Link href="/routes/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Add Route
          </Button>
        </Link>
      </div>

      <Suspense fallback={<div className="py-8 text-center text-muted-foreground">Loading...</div>}>
        <RouteListClient />
      </Suspense>
    </div>
  );
}
