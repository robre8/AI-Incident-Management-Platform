import { Link, useLocation } from "react-router-dom";
import { useTheme } from "../context/ThemeContext";

export default function Layout({ children }) {
  const { theme, toggleTheme } = useTheme();
  const location = useLocation();

  const isActive = (path) => location.pathname === path;

  return (
    <div className="min-h-screen bg-slate-50 text-slate-800 dark:bg-[#0a1120] dark:text-slate-200 bg-grid transition-colors duration-300">
      <header className="sticky top-0 z-50 border-b border-slate-200 dark:border-sky-500/10 bg-white/80 dark:bg-[#0a1120]/80 backdrop-blur-xl">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <Link to="/" className="flex items-center gap-3 group">
            {/* Logo mark */}
            <div className="relative flex h-8 w-8 items-center justify-center rounded-lg bg-sky-500 shadow-lg shadow-sky-500/25 group-hover:shadow-sky-500/40 transition-shadow">
              <svg className="h-4 w-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <span className="text-lg font-bold tracking-tight font-display">
              <span className="text-sky-500">AI</span>{" "}
              <span className="text-slate-700 dark:text-white">Incident</span>
            </span>
          </Link>

          <nav className="flex items-center gap-1">
            <Link
              to="/"
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                isActive("/")
                  ? "bg-sky-500/10 text-sky-600 dark:text-sky-400"
                  : "text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-white"
              }`}
            >
              Dashboard
            </Link>
            <Link
              to="/incidents/new"
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
                isActive("/incidents/new")
                  ? "bg-sky-500/10 text-sky-600 dark:text-sky-400"
                  : "text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-white"
              }`}
            >
              New Incident
            </Link>

            {/* Theme toggle */}
            <button
              onClick={toggleTheme}
              className="ml-2 flex h-8 w-8 items-center justify-center rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-500 dark:text-slate-400 hover:text-sky-500 dark:hover:text-sky-400 hover:border-sky-300 dark:hover:border-sky-500/30 transition-all"
              aria-label={`Switch to ${theme === "light" ? "dark" : "light"} mode`}
            >
              {theme === "light" ? (
                <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                </svg>
              ) : (
                <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
              )}
            </button>
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-6 py-8">
        {children}
      </main>
    </div>
  );
}
