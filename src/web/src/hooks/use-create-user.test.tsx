import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCreateUser } from './use-create-user';
import * as mutations from '@/lib/graphql/mutations';
import type { CreateUserInput, UserDto } from '@/types/user';
import { toast } from 'sonner';

vi.mock('@/lib/graphql/mutations');

const mockCreatedUser: UserDto = {
  id: '1',
  firstName: 'John',
  lastName: 'Doe',
  email: 'john@example.com',
  phoneNumber: '1234567890',
  status: 'Active',
  roleName: 'Admin',
  roleId: 'role-1',
  zoneId: null,
  depotId: null,
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
    vi.mocked(mutations.createUser).mockResolvedValue({
      createUser: mockCreatedUser,
    });

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input: CreateUserInput = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      phone: '1234567890',
      roleId: 'role-1',
      password: 'Password123!',
    };

    result.current.mutate(input);

    await waitFor(() =>
      expect(mutations.createUser).toHaveBeenCalledWith('test-token', input)
    );
  });

  it('returns created user on success', async () => {
    vi.mocked(mutations.createUser).mockResolvedValue({
      createUser: mockCreatedUser,
    });

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input: CreateUserInput = {
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      roleId: 'role-1',
      password: 'Password123!',
    };

    result.current.mutate(input);

    await waitFor(() =>
      expect(result.current.data?.createUser).toEqual(mockCreatedUser)
    );
  });

  it('shows toast on success', async () => {
    vi.mocked(mutations.createUser).mockResolvedValue({
      createUser: mockCreatedUser,
    });

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input: CreateUserInput = {
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
    vi.mocked(mutations.createUser).mockRejectedValue(new Error('API error'));

    const { result } = renderHook(() => useCreateUser(), {
      wrapper: createWrapper(),
    });

    const input: CreateUserInput = {
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
