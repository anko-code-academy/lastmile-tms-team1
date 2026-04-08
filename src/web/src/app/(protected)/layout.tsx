"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Truck,
  LayoutDashboard,
  LogOut,
  Route,
  Building2,
  MapPin,
  Users,
  Package,
  CircleUser,
} from "lucide-react";
import { signOut, useSession } from "next-auth/react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

const navItems = [
  { href: "/dashboard", label: "Dashboard", icon: LayoutDashboard },
  { href: "/vehicles", label: "Vehicles", icon: Truck },
  { href: "/routes", label: "Routes", icon: Route },
  { href: "/depots", label: "Depots", icon: Building2 },
  { href: "/zones", label: "Zones", icon: MapPin },
  { href: "/parcels", label: "Parcels", icon: Package },
  { href: "/users", label: "Users", icon: Users, roles: ["Admin"] },
  { href: "/drivers", label: "Drivers", icon: CircleUser },
];

export default function ProtectedLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const { data: session } = useSession();

  return (
    <div className="flex min-h-screen">
      {/* Sidebar */}
      <aside className="relative w-64 border-r bg-card flex flex-col">
        <div className="flex h-16 items-center border-b px-6">
          <Link href="/dashboard" className="flex items-center gap-2">
            <span className="text-xl font-bold">Last Mile TMS</span>
          </Link>
        </div>

        <nav className="p-4 space-y-1">
          {navItems.map((item) => {
            // Skip items if user lacks required roles
            if (item.roles && !item.roles.some((role) => session?.user?.roles?.includes(role))) {
              return null;
            }

            const Icon = item.icon;
            const isActive = pathname === item.href;

            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                )}
              >
                <Icon className="h-4 w-4" />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="mt-auto p-4 border-t">
          <form
            action={async () => {
              await signOut({ redirectTo: "/login" });
            }}
          >
            <Button
              variant="ghost"
              className="w-full justify-start gap-3"
              type="submit"
            >
              <LogOut className="h-4 w-4" />
              Sign Out
            </Button>
          </form>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-auto">{children}</main>
    </div>
  );
}
