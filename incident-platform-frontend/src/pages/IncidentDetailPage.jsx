import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  analyzeIncident,
  createLog,
  getIncidentById,
  getLogsByIncident,
  updateIncident,
  uploadAttachment,
} from "../api/incidents";
import LogList from "../components/LogList";
import LogForm from "../components/LogForm";
import AttachmentUpload from "../components/AttachmentUpload";
import AIAnalysisPanel from "../components/AIAnalysisPanel";

export default function IncidentDetailPage() {
  const { id } = useParams();

  const [incident, setIncident] = useState(null);
  const [logs, setLogs] = useState([]);
  const [logsError, setLogsError] = useState(null);
  const [loadingLogs, setLoadingLogs] = useState(true);
  const [analysis, setAnalysis] = useState(null);
  const [analysisError, setAnalysisError] = useState(null);
  const [loadingAnalysis, setLoadingAnalysis] = useState(false);
  const [updatingStatus, setUpdatingStatus] = useState(false);
  const [statusMessage, setStatusMessage] = useState(null);

  async function loadLogs() {
    setLoadingLogs(true);
    try {
      const logsData = await getLogsByIncident(id);
      setLogs(logsData);
      setLogsError(null);
    } catch {
      setLogsError("Logs are temporarily unavailable.");
    } finally {
      setLoadingLogs(false);
    }
  }

  useEffect(() => {
    async function load() {
      const incidentData = await getIncidentById(id);
      setIncident(incidentData);

      await loadLogs();
    }

    load();
  }, [id]);

  async function handleCreateLog(payload) {
    const created = await createLog(id, payload);
    setLogs((prev) => [created, ...prev]);

    // Keep client state aligned with backend ordering/shape without blocking UI.
    loadLogs();
  }

  async function handleSaveStatus() {
    if (!incident) return;

    setUpdatingStatus(true);
    setStatusMessage(null);
    try {
      const updated = await updateIncident(id, {
        title: incident.title,
        description: incident.description,
        status: incident.status,
      });
      setIncident(updated);
      setStatusMessage("Status updated.");
    } catch {
      setStatusMessage("Failed to update status.");
    } finally {
      setUpdatingStatus(false);
    }
  }

  async function handleAnalyze() {
    setLoadingAnalysis(true);
    setAnalysisError(null);
    try {
      const data = await analyzeIncident(id);
      setAnalysis(data);
    } catch {
      setAnalysisError("AI analysis is temporarily unavailable.");
    } finally {
      setLoadingAnalysis(false);
    }
  }

  async function handleUpload(file) {
    return uploadAttachment(id, file);
  }

  if (!incident) {
    return (
      <div className="flex items-center gap-2 text-slate-400 dark:text-slate-500">
        <svg className="h-4 w-4 animate-spin" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
        <p className="text-sm">Loading incident...</p>
      </div>
    );
  }

  const inputClass = "rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-[#0d1526] px-3 py-2 text-sm text-slate-800 dark:text-slate-200 outline-none focus:border-sky-400 dark:focus:border-sky-500 focus:ring-2 focus:ring-sky-500/20 transition-all";

  return (
    <div className="space-y-6">
      {/* Incident header card */}
      <div className="rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-6">
        <h1 className="text-2xl font-extrabold tracking-tight text-slate-800 dark:text-white font-display">{incident.title}</h1>
        <p className="mt-2 leading-relaxed text-slate-500 dark:text-slate-400">{incident.description}</p>

        <div className="mt-5 flex flex-wrap items-center gap-3">
          <label className="text-sm font-medium text-slate-500 dark:text-slate-400">Status</label>
          <select
            value={incident.status ?? "Open"}
            onChange={(e) =>
              setIncident((prev) => ({
                ...prev,
                status: e.target.value,
              }))
            }
            className={inputClass}
          >
            <option value="Open">Open</option>
            <option value="In Progress">In Progress</option>
            <option value="Resolved">Resolved</option>
            <option value="Closed">Closed</option>
          </select>
          <button
            type="button"
            onClick={handleSaveStatus}
            disabled={updatingStatus}
            className="rounded-lg bg-sky-500 px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-sky-500/25 hover:bg-sky-400 hover:shadow-sky-500/40 disabled:cursor-not-allowed disabled:opacity-50 disabled:shadow-none transition-all font-display"
          >
            {updatingStatus ? "Saving..." : "Save Status"}
          </button>
          {statusMessage && (
            <span className="text-sm text-slate-500 dark:text-slate-400">{statusMessage}</span>
          )}
        </div>

        <p className="mt-4 text-[10px] uppercase tracking-wider text-slate-300 dark:text-slate-600 break-all font-mono">{incident.id}</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="space-y-4">
          {logsError && (
            <div className="rounded-lg border border-rose-500/20 bg-rose-500/10 p-3 text-sm text-rose-600 dark:text-rose-400">
              {logsError}
            </div>
          )}
          <LogForm onSubmit={handleCreateLog} />
          <LogList logs={logs} loading={loadingLogs} />
        </div>
        <AttachmentUpload onUpload={handleUpload} />
      </div>

      <AIAnalysisPanel
        analysis={analysis}
        loading={loadingAnalysis}
        onAnalyze={handleAnalyze}
      />
      {analysisError && (
        <div className="rounded-lg border border-rose-500/20 bg-rose-500/10 p-3 text-sm text-rose-600 dark:text-rose-400">
          {analysisError}
        </div>
      )}
    </div>
  );
}
