"use client";

import { DriverForm } from "@/components/driver/driver-form";

interface EditDriverPageProps {
  params: Promise<{ id: string }>;
}

export default async function EditDriverPage({ params }: EditDriverPageProps) {
  const { id } = await params;
  return <DriverForm driverId={id} />;
}
