"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { createUser } from "@/services/users.service";
import { userKeys } from "@/lib/query-key-factory";
import { toast } from "sonner";

export function useCreateUser() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? "";

  return useMutation({
    mutationFn: (input: {
      firstName: string;
      lastName: string;
      email: string;
      phone?: string;
      roleId: string;
      zoneId?: string;
      depotId?: string;
      password: string;
    }) => createUser(token, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.all });
      toast.success("User created successfully");
    },
    onError: () => {
      toast.error("Failed to create user");
    },
  });
}
