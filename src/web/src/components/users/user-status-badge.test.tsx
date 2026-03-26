import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { UserStatusBadge } from './user-status-badge';

describe('UserStatusBadge', () => {
  it('renders Active status with green styling', () => {
    render(<UserStatusBadge status="ACTIVE" />);

    const badge = screen.getByText('Active');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-emerald-500/10');
    expect(badge.className).toContain('text-emerald-500');
  });

  it('renders Inactive status with gray styling', () => {
    render(<UserStatusBadge status="INACTIVE" />);

    const badge = screen.getByText('Inactive');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-muted');
    expect(badge.className).toContain('text-muted-foreground');
  });

  it('renders Suspended status with red styling', () => {
    render(<UserStatusBadge status="SUSPENDED" />);

    const badge = screen.getByText('Suspended');
    expect(badge).toBeInTheDocument();
    expect(badge.className).toContain('bg-red-500/10');
    expect(badge.className).toContain('text-red-500');
  });
});
