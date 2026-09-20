import type { BurndownPoint } from "../../types/Report";
import { DAY_MS, formatShortDate, utcDay } from "./dateUtils";

interface BurndownChartProps {
  points: BurndownPoint[];
  startDate: string;
  endDate: string;
  totalIssues: number;
}

const W = 640;
const H = 300;
const M = { top: 16, right: 16, bottom: 32, left: 36 };

function BurndownChart({
  points,
  startDate,
  endDate,
  totalIssues,
}: BurndownChartProps) {
  const start = utcDay(startDate);
  const totalDays = Math.max(Math.round((utcDay(endDate) - start) / DAY_MS), 1);
  const maxY = Math.max(totalIssues, 1);
  const plotW = W - M.left - M.right;
  const plotH = H - M.top - M.bottom;

  const x = (day: number) => M.left + (day / totalDays) * plotW;
  const y = (value: number) => M.top + (1 - value / maxY) * plotH;

  const actual = points.map((p) => ({
    day: Math.round((utcDay(p.date) - start) / DAY_MS),
    remaining: p.remaining,
    date: p.date,
  }));

  const yStep = Math.ceil(maxY / 5);
  const yTicks: number[] = [];
  for (let v = 0; v <= maxY; v += yStep) yTicks.push(v);

  const xStep = Math.ceil(totalDays / 6);
  const xTicks: number[] = [];
  for (let d = 0; d <= totalDays; d += xStep) xTicks.push(d);

  return (
    <div>
      <div className="mb-3 flex items-center gap-5 text-xs text-slate-500">
        <span className="flex items-center gap-2">
          <span className="h-0.5 w-5 bg-indigo-500" />
          Remaining
        </span>
        <span className="flex items-center gap-2">
          <span className="w-5 border-t-2 border-dashed border-slate-400" />
          Ideal
        </span>
      </div>

      <svg
        viewBox={`0 0 ${W} ${H}`}
        className="h-auto w-full"
        role="img"
        aria-label="Sprint burndown chart"
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

        {xTicks.map((d) => (
          <text
            key={d}
            x={x(d)}
            y={H - 10}
            textAnchor="middle"
            className="fill-slate-400 text-[11px]"
          >
            {formatShortDate(start + d * DAY_MS)}
          </text>
        ))}

        {/* Ideal line spans the whole sprint, start to end */}
        <line
          x1={x(0)}
          y1={y(totalIssues)}
          x2={x(totalDays)}
          y2={y(0)}
          strokeWidth={2}
          strokeDasharray="6 4"
          className="stroke-slate-400"
        />

        {actual.length > 0 ? (
          <>
            <polyline
              points={actual
                .map((p) => `${x(p.day)},${y(p.remaining)}`)
                .join(" ")}
              fill="none"
              strokeWidth={2.5}
              strokeLinejoin="round"
              strokeLinecap="round"
              className="stroke-indigo-500"
            />
            {actual.map((p) => (
              <circle
                key={p.day}
                cx={x(p.day)}
                cy={y(p.remaining)}
                r={3.5}
                className="fill-indigo-500"
              >
                <title>
                  {`${formatShortDate(p.date)}: ${p.remaining} remaining`}
                </title>
              </circle>
            ))}
          </>
        ) : (
          <text
            x={W / 2}
            y={H / 2}
            textAnchor="middle"
            className="fill-slate-400 text-[13px]"
          >
            This sprint hasn't started yet.
          </text>
        )}
      </svg>
    </div>
  );
}

export default BurndownChart;
