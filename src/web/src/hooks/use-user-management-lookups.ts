'use client';

import { useEffect, useState } from 'react';
import { useSession } from 'next-auth/react';
import { fetchUserManagementLookups } from '@/lib/graphql/queries';

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
  const token = session?.user?.accessToken ?? null;
  const [data, setData] = useState<UserManagementLookups | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) {
      setData(null);
      return;
    }

    const currentToken = token;
    let mounted = true;

    async function load() {
      setIsLoading(true);
      setError(null);
      try {
        const result = await fetchUserManagementLookups(currentToken);
        if (mounted) {
          setData(result.userManagementLookups);
        }
      } catch (err) {
        if (mounted) {
          setError(err instanceof Error ? err.message : 'Failed to load lookups');
        }
      } finally {
        if (mounted) {
          setIsLoading(false);
        }
      }
    }

    load();

    return () => {
      mounted = false;
    };
  }, [token]);

  return { data, isLoading, error };
}
