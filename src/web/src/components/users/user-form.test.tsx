import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { UserForm } from './user-form';
import type { User } from '@/types/user';

const mockUser: User = {
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
};

const mockZones = [
  { id: 'zone-1', name: 'Zone A', depotId: 'depot-1' },
  { id: 'zone-2', name: 'Zone B', depotId: 'depot-1' },
];

const mockDepots = [
  { id: 'depot-1', name: 'Depot Alpha' },
  { id: 'depot-2', name: 'Depot Beta' },
];

const mockRoles = [
  { id: 'role-1', name: 'Admin' },
  { id: 'role-2', name: 'Dispatcher' },
  { id: 'role-3', name: 'Driver' },
];

const mockOnSubmit = vi.fn();

describe('UserForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('create mode', () => {
    it('renders all required fields', () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} />
      );

      expect(screen.getByLabelText(/first name/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/last name/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/phone/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/role/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/^password$/i)).toBeInTheDocument();
    });

    it('validates firstName is required', async () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} />
      );

      await userEvent.click(screen.getByRole('button', { name: /create user/i }));

      expect(screen.getByText(/first name is required/i)).toBeInTheDocument();
    });

    it('validates lastName is required', async () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} />
      );

      await userEvent.click(screen.getByRole('button', { name: /create user/i }));

      expect(screen.getByText(/last name is required/i)).toBeInTheDocument();
    });

    it('submits form with valid data', async () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} />
      );

      await userEvent.type(screen.getByLabelText(/first name/i), 'John');
      await userEvent.type(screen.getByLabelText(/last name/i), 'Doe');
      await userEvent.type(screen.getByLabelText(/email/i), 'john@example.com');
      await userEvent.type(screen.getByLabelText(/phone/i), '1234567890');
      await userEvent.selectOptions(screen.getByLabelText(/role/i), 'role-1');
      await userEvent.type(screen.getByLabelText(/^password$/i), 'Password123!');

      await userEvent.click(screen.getByRole('button', { name: /create user/i }));

      expect(mockOnSubmit).toHaveBeenCalledWith({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        phone: '1234567890',
        roleId: 'role-1',
        password: 'Password123!',
      });
    });
  });

  describe('edit mode', () => {
    it('hides password field in edit mode', () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} user={mockUser} />
      );

      expect(screen.queryByLabelText(/^password$/i)).not.toBeInTheDocument();
    });

    it('pre-fills form with user data', () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} user={mockUser} />
      );

      expect(screen.getByLabelText(/first name/i)).toHaveValue('John');
      expect(screen.getByLabelText(/last name/i)).toHaveValue('Doe');
      expect(screen.getByLabelText(/email/i)).toHaveValue('john@example.com');
      expect(screen.getByLabelText(/phone/i)).toHaveValue('1234567890');
    });

    it('submits updated user data', async () => {
      render(
        <UserForm roles={mockRoles} zones={mockZones} depots={mockDepots} onSubmit={mockOnSubmit} user={mockUser} />
      );

      await userEvent.clear(screen.getByLabelText(/first name/i));
      await userEvent.type(screen.getByLabelText(/first name/i), 'Jane');

      await userEvent.click(screen.getByRole('button', { name: /update user/i }));

      expect(mockOnSubmit).toHaveBeenCalled();
    });
  });
});
