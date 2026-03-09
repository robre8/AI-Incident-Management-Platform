import { Link } from "react-router-dom";

const STATUS_CONFIG = {
  Open: { dot: "bg-rose-400", bg: "bg-rose-500/10 dark:bg-rose-500/15", text: "text-rose-600 dark:text-rose-400", pulse: true },
  "In Progress": { dot: "bg-amber-400", bg: "bg-amber-500/10 dark:bg-amber-500/15", text: "text-amber-600 dark:text-amber-400", pulse: true },
  Resolved: { dot: "bg-emerald-400", bg: "bg-emerald-500/10 dark:bg-emerald-500/15", text: "text-emerald-600 dark:text-emerald-400", pulse: false },
  Closed: { dot: "bg-slate-400", bg: "bg-slate-500/10 dark:bg-slate-500/15", text: "text-slate-500 dark:text-slate-400", pulse: false },
};

export default function IncidentCard({ incident, onDelete }) {
  const status = incident.status ?? "Open";
  const config = STATUS_CONFIG[status] ?? STATUS_CONFIG.Open;

  function handleDelete(e) {
    e.preventDefault();
    onDelete(incident.id);
  }

  return (
    <Link
      to={`/incidents/${incident.id}`}
      className="group relative block rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-5 transition-all duration-200 hover:border-sky-300 dark:hover:border-sky-500/30 hover:shadow-lg hover:shadow-sky-500/5 dark:hover:shadow-sky-500/10"
    >
      {/* Top accent line */}
      <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-sky-400/40 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />

      <div className="mb-3 flex items-start justify-between gap-3">
        <h3 className="text-base font-semibold leading-snug text-slate-800 dark:text-white font-display">{incident.title}</h3>
        <span className={`flex shrink-0 items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${config.bg} ${config.text}`}>
          <span className={`h-1.5 w-1.5 rounded-full ${config.dot} ${config.pulse ? "animate-pulse-dot" : ""}`} />
          {status}
        </span>
      </div>

      <p className="text-sm leading-relaxed text-slate-500 dark:text-slate-400 line-clamp-2">
        {incident.description}
      </p>

      <p className="mt-3 text-[10px] tracking-wider uppercase text-slate-300 dark:text-slate-600 break-all font-mono">
        {incident.id}
      </p>

      {onDelete && (
        <button
          onClick={handleDelete}
          className="absolute bottom-3 right-3 flex h-7 w-7 items-center justify-center rounded-lg text-slate-300 dark:text-slate-600 opacity-0 group-hover:opacity-100 hover:bg-rose-500/10 hover:text-rose-500 dark:hover:text-rose-400 transition-all"
          title="Delete incident"
        >
          <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      )}
    </Link>
  );
}
