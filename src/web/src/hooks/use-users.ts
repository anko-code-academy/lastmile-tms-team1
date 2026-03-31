"use client";

import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { useSession } from "next-auth/react";
import { userKeys } from "@/lib/query-key-factory";
import { fetchUsers } from "@/services/users.service";
import type { UsersFilter, PageInfo, UserDto } from "@/types/user";

interface UseUsersResult {
  users: UserDto[];
  pageInfo: PageInfo;
  totalCount: number;
  isLoading: boolean;
  gotoPage: (cursor: string | null) => void;
  gotoNextPage: () => void;
  gotoPrevPage: () => void;
}

export function useUsers(filters?: UsersFilter, pageSize = 10): UseUsersResult {
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? "";

  const [afterCursor, setAfterCursor] = useState<string | undefined>(undefined);

  const { data, isLoading } = useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => fetchUsers(token, filters, { first: pageSize, after: afterCursor }),
    enabled: !!token,
  });

  return {
    users: data?.users ?? [],
    pageInfo: data?.pageInfo ?? {
      hasNextPage: false,
      hasPreviousPage: false,
      startCursor: null,
      endCursor: null,
    },
    totalCount: data?.totalCount ?? 0,
    isLoading,
    gotoPage: (cursor) => setAfterCursor(cursor ?? undefined),
    gotoNextPage: () =>
      data?.pageInfo.hasNextPage &&
      setAfterCursor(data.pageInfo.endCursor ?? undefined),
    gotoPrevPage: () => setAfterCursor(undefined),
  };
}
