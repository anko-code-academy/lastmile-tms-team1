"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { activateUser } from "@/services/users.service";
import { userKeys } from "@/lib/query-key-factory";
import { toast } from "sonner";

export function useActivateUser() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? "";

  return useMutation({
    mutationFn: (userId: string) => activateUser(token, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.all });
      toast.success("User activated successfully");
    },
    onError: () => {
      toast.error("Failed to activate user");
    },
  });
}
