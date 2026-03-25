'use client';

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { UserForm } from './user-form';
import type { UserDto, CreateUserInput, UpdateUserInput } from '@/types/user';

interface UserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  roles: Array<{ id: string; name: string }>;
  onSubmit: (data: CreateUserInput | UpdateUserInput) => void;
  user?: UserDto;
  isLoading?: boolean;
}

export function UserDialog({
  open,
  onOpenChange,
  roles,
  onSubmit,
  user,
  isLoading,
}: UserDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>
            {user ? 'Edit User' : 'Create User'}
          </DialogTitle>
        </DialogHeader>
        <UserForm
          roles={roles}
          onSubmit={onSubmit}
          user={user}
          isLoading={isLoading}
        />
      </DialogContent>
    </Dialog>
  );
}
