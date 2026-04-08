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
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { useDriver, useCreateDriver, useUpdateDriver } from "@/hooks/use-drivers";
import { toast } from "sonner";


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
  email: z.string().min(1, "Email is required").email("Valid email is required").max(255),
  licenseNumber: z.string().min(1, "License number is required").max(50),
  licenseExpiryDate: z.string().min(1, "License expiry date is required"),
  photo: z.string().nullable().optional().transform((v) => v ?? ""),
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

  const { data: driverData, isLoading: driverLoading } = useDriver(driverId!);
  const createDriver = useCreateDriver();
  const updateDriver = useUpdateDriver();

  const form = useForm<DriverFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(driverFormSchema) as any,
    defaultValues: {
      email: "",
      licenseNumber: "",
      licenseExpiryDate: "",
      photo: "",
      shiftSchedules: [],
      daysOff: [],
    },
  });

  useEffect(() => {
    if (driverData) {
      form.reset({
        email: driverData.user?.email ?? "",
        licenseNumber: driverData.licenseNumber,
        licenseExpiryDate: driverData.licenseExpiryDate.slice(0, 16),
        photo: driverData.photo ?? "",
        shiftSchedules: driverData.shiftSchedules?.map(s => ({
          dayOfWeek: s.dayOfWeek,
          openTime: s.openTime || null,
          closeTime: s.closeTime || null,
        })) ?? [],
        daysOff: driverData.daysOff?.map(d => ({
          date: d.date.slice(0, 10),
        })) ?? [],
      });
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
      const shiftSchedules = values.shiftSchedules
        ?.filter(s => s.openTime && s.closeTime)
        ?.map(s => ({
          dayOfWeek: s.dayOfWeek,
          openTime: s.openTime,
          closeTime: s.closeTime,
        })) ?? [];

      const daysOff = values.daysOff?.map(d => ({
        date: `${d.date}T00:00:00Z`,
      })) ?? [];

      if (isEditing && driverId) {
        await updateDriver.mutateAsync({
          id: driverId,
          licenseNumber: values.licenseNumber,
          licenseExpiryDate: `${values.licenseExpiryDate}:00Z`,
          photo: values.photo || undefined,
          shiftSchedules,
          daysOff,
        });
        toast.success("Driver updated successfully");
      } else {
        await createDriver.mutateAsync({
          email: values.email,
          licenseNumber: values.licenseNumber,
          licenseExpiryDate: `${values.licenseExpiryDate}:00Z`,
          photo: values.photo || undefined,
          shiftSchedules,
          daysOff,
        });
        toast.success("Driver created successfully");
      }
      router.push("/drivers");
    } catch (err) {
      toast.error(isEditing ? "Failed to update driver" : "Failed to create driver");
      console.error(err);
    }
  }

  if ((driverLoading && isEditing)) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Loading...
        </CardContent>
      </Card>
    );
  }

  const daysOff = form.watch("daysOff") ?? [];

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
                  <h3 className="text-sm font-medium">User Information</h3>
                  <p className="text-sm text-muted-foreground">
                    The driver must be an existing user in the system. Enter their email to link the driver record.
                  </p>
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="email"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Email</FormLabel>
                          <FormControl>
                            <Input
                              placeholder="john.doe@example.com"
                              type="email"
                              disabled={isEditing}
                              {...field}
                            />
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

                {isEditing && driverData?.user && (
                  <div className="space-y-4">
                    <h3 className="text-sm font-medium">Driver Details</h3>
                    <div className="grid grid-cols-2 gap-4">
                      <div className="text-sm">
                        <span className="font-medium">Name:</span> {driverData.user.firstName} {driverData.user.lastName}
                      </div>
                      <div className="text-sm">
                        <span className="font-medium">Phone:</span> {driverData.user.phoneNumber ?? "N/A"}
                      </div>
                      <div className="text-sm">
                        <span className="font-medium">Zone:</span> {driverData.user.zone?.name ?? "N/A"}
                      </div>
                      <div className="text-sm">
                        <span className="font-medium">Depot:</span> {driverData.user.depot?.name ?? "N/A"}
                      </div>
                    </div>
                  </div>
                )}
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
              disabled={createDriver.isPending || updateDriver.isPending}
            >
              {isEditing ? "Update Driver" : "Create Driver"}
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}
