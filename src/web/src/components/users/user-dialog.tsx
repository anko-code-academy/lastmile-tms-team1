'use client';

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { UserForm } from './user-form';
import type { UserDto, CreateUserInput, UpdateUserInput } from '@/types/user';

interface ZoneLookup {
  id: string;
  name: string;
  depotId: string;
}

interface DepotLookup {
  id: string;
  name: string;
}

interface UserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  roles: Array<{ id: string; name: string }>;
  zones: ZoneLookup[];
  depots: DepotLookup[];
  onSubmit: (data: CreateUserInput | UpdateUserInput) => void;
  user?: UserDto;
  isLoading?: boolean;
}

export function UserDialog({
  open,
  onOpenChange,
  roles,
  zones,
  depots,
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
          zones={zones}
          depots={depots}
          onSubmit={onSubmit}
          user={user}
          isLoading={isLoading}
        />
      </DialogContent>
    </Dialog>
  );
}
