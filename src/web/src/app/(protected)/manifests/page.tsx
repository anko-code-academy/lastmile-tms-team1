import { Suspense } from "react";
import Link from "next/link";
import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ManifestListClient } from "./client";

export default function ManifestsPage() {
  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold">Manifests</h1>
          <p className="text-muted-foreground">
            Manage inbound parcel manifests
          </p>
        </div>
        <Link href="/manifests/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            Create Manifest
          </Button>
        </Link>
      </div>

      <Suspense fallback={<div className="py-8 text-center text-muted-foreground">Loading...</div>}>
        <ManifestListClient />
      </Suspense>
    </div>
  );
}
