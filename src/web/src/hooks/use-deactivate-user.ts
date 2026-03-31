"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { deactivateUser } from "@/services/users.service";
import { userKeys } from "@/lib/query-key-factory";
import { toast } from "sonner";

export function useDeactivateUser() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? "";

  return useMutation({
    mutationFn: (userId: string) => deactivateUser(token, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userKeys.all });
      toast.success("User deactivated successfully");
    },
    onError: () => {
      toast.error("Failed to deactivate user");
    },
  });
}
