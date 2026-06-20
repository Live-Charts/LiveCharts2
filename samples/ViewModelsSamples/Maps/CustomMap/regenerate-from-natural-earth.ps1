# Regenerates mexico-states.geojson from Natural Earth's 10m Admin-1
# dataset. Run if you need an updated source — the result is the
# committed mexico-states.geojson sibling file.
#
# Pipeline:
#   1. Download the global admin-1 file (~63MB) to a temp path (cached).
#   2. Filter to features with admin == 'Mexico' AND non-empty name +
#      iso_3166_2 (the source has one junk row missing both).
#   3. Reshape each feature to the LiveCharts2 GeoJSON schema —
#      properties = { name, shortName, setOf } — where shortName is
#      the ISO 3166-2 region code without the 'mx-' prefix, lowercased
#      (e.g. 'MX-AGU' -> 'agu').
#   4. Round coordinates to 3 decimals (~100m precision, same order as
#      the bundled world.geojson) to keep the file ~325 KB.
#
# Source repo:    https://github.com/martynafford/natural-earth-geojson
# License:        CC0-1.0 (public domain), derivative of Natural Earth Data
# Natural Earth:  https://www.naturalearthdata.com/

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$tmp = Join-Path $env:TEMP "ne_10m_admin_1_states_provinces.json"
$out = Join-Path $here "mexico-states.geojson"

if (-not (Test-Path $tmp)) {
    Write-Output "Downloading Natural Earth 10m Admin-1 (~63 MB) ..."
    Invoke-WebRequest `
        -Uri "https://raw.githubusercontent.com/martynafford/natural-earth-geojson/master/10m/cultural/ne_10m_admin_1_states_provinces.json" `
        -OutFile $tmp
}

Write-Output "Loading source file ..."
$data = Get-Content $tmp -Raw | ConvertFrom-Json -AsHashtable

$mx = @($data.features | Where-Object {
    $_.properties.admin -eq 'Mexico' -and $_.properties.name -and $_.properties.iso_3166_2
})
Write-Output "Filtered $($mx.Count) Mexican features"

function Roundify($coords) {
    if ($coords -is [double] -or $coords -is [decimal] -or $coords -is [long] -or $coords -is [int]) {
        return [Math]::Round([double]$coords, 3)
    }
    # Explicit List + .ToArray() — `@($x | ForEach-Object ...)` collapses
    # nested arrays back to flat ones, which corrupts the GeoJSON
    # polygon-of-rings-of-points-of-coords structure (4 levels deep for
    # MultiPolygon).
    $result = New-Object 'System.Collections.Generic.List[object]'
    foreach ($c in $coords) {
        $result.Add((Roundify $c))
    }
    # The leading comma is required: PowerShell unwraps single-element
    # array returns from functions ("if function returns [x], caller sees x"),
    # which loses the polygon level for any MultiPolygon containing a polygon
    # with exactly one ring. Wrapping in `, ...` returns the array as a
    # 1-element array containing the real array, defeating the unwrap.
    return , $result.ToArray()
}

$transformed = $mx | ForEach-Object {
    $short = ($_.properties.iso_3166_2 -replace '^MX-', '').ToLowerInvariant()
    @{
        type = 'Feature'
        properties = @{
            name = $_.properties.name
            shortName = $short
            setOf = 'mexico'
        }
        geometry = @{
            type = $_.geometry.type
            coordinates = Roundify $_.geometry.coordinates
        }
    }
}

$result = @{
    type = 'FeatureCollection'
    name = 'mexico-states'
    features = $transformed
}

$json = $result | ConvertTo-Json -Depth 100 -Compress
[System.IO.File]::WriteAllText($out, $json)

$sizeKB = [Math]::Round((Get-Item $out).Length / 1024)
Write-Output "Wrote $out ($sizeKB KB)"
