import Link from "next/link";
import { Package, ArrowDownToLine, ArrowRightLeft, Truck, RotateCcw } from "lucide-react";

const operations = [
  {
    href: "/depot/inbound",
    title: "Inbound Receiving",
    description: "Scan incoming parcels against a manifest",
    icon: ArrowDownToLine,
    roles: ["WarehouseOperator", "Admin", "OperationsManager"],
  },
  {
    href: "/depot/sorting",
    title: "Sort & Zone Assignment",
    description: "Scan parcels to sort and assign to zones",
    icon: ArrowRightLeft,
    roles: ["WarehouseOperator", "Admin", "OperationsManager"],
  },
  {
    href: "/depot/staging",
    title: "Staging for Route Load-Out",
    description: "Move sorted parcels to staging area for a route",
    icon: Package,
    roles: ["WarehouseOperator", "Admin", "OperationsManager"],
  },
  {
    href: "/depot/loadout",
    title: "Route Load-Out",
    description: "Scan parcels as they are loaded onto the vehicle",
    icon: Truck,
    roles: ["WarehouseOperator", "Admin", "OperationsManager"],
  },
  {
    href: "/depot/returns",
    title: "Returns Receiving",
    description: "Scan returned parcels back into the depot",
    icon: RotateCcw,
    roles: ["WarehouseOperator", "Admin", "OperationsManager"],
  },
];

export default function DepotPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Depot Operations</h1>
        <p className="text-muted-foreground">Select an operation to begin scanning</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {operations.map((op) => {
          const Icon = op.icon;
          return (
            <Link
              key={op.href}
              href={op.href}
              className="rounded-lg border bg-card p-6 hover:border-primary/50 hover:shadow-sm transition-all"
            >
              <div className="flex items-center gap-3 mb-2">
                <Icon className="h-5 w-5 text-primary" />
                <h3 className="font-semibold">{op.title}</h3>
              </div>
              <p className="text-sm text-muted-foreground">{op.description}</p>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
