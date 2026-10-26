#!/usr/bin/env bash
# Build and drop the DLL into the dev profile's ScriptEngine folder: the running game reloads it about a second later
# (all scripts reload together). The .pdb goes first (ScriptEngine reads symbols), the DLL by temp name + rename so the
# watcher never reads a half-written file.
set -euo pipefail
echo "Campfire is built into Jatify UI now: run ~/projects/JatifyUI/tools/dev.sh (a standalone copy here would switch Jatify's off)"; exit 1
R="$(cd "$(dirname "$0")/.." && pwd)"
VALHEIM="${VALHEIM:-/mnt/games/SteamLibrary/steamapps/common/Valheim}"
PROFILE="${PROFILE:-$HOME/.local/share/com.kesomannen.gale/valheim/profiles/jatify-ui-dev}"
DEST="$PROFILE/BepInEx/scripts"
dotnet build "$R/Campfire.csproj" -c Release -v minimal -o "$R/build" -p:ValheimDir="$VALHEIM" -p:ProfileDir="$PROFILE" | grep -E "error|warn|Build succeeded" | sort -u
mkdir -p "$DEST"
cp "$R/build/Campfire.pdb" "$DEST/.Campfire.pdb.tmp" && mv -f "$DEST/.Campfire.pdb.tmp" "$DEST/Campfire.pdb"
cp "$R/build/Campfire.dll" "$DEST/.Campfire.tmp" && mv -f "$DEST/.Campfire.tmp" "$DEST/Campfire.dll"
mkdir -p "$PROFILE/BepInEx/plugins/campfire-loading" && cp -u "$R/package/loading/"*.jpg "$PROFILE/BepInEx/plugins/campfire-loading/"   # the loading screen art (read from disk, not reloaded)
echo "dropped into $DEST (reloads in-game if the dev profile is running)"
