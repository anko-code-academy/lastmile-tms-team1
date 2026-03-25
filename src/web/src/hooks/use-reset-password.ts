import { useMutation } from '@tanstack/react-query';
import { resetPassword } from '@/lib/graphql/mutations';
import { toast } from 'sonner';

export function useResetPassword() {
  return useMutation({
    mutationFn: (email: string) => resetPassword(email),
    onSuccess: () => {
      // Always show success for security (don't reveal if user exists)
      toast.success(
        'If an account with that email exists, a password reset link has been sent'
      );
    },
  });
}
