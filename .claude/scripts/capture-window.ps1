#requires -Version 5.1
<#
.SYNOPSIS
    Captures a top-level window (full window bounds, including non-client
    chrome) to a PNG.

.DESCRIPTION
    Used by the repro-and-fix skill to grab a snapshot of a running sample
    app (Avalonia/WPF/WinUI) so an agent can analyze the chart visually
    without a human in the loop.

    Window selection (in priority order):
      1. -WindowTitle <substring>  — first window whose title contains the substring
      2. -ProcessName <name>       — first MainWindow of a process matching the name
                                    (e.g. AvaloniaSample.Desktop, WPFSample, WinUISample)

    Captures the window's full bounds (including title bar) using PrintWindow,
    which works even when the window is occluded or off-screen — important
    because the agent may not have control over window stacking.

.PARAMETER WindowTitle
    Substring of the target window's title. Case-insensitive. First match wins.

.PARAMETER ProcessName
    Process name (without .exe). The MainWindowHandle of the first process
    matching is used.

.PARAMETER OutPath
    Destination PNG path. Parent directory must exist.

.PARAMETER WaitSeconds
    Optional seconds to wait for the window to appear before failing. Default 10.

.EXAMPLE
    .\capture-window.ps1 -ProcessName AvaloniaSample.Desktop -OutPath .\repro.png

.EXAMPLE
    .\capture-window.ps1 -WindowTitle "AvaloniaSample" -OutPath .\repro.png -WaitSeconds 20
#>
[CmdletBinding(DefaultParameterSetName = 'ByProcess')]
param(
    [Parameter(ParameterSetName = 'ByTitle',   Mandatory)] [string] $WindowTitle,
    [Parameter(ParameterSetName = 'ByProcess', Mandatory)] [string] $ProcessName,
    [Parameter(Mandatory)] [string] $OutPath,
    [int] $WaitSeconds = 10
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

# PrintWindow + GetWindowRect are the right primitives — they work for occluded
# or background windows, which CopyFromScreen cannot.
$signature = @'
using System;
using System.Runtime.InteropServices;

public static class WinApi
{
    [DllImport("user32.dll")]
    public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);
}
'@
if (-not ([System.Management.Automation.PSTypeName]'WinApi').Type) {
    Add-Type -TypeDefinition $signature -Language CSharp
}

function Resolve-WindowHandle {
    param([string]$Title, [string]$Process)

    if ($Title) {
        # The EnumWindows callback runs in the script scope, so use a single
        # script-scoped slot to communicate the result. Reset on every call so
        # we never return $null on a no-match or a stale handle from a prior
        # invocation — the polling loop relies on Zero meaning "not yet".
        $script:found = [IntPtr]::Zero
        $cb = [WinApi+EnumWindowsProc] {
            param($hWnd, $lParam)
            if (-not [WinApi]::IsWindowVisible($hWnd)) { return $true }
            $len = [WinApi]::GetWindowTextLength($hWnd)
            if ($len -le 0) { return $true }
            $sb = New-Object System.Text.StringBuilder ($len + 1)
            [void][WinApi]::GetWindowText($hWnd, $sb, $sb.Capacity)
            if ($sb.ToString() -like "*$Title*") {
                $script:found = $hWnd
                return $false
            }
            return $true
        }
        [void][WinApi]::EnumWindows($cb, [IntPtr]::Zero)
        return $script:found
    }

    if ($Process) {
        $p = Get-Process -Name $Process -ErrorAction SilentlyContinue |
             Where-Object { $_.MainWindowHandle -ne 0 } |
             Select-Object -First 1
        if ($p) { return $p.MainWindowHandle }
    }

    return [IntPtr]::Zero
}

# poll until the window appears.
$deadline = (Get-Date).AddSeconds($WaitSeconds)
$hWnd = [IntPtr]::Zero
while ((Get-Date) -lt $deadline) {
    $hWnd = Resolve-WindowHandle -Title $WindowTitle -Process $ProcessName
    if ($hWnd -ne [IntPtr]::Zero) { break }
    Start-Sleep -Milliseconds 250
}

if ($hWnd -eq [IntPtr]::Zero) {
    if ($WindowTitle) {
        Write-Error "No visible window with title containing '$WindowTitle' found within $WaitSeconds s."
    } else {
        Write-Error "No visible window for process '$ProcessName' found within $WaitSeconds s."
    }
    exit 1
}

$rect = New-Object WinApi+RECT
[void][WinApi]::GetWindowRect($hWnd, [ref]$rect)
$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top
if ($w -le 0 -or $h -le 0) {
    Write-Error "Window has zero dimensions ($w x $h); is it minimized?"
    exit 1
}

$bmp = New-Object System.Drawing.Bitmap $w, $h
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $gfx.GetHdc()
try {
    # PW_RENDERFULLCONTENT = 0x00000002 — needed for DirectComposition / WinUI
    $ok = [WinApi]::PrintWindow($hWnd, $hdc, 0x2)
    if (-not $ok) {
        Write-Error "PrintWindow returned false for HWND $hWnd."
        exit 1
    }
}
finally {
    $gfx.ReleaseHdc($hdc)
    $gfx.Dispose()
}

$outDir = Split-Path -Parent $OutPath
if ($outDir -and -not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}
$bmp.Save($OutPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Output "Captured $w x $h to $OutPath"
