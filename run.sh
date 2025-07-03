#!/bin/bash

# Start IB Gateway
/entrypoint.sh &

# Wait for it to initialize
sleep 10

# Start your bot
dotnet FinanceMaker.Worker.dll &

# Dummy HTTP server for Cloud Run
python3 -m http.server 8080