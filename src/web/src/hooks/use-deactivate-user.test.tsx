import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useDeactivateUser } from './use-deactivate-user';
import * as usersService from '@/services/users.service';
import type { UserDto } from '@/types/user';
import { toast } from 'sonner';

vi.mock('@/services/users.service');

const mockDeactivatedUser: UserDto = {
  id: '1',
  firstName: 'John',
  lastName: 'Doe',
  email: 'john@example.com',
  phoneNumber: '1234567890',
  status: 'INACTIVE',
  roleName: 'Admin',
  roleId: 'role-1',
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

describe('useDeactivateUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls deactivateUser mutation', async () => {
    vi.mocked(usersService.deactivateUser).mockResolvedValue(mockDeactivatedUser);

    const { result } = renderHook(() => useDeactivateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() =>
      expect(usersService.deactivateUser).toHaveBeenCalledWith('test-token', '1')
    );
  });

  it('returns deactivated user on success', async () => {
    vi.mocked(usersService.deactivateUser).mockResolvedValue(mockDeactivatedUser);

    const { result } = renderHook(() => useDeactivateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });

  it('shows toast on success', async () => {
    vi.mocked(usersService.deactivateUser).mockResolvedValue(mockDeactivatedUser);

    const { result } = renderHook(() => useDeactivateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith('User deactivated successfully')
    );
  });

  it('shows toast on error', async () => {
    vi.mocked(usersService.deactivateUser).mockRejectedValue(new Error('API error'));

    const { result } = renderHook(() => useDeactivateUser(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('1');

    await waitFor(() =>
      expect(toast.error).toHaveBeenCalledWith('Failed to deactivate user')
    );
  });
});
