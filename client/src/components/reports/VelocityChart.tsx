import type { VelocityPoint } from "../../types/Report";

interface VelocityChartProps {
  data: VelocityPoint[];
}

const W = 640;
const H = 300;
const M = { top: 20, right: 16, bottom: 32, left: 36 };

function truncate(text: string, max: number) {
  return text.length > max ? `${text.slice(0, max - 1)}…` : text;
}

function VelocityChart({ data }: VelocityChartProps) {
  if (data.length === 0) {
    return (
      <p className="py-10 text-center text-sm text-slate-500">
        No completed sprints yet.
      </p>
    );
  }

  const plotW = W - M.left - M.right;
  const plotH = H - M.top - M.bottom;

  const maxRaw = Math.max(
    ...data.flatMap((d) => [d.committed, d.completed]),
    1,
  );
  const yStep = Math.ceil(maxRaw / 5);
  const maxY = yStep * Math.ceil(maxRaw / yStep);

  const yTicks: number[] = [];
  for (let v = 0; v <= maxY; v += yStep) yTicks.push(v);

  const y = (value: number) => M.top + (1 - value / maxY) * plotH;
  const groupW = plotW / data.length;
  const barW = Math.min(28, groupW * 0.28);
  const gap = 4;

  return (
    <div>
      <div className="mb-3 flex items-center gap-5 text-xs text-slate-500">
        <span className="flex items-center gap-2">
          <span className="h-2.5 w-2.5 rounded-sm bg-slate-300" />
          Committed
        </span>
        <span className="flex items-center gap-2">
          <span className="h-2.5 w-2.5 rounded-sm bg-emerald-500" />
          Completed
        </span>
      </div>

      <svg
        viewBox={`0 0 ${W} ${H}`}
        className="h-auto w-full"
        role="img"
        aria-label="Velocity chart: committed versus completed issues per sprint"
      >
        {yTicks.map((v) => (
          <g key={v}>
            <line
              x1={M.left}
              x2={W - M.right}
              y1={y(v)}
              y2={y(v)}
              className="stroke-slate-100"
            />
            <text
              x={M.left - 8}
              y={y(v)}
              textAnchor="end"
              dominantBaseline="middle"
              className="fill-slate-400 text-[11px]"
            >
              {v}
            </text>
          </g>
        ))}

        {data.map((d, i) => {
          const cx = M.left + groupW * (i + 0.5);
          const committedX = cx - barW - gap / 2;
          const completedX = cx + gap / 2;
          const committedY = y(d.committed);
          const completedY = y(d.completed);
          const baseline = y(0);

          return (
            <g key={d.sprintId}>
              <rect
                x={committedX}
                y={committedY}
                width={barW}
                height={baseline - committedY}
                rx={3}
                className="fill-slate-300"
              >
                <title>{`${d.sprintName}: ${d.committed} committed`}</title>
              </rect>
              <rect
                x={completedX}
                y={completedY}
                width={barW}
                height={baseline - completedY}
                rx={3}
                className="fill-emerald-500"
              >
                <title>{`${d.sprintName}: ${d.completed} completed`}</title>
              </rect>

              <text
                x={committedX + barW / 2}
                y={committedY - 5}
                textAnchor="middle"
                className="fill-slate-500 text-[11px]"
              >
                {d.committed}
              </text>
              <text
                x={completedX + barW / 2}
                y={completedY - 5}
                textAnchor="middle"
                className="fill-slate-500 text-[11px]"
              >
                {d.completed}
              </text>

              <text
                x={cx}
                y={H - 10}
                textAnchor="middle"
                className="fill-slate-500 text-[11px]"
              >
                {truncate(d.sprintName, 12)}
              </text>
            </g>
          );
        })}
      </svg>
    </div>
  );
}

export default VelocityChart;
