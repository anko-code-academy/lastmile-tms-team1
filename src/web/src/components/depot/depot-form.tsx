"use client";

import { useEffect } from "react";
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
import { GET_DEPOT_QUERY } from "@/lib/graphql/queries/depot";
import { CREATE_DEPOT_MUTATION, UPDATE_DEPOT_MUTATION } from "@/lib/graphql/mutations/depot";
import type { DepotDto, CreateDepotInput, UpdateDepotInput, AddressInput } from "@/lib/graphql/types";
import { toast } from "sonner";

interface DepotResponse {
  depot: DepotDto;
}

const depotFormSchema = z.object({
  name: z.string().min(1, "Depot name is required"),
  isActive: z.boolean(),
  address: z.object({
    street1: z.string().min(1, "Street address is required").optional(),
    street2: z.string().nullable().optional().transform((v) => v ?? ""),
    city: z.string().min(1, "City is required").optional(),
    state: z.string().min(1, "State is required").optional(),
    postalCode: z.string().min(1, "Postal code is required").optional(),
    countryCode: z.string().min(1).optional(),
    isResidential: z.boolean().optional(),
    contactName: z.string().nullable().optional().transform((v) => v ?? ""),
    companyName: z.string().nullable().optional().transform((v) => v ?? ""),
    phone: z.string().nullable().optional().transform((v) => v ?? ""),
    email: z.string().nullable().optional().transform((v) => v ?? ""),
  }).optional(),
});

type DepotFormValues = z.infer<typeof depotFormSchema>;

interface DepotFormProps {
  depotId?: string;
}

export function DepotForm({ depotId }: DepotFormProps) {
  const router = useRouter();
  const isEditing = !!depotId;

  const { data, isLoading } = useGraphQuery<DepotResponse, { id: string }>({
    queryKey: ["depots", depotId ?? ""] as string[],
    query: GET_DEPOT_QUERY,
    variables: { id: depotId! } as { id: string },
    enabled: isEditing,
  });

  const createMutation = useGraphMutation<{ createDepot: { id: string } }, { input: CreateDepotInput }>({
    mutation: CREATE_DEPOT_MUTATION,
    invalidateKeys: ["depots"],
  });

  const updateMutation = useGraphMutation<{ updateDepot: { id: string } }, { input: UpdateDepotInput }>({
    mutation: UPDATE_DEPOT_MUTATION,
    invalidateKeys: ["depots"],
  });

  const form = useForm<DepotFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(depotFormSchema) as any,
    defaultValues: {
      name: "",
      isActive: true,
    },
  });

  useEffect(() => {
    if (data?.depot) {
      const depot = data.depot;
      form.reset({
        name: depot.name,
        isActive: depot.isActive,
        address: depot.address
          ? {
              street1: depot.address.street1,
              street2: depot.address.street2 ?? "",
              city: depot.address.city,
              state: depot.address.state,
              postalCode: depot.address.postalCode,
              countryCode: depot.address.countryCode,
              isResidential: depot.address.isResidential,
              contactName: depot.address.contactName ?? "",
              companyName: depot.address.companyName ?? "",
              phone: depot.address.phone ?? "",
              email: depot.address.email ?? "",
            }
          : undefined,
      });
    }
  }, [data, form]);

  async function onSubmit(values: DepotFormValues) {
    try {
      // Only include address if there's actual address data
      const hasAddressData = values.address && (
        values.address.street1 ||
        values.address.city ||
        values.address.state ||
        values.address.postalCode
      );

      const address: AddressInput | undefined = hasAddressData
        ? ({
            ...(values.address!.street1 && { street1: values.address!.street1 }),
            ...(values.address!.street2 && { street2: values.address!.street2 }),
            ...(values.address!.city && { city: values.address!.city }),
            ...(values.address!.state && { state: values.address!.state }),
            ...(values.address!.postalCode && { postalCode: values.address!.postalCode }),
            ...(values.address!.countryCode && { countryCode: values.address!.countryCode }),
            ...(values.address!.isResidential !== undefined && { isResidential: values.address!.isResidential }),
            ...(values.address!.contactName && { contactName: values.address!.contactName }),
            ...(values.address!.companyName && { companyName: values.address!.companyName }),
            ...(values.address!.phone && { phone: values.address!.phone }),
            ...(values.address!.email && { email: values.address!.email }),
          } as AddressInput)
        : undefined;

      if (isEditing && depotId) {
        await updateMutation.mutateAsync({
          input: {
            id: depotId,
            name: values.name,
            address,
            isActive: values.isActive,
          },
        });
        toast.success("Depot updated successfully");
      } else {
        await createMutation.mutateAsync({
          input: {
            name: values.name,
            address,
            isActive: values.isActive,
          },
        });
        toast.success("Depot created successfully");
      }
      router.push("/depots");
    } catch (err) {
      toast.error(isEditing ? "Failed to update depot" : "Failed to create depot");
      console.error(err);
    }
  }

  if (isLoading && isEditing) {
    return (
      <Card>
        <CardContent className="py-8 text-center text-muted-foreground">
          Loading depot...
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{isEditing ? "Edit Depot" : "Create Depot"}</CardTitle>
        <CardDescription>
          {isEditing
            ? "Update the depot details below."
            : "Enter the depot details below."}
        </CardDescription>
      </CardHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <CardContent className="space-y-6">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Depot Name</FormLabel>
                  <FormControl>
                    <Input placeholder="Enter depot name" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="isActive"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <FormLabel>Active</FormLabel>
                    <FormDescription>
                      Mark this depot as active for operations
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

            <div className="space-y-4">
              <h3 className="text-sm font-medium">Address</h3>

              <FormField
                control={form.control}
                name="address.street1"
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
                control={form.control}
                name="address.street2"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Apt, Suite, etc. (optional)</FormLabel>
                    <FormControl>
                      <Input placeholder="Apt 4B" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="address.city"
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
                  control={form.control}
                  name="address.state"
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
                  control={form.control}
                  name="address.postalCode"
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
                  control={form.control}
                  name="address.countryCode"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Country Code</FormLabel>
                      <FormControl>
                        <Input placeholder="US" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </div>
          </CardContent>
          <CardFooter className="flex justify-between">
            <Button variant="outline" type="button" onClick={() => router.back()}>
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {isEditing ? "Update Depot" : "Create Depot"}
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}
