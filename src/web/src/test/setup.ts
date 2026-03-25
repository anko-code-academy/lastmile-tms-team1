import { vi } from 'vitest';
import * as userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';

// Mock next-auth/react
vi.mock('next-auth/react', () => ({
  useSession: vi.fn(() => ({
    data: { user: { accessToken: 'test-token' } },
    status: 'authenticated',
  })),
  signOut: vi.fn(),
}));

// Mock sonner toast
vi.mock('sonner', () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
    promise: vi.fn(),
  },
}));

// Re-export userEvent for convenience
export { userEvent };
