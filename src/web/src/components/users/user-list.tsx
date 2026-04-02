'use client';

import { useState } from 'react';
import { UserStatusBadge } from './user-status-badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Search, ChevronLeft, ChevronRight } from 'lucide-react';
import type { User, PageInfo } from '@/types/user';

interface UserListProps {
  users: User[];
  isLoading: boolean;
  onEdit: (user: User) => void;
  onDeactivate: (userId: string) => void;
  onActivate: (userId: string) => void;
  pageInfo?: PageInfo;
  totalCount?: number;
  onNextPage?: () => void;
  onPrevPage?: () => void;
}

export function UserList({
  users,
  isLoading,
  onEdit,
  onDeactivate,
  onActivate,
  pageInfo,
  totalCount,
  onNextPage,
  onPrevPage,
}: UserListProps) {
  const [searchQuery, setSearchQuery] = useState('');

  const filteredUsers = users.filter(
    (user) =>
      user.firstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      user.lastName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      user.email.toLowerCase().includes(searchQuery.toLowerCase())
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
        <p>No users found</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="relative">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder="Search users..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="pl-10 max-w-sm"
        />
      </div>

      <div className="rounded-md border">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="px-4 py-3 text-left text-sm font-medium">Name</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Email</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Phone</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Role</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Zone</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Depot</th>
              <th className="px-4 py-3 text-left text-sm font-medium">Status</th>
              <th className="px-4 py-3 text-right text-sm font-medium">Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredUsers.map((user) => (
              <tr key={user.id} className="border-b">
                <td className="px-4 py-3 text-sm">
                  {user.firstName} {user.lastName}
                </td>
                <td className="px-4 py-3 text-sm">{user.email}</td>
                <td className="px-4 py-3 text-sm">{user.phoneNumber ?? '-'}</td>
                <td className="px-4 py-3 text-sm">{user.role?.name ?? '-'}</td>
                <td className="px-4 py-3 text-sm">{user.zone?.name ?? '-'}</td>
                <td className="px-4 py-3 text-sm">{user.depot?.name ?? '-'}</td>
                <td className="px-4 py-3 text-sm">
                  <UserStatusBadge status={user.status} />
                </td>
                <td className="px-4 py-3 text-right">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => onEdit(user)}
                  >
                    Edit
                  </Button>
                  {user.status === 'ACTIVE' && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onDeactivate(user.id)}
                      className="text-red-500 hover:text-red-600"
                    >
                      Deactivate
                    </Button>
                  )}
                  {user.status === 'INACTIVE' && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => onActivate(user.id)}
                      className="text-green-500 hover:text-green-600"
                    >
                      Activate
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {pageInfo && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Total: {totalCount ?? 0} users
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={onPrevPage}
              disabled={!pageInfo.hasPreviousPage}
            >
              <ChevronLeft className="h-4 w-4 mr-1" />
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={onNextPage}
              disabled={!pageInfo.hasNextPage}
            >
              Next
              <ChevronRight className="h-4 w-4 ml-1" />
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
