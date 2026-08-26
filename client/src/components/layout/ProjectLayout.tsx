import ProjectNavigation from "../ProjectNavigation";
import { Outlet } from "react-router-dom";
import TopNav from "./TopNav";
import { useState } from "react";
import { useEffect } from "react";
import { getUser } from "../../services/authService";
import type { User } from "../../types/auth";

function ProjectLayout() {
  const [userData, setUserData] = useState<User>();

  useEffect(() => {
    async function loadUserData() {
      try {
        const userData = await getUser();
        setUserData(userData);
      } catch (error) {
        console.error("Failed to load user: ", error);
      }
    }
    loadUserData();
  }, []);

  return (
    <div>
      <TopNav
        name={userData?.name}
        email={userData?.email}
        avatarUrl={userData?.avatarUrl}
      />

      <ProjectNavigation />

      <Outlet />
    </div>
  );
}

export default ProjectLayout;
