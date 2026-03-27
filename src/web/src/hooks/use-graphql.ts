"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getGraphQLClientFromToken } from "@/lib/graphql/client";
import { useSession } from "next-auth/react";

export function useGraphQuery<TData, TVariables>({
  queryKey,
  query,
  variables,
  enabled = true,
}: {
  queryKey: string[];
  query: string;
  variables?: TVariables;
  enabled?: boolean;
}) {
  const { data: session } = useSession();

  return useQuery({
    queryKey,
    queryFn: async (): Promise<TData> => {
      if (!session?.user?.accessToken) {
        throw new Error("No access token available");
      }
      const client = getGraphQLClientFromToken(session.user.accessToken);
      return client.request<TData>(query, variables as Record<string, unknown>);
    },
    enabled: enabled && !!session?.user?.accessToken,
  });
}

export function useGraphMutation<TData, TVariables>({
  mutation,
  invalidateKeys = [],
}: {
  mutation: string;
  invalidateKeys?: string[];
}) {
  const { data: session } = useSession();
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (variables: TVariables): Promise<TData> => {
      if (!session?.user?.accessToken) {
        throw new Error("No access token available");
      }
      const client = getGraphQLClientFromToken(session.user.accessToken);
      return client.request<TData>(mutation, variables as Record<string, unknown>);
    },
    onSuccess: () => {
      invalidateKeys.forEach((key) => {
        queryClient.invalidateQueries({ queryKey: [key] });
      });
    },
  });
}
