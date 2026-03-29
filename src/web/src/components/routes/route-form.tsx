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
import Link from "next/link";
import { useVehicles } from "@/lib/hooks/use-vehicles";
import { VehicleStatus } from "@/types/vehicle";

const routeSchema = z.object({
  name: z.string().min(1, "Route name is required").max(100),
  plannedStartTime: z.string().min(1, "Planned start time is required"),
  totalDistanceKm: z.coerce.number().min(0.1, "Must be at least 0.1 km"),
  totalParcelCount: z.coerce.number().int().min(1, "Must be at least 1 parcel"),
  vehicleId: z.string().optional().nullable(),
});

type RouteFormValues = z.infer<typeof routeSchema>;

interface RouteFormProps {
  defaultValues?: Partial<RouteFormValues>;
  onSubmit: (values: RouteFormValues) => Promise<void>;
  isSubmitting?: boolean;
}

export function RouteForm({
  defaultValues,
  onSubmit,
  isSubmitting,
}: RouteFormProps) {
  const { data: vehicles = [] } = useVehicles();

  const form = useForm<RouteFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(routeSchema) as any,
    defaultValues: {
      name: "",
      plannedStartTime: "",
      totalDistanceKm: 0,
      totalParcelCount: 0,
      vehicleId: null,
      ...defaultValues,
    },
  });

  // Filter out retired vehicles - they cannot be assigned to routes
  const availableVehicles = vehicles.filter(
    (v) => v.status !== VehicleStatus.RETIRED
  );

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Route Name</FormLabel>
              <FormControl>
                <Input placeholder="Morning Route A" {...field} />
              </FormControl>
              <FormDescription>The name identifier for this route</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="plannedStartTime"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Planned Start Time</FormLabel>
              <FormControl>
                <Input type="datetime-local" {...field} />
              </FormControl>
              <FormDescription>When the route is scheduled to start</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            control={form.control}
            name="totalDistanceKm"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Total Distance (km)</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={0.1}
                    step={0.1}
                    placeholder="50.5"
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    name={field.name}
                    ref={field.ref}
                  />
                </FormControl>
                <FormDescription>Estimated route distance</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="totalParcelCount"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Total Parcels</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={1}
                    placeholder="25"
                    value={field.value}
                    onChange={field.onChange}
                    onBlur={field.onBlur}
                    name={field.name}
                    ref={field.ref}
                  />
                </FormControl>
                <FormDescription>Number of parcels to deliver</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="vehicleId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Assign Vehicle (Optional)</FormLabel>
              <FormControl>
                <select
                  className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                  value={field.value ?? ""}
                  onChange={(e) => field.onChange(e.target.value || null)}
                >
                  <option value="">No vehicle assigned</option>
                  {availableVehicles.map((vehicle) => (
                    <option key={vehicle.id} value={vehicle.id}>
                      {vehicle.registrationPlate} ({vehicle.type})
                    </option>
                  ))}
                </select>
              </FormControl>
              <FormDescription>Select a vehicle to assign to this route</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" asChild>
            <Link href="/routes">Cancel</Link>
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : "Save Route"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
