'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import type { UserDto, CreateUserInput, UpdateUserInput } from '@/types/user';

const createUserSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email'),
  phone: z.string().optional(),
  roleId: z.string().min(1, 'Role is required'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

const updateUserSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().email('Invalid email').optional(),
  phone: z.string().optional(),
  roleId: z.string().optional(),
});

type CreateUserFormData = z.infer<typeof createUserSchema>;
type UpdateUserFormData = z.infer<typeof updateUserSchema>;

interface UserFormProps {
  roles: Array<{ id: string; name: string }>;
  onSubmit: (data: CreateUserInput | UpdateUserInput) => void;
  user?: UserDto;
  isLoading?: boolean;
}

export function UserForm({ roles, onSubmit, user, isLoading }: UserFormProps) {
  const isEditMode = !!user;

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateUserFormData | UpdateUserFormData>({
    resolver: zodResolver(isEditMode ? updateUserSchema : createUserSchema),
    defaultValues: user
      ? {
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          phone: user.phoneNumber ?? '',
          roleId: user.roleId ?? '',
        }
      : undefined,
  });

  const onFormSubmit = (data: CreateUserFormData | UpdateUserFormData) => {
    if (isEditMode) {
      const { email, ...rest } = data as UpdateUserFormData & { email?: string };
      onSubmit({
        ...rest,
        ...(email !== undefined && { phone: data.phone }),
      } as UpdateUserInput);
    } else {
      onSubmit(data as CreateUserInput);
    }
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="firstName">First Name</Label>
          <Input
            id="firstName"
            {...register('firstName')}
            placeholder="John"
          />
          {errors.firstName && (
            <p className="text-sm text-red-500">{errors.firstName.message}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="lastName">Last Name</Label>
          <Input
            id="lastName"
            {...register('lastName')}
            placeholder="Doe"
          />
          {errors.lastName && (
            <p className="text-sm text-red-500">{errors.lastName.message}</p>
          )}
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="email">Email</Label>
        <Input
          id="email"
          type="email"
          {...register('email')}
          placeholder="john@example.com"
        />
        {errors.email && (
          <p className="text-sm text-red-500">{errors.email.message}</p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="phone">Phone</Label>
        <Input
          id="phone"
          type="tel"
          {...register('phone')}
          placeholder="1234567890"
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="roleId">Role</Label>
        <select
          id="roleId"
          {...register('roleId')}
          className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value="">Select a role</option>
          {roles.map((role) => (
            <option key={role.id} value={role.id}>
              {role.name}
            </option>
          ))}
        </select>
        {errors.roleId && (
          <p className="text-sm text-red-500">{errors.roleId.message}</p>
        )}
      </div>

      {!isEditMode && (
        <div className="space-y-2">
          <Label htmlFor="password">Password</Label>
          <Input
            id="password"
            type="password"
            {...register('password')}
            placeholder="••••••••"
          />
          {(errors as typeof errors & { password?: { message: string } }).password && (
            <p className="text-sm text-red-500">{(errors as typeof errors & { password?: { message: string } }).password?.message}</p>
          )}
        </div>
      )}

      <Button type="submit" disabled={isLoading}>
        {isLoading ? 'Submitting...' : isEditMode ? 'Update User' : 'Create User'}
      </Button>
    </form>
  );
}
