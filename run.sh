#!/bin/bash
set -euo pipefail

echo "[IB] Starting IB Gateway in background"
/opt/ibgateway/entrypoint.sh &

# Wait for port 4001
for i in {1..30}; do
  if nc -z localhost 4001; then
    echo "[IB] Gateway is up on port 4001"
    break
  fi
  sleep 2
done

echo "[Bot] Starting FinanceMaker.Worker"
dotnet /app/FinanceMaker.Worker.dll & BOT=$!

# Cloud Run needs a health port
python3 -u -m http.server 8080 &

wait -n
kill "$BOT" || true
exit 1