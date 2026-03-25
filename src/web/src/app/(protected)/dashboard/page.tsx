import { auth } from "@/auth";
import Link from "next/link";
import { Truck, Route } from "lucide-react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default async function DashboardPage() {
  const session = await auth();

  return (
    <div className="p-6">
      <div className="mb-8">
        <h1 className="text-3xl font-bold">Welcome, {session?.user?.name}</h1>
        <p className="text-muted-foreground">
          Transportation Management System
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4 mb-8">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Vehicles</CardTitle>
            <Truck className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">-</div>
            <p className="text-xs text-muted-foreground">Fleet vehicles</p>
            <Button variant="link" className="p-0 h-auto mt-2" asChild>
              <Link href="/vehicles">View vehicles</Link>
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium">Routes</CardTitle>
            <Route className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">-</div>
            <p className="text-xs text-muted-foreground">Delivery routes</p>
            <Button variant="link" className="p-0 h-auto mt-2" asChild>
              <Link href="/routes">View routes</Link>
            </Button>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Getting Started</CardTitle>
          <CardDescription>
            Use the navigation menu on the left to access different sections of the system.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2 text-sm">
            <li>
              <Link href="/vehicles" className="text-primary hover:underline">
                Manage Vehicles
              </Link>{" "}
              - Add, edit, or remove vehicles from your fleet
            </li>
            <li>
              <Link href="/routes" className="text-primary hover:underline">
                Manage Routes
              </Link>{" "}
              - Create and manage delivery routes
            </li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
