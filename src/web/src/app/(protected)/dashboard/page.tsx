import { auth, signOut } from "@/auth";
import { Button } from "@/components/ui/button";

export default async function DashboardPage() {
  const session = await auth();

  return (
    <main className="flex min-h-screen items-center justify-center">
      <div className="text-center">
        <h1 className="text-4xl font-bold mb-4">
          Welcome, {session?.user?.name}
        </h1>
        <p className="text-muted-foreground mb-8">
          You are now logged in to Last Mile TMS
        </p>
        <form
          action={async () => {
            "use server";
            await signOut({ redirectTo: "/login" });
          }}
        >
          <Button variant="outline" type="submit">
            Sign Out
          </Button>
        </form>
      </div>
    </main>
  );
}
