"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { useGraphQuery, useGraphMutation } from "@/hooks/use-graphql";
import { GET_DRIVER_QUERY } from "@/lib/graphql/queries/driver";
import { GET_ZONES_QUERY } from "@/lib/graphql/queries/zone";
import { GET_DEPOTS_QUERY } from "@/lib/graphql/queries/depot";
import { CREATE_DRIVER_MUTATION, UPDATE_DRIVER_MUTATION } from "@/lib/graphql/mutations/driver";
import { DayOfWeek } from "@/lib/graphql/types";
import type { DriverDto, ZoneSummaryDto, DepotSummaryDto, CreateDriverInput, UpdateDriverInput, ShiftScheduleInput, DayOffInput } from "@/lib/graphql/types";
import { toast } from "sonner";

interface DriverResponse {
  driver: DriverDto;
}

interface ZonesResponse {
  zones: {
    nodes: ZoneSummaryDto[];
  };
}

interface DepotsResponse {
  depots: {
    nodes: DepotSummaryDto[];
  };
}

const DAYS_OF_WEEK = [
  { value: "SUNDAY", label: "Sunday" },
  { value: "MONDAY", label: "Monday" },
  { value: "TUESDAY", label: "Tuesday" },
  { value: "WEDNESDAY", label: "Wednesday" },
  { value: "THURSDAY", label: "Thursday" },
  { value: "FRIDAY", label: "Friday" },
  { value: "SATURDAY", label: "Saturday" },
] as const;

const driverFormSchema = z.object({
  firstName: z.string().min(1, "First name is required").max(100),
  lastName: z.string().min(1, "Last name is required").max(100),
  phone: z.string().min(1, "Phone is required").max(20),
  email: z.string().min(1, "Email is required").email("Valid email is required").max(255),
  licenseNumber: z.string().min(1, "License number is required").max(50),
  licenseExpiryDate: z.string().min(1, "License expiry date is required"),
  photo: z.string().nullable().optional().transform((v) => v ?? ""),
  zoneId: z.string().min(1, "Zone is required"),
  depotId: z.string().min(1, "Depot is required"),
  isActive: z.boolean(),
  shiftSchedules: z.array(z.object({
    dayOfWeek: z.string(),
    openTime: z.string().nullable(),
    closeTime: z.string().nullable(),
  })).optional(),
  daysOff: z.array(z.object({
    date: z.string(),
  })).optional(),
});

type DriverFormValues = z.infer<typeof driverFormSchema>;

interface DriverFormProps {
  driverId?: string;
}

export function DriverForm({ driverId }: DriverFormProps) {
  const router = useRouter();
  const isEditing = !!driverId;
  const [activeTab, setActiveTab] = useState<"details" | "schedule" | "daysoff">("details");
  const [newDayOffDate, setNewDayOffDate] = useState("");

  const { data: driverData, isLoading: driverLoading } = useGraphQuery<DriverResponse, { id: string }>({
    queryKey: ["drivers", driverId ?? ""] as string[],
    query: GET_DRIVER_QUERY,
    variables: { id: driverId! } as { id: string },
    enabled: isEditing,
  });

  const { data: zonesData, isLoading: zonesLoading } = useGraphQuery<ZonesResponse, null>({
    queryKey: ["zones"],
    query: GET_ZONES_QUERY,
  });

  const { data: depotsData, isLoading: depotsLoading } = useGraphQuery<DepotsResponse, null>({
    queryKey: ["depots"],
    query: GET_DEPOTS_QUERY,
  });

  const createMutation = useGraphMutation<{ createDriver: { id: string } }, { input: CreateDriverInput }>({
    mutation: CREATE_DRIVER_MUTATION,
    invalidateKeys: ["drivers"],
  });

  const updateMutation = useGraphMutation<{ updateDriver: { id: string } }, { input: UpdateDriverInput }>({
    mutation: UPDATE_DRIVER_MUTATION,
    invalidateKeys: ["drivers"],
  });

  const form = useForm<DriverFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(driverFormSchema) as any,
    defaultValues: {
      firstName: "",
      lastName: "",
      phone: "",
      email: "",
      licenseNumber: "",
      licenseExpiryDate: "",
      photo: "",
      zoneId: "",
      depotId: "",
      isActive: true,
      shiftSchedules: [],
      daysOff: [],
    },
  });

  useEffect(() => {
    if (driverData?.driver) {
      const driver = driverData.driver;
      form.reset({
        firstName: driver.firstName,
        lastName: driver.lastName,
        phone: driver.phone,
        email: driver.email,
        licenseNumber: driver.licenseNumber,
        licenseExpiryDate: driver.licenseExpiryDate.slice(0, 16),
        photo: driver.photo ?? "",
        zoneId: driver.zoneId,
        depotId: driver.depotId,
        isActive: driver.isActive,
        shiftSchedules: driver.shiftSchedules?.map(s => ({
          dayOfWeek: DayOfWeek[s.dayOfWeek as number].toUpperCase() as string,
          openTime: s.openTime || null,
          closeTime: s.closeTime || null,
        })) ?? [],
        daysOff: driver.daysOff?.map(d => ({
          date: d.date.slice(0, 10),
        })) ?? [],
      });
      form.setValue("zoneId", driver.zoneId, { shouldTouch: true });
      form.setValue("depotId", driver.depotId, { shouldTouch: true });
    }
  }, [driverData, form]);

  const watchSchedules = form.watch("shiftSchedules");
  const schedules = watchSchedules ?? [];

  function handleScheduleTimeChange(dayOfWeek: string, field: "openTime" | "closeTime", value: string) {
    const existingIndex = schedules.findIndex(s => s.dayOfWeek === dayOfWeek);

    if (existingIndex >= 0) {
      const updated = [...schedules];
      updated[existingIndex] = { ...updated[existingIndex], [field]: value || null };
      form.setValue("shiftSchedules", updated, { shouldValidate: true });
    } else {
      form.setValue("shiftSchedules", [
        ...schedules,
        { dayOfWeek, openTime: field === "openTime" ? value || null : null, closeTime: field === "closeTime" ? value || null : null },
      ], { shouldValidate: true });
    }
  }

  function addDayOff() {
    if (!newDayOffDate) return;
    const daysOff = form.getValues("daysOff") ?? [];
    if (!daysOff.some(d => d.date === newDayOffDate)) {
      form.setValue("daysOff", [...daysOff, { date: newDayOffDate }]);
    }
    setNewDayOffDate("");
  }

  function removeDayOff(date: string) {
    const daysOff = form.getValues("daysOff") ?? [];
    form.setValue("daysOff", daysOff.filter(d => d.date !== date));
  }

  function getScheduleForDay(dayOfWeek: string): { openTime: string | null; closeTime: string | null } {
    const schedule = schedules.find(s => s.dayOfWeek === dayOfWeek);
    return { openTime: schedule?.openTime ?? null, closeTime: schedule?.closeTime ?? null };
  }

  async function onSubmit(values: DriverFormValues) {
    try {
      const shiftSchedules: ShiftScheduleInput[] = values.shiftSchedules
        ?.filter(s => s.openTime && s.closeTime)
        ?.map(s => ({
          dayOfWeek: s.dayOfWeek,
          openTime: s.openTime,
          closeTime: s.closeTime,
        })) ?? [];

      const daysOff: DayOffInput[] = values.daysOff?.map(d => ({
        date: d.date,
      })) ?? [];

      if (isEditing && driverId) {
        await updateMutation.mutateAsync({
          input: {
            id: driverId,
            firstName: values.firstName,
            lastName: values.lastName,
            phone: values.phone,
            email: values.email,
            licenseNumber: values.licenseNumber,
            licenseExpiryDate: `${values.licenseExpiryDate}:00Z`,
            photo: values.photo || undefined,
            zoneId: values.zoneId,
            depotId: values.depotId,
            isActive: values.isActive,
            shiftSchedules,
            daysOff,
          },
        });
        toast.success("Driver updated successfully");
      } else {
        await createMutation.mutateAsync({
          input: {
            firstName: values.firstName,
            lastName: values.lastName,
            phone: values.phone,
            email: values.email,
            licenseNumber: values.licenseNumber,
            licenseExpiryDate: `${values.licenseExpiryDate}:00Z`,
            photo: values.photo || undefined,
            zoneId: values.zoneId,
            depotId: values.depotId,
            isActive: values.isActive,
            shiftSchedules,
            daysOff,
          },
        });
        toast.success("Driver created successfully");
      }
      router.push("/drivers");
    } catch (err) {
      toast.error(isEditing ? "Failed to update driver" : "Failed to create driver");
      console.error(err);
    }
  }

  if ((driverLoading && isEditing) || zonesLoading || depotsLoading) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Loading...
        </CardContent>
      </Card>
    );
  }

  const zones = zonesData?.zones?.nodes ?? [];
  const depots = depotsData?.depots?.nodes ?? [];
  const daysOff = form.getValues("daysOff") ?? [];

  return (
    <Card>
      <CardHeader>
        <CardTitle>{isEditing ? "Edit Driver" : "Create Driver"}</CardTitle>
        <CardDescription>
          {isEditing
            ? "Update the driver details below."
            : "Enter the driver details below."}
        </CardDescription>
      </CardHeader>

      <div className="border-b px-6">
        <nav className="flex space-x-4 -mb-px">
          <button
            type="button"
            onClick={() => setActiveTab("details")}
            className={`py-3 px-1 border-b-2 text-sm font-medium ${
              activeTab === "details"
                ? "border-primary text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground"
            }`}
          >
            Details
          </button>
          <button
            type="button"
            onClick={() => setActiveTab("schedule")}
            className={`py-3 px-1 border-b-2 text-sm font-medium ${
              activeTab === "schedule"
                ? "border-primary text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground"
            }`}
          >
            Schedule
          </button>
          <button
            type="button"
            onClick={() => setActiveTab("daysoff")}
            className={`py-3 px-1 border-b-2 text-sm font-medium ${
              activeTab === "daysoff"
                ? "border-primary text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground hover:border-muted-foreground"
            }`}
          >
            Days Off
          </button>
        </nav>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <CardContent className="space-y-6 pt-6">
            {activeTab === "details" && (
              <>
                <div className="space-y-4">
                  <h3 className="text-sm font-medium">Personal Information</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="firstName"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>First Name</FormLabel>
                          <FormControl>
                            <Input placeholder="John" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="lastName"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Last Name</FormLabel>
                          <FormControl>
                            <Input placeholder="Doe" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                <div className="space-y-4">
                  <h3 className="text-sm font-medium">Contact Information</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="phone"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Phone</FormLabel>
                          <FormControl>
                            <Input placeholder="+1 555-123-4567" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="email"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Email</FormLabel>
                          <FormControl>
                            <Input placeholder="john.doe@example.com" type="email" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                <div className="space-y-4">
                  <h3 className="text-sm font-medium">License Information</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="licenseNumber"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>License Number</FormLabel>
                          <FormControl>
                            <Input placeholder="DL-123456789" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="licenseExpiryDate"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>License Expiry Date (UTC)</FormLabel>
                          <FormControl>
                            <Input type="datetime-local" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>

                  <FormField
                    control={form.control}
                    name="photo"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Photo URL (optional)</FormLabel>
                        <FormControl>
                          <Input placeholder="https://example.com/photo.jpg" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="space-y-4">
                  <h3 className="text-sm font-medium">Assignment</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="zoneId"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Zone</FormLabel>
                          <FormControl>
                            <select
                              className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(e.target.value)}
                            >
                              <option value="">Select a zone</option>
                              {zones.map((zone) => (
                                <option key={zone.id} value={zone.id}>
                                  {zone.name}
                                </option>
                              ))}
                            </select>
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="depotId"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Depot</FormLabel>
                          <FormControl>
                            <select
                              className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(e.target.value)}
                            >
                              <option value="">Select a depot</option>
                              {depots.map((depot) => (
                                <option key={depot.id} value={depot.id}>
                                  {depot.name}
                                </option>
                              ))}
                            </select>
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                <FormField
                  control={form.control}
                  name="isActive"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-start justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel>Active</FormLabel>
                        <FormDescription>
                          Mark this driver as active for dispatch
                        </FormDescription>
                      </div>
                      <FormControl>
                        <input
                          type="checkbox"
                          checked={field.value}
                          onChange={field.onChange}
                          className="mt-1 h-4 w-4 rounded border-gray-300"
                        />
                      </FormControl>
                    </FormItem>
                  )}
                />
              </>
            )}

            {activeTab === "schedule" && (
              <div className="space-y-4">
                <h3 className="text-sm font-medium">Weekly Schedule</h3>
                <p className="text-sm text-muted-foreground">
                  Set the operating hours for each day of the week. Leave empty if the driver does not work that day.
                </p>
                <div className="space-y-3">
                  {DAYS_OF_WEEK.map(({ value, label }) => {
                    const schedule = getScheduleForDay(value);
                    return (
                      <div key={value} className="grid grid-cols-3 gap-4 items-center">
                        <span className="text-sm font-medium">{label}</span>
                        <div>
                          <Input
                            type="time"
                            step="1"
                            placeholder="Open"
                            value={schedule.openTime ?? ""}
                            onChange={(e) => handleScheduleTimeChange(value, "openTime", e.target.value)}
                          />
                        </div>
                        <div>
                          <Input
                            type="time"
                            step="1"
                            placeholder="Close"
                            value={schedule.closeTime ?? ""}
                            onChange={(e) => handleScheduleTimeChange(value, "closeTime", e.target.value)}
                          />
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}

            {activeTab === "daysoff" && (
              <div className="space-y-4">
                <h3 className="text-sm font-medium">Days Off</h3>
                <p className="text-sm text-muted-foreground">
                  Add days when the driver is not available for dispatch.
                </p>
                <div className="flex gap-2">
                  <Input
                    type="date"
                    value={newDayOffDate}
                    onChange={(e) => setNewDayOffDate(e.target.value)}
                  />
                  <Button type="button" variant="outline" onClick={addDayOff}>
                    Add
                  </Button>
                </div>
                {daysOff.length > 0 && (
                  <div className="mt-4 space-y-2">
                    {daysOff.map((dayOff, index) => (
                      <div key={index} className="flex items-center justify-between rounded-md border px-3 py-2">
                        <span className="text-sm">{dayOff.date}</span>
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => removeDayOff(dayOff.date)}
                        >
                          Remove
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </CardContent>
          <CardFooter className="flex justify-between">
            <Button variant="outline" type="button" onClick={() => router.push("/drivers")}>
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {isEditing ? "Update Driver" : "Create Driver"}
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}
