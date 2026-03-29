"use client";

import { use } from "react";
import { DepotForm } from "@/components/depot/depot-form";

interface EditDepotPageProps {
  params: Promise<{ id: string }>;
}

export default function EditDepotPage({ params }: EditDepotPageProps) {
  const { id } = use(params);
  return <DepotForm depotId={id} />;
}
