import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useResetPassword } from './use-reset-password';
import * as mutations from '@/lib/graphql/mutations';
import { toast } from 'sonner';

vi.mock('@/lib/graphql/mutations');

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

  it('calls resetPassword mutation without token', async () => {
    vi.mocked(mutations.resetPassword).mockResolvedValue({
      resetPassword: true,
    });

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('john@example.com');

    await waitFor(() =>
      expect(mutations.resetPassword).toHaveBeenCalledWith('john@example.com')
    );
  });

  it('returns true on success', async () => {
    vi.mocked(mutations.resetPassword).mockResolvedValue({
      resetPassword: true,
    });

    const { result } = renderHook(() => useResetPassword(), {
      wrapper: createWrapper(),
    });

    result.current.mutate('john@example.com');

    await waitFor(() => expect(result.current.data?.resetPassword).toBe(true));
  });

  it('shows success toast even on failure for security', async () => {
    // Reset password always returns true for security reasons
    vi.mocked(mutations.resetPassword).mockResolvedValue({
      resetPassword: true,
    });

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
