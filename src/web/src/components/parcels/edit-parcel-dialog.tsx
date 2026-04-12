"use client";

import { useForm, type Control } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { useUpdateParcel } from "@/hooks/use-parcels";
import {
  ServiceType,
  ParcelType,
  type GetParcelByTrackingNumberQuery,
} from "@/graphql/generated/graphql";

type ParcelData = NonNullable<GetParcelByTrackingNumberQuery["parcelByTrackingNumber"]>;

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

const editParcelSchema = z.object({
  description: z.string().nullable().optional().transform((v) => v || undefined),
  serviceType: z.string().optional(),
  parcelType: z.string().nullable().optional().transform((v) => v || undefined),
  weight: z.number().min(0.01, "Weight must be greater than 0").optional(),
  length: z.number().min(0.01).optional(),
  width: z.number().min(0.01).optional(),
  height: z.number().min(0.01).optional(),
  shipperAddress: addressSchema.optional(),
  recipientAddress: addressSchema.optional(),
});

type EditParcelValues = z.infer<typeof editParcelSchema>;

const selectClass =
  "flex w-full items-center justify-between rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50";

interface EditParcelDialogProps {
  parcel: ParcelData;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

function AddressFields({ prefix, control }: { prefix: "shipperAddress" | "recipientAddress"; control: Control<EditParcelValues> }) {
  return (
    <div className="space-y-4">
      <FormField
        control={control}
        name={`${prefix}.street1`}
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
        control={control}
        name={`${prefix}.street2`}
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
          control={control}
          name={`${prefix}.city`}
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
          control={control}
          name={`${prefix}.state`}
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
          control={control}
          name={`${prefix}.postalCode`}
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
          control={control}
          name={`${prefix}.countryCode`}
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
      <div className="grid grid-cols-2 gap-4">
        <FormField
          control={control}
          name={`${prefix}.contactName`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Contact Name</FormLabel>
              <FormControl>
                <Input placeholder="John Doe" {...field} value={field.value ?? ""} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={control}
          name={`${prefix}.companyName`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Company Name</FormLabel>
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
          control={control}
          name={`${prefix}.phone`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Phone</FormLabel>
              <FormControl>
                <Input placeholder="+1234567890" {...field} value={field.value ?? ""} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={control}
          name={`${prefix}.email`}
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
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

export function EditParcelDialog({ parcel, open, onOpenChange }: EditParcelDialogProps) {
  const updateParcel = useUpdateParcel();

  const form = useForm<EditParcelValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(editParcelSchema) as any,
    defaultValues: {
      description: parcel.description ?? "",
      serviceType: parcel.serviceType,
      parcelType: parcel.parcelType ?? "",
      weight: Number(parcel.weight),
      length: Number(parcel.length),
      width: Number(parcel.width),
      height: Number(parcel.height),
      shipperAddress: {
        street1: parcel.shipperAddress.street1,
        street2: parcel.shipperAddress.street2 ?? "",
        city: parcel.shipperAddress.city,
        state: parcel.shipperAddress.state,
        postalCode: parcel.shipperAddress.postalCode,
        countryCode: parcel.shipperAddress.countryCode ?? "US",
        isResidential: parcel.shipperAddress.isResidential,
        contactName: parcel.shipperAddress.contactName ?? "",
        companyName: parcel.shipperAddress.companyName ?? "",
        phone: parcel.shipperAddress.phone ?? "",
        email: parcel.shipperAddress.email ?? "",
      },
      recipientAddress: {
        street1: parcel.recipientAddress.street1,
        street2: parcel.recipientAddress.street2 ?? "",
        city: parcel.recipientAddress.city,
        state: parcel.recipientAddress.state,
        postalCode: parcel.recipientAddress.postalCode,
        countryCode: parcel.recipientAddress.countryCode ?? "US",
        isResidential: parcel.recipientAddress.isResidential,
        contactName: parcel.recipientAddress.contactName ?? "",
        companyName: parcel.recipientAddress.companyName ?? "",
        phone: parcel.recipientAddress.phone ?? "",
        email: parcel.recipientAddress.email ?? "",
      },
    },
  });

  async function onSubmit(values: EditParcelValues) {
    try {
      await updateParcel.mutateAsync({
        id: parcel.id,
        description: values.description,
        serviceType: values.serviceType as ServiceType,
        parcelType: (values.parcelType as ParcelType) || undefined,
        weight: values.weight,
        length: values.length,
        width: values.width,
        height: values.height,
        shipperAddress: values.shipperAddress
          ? {
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
            }
          : undefined,
        recipientAddress: values.recipientAddress
          ? {
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
            }
          : undefined,
      });
      onOpenChange(false);
    } catch (err) {
      console.error(err);
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Edit Parcel</DialogTitle>
          <DialogDescription>
            Update parcel details for {parcel.trackingNumber}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            {/* Parcel Info */}
            <div className="space-y-4">
              <h3 className="text-sm font-medium">Parcel Information</h3>
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Description</FormLabel>
                    <FormControl>
                      <Input placeholder="Package description" {...field} value={field.value ?? ""} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="serviceType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Service Type</FormLabel>
                      <FormControl>
                        <select
                          className={selectClass}
                          value={field.value}
                          onChange={(e) => field.onChange(e.target.value)}
                        >
                          <option value={ServiceType.Economy}>Economy</option>
                          <option value={ServiceType.Standard}>Standard</option>
                          <option value={ServiceType.Express}>Express</option>
                          <option value={ServiceType.Overnight}>Overnight</option>
                        </select>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="parcelType"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Parcel Type</FormLabel>
                      <FormControl>
                        <select
                          className={selectClass}
                          value={field.value ?? ""}
                          onChange={(e) => field.onChange(e.target.value || undefined)}
                        >
                          <option value="">Select type...</option>
                          <option value={ParcelType.Package}>Package</option>
                          <option value={ParcelType.Envelope}>Envelope</option>
                          <option value={ParcelType.Pallet}>Pallet</option>
                          <option value={ParcelType.Bulk}>Bulk</option>
                        </select>
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
                      <FormLabel>Weight ({parcel.weightUnit})</FormLabel>
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
                  name="length"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Length ({parcel.dimensionUnit})</FormLabel>
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
                      <FormLabel>Width ({parcel.dimensionUnit})</FormLabel>
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
                  name="height"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Height ({parcel.dimensionUnit})</FormLabel>
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
            </div>

            {/* Shipper Address */}
            <div className="space-y-4">
              <h3 className="text-sm font-medium">Shipper Address</h3>
              <AddressFields prefix="shipperAddress" control={form.control} />
            </div>

            {/* Recipient Address */}
            <div className="space-y-4">
              <h3 className="text-sm font-medium">Recipient Address</h3>
              <AddressFields prefix="recipientAddress" control={form.control} />
            </div>

            <DialogFooter>
              <Button variant="outline" type="button" onClick={() => onOpenChange(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={updateParcel.isPending}>
                {updateParcel.isPending ? "Saving..." : "Save Changes"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
