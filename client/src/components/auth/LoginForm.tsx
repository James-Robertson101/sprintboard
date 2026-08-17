import { Link } from "react-router-dom";
import Button from "../Button";
import { useState } from "react";
import React from "react";
import { loginUser, handleGoogleLogin } from "../../services/authService";
import type { LoginData } from "../../types/auth";
import { useNavigate } from "react-router-dom";

function LoginForm() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    const data: LoginData = {
      email,
      password,
    };
    try {
      await loginUser(data);
      navigate("/projectList");
    } catch {
      setError("Invalid Email or Password. ");
    }
  }

  function handleGoogleClick() {
    try {
      handleGoogleLogin();
    } catch (error) {
      setError(
        error instanceof Error
          ? error.message
          : "Unable to connect to the server.",
      );
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
          className="login-error rounded-md border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
        >
          {error}
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
          onChange={(e) => {
            setEmail(e.target.value);
          }}
          type="email"
          id="email"
          name="email"
          autoComplete="email"
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
          value={email}
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
          onChange={(e) => {
            setPassword(e.target.value);
          }}
          type="password"
          id="password"
          name="password"
          autoComplete="current-password"
          required
          className="w-full rounded-md border border-border bg-surface px-3 py-2 text-text outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
          value={password}
        />
      </div>

      <Button type="submit" size="large">
        Login
      </Button>

      <p className="text-center text-sm text-muted">
        Don't have an account?{" "}
        <Link
          to="/register"
          className="font-medium text-primary transition hover:text-primary-hover"
        >
          Register
        </Link>
      </p>

      <div className="flex items-center gap-3">
        <div className="h-px flex-1 bg-border" />
        <span className="text-sm text-muted">OR</span>
        <div className="h-px flex-1 bg-border" />
      </div>

      <button
        type="button"
        onClick={handleGoogleClick}
        className="flex w-full items-center justify-center gap-3 rounded-md border border-border bg-surface px-4 py-2.5 font-medium text-text transition hover:bg-slate-50"
      >
        <span className="text-lg font-semibold">G</span>
        Continue with Google
      </button>
    </form>
  );
}

export default LoginForm;
