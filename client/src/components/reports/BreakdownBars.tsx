export interface BreakdownItem {
  label: string;
  count: number;
  barClass: string; // full Tailwind class, e.g. "bg-emerald-500"
}

interface BreakdownBarsProps {
  items: BreakdownItem[];
  total: number;
}

function BreakdownBars({ items, total }: BreakdownBarsProps) {
  return (
    <ul className="space-y-3">
      {items.map((item) => {
        const pct = total === 0 ? 0 : (item.count / total) * 100;
        return (
          <li key={item.label}>
            <div className="mb-1 flex items-center justify-between text-sm">
              <span className="text-slate-700">{item.label}</span>
              <span className="tabular-nums text-slate-500">{item.count}</span>
            </div>
            <div className="h-2 overflow-hidden rounded-full bg-slate-100">
              <div
                className={`h-full rounded-full ${item.barClass}`}
                style={{ width: `${pct}%` }}
              />
            </div>
          </li>
        );
      })}
    </ul>
  );
}

export default BreakdownBars;
