/**
 * Ashare API failover gateway (Cloudflare Worker).
 *
 * Sits at https://api.ashare.sa and transparently routes every request to the
 * PRIMARY backend (Alibaba ECS). If the primary is unreachable, times out, or
 * returns a 5xx, the same request is re-sent to the FALLBACK backend
 * (runasp.net). The published mobile app keeps calling api.ashare.sa and never
 * notices the switch.
 *
 * Failover triggers (primary considered "down"):
 *   - connection / DNS error            (fetch throws)
 *   - no response within PRIMARY_TIMEOUT_MS (aborted)
 *   - HTTP status >= 500                (500/502/503/504)
 * A 2xx/3xx/4xx from the primary is returned as-is — a 401/404 is a real
 * answer, not an outage.
 *
 * A tiny per-isolate circuit breaker skips the primary for CIRCUIT_OPEN_MS
 * after a failure, so during a prolonged outage clients don't each wait for a
 * timeout. It is best-effort (isolates are ephemeral) and self-heals via a
 * half-open retry once the window expires.
 *
 * Origins and timeouts are overridable via Worker vars (see wrangler.toml).
 */

let primaryDownUntil = 0; // per-isolate circuit-breaker deadline (ms epoch)

function buildRequest(originBase, request, bodyBuffer) {
  const incoming = new URL(request.url);
  const target = new URL(incoming.pathname + incoming.search, originBase);
  const headers = new Headers(request.headers);
  headers.delete("host");           // let fetch set Host from the target origin
  headers.delete("content-length"); // recomputed from the body by fetch
  return new Request(target.toString(), {
    method: request.method,
    headers,
    body: bodyBuffer,               // undefined for GET/HEAD
    redirect: "manual",
  });
}

async function fetchWithTimeout(req, ms) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), ms);
  try {
    return await fetch(req, { signal: controller.signal });
  } finally {
    clearTimeout(timer);
  }
}

export default {
  async fetch(request, env) {
    const PRIMARY = env.PRIMARY_ORIGIN || "http://8.213.82.216:8080";
    const FALLBACK = env.FALLBACK_ORIGIN || "https://ashareapi1.runasp.net";
    const PRIMARY_TIMEOUT_MS = Number(env.PRIMARY_TIMEOUT_MS || 7000);
    const FALLBACK_TIMEOUT_MS = Number(env.FALLBACK_TIMEOUT_MS || 20000);
    const CIRCUIT_OPEN_MS = Number(env.CIRCUIT_OPEN_MS || 30000);

    // Buffer the body once so the SAME request can be replayed on the fallback.
    const hasBody = request.method !== "GET" && request.method !== "HEAD";
    const bodyBuffer = hasBody ? await request.arrayBuffer() : undefined;

    // Try the primary unless the breaker is open.
    if (primaryDownUntil <= Date.now()) {
      try {
        const resp = await fetchWithTimeout(
          buildRequest(PRIMARY, request, bodyBuffer),
          PRIMARY_TIMEOUT_MS
        );
        if (resp.status < 500) {
          return resp; // primary answered (success or a genuine client error)
        }
        // 5xx -> treat as an outage and fall through to the fallback
      } catch (_) {
        // network error / timeout -> outage
      }
      primaryDownUntil = Date.now() + CIRCUIT_OPEN_MS; // open the breaker briefly
    }

    // Fallback (also the path taken while the breaker is open).
    try {
      return await fetchWithTimeout(
        buildRequest(FALLBACK, request, bodyBuffer),
        FALLBACK_TIMEOUT_MS
      );
    } catch (_) {
      return new Response(
        "Ashare gateway: both primary and fallback backends are unavailable.",
        { status: 502, headers: { "content-type": "text/plain; charset=utf-8" } }
      );
    }
  },
};
