import type { UserStatus } from '@/types/user';

interface UserStatusBadgeProps {
  status: UserStatus;
}

export function UserStatusBadge({ status }: UserStatusBadgeProps) {
  const baseClasses = 'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium';

  const statusClasses: Record<UserStatus, string> = {
    ACTIVE: 'bg-emerald-500/10 text-emerald-500',
    INACTIVE: 'bg-muted text-muted-foreground',
    SUSPENDED: 'bg-red-500/10 text-red-500',
  };

  return (
    <span className={`${baseClasses} ${statusClasses[status]}`}>
      {status}
    </span>
  );
}
