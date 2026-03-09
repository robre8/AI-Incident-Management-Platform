import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import DashboardPage from "../../pages/DashboardPage";
import * as api from "../../api/incidents";

vi.mock("../../api/incidents");

const incidents = [
  { id: "1", title: "DB Down", description: "Postgres OOM", status: "Open" },
  { id: "2", title: "API Latency", description: "p99 > 5s", status: "Resolved" },
];

function renderPage() {
  return render(
    <MemoryRouter>
      <DashboardPage />
    </MemoryRouter>
  );
}

beforeEach(() => {
  vi.clearAllMocks();
  api.getIncidents.mockResolvedValue(incidents);
  api.deleteIncident.mockResolvedValue(undefined);
});

describe("DashboardPage", () => {
  it("shows loading state then renders incident cards", async () => {
    renderPage();

    expect(screen.getByText("Loading incidents...")).toBeInTheDocument();

    await waitFor(() => {
      expect(screen.getByText("DB Down")).toBeInTheDocument();
    });
    expect(screen.getByText("API Latency")).toBeInTheDocument();
  });

  it("shows empty state when no incidents exist", async () => {
    api.getIncidents.mockResolvedValue([]);
    renderPage();

    await waitFor(() => {
      expect(screen.getByText("No incidents found.")).toBeInTheDocument();
    });
  });

  it("deletes an incident after confirmation and removes it from the list", async () => {
    renderPage();
    const user = userEvent.setup();

    await waitFor(() => screen.getByText("DB Down"));

    const deleteBtns = screen.getAllByTitle("Delete incident");
    await user.click(deleteBtns[0]);

    // Modal should appear
    expect(screen.getByText("Delete Incident")).toBeInTheDocument();

    // Confirm deletion
    await user.click(screen.getByRole("button", { name: "Delete" }));

    expect(api.deleteIncident).toHaveBeenCalledWith("1");

    await waitFor(() => {
      expect(screen.queryByText("Delete Incident")).not.toBeInTheDocument();
    });
    // The deleted incident card should be gone
    expect(screen.queryByRole("link", { name: /DB Down/ })).not.toBeInTheDocument();
    expect(screen.getByText("API Latency")).toBeInTheDocument();
  });

  it("does not delete when user cancels confirmation", async () => {
    renderPage();
    const user = userEvent.setup();

    await waitFor(() => screen.getByText("DB Down"));

    const deleteBtns = screen.getAllByTitle("Delete incident");
    await user.click(deleteBtns[0]);

    // Modal should appear
    expect(screen.getByText("Delete Incident")).toBeInTheDocument();

    // Cancel
    await user.click(screen.getByRole("button", { name: "Cancel" }));

    expect(api.deleteIncident).not.toHaveBeenCalled();
    expect(screen.getByText("DB Down")).toBeInTheDocument();
    // Modal should be closed
    expect(screen.queryByText("Delete Incident")).not.toBeInTheDocument();
  });

  it("shows error when delete fails", async () => {
    api.deleteIncident.mockRejectedValue(new Error("Server error"));
    renderPage();
    const user = userEvent.setup();

    await waitFor(() => screen.getByText("DB Down"));

    const deleteBtns = screen.getAllByTitle("Delete incident");
    await user.click(deleteBtns[0]);

    // Confirm deletion
    await user.click(screen.getByRole("button", { name: "Delete" }));

    await waitFor(() => {
      expect(screen.getByText("Failed to delete incident.")).toBeInTheDocument();
    });
    // Modal should still be open with the error, incident still in the list
    expect(screen.getByText("Delete Incident")).toBeInTheDocument();
  });
});
