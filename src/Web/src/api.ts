import type {
  CreatePolicyRequest,
  CreatePolicyResult,
  Invoice,
  ProblemDetails,
  RecordTripRequest,
  Trip,
} from './types';

// Relative URLs: nginx (Docker) or the Vite proxy (`npm run dev`) routes them to the right service.

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await fetch(url, init);
  } catch {
    throw new Error('Could not reach the server. Is the backend running?');
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails;
    throw new Error(problem.title ?? `Something went wrong (HTTP ${response.status}).`);
  }

  return (await response.json()) as T;
}

function postJson<T>(url: string, body: unknown): Promise<T> {
  return request<T>(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
}

export const api = {
  createPolicy: (body: CreatePolicyRequest) => postJson<CreatePolicyResult>('/api/policies', body),

  recordTrip: (body: RecordTripRequest) => postJson<unknown>('/api/trips', body),

  getTrips: (policyNumber: string) =>
    request<Trip[]>(`/api/trips/policy/${encodeURIComponent(policyNumber)}`),

  getInvoices: (policyNumber: string) =>
    request<Invoice[]>(`/api/billing/policy/${encodeURIComponent(policyNumber)}`),
};
