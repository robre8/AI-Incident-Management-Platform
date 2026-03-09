const SEVERITY_STYLES = {
  Critical: "bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20",
  High: "bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-500/20",
  Medium: "bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20",
  Low: "bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20",
};

export default function AIAnalysisPanel({ analysis, loading, onAnalyze }) {
  return (
    <div className="rounded-xl border border-slate-200 dark:border-sky-500/10 bg-white dark:bg-[#131c31] p-5">
      <div className="mb-5 flex items-center justify-between">
        <div className="flex items-center gap-2">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-violet-500/10 dark:bg-violet-500/15">
            <svg className="h-4 w-4 text-violet-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
            </svg>
          </div>
          <h2 className="text-base font-semibold text-slate-800 dark:text-white font-display">AI Analysis</h2>
        </div>
        <button
          onClick={onAnalyze}
          disabled={loading}
          className="rounded-lg bg-violet-500 px-4 py-2 text-sm font-semibold text-white shadow-lg shadow-violet-500/25 hover:bg-violet-400 hover:shadow-violet-500/40 disabled:opacity-50 disabled:shadow-none transition-all font-display"
        >
          {loading ? "Analyzing..." : "Run Analysis"}
        </button>
      </div>

      {!analysis ? (
        <div className="rounded-lg border border-dashed border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-[#0d1526] p-6 text-center">
          <p className="text-sm text-slate-400 dark:text-slate-500">
            Run analysis to inspect severity, root cause and suggested fixes.
          </p>
        </div>
      ) : (
        <div className="space-y-4 text-sm">
          <div className="flex items-center gap-2">
            <span className={`rounded-full border px-3 py-1 text-xs font-semibold ${SEVERITY_STYLES[analysis.severity] ?? SEVERITY_STYLES.Medium}`}>
              {analysis.severity}
            </span>
            {analysis.category && (
              <span className="rounded-full bg-sky-500/10 dark:bg-sky-500/15 border border-sky-500/20 px-3 py-1 text-xs font-medium text-sky-600 dark:text-sky-400 font-mono">
                {analysis.category}
              </span>
            )}
          </div>

          <div>
            <p className="mb-1 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">Root Cause</p>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed">{analysis.rootCause}</p>
          </div>

          <div>
            <p className="mb-1 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">Suggested Fix</p>
            <p className="text-slate-600 dark:text-slate-300 leading-relaxed">{analysis.suggestedFix}</p>
          </div>

          <div>
            <p className="mb-2 text-xs font-semibold uppercase tracking-wider text-slate-400 dark:text-slate-500">Recommended Tests</p>
            <ul className="space-y-1.5">
              {analysis.recommendedTests?.map((item, index) => (
                <li key={index} className="flex items-start gap-2 text-slate-600 dark:text-slate-300">
                  <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-violet-400" />
                  {item}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}
    </div>
  );
}
