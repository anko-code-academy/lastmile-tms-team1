"use client";

import { useRouter } from "next/navigation";
import { useForm, useWatch } from "react-hook-form";
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
import { useCreateParcel } from "@/hooks/use-parcels";
import { CreateParcelInput, ServiceType, ParcelType, WeightUnit, DimensionUnit } from "@/lib/graphql/types";

// Simple approximate delivery calculation (skips weekends only, no holidays)
function getEstimatedDelivery(serviceType: ServiceType): string {
  const businessDays = {
    [ServiceType.ECONOMY]: 10,
    [ServiceType.STANDARD]: 5,
    [ServiceType.EXPRESS]: 2,
    [ServiceType.OVERNIGHT]: 1,
  }[serviceType] ?? 5;

  let count = 0;
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  while (count < businessDays) {
    d.setDate(d.getDate() + 1);
    const day = d.getDay();
    if (day !== 0 && day !== 6) count++;
  }
  return d.toLocaleDateString("en-US", { weekday: "long", month: "short", day: "numeric" });
}

const addressSchema = z.object({
  street1: z.string().min(1, "Street address is required"),
  street2: z.string().nullable().optional().transform((v) => v ?? ""),
  city: z.string().min(1, "City is required"),
  state: z.string().min(1, "State is required"),
  postalCode: z.string().min(1, "Postal code is required"),
  countryCode: z.string().min(2).max(2).optional(),
  isResidential: z.boolean().optional(),
  contactName: z.string().nullable().optional().transform((v) => v ?? ""),
  companyName: z.string().nullable().optional().transform((v) => v ?? ""),
  phone: z.string().nullable().optional().transform((v) => v ?? ""),
  email: z.string().nullable().optional().transform((v) => v ?? ""),
});

const parcelFormSchema = z.object({
  description: z.string().optional(),
  serviceType: z.string(),
  shipperAddress: addressSchema,
  recipientAddress: addressSchema,
  weight: z.number().min(0.01, "Weight must be greater than 0"),
  weightUnit: z.string(),
  length: z.number().min(0.01, "Length must be greater than 0"),
  width: z.number().min(0.01, "Width must be greater than 0"),
  height: z.number().min(0.01, "Height must be greater than 0"),
  dimensionUnit: z.string(),
  declaredValue: z.number().min(0, "Declared value must be 0 or greater"),
  currency: z.string().min(3).max(3).optional(),
  parcelType: z.string().nullable().optional().transform((v) => v ?? ""),
  notes: z.string().nullable().optional().transform((v) => v ?? ""),
});

type ParcelFormValues = z.infer<typeof parcelFormSchema>;

function AddressSection({ title, prefix }: { title: string; prefix: string }) {
  return (
    <div className="space-y-4">
      <h3 className="text-sm font-medium">{title}</h3>

      <FormField
        name={`${prefix}Address.street1`}
        render={({ field }) => (
          <FormItem>
            <FormLabel>Street Address</FormLabel>
            <FormControl>
              <Input placeholder="123 Main St" {...field} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <FormField
        name={`${prefix}Address.street2`}
        render={({ field }) => (
          <FormItem>
            <FormLabel>Apt, Suite, etc. (optional)</FormLabel>
            <FormControl>
              <Input placeholder="Apt 4B" {...field} value={field.value ?? ""} />
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />

      <div className="grid grid-cols-2 gap-4">
        <FormField
          name={`${prefix}Address.city`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>City</FormLabel>
              <FormControl>
                <Input placeholder="New York" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          name={`${prefix}Address.state`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>State</FormLabel>
              <FormControl>
                <Input placeholder="NY" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FormField
          name={`${prefix}Address.postalCode`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Postal Code</FormLabel>
              <FormControl>
                <Input placeholder="10001" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          name={`${prefix}Address.countryCode`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Country Code</FormLabel>
              <FormControl>
                <Input placeholder="US" {...field} value={field.value ?? "US"} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      <FormField
        name={`${prefix}Address.isResidential`}
        render={({ field }) => (
          <FormItem className="flex flex-row items-start justify-between rounded-lg border p-4">
            <div className="space-y-0.5">
              <FormLabel>Residential Address</FormLabel>
            </div>
            <FormControl>
              <input
                type="checkbox"
                checked={field.value ?? false}
                onChange={field.onChange}
                className="mt-1 h-4 w-4 rounded border-gray-300"
              />
            </FormControl>
          </FormItem>
        )}
      />

      <div className="grid grid-cols-2 gap-4">
        <FormField
          name={`${prefix}Address.contactName`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Contact Name (optional)</FormLabel>
              <FormControl>
                <Input placeholder="John Doe" {...field} value={field.value ?? ""} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          name={`${prefix}Address.companyName`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Company Name (optional)</FormLabel>
              <FormControl>
                <Input placeholder="Acme Inc" {...field} value={field.value ?? ""} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <FormField
          name={`${prefix}Address.phone`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Phone (optional)</FormLabel>
              <FormControl>
                <Input placeholder="+1234567890" {...field} value={field.value ?? ""} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          name={`${prefix}Address.email`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email (optional)</FormLabel>
              <FormControl>
                <Input placeholder="email@example.com" {...field} value={field.value ?? ""} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
      </div>
    </div>
  );
}

export function ParcelForm() {
  const router = useRouter();
  const createParcel = useCreateParcel();

  const form = useForm<ParcelFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(parcelFormSchema) as any,
    defaultValues: {
      description: "",
      serviceType: ServiceType.STANDARD,
      weight: 1,
      weightUnit: WeightUnit.Kg,
      length: 10,
      width: 10,
      height: 10,
      dimensionUnit: DimensionUnit.Cm,
      declaredValue: 0,
      currency: "USD",
      parcelType: "",
      notes: "",
      shipperAddress: {
        street1: "",
        street2: "",
        city: "",
        state: "",
        postalCode: "",
        countryCode: "US",
        isResidential: false,
        contactName: "",
        companyName: "",
        phone: "",
        email: "",
      },
      recipientAddress: {
        street1: "",
        street2: "",
        city: "",
        state: "",
        postalCode: "",
        countryCode: "US",
        isResidential: false,
        contactName: "",
        companyName: "",
        phone: "",
        email: "",
      },
    },
  });

  const serviceType = useWatch({ control: form.control, name: "serviceType" });
  const estimatedDelivery = serviceType
    ? getEstimatedDelivery(serviceType as ServiceType)
    : "";

  async function onSubmit(values: ParcelFormValues) {
    try {
      const input: CreateParcelInput = {
        description: values.description || undefined,
        serviceType: values.serviceType as ServiceType,
        shipperAddress: {
          street1: values.shipperAddress.street1,
          street2: values.shipperAddress.street2 || undefined,
          city: values.shipperAddress.city,
          state: values.shipperAddress.state,
          postalCode: values.shipperAddress.postalCode,
          countryCode: values.shipperAddress.countryCode,
          isResidential: values.shipperAddress.isResidential,
          contactName: values.shipperAddress.contactName || undefined,
          companyName: values.shipperAddress.companyName || undefined,
          phone: values.shipperAddress.phone || undefined,
          email: values.shipperAddress.email || undefined,
        },
        recipientAddress: {
          street1: values.recipientAddress.street1,
          street2: values.recipientAddress.street2 || undefined,
          city: values.recipientAddress.city,
          state: values.recipientAddress.state,
          postalCode: values.recipientAddress.postalCode,
          countryCode: values.recipientAddress.countryCode,
          isResidential: values.recipientAddress.isResidential,
          contactName: values.recipientAddress.contactName || undefined,
          companyName: values.recipientAddress.companyName || undefined,
          phone: values.recipientAddress.phone || undefined,
          email: values.recipientAddress.email || undefined,
        },
        weight: values.weight,
        weightUnit: values.weightUnit as WeightUnit,
        length: values.length,
        width: values.width,
        height: values.height,
        dimensionUnit: values.dimensionUnit as DimensionUnit,
        declaredValue: values.declaredValue,
        currency: values.currency || "USD",
        parcelType: (values.parcelType as ParcelType) || undefined,
        notes: values.notes || undefined,
      };

      await createParcel.mutateAsync(input);
      router.push("/parcels");
    } catch (err) {
      console.error(err);
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Register Parcel</CardTitle>
        <CardDescription>
          Enter the parcel details below to register a new shipment.
        </CardDescription>
      </CardHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <CardContent className="space-y-8">
            <AddressSection title="Shipper Address" prefix="shipper" />
            <AddressSection title="Recipient Address" prefix="recipient" />

            <div className="space-y-4">
              <h3 className="text-sm font-medium">Package Information</h3>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="serviceType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Service Type</FormLabel>
                      <FormControl>
                        <select
                          className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                          value={field.value}
                          onChange={(e) => field.onChange(e.target.value)}
                        >
                          <option value={ServiceType.ECONOMY}>Economy</option>
                          <option value={ServiceType.STANDARD}>Standard</option>
                          <option value={ServiceType.EXPRESS}>Express</option>
                          <option value={ServiceType.OVERNIGHT}>Overnight</option>
                        </select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Description (optional)</FormLabel>
                      <FormControl>
                        <Input placeholder="Package description" {...field} value={field.value ?? ""} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-4 gap-4">
                <FormField
                  control={form.control}
                  name="weight"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Weight</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.01"
                          {...field}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="weightUnit"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Unit</FormLabel>
                      <FormControl>
                        <select
                          className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                          value={field.value}
                          onChange={(e) => field.onChange(e.target.value)}
                        >
                          <option value={WeightUnit.Kg}>kg</option>
                          <option value={WeightUnit.Lb}>lb</option>
                        </select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="length"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Length</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.1"
                          {...field}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="width"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Width</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.1"
                          {...field}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-4 gap-4">
                <FormField
                  control={form.control}
                  name="height"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Height</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.1"
                          {...field}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="dimensionUnit"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Unit</FormLabel>
                      <FormControl>
                        <select
                          className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                          value={field.value}
                          onChange={(e) => field.onChange(e.target.value)}
                        >
                          <option value={DimensionUnit.Cm}>cm</option>
                          <option value={DimensionUnit.In}>in</option>
                        </select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="declaredValue"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Declared Value</FormLabel>
                      <FormControl>
                        <Input
                          type="number"
                          step="0.01"
                          {...field}
                          onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="currency"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Currency</FormLabel>
                      <FormControl>
                        <Input placeholder="USD" {...field} value={field.value ?? "USD"} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="parcelType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Parcel Type (optional)</FormLabel>
                      <FormControl>
                        <select
                          className="flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
                          value={field.value ?? ""}
                          onChange={(e) => field.onChange(e.target.value || undefined)}
                        >
                          <option value="">Select type...</option>
                          <option value={ParcelType.PACKAGE}>Package</option>
                          <option value={ParcelType.ENVELOPE}>Envelope</option>
                          <option value={ParcelType.PALLET}>Pallet</option>
                          <option value={ParcelType.BULK}>Bulk</option>
                        </select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="flex flex-col justify-center rounded-md border border-input bg-muted/50 px-3 py-2 text-sm">
                  <span className="text-muted-foreground">Est. Delivery</span>
                  <span className="font-medium">{estimatedDelivery || "—"}</span>
                </div>
              </div>

              <FormField
                control={form.control}
                name="notes"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Notes (optional)</FormLabel>
                    <FormControl>
                      <Input placeholder="Special handling instructions..." {...field} value={field.value ?? ""} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </CardContent>
          <CardFooter className="flex justify-between">
            <Button variant="outline" type="button" onClick={() => router.push("/parcels")}>
              Cancel
            </Button>
            <Button type="submit" disabled={createParcel.isPending}>
              Register Parcel
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}
