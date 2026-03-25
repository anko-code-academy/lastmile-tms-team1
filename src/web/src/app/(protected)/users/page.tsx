'use client';

import { useState } from 'react';
import { useSession } from 'next-auth/react';
import { useUsers } from '@/hooks/use-users';
import { useCreateUser } from '@/hooks/use-create-user';
import { useUpdateUser } from '@/hooks/use-update-user';
import { useDeactivateUser } from '@/hooks/use-deactivate-user';
import { useUserManagementLookups } from '@/hooks/use-user-management-lookups';
import { UserList } from '@/components/users/user-list';
import { UserDialog } from '@/components/users/user-dialog';
import { Button } from '@/components/ui/button';
import { Plus } from 'lucide-react';
import type { UserDto, CreateUserInput, UpdateUserInput } from '@/types/user';

export default function UsersPage() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserDto | undefined>();

  const { data: session } = useSession();
  const token = session?.user?.accessToken ?? null;
  const { data, isLoading: isLoadingUsers } = useUsers();
  const { data: lookups, isLoading: isLoadingLookups } = useUserManagementLookups();
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();
  const deactivateUser = useDeactivateUser();

  const users = data?.users ?? [];
  const isLoading = isLoadingUsers || isLoadingLookups;

  const roles = lookups?.roles ?? [];

  const handleOpenCreate = () => {
    setSelectedUser(undefined);
    setDialogOpen(true);
  };

  const handleOpenEdit = (user: UserDto) => {
    // Get fresh user data from the users list (which is updated by React Query cache)
    const freshUser = users.find((u) => u.id === user.id);
    setSelectedUser(freshUser || user);
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedUser(undefined);
  };

  const handleSubmit = (data: CreateUserInput | UpdateUserInput) => {
    if (selectedUser) {
      updateUser.mutate(
        { userId: selectedUser.id, input: data as UpdateUserInput },
        { onSuccess: handleCloseDialog }
      );
    } else {
      createUser.mutate(data as CreateUserInput, { onSuccess: handleCloseDialog });
    }
  };

  const handleDeactivate = (userId: string) => {
    deactivateUser.mutate(userId);
  };

  return (
    <div className="container mx-auto py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold">Users</h1>
        <Button onClick={handleOpenCreate}>
          <Plus className="mr-2 h-4 w-4" />
          Create User
        </Button>
      </div>

      <UserList
        users={users}
        isLoading={isLoading}
        onEdit={handleOpenEdit}
        onDeactivate={handleDeactivate}
      />

      <UserDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        roles={roles}
        onSubmit={handleSubmit}
        user={selectedUser}
        isLoading={createUser.isPending || updateUser.isPending}
      />
    </div>
  );
}
