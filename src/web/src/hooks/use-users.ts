import { useQuery } from '@tanstack/react-query';
import { useSession } from 'next-auth/react';
import { fetchUsers } from '@/lib/graphql/queries';
import type { UsersFilter } from '@/types/user';

export function useUsers(filters?: UsersFilter) {
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? '';

  return useQuery({
    queryKey: ['users', filters, token],
    queryFn: () => fetchUsers(token, filters),
    enabled: !!token,
  });
}
