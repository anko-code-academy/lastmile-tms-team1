"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ManifestForm } from "@/components/manifests/manifest-form";
import { useCreateManifest } from "@/hooks/use-manifests";

export default function NewManifestPage() {
  const router = useRouter();
  const createManifest = useCreateManifest();

  const handleSubmit = async (values: {
    name: string;
    depotId: string;
    trackingNumbers: string[];
  }) => {
    try {
      await createManifest.mutateAsync(values);
      router.push("/manifests");
    } catch {
      // Error toast handled by the hook
    }
  };

  return (
    <div className="p-6">
      <div className="flex items-center gap-4 mb-6">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/manifests">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold">Create Manifest</h1>
          <p className="text-muted-foreground">
            Create a new inbound parcel manifest
          </p>
        </div>
      </div>

      <Card className="max-w-2xl">
        <CardHeader>
          <CardTitle>Manifest Information</CardTitle>
        </CardHeader>
        <CardContent>
          <ManifestForm
            onSubmit={handleSubmit}
            isSubmitting={createManifest.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
