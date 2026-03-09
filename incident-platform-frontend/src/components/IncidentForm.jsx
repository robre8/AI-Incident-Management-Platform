import { useState } from "react";

export default function IncidentForm({ onSubmit, loading }) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState("Open");

  async function handleSubmit(e) {
    e.preventDefault();
    await onSubmit({ title, description, status });
    setTitle("");
    setDescription("");
    setStatus("Open");
  }

  const inputClass = "w-full rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-[#0d1526] px-4 py-2.5 text-sm text-slate-800 dark:text-slate-200 outline-none focus:border-sky-400 dark:focus:border-sky-500 focus:ring-2 focus:ring-sky-500/20 transition-all";

  return (
    <form
      onSubmit={handleSubmit}
      className="rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-6"
    >
      <div className="mb-5">
        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300 font-display">Title</label>
        <input
          className={inputClass}
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Production outage"
          required
        />
      </div>

      <div className="mb-5">
        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300 font-display">Description</label>
        <textarea
          className={inputClass}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Describe the incident"
          rows={4}
          required
        />
      </div>

      <div className="mb-6">
        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300 font-display">Status</label>
        <select
          className={inputClass}
          value={status}
          onChange={(e) => setStatus(e.target.value)}
        >
          <option value="Open">Open</option>
          <option value="In Progress">In Progress</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
        </select>
      </div>

      <button
        type="submit"
        disabled={loading}
        className="rounded-lg bg-sky-500 px-5 py-2.5 text-sm font-semibold text-white shadow-lg shadow-sky-500/25 hover:bg-sky-400 hover:shadow-sky-500/40 disabled:opacity-50 disabled:shadow-none transition-all font-display"
      >
        {loading ? "Creating..." : "Create Incident"}
      </button>
    </form>
  );
}
