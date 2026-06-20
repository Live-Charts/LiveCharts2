#!/usr/bin/env bash
# Captures a top-level window of a running app to a PNG (Linux).
#
# Used by the repro-and-fix skill as a fallback when LVC_SCREENSHOT (in-app
# RenderTargetBitmap) isn't available. Linux is fragmented across X11 and
# Wayland with several compositors per session, so this script tries the
# tools in priority order and uses the first one that's installed:
#
#   X11:     `xdotool` + `import` (ImageMagick)
#   Wayland: `swaymsg` + `grim`   (sway)
#            `gnome-screenshot`   (GNOME, captures focused window — needs
#                                  the target window to be focused first)
#
# Usage:
#   capture-window-linux.sh <ProcessName> <OutPath> [WaitSeconds]
#
# Example:
#   capture-window-linux.sh AvaloniaSample.Desktop ./out.png
#
# If none of the supported tools are present, prints a hint with apt/dnf
# install commands and exits 1. If your environment uses a different
# compositor (Hyprland, KDE on Wayland, etc.) extend this script with the
# matching capture command.

set -euo pipefail

if [[ $# -lt 2 ]]; then
    echo "usage: $(basename "$0") <ProcessName> <OutPath> [WaitSeconds]" >&2
    exit 64
fi

proc_name="$1"
out_path="$2"
wait_seconds="${3:-10}"

mkdir -p "$(dirname "$out_path")"

is_wayland="${WAYLAND_DISPLAY:-}"

if [[ -z "$is_wayland" ]] && command -v xdotool >/dev/null && command -v import >/dev/null; then
    # X11 path: find a window owned by a process matching the name and capture it.
    deadline=$(( $(date +%s) + wait_seconds ))
    win=""
    while [[ $(date +%s) -lt $deadline ]]; do
        win=$(xdotool search --name "$proc_name" 2>/dev/null | head -1 || true)
        if [[ -z "$win" ]]; then
            # fall back to searching by class — many .NET apps don't set a
            # window title that matches the process name.
            win=$(xdotool search --class "$proc_name" 2>/dev/null | head -1 || true)
        fi
        if [[ -n "$win" ]]; then break; fi
        sleep 0.25
    done

    if [[ -z "$win" ]]; then
        echo "no X11 window matching '$proc_name' within ${wait_seconds}s" >&2
        exit 1
    fi

    import -window "$win" "$out_path"
    echo "captured X11 window $win of '$proc_name' to $out_path"
    exit 0
fi

if [[ -n "$is_wayland" ]] && command -v swaymsg >/dev/null && command -v grim >/dev/null && command -v jq >/dev/null; then
    # sway path: walk the tree, find the focused window matching the app_id.
    # `jq` is required to query swaymsg JSON; if it's missing, fall through to
    # gnome-screenshot (set -e would otherwise abort the whole script).
    deadline=$(( $(date +%s) + wait_seconds ))
    rect=""
    while [[ $(date +%s) -lt $deadline ]]; do
        rect=$(swaymsg -t get_tree |
            jq -r --arg name "$proc_name" \
                '.. | objects | select(.app_id != null and (.app_id | contains($name))) |
                "\(.rect.x),\(.rect.y) \(.rect.width)x\(.rect.height)"' |
            head -1 || true)
        if [[ -n "$rect" ]]; then break; fi
        sleep 0.25
    done

    if [[ -z "$rect" ]]; then
        echo "no sway window matching '$proc_name' within ${wait_seconds}s" >&2
        exit 1
    fi

    grim -g "$rect" "$out_path"
    echo "captured sway window of '$proc_name' to $out_path"
    exit 0
fi

if command -v gnome-screenshot >/dev/null; then
    echo "WARNING: falling back to gnome-screenshot --window — focus the target window manually" >&2
    sleep "$wait_seconds"
    gnome-screenshot --window --file="$out_path"
    echo "captured focused window to $out_path"
    exit 0
fi

cat >&2 <<EOF
no supported screenshot tool found. install one of:
  X11:     sudo apt install xdotool imagemagick
  sway:    sudo apt install grim jq                       (jq required for tree query)
  GNOME:   sudo apt install gnome-screenshot              (manual focus)
EOF
exit 1
