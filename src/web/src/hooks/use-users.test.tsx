import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useUsers } from './use-users';
import * as queries from '@/lib/graphql/queries';
import type { UserDto } from '@/types/user';

vi.mock('@/lib/graphql/queries');

const mockUsers: UserDto[] = [
  {
    id: '1',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@example.com',
    phoneNumber: '1234567890',
    status: 'ACTIVE',
    roleName: 'Admin',
    roleId: 'role-1',
    zoneId: null,
    depotId: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    firstName: 'Jane',
    lastName: 'Smith',
    email: 'jane@example.com',
    phoneNumber: '0987654321',
    status: 'INACTIVE',
    roleName: 'Dispatcher',
    roleId: 'role-2',
    zoneId: null,
    depotId: null,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

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

describe('useUsers', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('returns users on success', async () => {
    vi.mocked(queries.fetchUsers).mockResolvedValue({ users: mockUsers });

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.users).toEqual(mockUsers);
    expect(queries.fetchUsers).toHaveBeenCalledWith('test-token', undefined);
  });

  it('returns loading state', async () => {
    vi.mocked(queries.fetchUsers).mockResolvedValue({ users: mockUsers });

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
  });

  it('returns error on failure', async () => {
    vi.mocked(queries.fetchUsers).mockRejectedValue(new Error('API error'));

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));

    expect(result.current.error).toBeDefined();
  });

  it('applies filters when provided', async () => {
    vi.mocked(queries.fetchUsers).mockResolvedValue({ users: mockUsers });
    const filters = {
      where: { search: 'John', status: 'ACTIVE' as const },
      order: { field: 'firstName', direction: 'ASC' as const },
    };

    renderHook(() => useUsers(filters), {
      wrapper: createWrapper(),
    });

    await waitFor(() =>
      expect(queries.fetchUsers).toHaveBeenCalledWith('test-token', filters)
    );
  });
});
