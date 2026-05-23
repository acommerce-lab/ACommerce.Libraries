Rukkab (رُكَّاب)

Minimal scaffolding for the Rukkab ride-hailing app.

Structure:
- Driver/Driver.Api  -> backend API for drivers
- Driver/Driver.Web  -> Blazor WebAssembly presentation for drivers
- Rider/Rider.Api    -> backend API for riders
- Rider/Rider.Web    -> Blazor WebAssembly presentation for riders
- Shared/Rukkab.Shared.Blazor -> Razor class library with shared Blazor components

This scaffold intentionally keeps projects minimal. Add ProjectReferences to existing shared libraries in `libs/` as needed (authentication, notifications, realtime, files, etc.).

Quick run

1. Start everything (will kill previous runs):

```bash
./scripts/run-rukkab.sh
```

2. Web UIs (after startup):
- Rider UI: http://127.0.0.1:5003
- Driver UI: http://127.0.0.1:5004

Demo credentials

- Rider id: `rider-1`
- Driver id: `driver-1`

Logs

- Logs are written to `logs/rukkab/*.log`. Use `tail -f` to follow them during a run.

Notes

- SignalR is used for Chat/Notifications. If SignalR is unavailable the UI falls back to simple HTTP or a local demo-mode store.
- If any port (5001-5004) is already in use the runner will attempt to kill the previous devserver process.

Smoke-check helper

There is a small smoke check script at `scripts/smoke-checks.sh` that performs lightweight health checks of the APIs and web roots. Run it after the runner to validate services.

If you want me to continue I will carry on with the remaining tasks: polish styles, add minimal component tests, and harden the runner.
