import { useAuth } from "../../context/useAuth";
import TopNav from "./TopNav";
import Sidebar from "./Sidebar";
import { Outlet } from "react-router-dom";
function AppLayout() {
  const { user } = useAuth();

  return (
    <div className="min-h-screen bg-slate-50">
      <TopNav
        name={user?.name}
        email={user?.email}
        avatarUrl={user?.avatarUrl}
      />

      <div className="flex">
        <Sidebar />

        <main className="min-w-0 flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default AppLayout;
