# Captura el Game view del Editor abierto y lo guarda como PNG fuera de Assets.
# Uso:
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools/Capture-GameView.ps1 -Name V020_carta
# Por defecto guarda en Docs/Capturas/<Name>.png (registro visual de cada version).
# Respeta la proporcion real del Game view (si es "Free Aspect" no tiene por que ser 16:9): solo se fija el ancho.
# No se usa --save_path de la CLI: lo resuelve dentro de Assets y Unity lo importaria como asset.
# Solo caracteres ASCII en este archivo: PowerShell 5.1 lee los .ps1 sin BOM como ANSI.

param(
    [Parameter(Mandatory = $true)][string]$Name,
    [string]$OutDir = '',
    [int]$Width = 1280,
    [ValidateSet('game_view', 'scene_view')][string]$View = 'game_view'
)

$root = Split-Path -Parent $PSScriptRoot
if (-not $OutDir) { $OutDir = Join-Path $root 'Docs\Capturas' }
$unity = Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
$project = Join-Path $root 'SajitariosGamespot'
$env:UNITY_NO_CONSENT_PROMPT = '1'
$env:UNITY_NO_PAGER = '1'

# Resolucion real con la que se renderiza el Game view
$probe = Join-Path $env:TEMP 'gamespot_gameview_size.cs'
Set-Content -Path $probe -Encoding ASCII -Value 'uint w, h; UnityEditor.PlayModeWindow.GetRenderingResolution(out w, out h); return w + "x" + h;'
$size = ((& $unity command script eval_file --file $probe --project-path $project --no-banner --json 2>&1 | Out-String) | ConvertFrom-Json).data.result.result
$Height = [int][Math]::Round($Width * 9 / 16)
if ($size -match '^(\d+)x(\d+)$' -and [int]$Matches[1] -gt 0) {
    $Height = [int][Math]::Round($Width * [int]$Matches[2] / [int]$Matches[1])
}

$raw = & $unity command capture $View --width $Width --height $Height --include_inline_image true `
    --project-path $project --no-banner --json 2>&1 | Out-String
$response = $raw | ConvertFrom-Json
if (-not $response.success) { throw "La captura ha fallado: $raw" }

# La imagen llega en base64 dentro del resultado; se busca el primer campo que lo contenga
$result = $response.data.result
$base64 = $null
foreach ($prop in $result.PSObject.Properties) {
    if ($prop.Value -is [string] -and $prop.Value.Length -gt 1000) { $base64 = $prop.Value; break }
}
if (-not $base64) { throw "La respuesta no trae imagen en linea: $($result | ConvertTo-Json -Compress)" }
if ($base64 -match '^data:image/\w+;base64,') { $base64 = $base64 -replace '^data:image/\w+;base64,', '' }

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$outPath = Join-Path $OutDir "$Name.png"
[System.IO.File]::WriteAllBytes($outPath, [Convert]::FromBase64String($base64))
Write-Output "Captura guardada: $outPath ($Width x $Height, Game view $size)"
