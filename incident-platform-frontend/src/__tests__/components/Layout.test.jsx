import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { ThemeProvider } from "../../context/ThemeContext";
import Layout from "../../components/Layout";

describe("Layout", () => {
  it("renders navigation links to Dashboard and New Incident", () => {
    render(
      <ThemeProvider>
        <MemoryRouter>
          <Layout>
            <div>child content</div>
          </Layout>
        </MemoryRouter>
      </ThemeProvider>
    );

    expect(screen.getByText("Dashboard")).toBeInTheDocument();
    expect(screen.getByText("New Incident")).toBeInTheDocument();
    expect(screen.getByText("AI")).toBeInTheDocument();
    expect(screen.getByText("Incident")).toBeInTheDocument();
  });

  it("renders children inside the main area", () => {
    render(
      <ThemeProvider>
        <MemoryRouter>
          <Layout>
            <p>Test child</p>
          </Layout>
        </MemoryRouter>
      </ThemeProvider>
    );

    expect(screen.getByText("Test child")).toBeInTheDocument();
  });

  it("links point to correct routes", () => {
    render(
      <ThemeProvider>
        <MemoryRouter>
          <Layout>content</Layout>
        </MemoryRouter>
      </ThemeProvider>
    );

    const links = screen.getAllByRole("link");
    const hrefs = links.map((l) => l.getAttribute("href"));

    expect(hrefs).toContain("/");
    expect(hrefs).toContain("/incidents/new");
  });
});
