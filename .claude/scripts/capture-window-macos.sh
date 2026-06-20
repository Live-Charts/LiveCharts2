#!/usr/bin/env bash
# Captures a top-level window of a running app to a PNG (macOS).
#
# Used by the repro-and-fix skill as a fallback when LVC_SCREENSHOT (in-app
# RenderTargetBitmap) isn't available — primarily for MAUI and Uno on macOS,
# where the framework lacks a unified render-to-bitmap surface.
#
# Window selection is by process name (the same name shown in Activity
# Monitor), looked up via System Events / osascript. The frontmost window of
# the matching process is captured. If multiple windows are open, focus the
# right one before running the script.
#
# Usage:
#   capture-window-macos.sh <ProcessName> <OutPath> [WaitSeconds]
#
# Example:
#   capture-window-macos.sh MauiSample ./.claude/screenshots/before.png
#   capture-window-macos.sh "AvaloniaSample.Desktop" ./out.png 20
#
# Requires: macOS (uses `screencapture` and `osascript`, both shipped with
# the OS). Will fail if the process isn't running or has no visible window
# within the wait timeout.

set -euo pipefail

if [[ $# -lt 2 ]]; then
    echo "usage: $(basename "$0") <ProcessName> <OutPath> [WaitSeconds]" >&2
    exit 64
fi

proc_name="$1"
out_path="$2"
wait_seconds="${3:-10}"

mkdir -p "$(dirname "$out_path")"

# Poll for the process' frontmost window ID. screencapture -l takes the
# numeric window id, which we can fetch via System Events.
deadline=$(( $(date +%s) + wait_seconds ))
window_id=""
while [[ $(date +%s) -lt $deadline ]]; do
    set +e
    window_id=$(osascript <<EOF 2>/dev/null
tell application "System Events"
    if exists application process "$proc_name" then
        try
            return id of front window of (first process whose name is "$proc_name")
        end try
    end if
end tell
EOF
    )
    set -e
    if [[ -n "$window_id" && "$window_id" != "missing value" ]]; then
        break
    fi
    sleep 0.25
done

if [[ -z "$window_id" || "$window_id" == "missing value" ]]; then
    echo "no visible window for process '$proc_name' within ${wait_seconds}s" >&2
    exit 1
fi

# -o omits the drop-shadow; -t png is explicit format; -l takes the window id.
screencapture -o -t png -l "$window_id" "$out_path"
echo "captured window $window_id of '$proc_name' to $out_path"
