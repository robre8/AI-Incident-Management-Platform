import { useState } from "react";

const DEFAULT_FORM = {
  service: "frontend",
  logLevel: "Info",
  message: "",
};

export default function LogForm({ onSubmit }) {
  const [form, setForm] = useState(DEFAULT_FORM);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    if (!form.message.trim()) return;

    setLoading(true);
    setError(null);
    try {
      await onSubmit({
        service: form.service,
        logLevel: form.logLevel,
        message: form.message.trim(),
      });
      setForm({ ...form, message: "" });
    } catch {
      setError("Failed to create log. Try again.");
    } finally {
      setLoading(false);
    }
  }

  const inputClass = "w-full rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-[#0d1526] px-3 py-2 text-sm text-slate-800 dark:text-slate-200 outline-none focus:border-sky-400 dark:focus:border-sky-500 focus:ring-2 focus:ring-sky-500/20 transition-all";

  return (
    <div className="rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-5">
      <div className="mb-4 flex items-center gap-2">
        <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-sky-500/10 dark:bg-sky-500/15">
          <svg className="h-4 w-4 text-sky-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 6h16M4 12h16M4 18h7" />
          </svg>
        </div>
        <h2 className="text-base font-semibold text-slate-800 dark:text-white font-display">Create Log</h2>
      </div>

      <form onSubmit={handleSubmit} className="space-y-3">
        <input
          type="text"
          value={form.service}
          onChange={(e) => setForm({ ...form, service: e.target.value })}
          placeholder="Service"
          className={inputClass}
        />

        <select
          value={form.logLevel}
          onChange={(e) => setForm({ ...form, logLevel: e.target.value })}
          className={inputClass}
        >
          <option value="Info">Info</option>
          <option value="Warning">Warning</option>
          <option value="Error">Error</option>
        </select>

        <textarea
          value={form.message}
          onChange={(e) => setForm({ ...form, message: e.target.value })}
          placeholder="Log message"
          rows={3}
          className={inputClass}
        />

        <button
          type="submit"
          disabled={loading || !form.message.trim()}
          className="rounded-lg bg-sky-500 px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-sky-500/25 hover:bg-sky-400 hover:shadow-sky-500/40 disabled:cursor-not-allowed disabled:opacity-50 disabled:shadow-none transition-all font-display"
        >
          {loading ? "Creating..." : "Create Log"}
        </button>
      </form>

      {error && (
        <p className="mt-3 rounded-lg border border-rose-500/20 bg-rose-500/10 p-3 text-sm text-rose-600 dark:text-rose-400">
          {error}
        </p>
      )}
    </div>
  );
}
