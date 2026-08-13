import AuthForm from "../components/layout/AuthForm";

function Register() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4">
      <AuthForm mode="register" />
    </main>
  );
}

export default Register;
