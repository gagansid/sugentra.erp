#!/usr/bin/env bash
# Kills whatever is listening on the UI's ports (stale dotnet run instances) before starting fresh.
set -e
cd "$(dirname "$0")/.."

for port in 5037 7022; do
  pid=$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null || true)
  if [ -n "$pid" ]; then
    echo "Killing stale process $pid on port $port"
    kill -9 $pid
  fi
done

dotnet run --project src/Sugentra.ERP.UI "$@"
