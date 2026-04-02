"use client";

import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { fetchUserManagementLookups } from "@/services/users.service";
import { userKeys } from "@/lib/query-key-factory";

interface RoleLookup {
  id: string;
  name: string;
  description: string | null;
}

interface DepotLookup {
  id: string;
  name: string;
}

interface ZoneLookup {
  id: string;
  name: string;
  depotId: string;
}

interface UserManagementLookups {
  roles: RoleLookup[];
  depots: DepotLookup[];
  zones: ZoneLookup[];
}

export function useUserManagementLookups() {
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? "";

  return useQuery({
    queryKey: userKeys.lookups(),
    queryFn: () => fetchUserManagementLookups(token),
    enabled: !!token,
  });
}
