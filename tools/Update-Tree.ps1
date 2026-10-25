# Regenera TREE.md con el arbol del repo, sin las carpetas que generan Unity, Flutter y los IDE.
# Adaptado de AZUCAR (tools/Update-Tree.ps1).
# Uso (desde cualquier carpeta):
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools/Update-Tree.ps1
# Solo caracteres ASCII en este archivo: PowerShell 5.1 lee los .ps1 sin BOM como ANSI.

param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot),
    [string]$Output = ''
)

if (-not $Output) { $Output = Join-Path $Root 'TREE.md' }

# Carpetas que no se listan (generadas, cache o control de versiones)
$ExcludeDirs = @('Library', 'Temp', 'Logs', 'obj', 'UserSettings', '.git', '.vs', '.idea', '.vscode', '.utmp',
                 'Build', 'Builds', 'MemoryCaptures', 'Recordings', 'node_modules', '__pycache__', '.venv',
                 '.dart_tool', 'build', 'coverage')
# Carpetas que se excluyen por patron (la depuracion de Burst que deja cada build de Android junto al APK)
$ExcludeDirPatterns = @('*_BurstDebugInformation_DoNotShip')
# Carpetas que se muestran plegadas: solo su nombre y cuantos elementos tienen
# (Flutter = la app original en Legacy/Flutter, de solo lectura hasta cerrar la migracion)
$CollapseDirs = @('skills', 'TextMesh Pro', 'Flutter', 'Capturas')
# Archivos que no se listan
$ExcludeFiles = @('*.meta', '*.csproj', '*.sln', '*.slnx', '*.user', '*.pidb', '*.unityproj', '.DS_Store', 'Thumbs.db', '*.apk', '*.aab')

$Tee    = [string][char]0x251C + [char]0x2500 + [char]0x2500 + ' '
$Corner = [string][char]0x2514 + [char]0x2500 + [char]0x2500 + ' '
$Pipe   = [string][char]0x2502 + '   '
$Blank  = '    '

function Test-ExcludedFile([string]$name) {
    foreach ($pattern in $ExcludeFiles) { if ($name -like $pattern) { return $true } }
    return $false
}

function Test-ExcludedDirPattern([string]$name) {
    foreach ($pattern in $ExcludeDirPatterns) { if ($name -like $pattern) { return $true } }
    return $false
}

function Get-Children([string]$path) {
    $dirs  = Get-ChildItem -LiteralPath $path -Directory -Force -ErrorAction SilentlyContinue |
             Where-Object { $ExcludeDirs -notcontains $_.Name -and -not (Test-ExcludedDirPattern $_.Name) } | Sort-Object Name
    $files = Get-ChildItem -LiteralPath $path -File -Force -ErrorAction SilentlyContinue |
             Where-Object { -not (Test-ExcludedFile $_.Name) } | Sort-Object Name
    return @($dirs) + @($files)
}

function Write-Branch([string]$path, [string]$prefix, [System.Collections.Generic.List[string]]$lines) {
    $children = Get-Children $path
    for ($i = 0; $i -lt $children.Count; $i++) {
        $child = $children[$i]
        $isLast = ($i -eq $children.Count - 1)
        $connector = if ($isLast) { $Corner } else { $Tee }
        if ($child.PSIsContainer) {
            if ($CollapseDirs -contains $child.Name) {
                $count = @(Get-ChildItem -LiteralPath $child.FullName -Force -ErrorAction SilentlyContinue).Count
                $lines.Add("$prefix$connector$($child.Name)/  ($count elementos, plegado)")
                continue
            }
            $lines.Add("$prefix$connector$($child.Name)/")
            $nextPrefix = if ($isLast) { $prefix + $Blank } else { $prefix + $Pipe }
            Write-Branch $child.FullName $nextPrefix $lines
        } else {
            $lines.Add("$prefix$connector$($child.Name)")
        }
    }
}

$rootName = Split-Path -Leaf $Root
$tree = New-Object 'System.Collections.Generic.List[string]'
$tree.Add("$rootName/")
Write-Branch $Root '' $tree

$header = @(
    '# TREE',
    '',
    "Estructura del repo. Generado con ``tools/Update-Tree.ps1`` el $(Get-Date -Format 'yyyy-MM-dd HH:mm').",
    '',
    "No se listan: $($ExcludeDirs -join ', '), $($ExcludeFiles -join ', ').",
    "Plegadas: $($CollapseDirs -join ', ') (Flutter = la app original en Legacy/Flutter, solo lectura).",
    "Se regenera cada vez que se crea, mueve o borra algo; no se edita a mano (CLAUDE.md, seccion 1).",
    '',
    '```'
)
$footer = @('```', '')
$content = ($header + $tree + $footer) -join "`n"
[System.IO.File]::WriteAllText($Output, $content, (New-Object System.Text.UTF8Encoding($false)))
Write-Output "TREE.md actualizado: $($tree.Count) lineas -> $Output"
