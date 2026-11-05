---
name: run-idle-dwarfs
description: Launch Idle Dwarfs locally (ASP.NET Core server + Vue client) and check that both respond. Use when asked to run, start or open the game.
---

# Run Idle Dwarfs

Two processes: the .NET server (`IdleDwarfsGame.Server`) and the Vue dev server (`IdleDwarfsGame.Client`).
Run both in the background, write logs to the scratchpad, and wait for the ready lines.

## Gotchas
- `dotnet` is **not on PATH**. It lives at `~/.dotnet/dotnet`. Export `DOTNET_ROOT=$HOME/.dotnet PATH=$HOME/.dotnet:$PATH` first.
- The client calls the API at `https://localhost:7232` (src baseURL), so use the **`https`** launch profile, not `http`.
- Before starting, check that ports 7232/5015/8080 are free: `lsof -iTCP -sTCP:LISTEN -n -P | grep -E "7232|5015|8080"`.

## 1. Server
```bash
cd IdleDwarfsGame.Server
export DOTNET_ROOT=$HOME/.dotnet PATH=$HOME/.dotnet:$PATH
dotnet run --launch-profile https > <scratchpad>/server.log 2>&1   # run_in_background
```
Ready when `server.log` contains `Now listening on: https://localhost:7232` (also http://localhost:5015).

## 2. Client
```bash
cd IdleDwarfsGame.Client
npm run serve > <scratchpad>/client.log 2>&1   # run_in_background
```
Ready when `client.log` contains `App running at:` / `http://localhost:8080/`.
Sass "legacy-js-api" deprecation warnings are harmless.

## 3. Wait + smoke check
```bash
for i in $(seq 120); do
  grep -qE "Now listening.*7232|error|Unhandled" server.log && grep -qE "App running at|Failed to compile|ERROR" client.log && break
  sleep 1
done
curl -s  -o /dev/null -w "client %{http_code}\n" http://localhost:8080/    # expect 200
curl -sk -o /dev/null -w "server %{http_code}\n" https://localhost:7232/   # expect 302
open http://localhost:8080/
```

## Troubleshooting
- Browser network/certificate errors against 7232 → `~/.dotnet/dotnet dev-certs https --trust`.
- Exit code 127 from `dotnet run` → PATH export was missed.
