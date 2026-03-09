import { useState } from "react";

export default function AttachmentUpload({ onUpload }) {
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [error, setError] = useState(null);

  async function handleSubmit(e) {
    e.preventDefault();
    if (!file) {
      setError("Select a file first.");
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const data = await onUpload(file);
      setResult(data);
      setFile(null);
      e.target.reset();
    } catch (err) {
      const message =
        err?.response?.data?.detail ||
        err?.response?.data ||
        err?.message ||
        "Upload failed. Try again.";
      setError(String(message));
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-5">
      <div className="mb-4 flex items-center gap-2">
        <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-amber-500/10 dark:bg-amber-500/15">
          <svg className="h-4 w-4 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
          </svg>
        </div>
        <h2 className="text-base font-semibold text-slate-800 dark:text-white font-display">Attachments</h2>
      </div>

      <form onSubmit={handleSubmit} className="space-y-3">
        <div className="flex flex-wrap items-center gap-3">
          <input
            id="attachment-file"
            type="file"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
            className="hidden"
          />
          <label
            htmlFor="attachment-file"
            className="cursor-pointer rounded-lg border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-[#0d1526] px-4 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 hover:border-sky-300 dark:hover:border-sky-500/30 hover:text-sky-500 transition-all"
          >
            Choose File
          </label>
          <span className="text-sm text-slate-400 dark:text-slate-500 font-mono">
            {file ? file.name : "No file selected"}
          </span>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="rounded-lg bg-amber-500 px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-amber-500/25 hover:bg-amber-400 hover:shadow-amber-500/40 disabled:cursor-not-allowed disabled:opacity-50 disabled:shadow-none transition-all font-display"
        >
          {loading ? "Uploading..." : "Upload File"}
        </button>
      </form>

      {error && (
        <div className="mt-4 rounded-lg border border-rose-500/20 bg-rose-500/10 p-3 text-sm text-rose-600 dark:text-rose-400">
          {error}
        </div>
      )}

      {result && (
        <div className="mt-4 rounded-lg border border-emerald-500/20 bg-emerald-500/10 p-3 text-sm">
          <p className="font-medium text-emerald-600 dark:text-emerald-400">Uploaded successfully</p>
          <p className="mt-1 break-all text-emerald-600/70 dark:text-emerald-400/70 font-mono text-xs">{result.fileKey}</p>
        </div>
      )}
    </div>
  );
}
