import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useSession } from 'next-auth/react';
import { deactivateUser } from '@/lib/graphql/mutations';
import { toast } from 'sonner';

export function useDeactivateUser() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? '';

  return useMutation({
    mutationFn: (userId: string) => deactivateUser(token, userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('User deactivated successfully');
    },
    onError: () => {
      toast.error('Failed to deactivate user');
    },
  });
}
