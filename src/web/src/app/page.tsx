import { auth } from "@/auth";
import { redirect } from "next/navigation";
import Link from "next/link";

export default async function Home() {
  const session = await auth();

  if (session?.user) {
    redirect("/dashboard");
  }

  return (
    <>
      <nav className="border-b bg-background">
        <div className="flex h-16 items-center px-4">
          <div className="font-semibold text-lg">Last Mile TMS</div>
        </div>
      </nav>
      <main className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <h1 className="text-4xl font-bold mb-4">Last Mile TMS</h1>
          <p className="text-muted-foreground mb-8">
            Transportation Management System
          </p>
          <Link
            href="/login"
            className="inline-flex items-center justify-center rounded-md bg-primary px-6 py-3 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            Sign In
          </Link>
        </div>
      </main>
    </>
  );
}
