import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useSession } from 'next-auth/react';
import { createUser } from '@/lib/graphql/mutations';
import type { CreateUserInput } from '@/types/user';
import { toast } from 'sonner';

export function useCreateUser() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? '';

  return useMutation({
    mutationFn: (input: CreateUserInput) => createUser(token, input),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('User created successfully');
    },
    onError: () => {
      toast.error('Failed to create user');
    },
  });
}
