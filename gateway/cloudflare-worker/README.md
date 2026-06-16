# Ashare API failover gateway (Cloudflare Worker)

A transparent reverse proxy that lives at `https://api.ashare.sa`:

```
app → https://api.ashare.sa → [Worker] → http://origin.ashare.sa:8080   (primary: Alibaba)
                                       ↘ on failure → https://ashareapi1.runasp.net  (fallback)
```

The published mobile app keeps calling `api.ashare.sa`, so **no app update is
needed**. When Alibaba is down (timeout / connection error / 5xx) the Worker
re-sends the request to the runasp.net backend automatically.

## Prerequisite
`ashare.sa` must be a **zone on your Cloudflare account** (its DNS managed by
Cloudflare). If it isn't yet, add the domain to Cloudflare (free) and update the
nameservers at your registrar first.

> **The primary MUST be a hostname, not a raw IP.** Cloudflare Workers cannot
> `fetch()` a raw IP address — it returns a 403 with body `error code: 1003`.
> Add a **DNS-only (grey-cloud)** record, e.g. `origin.ashare.sa` → `8.213.82.216`,
> and use `http://origin.ashare.sa:8080` as `PRIMARY_ORIGIN`. (The fallback is
> already a hostname, so it's fine.)

## Deploy — Option A: Dashboard (no tooling)
1. Cloudflare → **Workers & Pages → Create → Worker** → name `ashare-api-gateway` → Deploy.
2. **Edit code** → paste the contents of `worker.js` → **Save and deploy**.
3. **Settings → Variables and secrets** (Plaintext):
   - `PRIMARY_ORIGIN` = `http://origin.ashare.sa:8080`
   - `FALLBACK_ORIGIN` = `https://ashareapi1.runasp.net`
   - (optional) `PRIMARY_TIMEOUT_MS=7000`, `FALLBACK_TIMEOUT_MS=20000`, `CIRCUIT_OPEN_MS=30000`
4. Bind it to the hostname — pick ONE:
   - **Custom Domain (recommended):** Worker → **Settings → Domains & Routes → Add → Custom Domain** → `api.ashare.sa`. Cloudflare creates/points the DNS for you.
   - **Route:** add route `api.ashare.sa/*` on zone `ashare.sa`. Requires a *proxied* (orange-cloud) DNS record for `api.ashare.sa` to exist.

## Deploy — Option B: Wrangler CLI
```bash
npm i -g wrangler
wrangler login
cd gateway/cloudflare-worker
wrangler deploy
```
Edit `routes`/`zone_name` in `wrangler.toml` if your zone differs.

## Verify
```bash
curl -i https://api.ashare.sa/health
```
- Alibaba up  → response served by the primary.
- Alibaba down → still `200` (served by runasp.net) — that's the failover working.

## Notes
- **Failover triggers:** connection error, timeout (`> PRIMARY_TIMEOUT_MS`), or HTTP `5xx`. `4xx` pass through (they are real answers, e.g. 401/404).
- **Bodies** are buffered so POST/PUT can be replayed on the fallback — stays within Cloudflare's request-size limit (100 MB on the free plan). Replays only happen when the primary did NOT process the request (connection/timeout), so double-execution risk is minimal.
- **Circuit breaker** is best-effort per-isolate: after a primary failure it skips the primary for `CIRCUIT_OPEN_MS` to avoid making every client wait for a timeout, then half-opens automatically.
- The primary is plain **HTTP on port 8080** — Cloudflare Workers allow `fetch` to `:8080`.
- Free plan: 100k requests/day. Plenty for failover fronting.
