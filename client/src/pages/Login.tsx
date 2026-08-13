import AuthForm from "../components/layout/AuthForm";

function Login() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4">
      <AuthForm mode="login" />
    </main>
  );
}

export default Login;
