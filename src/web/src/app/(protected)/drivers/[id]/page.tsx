"use client";

import { use } from "react";
import { DriverForm } from "@/components/driver/driver-form";

interface EditDriverPageProps {
  params: Promise<{ id: string }>;
}

export default function EditDriverPage({ params }: EditDriverPageProps) {
  const { id } = use(params);
  return <DriverForm driverId={id} />;
}
