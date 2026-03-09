import { useEffect, useState } from "react";
import { getIncidents, deleteIncident } from "../api/incidents";
import IncidentCard from "../components/IncidentCard";

export default function DashboardPage() {
  const [incidents, setIncidents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleteError, setDeleteError] = useState(null);
  const [deleting, setDeleting] = useState(false);

  async function loadIncidents() {
    setLoading(true);
    try {
      const data = await getIncidents();
      setIncidents(data);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadIncidents();
  }, []);

  function handleDelete(id) {
    const incident = incidents.find((i) => i.id === id);
    setDeleteTarget(incident ?? { id });
    setDeleteError(null);
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    setDeleteError(null);
    try {
      await deleteIncident(deleteTarget.id);
      setIncidents((prev) => prev.filter((inc) => inc.id !== deleteTarget.id));
      setDeleteTarget(null);
    } catch {
      setDeleteError("Failed to delete incident.");
    } finally {
      setDeleting(false);
    }
  }

  function cancelDelete() {
    setDeleteTarget(null);
    setDeleteError(null);
  }

  const openCount = incidents.filter((i) => i.status === "Open" || !i.status).length;
  const inProgressCount = incidents.filter((i) => i.status === "In Progress").length;

  return (
    <div>
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-extrabold tracking-tight text-slate-800 dark:text-white font-display">
          Incident Dashboard
        </h1>
        <p className="mt-1 text-slate-500 dark:text-slate-400">
          Monitor incidents, logs, attachments and AI insights.
        </p>
      </div>

      {/* Stats strip */}
      {!loading && incidents.length > 0 && (
        <div className="mb-6 flex flex-wrap gap-3">
          <div className="flex items-center gap-2 rounded-lg border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] px-4 py-2">
            <span className="text-2xl font-bold text-slate-800 dark:text-white font-display">{incidents.length}</span>
            <span className="text-xs uppercase tracking-wider text-slate-400 dark:text-slate-500">Total</span>
          </div>
          {openCount > 0 && (
            <div className="flex items-center gap-2 rounded-lg border border-rose-500/20 bg-rose-500/5 px-4 py-2">
              <span className="h-2 w-2 rounded-full bg-rose-400 animate-pulse-dot" />
              <span className="text-2xl font-bold text-rose-600 dark:text-rose-400 font-display">{openCount}</span>
              <span className="text-xs uppercase tracking-wider text-rose-400 dark:text-rose-500">Open</span>
            </div>
          )}
          {inProgressCount > 0 && (
            <div className="flex items-center gap-2 rounded-lg border border-amber-500/20 bg-amber-500/5 px-4 py-2">
              <span className="h-2 w-2 rounded-full bg-amber-400 animate-pulse-dot" />
              <span className="text-2xl font-bold text-amber-600 dark:text-amber-400 font-display">{inProgressCount}</span>
              <span className="text-xs uppercase tracking-wider text-amber-400 dark:text-amber-500">In Progress</span>
            </div>
          )}
        </div>
      )}

      {loading ? (
        <div className="flex items-center gap-2 text-slate-400 dark:text-slate-500">
          <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
          </svg>
          <p className="text-sm">Loading incidents...</p>
        </div>
      ) : incidents.length === 0 ? (
        <div className="rounded-xl border border-dashed border-slate-200 dark:border-slate-700 bg-white dark:bg-[#131c31] p-12 text-center">
          <p className="text-sm text-slate-400 dark:text-slate-500">No incidents found.</p>
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
          {incidents.map((incident) => (
            <IncidentCard key={incident.id} incident={incident} onDelete={handleDelete} />
          ))}
        </div>
      )}

      {/* Delete confirmation modal */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          {/* Backdrop */}
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={cancelDelete} />

          {/* Modal */}
          <div className="relative w-full max-w-sm rounded-2xl border border-rose-500/20 bg-white dark:bg-[#131c31] p-6 shadow-2xl shadow-rose-500/10">
            {/* Icon */}
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-rose-500/10">
              <svg className="h-6 w-6 text-rose-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </div>

            <h3 className="text-center text-lg font-bold text-slate-800 dark:text-white font-display">
              Delete Incident
            </h3>
            <p className="mt-2 text-center text-sm text-slate-500 dark:text-slate-400">
              Are you sure you want to delete{" "}
              <span className="font-semibold text-slate-700 dark:text-slate-200">
                {deleteTarget.title ?? "this incident"}
              </span>
              ? This action cannot be undone.
            </p>

            {deleteError && (
              <p className="mt-3 rounded-lg bg-rose-500/10 px-3 py-2 text-center text-xs font-medium text-rose-600 dark:text-rose-400">
                {deleteError}
              </p>
            )}

            <div className="mt-6 flex gap-3">
              <button
                onClick={cancelDelete}
                disabled={deleting}
                className="flex-1 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-[#0d1526] px-4 py-2.5 text-sm font-medium text-slate-600 dark:text-slate-300 transition-colors hover:bg-slate-50 dark:hover:bg-slate-800 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={confirmDelete}
                disabled={deleting}
                className="flex-1 rounded-xl bg-rose-500 px-4 py-2.5 text-sm font-semibold text-white shadow-lg shadow-rose-500/25 transition-all hover:bg-rose-600 hover:shadow-rose-500/40 disabled:opacity-50"
              >
                {deleting ? (
                  <span className="flex items-center justify-center gap-2">
                    <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                    </svg>
                    Deleting…
                  </span>
                ) : (
                  "Delete"
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
