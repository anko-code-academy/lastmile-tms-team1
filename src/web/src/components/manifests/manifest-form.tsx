"use client";

import { useState, useCallback } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import Link from "next/link";
import { Upload, Type, Check, X } from "lucide-react";
import { useDepots } from "@/hooks/use-depots";

const TRACKING_NUMBER_PATTERN = /^LM-\d{6}-[A-Z0-9]{6}$/;

const manifestSchema = z.object({
  name: z.string().min(1, "Manifest name is required").max(200),
  depotId: z.string().min(1, "Select a depot"),
  trackingNumbers: z.array(z.string()).min(1, "At least one tracking number is required"),
});

type ManifestFormValues = z.infer<typeof manifestSchema>;

interface ManifestFormProps {
  onSubmit: (values: ManifestFormValues) => Promise<void>;
  isSubmitting?: boolean;
}

function parseTrackingNumbers(content: string): { valid: string[]; invalid: string[] } {
  const lines = content
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);

  const headerNames = new Set(["trackingnumber", "tracking_number", "tracking number"]);

  const valid: string[] = [];
  const invalid: string[] = [];

  for (const line of lines) {
    if (headerNames.has(line.toLowerCase())) continue;
    if (TRACKING_NUMBER_PATTERN.test(line)) {
      valid.push(line);
    } else {
      invalid.push(line);
    }
  }

  return { valid: [...new Set(valid)], invalid };
}

export function ManifestForm({ onSubmit, isSubmitting }: ManifestFormProps) {
  const [inputMode, setInputMode] = useState<"csv" | "manual">("csv");
  const [parsedValid, setParsedValid] = useState<string[]>([]);
  const [parsedInvalid, setParsedInvalid] = useState<string[]>([]);
  const [csvFileName, setCsvFileName] = useState<string | null>(null);

  const form = useForm<ManifestFormValues>({
    resolver: zodResolver(manifestSchema),
    defaultValues: {
      name: "",
      depotId: "",
      trackingNumbers: [],
    },
  });

  const { data: depots, isLoading: depotsLoading } = useDepots();

  const handleParsed = useCallback(
    (valid: string[], invalid: string[]) => {
      setParsedValid(valid);
      setParsedInvalid(invalid);
      form.setValue("trackingNumbers", valid, { shouldValidate: true });
    },
    [form]
  );

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setCsvFileName(file.name);
    const reader = new FileReader();
    reader.onload = (event) => {
      const content = event.target?.result as string;
      const { valid, invalid } = parseTrackingNumbers(content);
      handleParsed(valid, invalid);
    };
    reader.readAsText(file);
  };

  const handleManualInput = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    const text = e.target.value;
    const { valid, invalid } = parseTrackingNumbers(text);
    handleParsed(valid, invalid);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Manifest Name</FormLabel>
              <FormControl>
                <Input placeholder="e.g. Morning Shipment #12" {...field} />
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
                  value={field.value}
                  onChange={(e) => field.onChange(e.target.value)}
                  disabled={depotsLoading}
                >
                  <option value="">Select depot</option>
                  {(depots ?? []).map((depot) => (
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

        <div>
          <FormLabel className="mb-2 block">Tracking Numbers</FormLabel>
          <div className="flex gap-2 mb-3">
            <Button
              type="button"
              variant={inputMode === "csv" ? "default" : "outline"}
              size="sm"
              onClick={() => setInputMode("csv")}
            >
              <Upload className="h-4 w-4 mr-1" />
              Upload CSV
            </Button>
            <Button
              type="button"
              variant={inputMode === "manual" ? "default" : "outline"}
              size="sm"
              onClick={() => setInputMode("manual")}
            >
              <Type className="h-4 w-4 mr-1" />
              Enter Manually
            </Button>
          </div>

          {inputMode === "csv" ? (
            <div>
              <Input
                type="file"
                accept=".csv"
                onChange={handleFileChange}
                className="max-w-md"
              />
              {csvFileName && (
                <p className="text-sm text-muted-foreground mt-1">
                  File: {csvFileName}
                </p>
              )}
            </div>
          ) : (
            <textarea
              className="flex w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-xs ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50 min-h-[120px] font-mono"
              placeholder={"LM-260412-ABC123\nLM-260412-DEF456"}
              onChange={handleManualInput}
            />
          )}

          {parsedValid.length > 0 && (
            <div className="mt-3 border rounded-md p-3">
              <div className="flex items-center gap-4 text-sm mb-2">
                <span className="text-green-600 font-medium">
                  <Check className="h-3 w-3 inline mr-1" />
                  {parsedValid.length} valid
                </span>
                {parsedInvalid.length > 0 && (
                  <span className="text-red-600 font-medium">
                    <X className="h-3 w-3 inline mr-1" />
                    {parsedInvalid.length} invalid (will be excluded)
                  </span>
                )}
              </div>
              <div className="max-h-32 overflow-y-auto text-xs space-y-0.5">
                {parsedValid.map((tn) => (
                  <div key={tn} className="text-muted-foreground">
                    {tn}
                  </div>
                ))}
                {parsedInvalid.map((tn, i) => (
                  <div key={`inv-${i}`} className="text-red-500 line-through">
                    {tn}
                  </div>
                ))}
              </div>
            </div>
          )}

          {form.formState.errors.trackingNumbers && (
            <p className="text-sm font-medium text-destructive mt-1">
              {form.formState.errors.trackingNumbers.message}
            </p>
          )}
        </div>

        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" asChild>
            <Link href="/manifests">Cancel</Link>
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating..." : "Create Manifest"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
