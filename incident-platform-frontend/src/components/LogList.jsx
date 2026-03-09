const LOG_LEVEL_STYLES = {
  Info: "text-sky-500 bg-sky-500/10",
  Warning: "text-amber-500 bg-amber-500/10",
  Error: "text-rose-500 bg-rose-500/10",
};

export default function LogList({ logs, loading = false }) {
  return (
    <div className="rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-5">
      <div className="mb-4 flex items-center gap-2">
        <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-emerald-500/10 dark:bg-emerald-500/15">
          <svg className="h-4 w-4 text-emerald-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
        </div>
        <h2 className="text-base font-semibold text-slate-800 dark:text-white font-display">Logs</h2>
        {!loading && logs.length > 0 && (
          <span className="ml-auto rounded-full bg-slate-100 dark:bg-slate-800 px-2 py-0.5 text-xs font-medium text-slate-500 dark:text-slate-400">{logs.length}</span>
        )}
      </div>

      {loading ? (
        <p className="text-sm text-slate-400 dark:text-slate-500">Loading logs...</p>
      ) : logs.length === 0 ? (
        <div className="rounded-lg border border-dashed border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-[#0d1526] p-6 text-center">
          <p className="text-sm text-slate-400 dark:text-slate-500">No logs found.</p>
        </div>
      ) : (
        <div className="space-y-2">
          {logs.map((log) => (
            <div key={log.id} className="rounded-lg border border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-[#0d1526] p-3">
              <div className="mb-1.5 flex items-center justify-between">
                <span className="text-sm font-medium text-slate-700 dark:text-slate-200 font-mono">{log.service}</span>
                <span className={`rounded-full px-2 py-0.5 text-[10px] font-semibold uppercase tracking-wider ${LOG_LEVEL_STYLES[log.logLevel] ?? LOG_LEVEL_STYLES.Info}`}>{log.logLevel}</span>
              </div>

              <p className="text-sm leading-relaxed text-slate-600 dark:text-slate-400">{log.message}</p>

              <p className="mt-2 text-[10px] text-slate-300 dark:text-slate-600 font-mono">
                {log.timestamp}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
