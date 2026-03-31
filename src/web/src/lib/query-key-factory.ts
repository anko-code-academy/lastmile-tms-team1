/* eslint-disable @typescript-eslint/no-explicit-any */
export const userKeys = {
  all: ['users'] as const,
  lists: () => [...userKeys.all, 'list'] as const,
  list: (filters?: any) => [...userKeys.lists(), filters] as const,
  lookups: () => [...userKeys.all, 'lookups'] as const,
  details: () => [...userKeys.all, 'detail'] as const,
  detail: (id: string) => [...userKeys.details(), id] as const,
};

export const vehicleKeys = {
  all: ['vehicles'] as const,
  lists: () => [...vehicleKeys.all, 'list'] as const,
  list: (filters?: any) => [...vehicleKeys.lists(), filters] as const,
  details: () => [...vehicleKeys.all, 'detail'] as const,
  detail: (id: string) => [...vehicleKeys.details(), id] as const,
  history: (id: string) => [...vehicleKeys.detail(id), 'history'] as const,
};

export const routeKeys = {
  all: ['routes'] as const,
  lists: () => [...routeKeys.all, 'list'] as const,
  list: (filters?: any) => [...routeKeys.lists(), filters] as const,
  details: () => [...routeKeys.all, 'detail'] as const,
  detail: (id: string) => [...routeKeys.details(), id] as const,
};

export const depotKeys = {
  all: ['depots'] as const,
  lists: () => [...depotKeys.all, 'list'] as const,
  list: () => [...depotKeys.lists()] as const,
  details: () => [...depotKeys.all, 'detail'] as const,
  detail: (id: string) => [...depotKeys.details(), id] as const,
};

export const zoneKeys = {
  all: ['zones'] as const,
  lists: () => [...zoneKeys.all, 'list'] as const,
  list: () => [...zoneKeys.lists()] as const,
  details: () => [...zoneKeys.all, 'detail'] as const,
  detail: (id: string) => [...zoneKeys.details(), id] as const,
};
