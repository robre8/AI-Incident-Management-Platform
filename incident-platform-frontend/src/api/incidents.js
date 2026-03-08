import { api } from "./client";

export async function getIncidents() {
  const res = await api.get("/api/incident");
  return res.data;
}

export async function getIncidentById(id) {
  const res = await api.get(`/api/incident/${id}`);
  return res.data;
}

export async function createIncident(payload) {
  const res = await api.post("/api/incident", payload);
  return res.data;
}

export async function getLogsByIncident(id) {
  const res = await api.get(`/api/incidents/${id}/logs`);
  return res.data;
}

export async function createLog(id, payload) {
  const res = await api.post(`/api/incidents/${id}/logs`, payload);
  return res.data;
}

export async function analyzeIncident(id) {
  const res = await api.post(`/api/ai/analyze/${id}`);
  return res.data;
}

export async function uploadAttachment(id, file) {
  const formData = new FormData();
  formData.append("file", file);

  const res = await api.post(`/api/incidents/${id}/attachments`, formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return res.data;
}
