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
import { useVehicles } from "@/hooks/use-vehicles";
import { useZones } from "@/hooks/use-zones";
import { useAvailableDrivers } from "@/hooks/use-routes";
import { VehicleStatus } from "@/types/vehicle";

const routeSchema = z.object({
  name: z.string().min(1, "Route name is required").max(100),
  plannedStartTime: z.string().min(1, "Planned start time is required"),
  zoneId: z.string().optional().nullable(),
  vehicleId: z.string().optional().nullable(),
  driverId: z.string().optional().nullable(),
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
  const { data: zones = [] } = useZones();

  const form = useForm<RouteFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(routeSchema) as any,
    defaultValues: {
      name: "",
      plannedStartTime: "",
      zoneId: null,
      vehicleId: null,
      driverId: null,
      ...defaultValues,
    },
  });

  const plannedStartTime = form.watch("plannedStartTime");
  const dateForDrivers = plannedStartTime ? plannedStartTime.split("T")[0] : undefined;
  const { data: availableDrivers = [] } = useAvailableDrivers(dateForDrivers);

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

        <div className="grid grid-cols-2 gap-4">
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

          <FormField
            control={form.control}
            name="zoneId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Zone (Optional)</FormLabel>
                <FormControl>
                  <select
                    className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.value || null)}
                  >
                    <option value="">No zone assigned</option>
                    {zones.map((zone) => (
                      <option key={zone.id} value={zone.id}>
                        {zone.name}
                      </option>
                    ))}
                  </select>
                </FormControl>
                <FormDescription>Zone for auto-assigning parcels</FormDescription>
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

        <FormField
          control={form.control}
          name="driverId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Assign Driver (Optional)</FormLabel>
              <FormControl>
                <select
                  className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                  value={field.value ?? ""}
                  onChange={(e) => field.onChange(e.target.value || null)}
                  disabled={!plannedStartTime}
                >
                  <option value="">
                    {!plannedStartTime ? "Select a date first" : "No driver assigned"}
                  </option>
                  {availableDrivers.map((driver) => (
                    <option key={driver.id} value={driver.id}>
                      {driver.name}
                      {driver.assignedRoutes.length > 0
                        ? ` (${driver.assignedRoutes.length} route${driver.assignedRoutes.length > 1 ? "s" : ""} assigned)`
                        : ""}
                    </option>
                  ))}
                </select>
              </FormControl>
              <FormDescription>
                {!plannedStartTime
                  ? "Select a planned start time to see available drivers"
                  : "Available drivers for the selected date (excludes drivers on day off)"}
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {form.watch("driverId") && availableDrivers.length > 0 && (
          <DriverWorkloadInfo
            driver={availableDrivers.find((d) => d.id === form.watch("driverId"))}
          />
        )}

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

function DriverWorkloadInfo({
  driver,
}: {
  driver: { name: string; shift?: { openTime: string; closeTime: string } | null; assignedRoutes: { id: string; name: string; status: string }[] } | undefined;
}) {
  if (!driver) return null;

  return (
    <div className="rounded-md border bg-muted/50 p-3 text-sm">
      <p className="font-medium">{driver.name}</p>
      {driver.shift && (
        <p className="text-muted-foreground">
          Shift: {driver.shift.openTime} - {driver.shift.closeTime}
        </p>
      )}
      {driver.assignedRoutes.length > 0 ? (
        <div className="mt-1">
          <p className="text-muted-foreground">Assigned routes:</p>
          <ul className="ml-4 list-disc text-muted-foreground">
            {driver.assignedRoutes.map((route) => (
              <li key={route.id}>
                {route.name} ({route.status.replace("_", " ")})
              </li>
            ))}
          </ul>
        </div>
      ) : (
        <p className="text-muted-foreground">No other routes assigned this day</p>
      )}
    </div>
  );
}
