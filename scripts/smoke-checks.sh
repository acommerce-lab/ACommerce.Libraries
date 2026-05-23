#!/usr/bin/env bash
set -euo pipefail

echo "Running smoke checks for Rukkab demo..."

check() {
  url=$1
  name=$2
  code=$(curl -s -o /dev/null -w "%{http_code}" "$url" || echo "000")
  echo "$name -> $url -> HTTP $code"
}

check http://127.0.0.1:5001/ "Rider API root"
check http://127.0.0.1:5002/ "Driver API root"
check http://127.0.0.1:5003/ "Rider Web"
check http://127.0.0.1:5004/ "Driver Web"

# Check hubs (best-effort)
check http://127.0.0.1:5001/hubs/chat "Rider Chat hub (HTTP probe)"
check http://127.0.0.1:5002/hubs/chat "Driver Chat hub (HTTP probe)"

echo "Smoke checks complete." 
