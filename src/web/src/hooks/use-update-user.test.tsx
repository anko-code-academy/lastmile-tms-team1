import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useUpdateUser } from './use-update-user';
import * as usersService from '@/services/users.service';
import type { UpdateUserInput, UserDto } from '@/types/user';
import { toast } from 'sonner';

vi.mock('@/services/users.service');

const mockUpdatedUser: UserDto = {
  id: '1',
  firstName: 'John',
  lastName: 'Doe Updated',
  email: 'john@example.com',
  phoneNumber: '1234567890',
  status: 'ACTIVE',
  roleId: 'role-1',
  roleName: 'Admin',
  zoneId: null,
  zoneName: null,
  depotId: null,
  depotName: null,
  createdAt: '2024-01-01T00:00:00Z',
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

  return function Wrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}

describe('useUpdateUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls updateUser mutation', async () => {
    vi.mocked(usersService.updateUser).mockResolvedValue(mockUpdatedUser);

    const { result } = renderHook(() => useUpdateUser(), {
      wrapper: createWrapper(),
    });

    const input: UpdateUserInput = {
      firstName: 'John',
      lastName: 'Doe Updated',
      email: 'john@example.com',
    };

    result.current.mutate({ userId: '1', input });

    await waitFor(() =>
      expect(usersService.updateUser).toHaveBeenCalled()
    );
  });

  it('returns updated user on success', async () => {
    vi.mocked(usersService.updateUser).mockResolvedValue(mockUpdatedUser);

    const { result } = renderHook(() => useUpdateUser(), {
      wrapper: createWrapper(),
    });

    const input: UpdateUserInput = {
      firstName: 'John',
      lastName: 'Doe Updated',
      email: 'john@example.com',
    };

    result.current.mutate({ userId: '1', input });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });

  it('shows toast on success', async () => {
    vi.mocked(usersService.updateUser).mockResolvedValue(mockUpdatedUser);

    const { result } = renderHook(() => useUpdateUser(), {
      wrapper: createWrapper(),
    });

    const input: UpdateUserInput = {
      firstName: 'John',
      lastName: 'Doe Updated',
      email: 'john@example.com',
    };

    result.current.mutate({ userId: '1', input });

    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith('User updated successfully')
    );
  });

  it('shows toast on error', async () => {
    vi.mocked(usersService.updateUser).mockRejectedValue(new Error('API error'));

    const { result } = renderHook(() => useUpdateUser(), {
      wrapper: createWrapper(),
    });

    const input: UpdateUserInput = {
      firstName: 'John',
      lastName: 'Doe Updated',
      email: 'john@example.com',
    };

    result.current.mutate({ userId: '1', input });

    await waitFor(() =>
      expect(toast.error).toHaveBeenCalledWith('Failed to update user')
    );
  });
});
