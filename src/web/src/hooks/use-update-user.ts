import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useSession } from 'next-auth/react';
import { updateUser } from '@/lib/graphql/mutations';
import type { UpdateUserInput, UserDto } from '@/types/user';
import { toast } from 'sonner';

export function useUpdateUser() {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? '';

  return useMutation({
    mutationFn: ({ userId, input }: { userId: string; input: UpdateUserInput }) =>
      updateUser(token, userId, input),
    onSuccess: (data) => {
      // Update the query cache directly with the updated user
      queryClient.setQueryData(['users'], (old: { users: UserDto[] } | undefined) => {
        if (!old?.users) return old;
        return {
          users: old.users.map((user) =>
            user.id === data.updateUser.id ? data.updateUser : user
          ),
        };
      });
      toast.success('User updated successfully');
    },
    onError: () => {
      toast.error('Failed to update user');
    },
  });
}
