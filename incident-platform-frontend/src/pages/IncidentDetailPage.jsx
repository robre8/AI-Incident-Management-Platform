import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import {
  analyzeIncident,
  getIncidentById,
  getLogsByIncident,
  uploadAttachment,
} from "../api/incidents";
import LogList from "../components/LogList";
import AttachmentUpload from "../components/AttachmentUpload";
import AIAnalysisPanel from "../components/AIAnalysisPanel";

export default function IncidentDetailPage() {
  const { id } = useParams();

  const [incident, setIncident] = useState(null);
  const [logs, setLogs] = useState([]);
  const [logsError, setLogsError] = useState(null);
  const [analysis, setAnalysis] = useState(null);
  const [loadingAnalysis, setLoadingAnalysis] = useState(false);

  useEffect(() => {
    async function load() {
      const incidentData = await getIncidentById(id);
      setIncident(incidentData);

      try {
        const logsData = await getLogsByIncident(id);
        setLogs(logsData);
        setLogsError(null);
      } catch {
        setLogs([]);
        setLogsError("Logs are temporarily unavailable.");
      }
    }

    load();
  }, [id]);

  async function handleAnalyze() {
    setLoadingAnalysis(true);
    try {
      const data = await analyzeIncident(id);
      setAnalysis(data);
    } finally {
      setLoadingAnalysis(false);
    }
  }

  async function handleUpload(file) {
    return uploadAttachment(id, file);
  }

  if (!incident) {
    return <p>Loading incident...</p>;
  }

  return (
    <div className="space-y-6">
      <div className="rounded-xl border bg-white p-6 shadow-sm">
        <h1 className="text-3xl font-bold">{incident.title}</h1>
        <p className="mt-2 text-slate-600">{incident.description}</p>
        <p className="mt-4 text-xs text-slate-400 break-all">{incident.id}</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="space-y-3">
          {logsError && (
            <div className="rounded-lg border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800">
              {logsError}
            </div>
          )}
          <LogList logs={logs} />
        </div>
        <AttachmentUpload onUpload={handleUpload} />
      </div>

      <AIAnalysisPanel
        analysis={analysis}
        loading={loadingAnalysis}
        onAnalyze={handleAnalyze}
      />
    </div>
  );
}
