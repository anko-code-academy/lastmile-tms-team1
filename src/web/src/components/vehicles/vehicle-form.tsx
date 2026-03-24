"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import Link from "next/link";
import { VehicleType } from "@/types/vehicle";

const vehicleSchema = z.object({
  registrationPlate: z.string().min(1, "Registration plate is required").max(20),
  type: z.nativeEnum(VehicleType, {
    message: "Vehicle type is required",
  }),
  parcelCapacity: z.coerce.number().int().min(1, "Must be at least 1"),
  weightCapacityKg: z.coerce.number().min(0.1, "Must be at least 0.1 kg"),
  depotId: z.string().optional().nullable(),
});

type VehicleFormValues = z.infer<typeof vehicleSchema>;

interface VehicleFormProps {
  defaultValues?: Partial<VehicleFormValues>;
  onSubmit: (values: VehicleFormValues) => Promise<void>;
  isSubmitting?: boolean;
}

export function VehicleForm({
  defaultValues,
  onSubmit,
  isSubmitting,
}: VehicleFormProps) {
  const form = useForm<VehicleFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(vehicleSchema) as any,
    defaultValues: {
      registrationPlate: "",
      type: undefined,
      parcelCapacity: 0,
      weightCapacityKg: 0,
      depotId: null,
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="registrationPlate"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Registration Plate</FormLabel>
              <FormControl>
                <Input placeholder="ABC-123" {...field} />
              </FormControl>
              <FormDescription>
                The vehicle registration/license plate number
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="type"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Vehicle Type</FormLabel>
              <Select
                onValueChange={(value) => field.onChange(value as VehicleType)}
                value={field.value as string}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select vehicle type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={VehicleType.Van}>Van</SelectItem>
                  <SelectItem value={VehicleType.Car}>Car</SelectItem>
                  <SelectItem value={VehicleType.Bike}>Bike</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="parcelCapacity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Parcel Capacity</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={1}
                    placeholder="100"
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    name={field.name}
                    ref={field.ref}
                  />
                </FormControl>
                <FormDescription>Max number of parcels</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="weightCapacityKg"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Weight Capacity (kg)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={0.1}
                    step={0.1}
                    placeholder="500"
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    name={field.name}
                    ref={field.ref}
                  />
                </FormControl>
                <FormDescription>Max weight in kg</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="depotId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Depot (Optional)</FormLabel>
              <FormControl>
                <Input
                  placeholder="Depot ID"
                  value={field.value ?? ""}
                  onChange={field.onChange}
                  onBlur={field.onBlur}
                  name={field.name}
                  ref={field.ref}
                />
              </FormControl>
              <FormDescription>Assign to a depot</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" asChild>
            <Link href="/vehicles">Cancel</Link>
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : "Save Vehicle"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
