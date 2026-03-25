import { auth, signOut } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Session } from "next-auth";

async function Navbar({ session }: { session: Session | null }) {
  const isAdmin = session?.user?.name === "admin";

  return (
    <nav className="border-b bg-background">
      <div className="flex h-16 items-center px-4 gap-4">
        <div className="flex items-center gap-6">
          <Link href="/dashboard" className="font-semibold text-lg">
            Last Mile TMS
          </Link>
          <div className="flex gap-4">
            <Link
              href="/dashboard"
              className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
            >
              Dashboard
            </Link>
            {isAdmin && (
              <Link
                href="/users"
                className="text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
              >
                Users
              </Link>
            )}
          </div>
        </div>
        <div className="ml-auto flex items-center gap-4">
          <span className="text-sm text-muted-foreground">
            {session?.user?.name}
          </span>
          <form
            action={async () => {
              "use server";
              await signOut({ redirectTo: "/login" });
            }}
          >
            <Button variant="outline" size="sm" type="submit">
              Sign Out
            </Button>
          </form>
        </div>
      </div>
    </nav>
  );
}

export default async function ProtectedLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const session = await auth();

  if (!session?.user) {
    redirect("/login");
  }

  return (
    <>
      <Navbar session={session} />
      <main>{children}</main>
    </>
  );
}
