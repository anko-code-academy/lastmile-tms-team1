import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCreateUser } from './use-create-user';
import * as usersService from '@/services/users.service';
import type { UserDto } from '@/types/user';
import { toast } from 'sonner';

vi.mock('@/services/users.service');

const mockCreatedUser: UserDto = {
  id: '1',
  firstName: 'John',
  lastName: 'Doe',
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

describe('useCreateUser', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls createUser mutation', async () => {
    vi.mocked(usersService.createUser).mockResolvedValue(mockCreatedUser);

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      phone: '1234567890',
      roleId: 'role-1',
      password: 'Password123!',
    };

    result.current.mutate(input);

    await waitFor(() =>
      expect(usersService.createUser).toHaveBeenCalled()
    );
  });

  it('returns created user on success', async () => {
    vi.mocked(usersService.createUser).mockResolvedValue(mockCreatedUser);

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      roleId: 'role-1',
      password: 'Password123!',
    };

    result.current.mutate(input);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });

  it('shows toast on success', async () => {
    vi.mocked(usersService.createUser).mockResolvedValue(mockCreatedUser);

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      roleId: 'role-1',
      password: 'Password123!',
    };

    result.current.mutate(input);

    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith('User created successfully')
    );
  });

  it('shows toast on error', async () => {
    vi.mocked(usersService.createUser).mockRejectedValue(new Error('API error'));

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      roleId: 'role-1',
      password: 'Password123!',
    };

    result.current.mutate(input);

    await waitFor(() =>
      expect(toast.error).toHaveBeenCalledWith('Failed to create user')
    );
  });
});
