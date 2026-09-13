export async function reseedIfDue() {
  try {
    await fetch(`${import.meta.env.VITE_API_URL}/api/system/reseed`, {
      method: "POST",
    });
  } catch {
    // Non-critical — safe to ignore failures here
  }
}
