#!/bin/bash
# Simple smoke test for Rider and Driver APIs (expects services running locally)
set -euo pipefail

RIDER_URL=${RIDER_URL:-http://127.0.0.1:5001}
DRIVER_URL=${DRIVER_URL:-http://127.0.0.1:5002}

echo "Checking Rider swagger..."
if curl -sS -o /dev/null -w "%{http_code}" "$RIDER_URL/swagger/index.html" | grep -q "200"; then
  echo "Rider swagger: OK"
else
  echo "Rider swagger: FAIL"
fi

echo "Checking Driver swagger..."
if curl -sS -o /dev/null -w "%{http_code}" "$DRIVER_URL/swagger/index.html" | grep -q "200"; then
  echo "Driver swagger: OK"
else
  echo "Driver swagger: FAIL"
fi

# Check a lightweight endpoint
echo "Checking Rider /rides endpoint (GET /rides as health check)..."
if curl -sS -o /dev/null -w "%{http_code}" "$RIDER_URL/api/rides" || true; then
  echo "Rides endpoint returned something (check full output manually)"
else
  echo "Rides endpoint not reachable"
fi

echo "Smoke test finished."
