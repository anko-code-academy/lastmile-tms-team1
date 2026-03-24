"use client";

import { use } from "react";
import { ZoneForm } from "@/components/zone/zone-form";

interface EditZonePageProps {
  params: Promise<{ id: string }>;
}

export default function EditZonePage({ params }: EditZonePageProps) {
  const { id } = use(params);
  return <ZoneForm zoneId={id} />;
}
