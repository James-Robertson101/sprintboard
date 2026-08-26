import { useState } from "react";
import AvatarModal from "../components/auth/AvatarModal";
import Button from "../components/Button";
import { getUser } from "../services/authService";
import { useEffect } from "react";
import TopNav from "../components/layout/TopNav";
import type { User } from "../types/auth";
import { createProject } from "../services/projectService";
import type { ProjectData } from "../types/project";

function CreateProject() {
  const [projectName, setProjectName] = useState("");
  const [description, setDescription] = useState("");
  const [avatarUrl, setAvatarUrl] = useState("");
  const [avatarModalOpen, setAvatarModalOpen] = useState(false);
  const [userData, setUserData] = useState<User>();

  async function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    const ProjectData: ProjectData = {
      name: projectName,
      description: description,
      avatarUrl: avatarUrl,
    };
    try {
      await createProject(ProjectData);
    } catch (error) {
      console.log("failed to create project: ", error);
    }
  }
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
    <>
      <TopNav
        name={userData?.name}
        email={userData?.email}
        avatarUrl={avatarUrl}
      />
      <div className="flex w-screen h-screen justify-center items-center">
        <form onSubmit={handleSubmit} className="flex flex-col">
          <h1 className="text-4xl">Create Project</h1>
          <input
            type="text"
            value={projectName}
            onChange={(e) => setProjectName(e.target.value)}
            placeholder="Enter your Project Name"
          />
          <input
            type="text"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="enter the project description"
          />

          {avatarUrl ? (
            <img
              src={avatarUrl}
              alt="Selected avatar"
              className="h-16 w-16 rounded-full border border-border object-cover"
            />
          ) : (
            <div className="flex h-16 w-16 items-center justify-center rounded-full border border-dashed border-border bg-slate-50 text-xs text-muted">
              None
            </div>
          )}

          <button
            type="button"
            onClick={() => {
              setAvatarModalOpen(true);
            }}
            className="rounded-md border border-border bg-surface px-4 py-2 text-sm font-medium text-text transition hover:bg-slate-50"
          >
            {avatarUrl ? "Change avatar" : "Choose avatar"}
          </button>
          <Button type="submit">Create</Button>
        </form>
      </div>

      {avatarModalOpen && (
        <AvatarModal
          name={projectName}
          selectedAvatarUrl={avatarUrl}
          onSelect={setAvatarUrl}
          onClose={() => setAvatarModalOpen(false)}
        />
      )}
    </>
  );
}
export default CreateProject;
