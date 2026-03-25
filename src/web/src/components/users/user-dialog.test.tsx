import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserDialog } from './user-dialog';
import type { UserDto } from '@/types/user';

const mockUser: UserDto = {
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

const mockRoles = [
  { id: 'role-1', name: 'Admin' },
  { id: 'role-2', name: 'Dispatcher' },
  { id: 'role-3', name: 'Driver' },
];

const mockOnSubmit = vi.fn();

describe('UserDialog', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('opens dialog when open prop is true', () => {
    render(
      <UserDialog
        open={true}
        onOpenChange={vi.fn()}
        roles={mockRoles}
        onSubmit={mockOnSubmit}
      />
    );

    // Check for the dialog role
    expect(screen.getByRole('dialog')).toBeInTheDocument();
  });

  it('shows Create User title when no user is provided', () => {
    render(
      <UserDialog
        open={true}
        onOpenChange={vi.fn()}
        roles={mockRoles}
        onSubmit={mockOnSubmit}
      />
    );

    // DialogTitle is an h2 element
    expect(screen.getByRole('dialog').querySelector('h2')?.textContent).toBe('Create User');
  });

  it('shows Edit User title when user is provided', () => {
    render(
      <UserDialog
        open={true}
        onOpenChange={vi.fn()}
        roles={mockRoles}
        onSubmit={mockOnSubmit}
        user={mockUser}
      />
    );

    expect(screen.getByRole('dialog').querySelector('h2')?.textContent).toBe('Edit User');
  });

  it('renders UserForm inside dialog', () => {
    render(
      <UserDialog
        open={true}
        onOpenChange={vi.fn()}
        roles={mockRoles}
        onSubmit={mockOnSubmit}
      />
    );

    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument();
  });

  it('calls onOpenChange when dialog is closed', async () => {
    const onOpenChange = vi.fn();

    render(
      <UserDialog
        open={true}
        onOpenChange={onOpenChange}
        roles={mockRoles}
        onSubmit={mockOnSubmit}
      />
    );

    // Find and click the close button
    const closeButton = screen.getByRole('button', { name: /close/i });
    await userEvent.click(closeButton);

    expect(onOpenChange).toHaveBeenCalledWith(false);
  });
});
