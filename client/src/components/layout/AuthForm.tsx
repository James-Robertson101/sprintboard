import { Link } from "react-router-dom";
import Button from "../Button";
import type { AuthFormProps } from "../../types/AuthFormProps";

function AuthForm({ mode }: AuthFormProps) {
  const isRegister = mode === "register";

  return (
    <form className="flex w-full max-w-md flex-col gap-4 rounded-xl bg-surface p-8 shadow-sm">
      {isRegister && (
        <div>
          <label
            htmlFor="name"
            className="mb-1 block text-sm font-medium text-text"
          >
            Name
          </label>

          <input
            type="text"
            id="name"
            name="name"
            autoComplete="name"
            required
            className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
          />
        </div>
      )}

      <div>
        <label
          htmlFor="email"
          className="mb-1 block text-sm font-medium text-text"
        >
          Email
        </label>

        <input
          type="email"
          id="email"
          name="email"
          autoComplete="email"
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      </div>

      <div>
        <label
          htmlFor="password"
          className="mb-1 block text-sm font-medium text-text"
        >
          Password
        </label>

        <input
          type="password"
          id="password"
          name="password"
          autoComplete={isRegister ? "new-password" : "current-password"}
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      </div>

      {isRegister && (
        <div>
          <label
            htmlFor="confirmPassword"
            className="mb-1 block text-sm font-medium text-text"
          >
            Confirm Password
          </label>

          <input
            type="password"
            id="confirmPassword"
            name="confirmPassword"
            autoComplete="new-password"
            required
            className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
          />
        </div>
      )}

      <Button type="submit" size="large">
        {isRegister ? "Register" : "Login"}
      </Button>

      <p className="text-center text-sm text-muted">
        {isRegister ? "Already have an account?" : "Don't have an account?"}{" "}
        <Link
          to={isRegister ? "/login" : "/register"}
          className="font-medium text-primary transition hover:text-primary-hover"
        >
          {isRegister ? "Login" : "Register"}
        </Link>
      </p>

      <div className="flex items-center gap-3">
        <div className="h-px flex-1 bg-border" />
        <span className="text-sm text-muted">OR</span>
        <div className="h-px flex-1 bg-border" />
      </div>

      <button
        type="button"
        className="flex w-full items-center justify-center gap-3 rounded-md border border-border bg-surface px-4 py-2.5 font-medium text-text transition hover:bg-slate-50"
      >
        <span className="text-lg font-semibold">G</span>
        Continue with Google
      </button>
    </form>
  );
}

export default AuthForm;
