'use client';

import { useMemo } from 'react';
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
  zoneId: z.string().optional(),
  depotId: z.string().optional(),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

const updateUserSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().min(1, 'Email is required').email('Invalid email'),
  phone: z.string().optional(),
  roleId: z.string().optional(),
  zoneId: z.string().optional(),
  depotId: z.string().optional(),
});

type CreateUserFormData = z.infer<typeof createUserSchema>;
type UpdateUserFormData = z.infer<typeof updateUserSchema>;

interface ZoneLookup {
  id: string;
  name: string;
  depotId: string;
}

interface DepotLookup {
  id: string;
  name: string;
}

interface UserFormProps {
  roles: Array<{ id: string; name: string }>;
  zones: ZoneLookup[];
  depots: DepotLookup[];
  onSubmit: (data: CreateUserInput | UpdateUserInput) => void;
  user?: UserDto;
  isLoading?: boolean;
}

export function UserForm({ roles, zones, depots, onSubmit, user, isLoading }: UserFormProps) {
  const isEditMode = !!user;

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    resetField,
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
          zoneId: user.zoneId ?? '',
          depotId: user.depotId ?? '',
        }
      : undefined,
  });

  const selectedDepotId = watch('depotId');
  const selectedZoneId = watch('zoneId');

  // Zones filtered by selected depot (if any)
  const filteredZones = useMemo(() => {
    if (!selectedDepotId) return zones;
    return zones.filter((z) => z.depotId === selectedDepotId);
  }, [zones, selectedDepotId]);

  const handleDepotChange = (depotId: string) => {
    setValue('depotId', depotId || undefined);
    resetField('zoneId');
  };

  const handleZoneChange = (zoneId: string) => {
    setValue('zoneId', zoneId || undefined);
    resetField('depotId');
  };

  const onFormSubmit = (data: CreateUserFormData | UpdateUserFormData) => {
    const zoneId = data.zoneId || undefined;
    const depotId = data.depotId || undefined;

    if (isEditMode) {
      const { email, ...rest } = data as UpdateUserFormData & { email?: string };
      onSubmit({
        ...rest,
        email: email as string,
        zoneId,
        depotId,
      } as UpdateUserInput);
    } else {
      onSubmit({
        ...(data as CreateUserFormData),
        zoneId,
        depotId,
      } as CreateUserInput);
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

      <div className="space-y-2">
        <Label htmlFor="depotId">Depot</Label>
        <select
          id="depotId"
          value={selectedDepotId ?? ''}
          onChange={(e) => handleDepotChange(e.target.value)}
          className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value="">No depot</option>
          {depots.map((depot) => (
            <option key={depot.id} value={depot.id}>
              {depot.name}
            </option>
          ))}
        </select>
        <p className="text-xs text-muted-foreground">
          Warehouse operators are assigned to a depot
        </p>
      </div>

      <div className="space-y-2">
        <Label htmlFor="zoneId">Zone</Label>
        <select
          id="zoneId"
          value={selectedZoneId ?? ''}
          onChange={(e) => handleZoneChange(e.target.value)}
          disabled={!selectedDepotId && zones.length > 0}
          className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium file:text-foreground placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option value="">No zone</option>
          {filteredZones.map((zone) => (
            <option key={zone.id} value={zone.id}>
              {zone.name}
            </option>
          ))}
        </select>
        <p className="text-xs text-muted-foreground">
          Drivers and dispatchers are assigned to a zone within a depot
        </p>
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
