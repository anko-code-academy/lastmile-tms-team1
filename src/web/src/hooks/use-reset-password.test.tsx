import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useResetPassword } from './use-reset-password';
import * as usersService from '@/services/users.service';
import { toast } from 'sonner';

vi.mock('@/services/users.service');

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

describe('useResetPassword', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('calls resetPassword mutation', async () => {
    vi.mocked(usersService.resetPassword).mockResolvedValue(true);

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('john@example.com');

    await waitFor(() =>
      expect(usersService.resetPassword).toHaveBeenCalledWith('john@example.com')
    );
  });

  it('returns true on success', async () => {
    vi.mocked(usersService.resetPassword).mockResolvedValue(true);

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('john@example.com');

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });

  it('shows success toast even on failure for security', async () => {
    // Reset password always shows success for security reasons
    vi.mocked(usersService.resetPassword).mockResolvedValue(true);

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('john@example.com');

    await waitFor(() =>
      expect(toast.success).toHaveBeenCalledWith(
        'If an account with that email exists, a password reset link has been sent'
      )
    );
  });
});
