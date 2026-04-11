"use client";

import { useEffect, useRef } from "react";
import { Input } from "@/components/ui/input";

interface ScanInputProps {
  placeholder: string;
  disabled: boolean;
  onSubmit: (value: string) => void;
}

export function ScanInput({ placeholder, disabled, onSubmit }: ScanInputProps) {
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!disabled) {
      inputRef.current?.focus();
    }
  }, [disabled]);

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter") {
      e.preventDefault();
      const value = (e.target as HTMLInputElement).value.trim();
      if (value) {
        onSubmit(value);
        (e.target as HTMLInputElement).value = "";
      }
    }
  };

  return (
    <Input
      ref={inputRef}
      placeholder={placeholder}
      disabled={disabled}
      onKeyDown={handleKeyDown}
      className="text-lg font-mono"
      autoComplete="off"
    />
  );
}
