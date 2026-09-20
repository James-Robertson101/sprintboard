export const DAY_MS = 86_400_000;

// Truncates a date to UTC midnight. The API returns UTC timestamps, and the
// burndown points are at UTC midnight, so everything is compared in UTC to
// avoid off-by-one-day bugs in timezones behind UTC.
export function utcDay(value: string | number | Date): number {
  const d = new Date(value);
  return Date.UTC(d.getUTCFullYear(), d.getUTCMonth(), d.getUTCDate());
}

export function formatShortDate(value: string | number): string {
  return new Date(value).toLocaleDateString(undefined, {
    day: "numeric",
    month: "short",
    timeZone: "UTC",
  });
}

export function formatDateRange(startIso: string, endIso: string): string {
  return `${formatShortDate(startIso)} – ${formatShortDate(endIso)}`;
}

export function daysLeft(endIso: string): number {
  const diff = Math.round((utcDay(endIso) - utcDay(Date.now())) / DAY_MS);
  return Math.max(0, diff);
}
