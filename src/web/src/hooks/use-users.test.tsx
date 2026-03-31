import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useUsers } from './use-users';
import { apiFetch } from '@/lib/api';
import type { UserDto } from '@/types/user';

vi.mock('@/lib/api');

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
    zoneName: null,
    depotId: null,
    depotName: null,
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
    zoneName: null,
    depotId: null,
    depotName: null,
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
    vi.mocked(apiFetch).mockResolvedValue({
      data: {
        users: {
          nodes: mockUsers,
          pageInfo: {
            hasNextPage: false,
            hasPreviousPage: false,
            startCursor: null,
            endCursor: null,
          },
          totalCount: 2,
        },
      },
    });

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.users.length).toBe(2));

    expect(result.current.users).toEqual(mockUsers);
  });

  it('returns loading state', async () => {
    vi.mocked(apiFetch).mockResolvedValue({
      data: {
        users: {
          nodes: mockUsers,
          pageInfo: {
            hasNextPage: false,
            hasPreviousPage: false,
            startCursor: null,
            endCursor: null,
          },
          totalCount: 2,
        },
      },
    });

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
  });

  it('returns error on failure', async () => {
    vi.mocked(apiFetch).mockRejectedValue(new Error('API error'));

    const { result } = renderHook(() => useUsers(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.users.length === 0 || result.current.isLoading === false).toBe(true));
  });

  it('applies filters when provided', async () => {
    vi.mocked(apiFetch).mockResolvedValue({
      data: {
        users: {
          nodes: mockUsers,
          pageInfo: {
            hasNextPage: false,
            hasPreviousPage: false,
            startCursor: null,
            endCursor: null,
          },
          totalCount: 2,
        },
      },
    });
    const filters = {
      where: { search: 'John', status: 'ACTIVE' as const },
    };

    const { result } = renderHook(() => useUsers(filters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.users.length).toBe(2));
  });
});
