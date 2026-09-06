#!/usr/bin/env bash
# Kills any stale Sugentra.ERP.UI dotnet processes and anything listening on its ports.
set -e

for port in 5037 7022; do
  pid=$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null || true)
  if [ -n "$pid" ]; then
    echo "Killing stale process $pid on port $port"
    kill -9 $pid
  fi
done

pkill -9 -f 'Sugentra.ERP.UI' 2>/dev/null || true
echo "Done."
