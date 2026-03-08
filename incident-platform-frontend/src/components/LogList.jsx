export default function LogList({ logs, loading = false }) {
  return (
    <div className="rounded-xl border bg-white p-4 shadow-sm">
      <h2 className="mb-4 text-lg font-semibold">Logs</h2>

      {loading ? (
        <p className="text-sm text-slate-500">Loading logs...</p>
      ) : logs.length === 0 ? (
        <p className="text-sm text-slate-500">No logs found.</p>
      ) : (
        <div className="space-y-3">
          {logs.map((log) => (
            <div key={log.id} className="rounded-lg border p-3">
              <div className="mb-1 flex items-center justify-between">
                <span className="font-medium">{log.service}</span>
                <span className="text-xs text-slate-500">{log.logLevel}</span>
              </div>

              <p className="text-sm text-slate-700">{log.message}</p>

              <p className="mt-2 text-xs text-slate-400">
                {log.timestamp}
              </p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
