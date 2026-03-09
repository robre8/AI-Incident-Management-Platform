import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { createIncident } from "../api/incidents";
import IncidentForm from "../components/IncidentForm";

export default function CreateIncidentPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  async function handleCreate(payload) {
    setLoading(true);
    try {
      const created = await createIncident(payload);
      navigate(`/incidents/${created.id}`);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="max-w-2xl">
      <h1 className="mb-6 text-3xl font-extrabold tracking-tight text-slate-800 dark:text-white font-display">Create Incident</h1>
      <IncidentForm onSubmit={handleCreate} loading={loading} />
    </div>
  );
}
