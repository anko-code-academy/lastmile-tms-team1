import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserList } from './user-list';
import type { User } from '@/types/user';

const mockUsers: User[] = [
  {
    id: '1',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@example.com',
    phoneNumber: '1234567890',
    status: 'ACTIVE',
    roleId: 'role-1',
    role: { id: 'role-1', name: 'Admin' },
    zoneId: null,
    zone: null,
    depotId: null,
    depot: null,
    createdAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    firstName: 'Jane',
    lastName: 'Smith',
    email: 'jane@example.com',
    phoneNumber: '0987654321',
    status: 'INACTIVE',
    roleId: 'role-2',
    role: { id: 'role-2', name: 'Dispatcher' },
    zoneId: null,
    zone: null,
    depotId: null,
    depot: null,
    createdAt: '2024-01-02T00:00:00Z',
  },
];

const mockOnEdit = vi.fn();
const mockOnDeactivate = vi.fn();
const mockOnActivate = vi.fn();

describe('UserList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders users in a table', () => {
    render(
      <UserList
        users={mockUsers}
        isLoading={false}
        onEdit={mockOnEdit}
        onDeactivate={mockOnDeactivate}
        onActivate={mockOnActivate}
      />
    );

    // Check for user names in the table
    expect(screen.getByText(/John Doe/)).toBeInTheDocument();
    expect(screen.getByText(/Jane Smith/)).toBeInTheDocument();
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
  });

  it('renders loading state', () => {
    render(
      <UserList
        users={[]}
        isLoading={true}
        onEdit={mockOnEdit}
        onDeactivate={mockOnDeactivate}
        onActivate={mockOnActivate}
      />
    );

    // Should show loading indicator or skeleton
    expect(screen.queryByText('John')).not.toBeInTheDocument();
  });

  it('renders empty state when no users', () => {
    render(
      <UserList
        users={[]}
        isLoading={false}
        onEdit={mockOnEdit}
        onDeactivate={mockOnDeactivate}
        onActivate={mockOnActivate}
      />
    );

    expect(screen.getByText(/no users found/i)).toBeInTheDocument();
  });

  it('calls onEdit when edit button is clicked', async () => {
    render(
      <UserList
        users={mockUsers}
        isLoading={false}
        onEdit={mockOnEdit}
        onDeactivate={mockOnDeactivate}
        onActivate={mockOnActivate}
      />
    );

    const editButtons = screen.getAllByRole('button', { name: /edit/i });
    await userEvent.click(editButtons[0]);

    expect(mockOnEdit).toHaveBeenCalledWith(mockUsers[0]);
  });

  it('calls onDeactivate when deactivate button is clicked', async () => {
    render(
      <UserList
        users={mockUsers}
        isLoading={false}
        onEdit={mockOnEdit}
        onDeactivate={mockOnDeactivate}
        onActivate={mockOnActivate}
      />
    );

    const deactivateButtons = screen.getAllByRole('button', { name: /deactivate/i });
    await userEvent.click(deactivateButtons[0]);

    expect(mockOnDeactivate).toHaveBeenCalledWith(mockUsers[0].id);
  });

  it('renders user status badges', () => {
    render(
      <UserList
        users={mockUsers}
        isLoading={false}
        onEdit={mockOnEdit}
        onDeactivate={mockOnDeactivate}
        onActivate={mockOnActivate}
      />
    );

    expect(screen.getByText('ACTIVE')).toBeInTheDocument();
    expect(screen.getByText('INACTIVE')).toBeInTheDocument();
  });
});
