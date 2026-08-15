import { Link } from "react-router-dom";
import Button from "../Button";
import { useState } from "react";
import React from "react";
import { registerUser } from "../../services/authService";
import type { RegisterData } from "../../types/auth";

function RegisterForm() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    if (password !== confirmPassword) {
      setError("Passwords don't match");
      return;
    }
    const data: RegisterData = {
      name,
      email,
      password,
    };

    try {
      const response = await registerUser(data);
      console.log(response);
    } catch (err) {
      console.error(err);
    }
  }
  return (
    <form
      onSubmit={handleSubmit}
      className="flex w-full max-w-md flex-col gap-4 rounded-xl bg-surface p-8 shadow-sm"
    >
      {error && (
        <div
          role="alert"
          className="password-error rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
        >
          {error}
        </div>
      )}
      <div>
        <label
          htmlFor="name"
          className="mb-1 block text-sm font-medium text-text"
        >
          Name
        </label>

        <input
          onChange={(e) => setName(e.target.value)}
          value={name}
          type="text"
          id="name"
          name="name"
          autoComplete="name"
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      </div>

      <div>
        <label
          htmlFor="email"
          className="mb-1 block text-sm font-medium text-text"
        >
          Email
        </label>

        <input
          onChange={(e) => setEmail(e.target.value)}
          value={email}
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
          onChange={(e) => setPassword(e.target.value)}
          value={password}
          type="password"
          id="password"
          name="password"
          autoComplete="new-password"
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      </div>

      <div>
        <label
          htmlFor="confirmPassword"
          className="mb-1 block text-sm font-medium text-text"
        >
          Confirm Password
        </label>

        <input
          onChange={(e) => setConfirmPassword(e.target.value)}
          value={confirmPassword}
          type="password"
          id="confirmPassword"
          name="confirmPassword"
          autoComplete="new-password"
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
        />
      </div>

      <Button type="submit" size="large">
        Register
      </Button>

      <p className="text-center text-sm text-muted">
        Already have an account?{" "}
        <Link
          to="/login"
          className="font-medium text-primary transition hover:text-primary-hover"
        >
          Login
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

export default RegisterForm;
