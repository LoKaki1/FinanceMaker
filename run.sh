#!/bin/bash

# Start IB Gateway + IBC
/root/scripts/run.sh

# Wait for it to become responsive
sleep 15

# Run your C# bot
dotnet /app/FinanceMaker.Worker.dll &

# Serve HTTP for Cloud Run health
python3 -m http.server 8080