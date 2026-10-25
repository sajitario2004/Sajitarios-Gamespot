# Funciones auxiliares para trabajar con el Editor abierto mediante la CLI `unity` (Pipeline).
# Adaptado de AZUCAR (C:\Users\nacha\Documents\DEV\Unity\AzucarClaudio\tools\UnityCli.ps1).
# Uso (cargar en cada llamada de PowerShell, desde la raiz del repo):
#   . tools/UnityCli.ps1
#   U editor status                        # comando -> objeto JSON (data.result)
#   E C:\ruta\snippet.cs                   # script eval_file -> texto devuelto por el snippet
#   Sync-Unity                             # AssetDatabase.Refresh + recompilar + esperar
#   Get-UnityConsole [-Tail 10]            # recuento de la consola (y ultimas entradas)
#   Invoke-UnityTests EditMode|PlayMode    # ejecuta los tests del proyecto y resume
#   Start-UnityPlay                        # entra en Play y hace que el juego avance aunque el Editor no tenga foco
#   Test-UnityPlay                         # entra en Play, mira la consola y sale
# Solo caracteres ASCII en este archivo: PowerShell 5.1 lee los .ps1 sin BOM como ANSI.

$script:UnityExe = Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
$script:UnityProject = Join-Path (Split-Path -Parent $PSScriptRoot) 'SajitariosGamespot'
$env:UNITY_NO_CONSENT_PROMPT = '1'
$env:UNITY_NO_PAGER = '1'

function U {
    $raw = & $script:UnityExe command @args --project-path $script:UnityProject --no-banner --json 2>&1 | Out-String
    try { $json = $raw | ConvertFrom-Json } catch { throw "Respuesta no valida de la CLI: $raw" }
    if (-not $json.success) { Write-Warning ("Fallo en '" + ($args -join ' ') + "': " + ($json.errors | ConvertTo-Json -Compress -Depth 4)) }
    return $json.data.result
}

function E([string]$File) {
    $result = U script eval_file --file $File
    if ($null -eq $result) { return $null }
    if (-not $result.success) { return ("EVAL ERROR: " + ($result | ConvertTo-Json -Depth 6 -Compress)) }
    return $result.result
}

function Wait-UnityCompile([int]$TimeoutSeconds = 120) {
    # Durante la recarga de dominio la CLI no encuentra el Editor: se reintenta sin avisar
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 1500
        $raw = & $script:UnityExe command script recompile_status --project-path $script:UnityProject --no-banner --json 2>&1 | Out-String
        try { $state = ($raw | ConvertFrom-Json).data.result.status } catch { $state = 'reloading' }
        if ($state -in @('completed', 'up_to_date')) { return $state }
    }
    return 'timeout'
}

function Wait-UnityReady([int]$TimeoutSeconds = 120) {
    # Tras una recarga larga la CLI tarda en volver a ver el Editor: se espera sin avisar
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        $raw = & $script:UnityExe command editor status --project-path $script:UnityProject --no-banner --json 2>&1 | Out-String
        try { $state = ($raw | ConvertFrom-Json).data.result.status } catch { $state = $null }
        if ($state -eq 'ready') { return $true }
        Start-Sleep -Seconds 2
    }
    return $false
}

function Sync-Unity {
    $refresh = Join-Path $env:TEMP 'gamespot_refresh.cs'
    Set-Content -Path $refresh -Value 'UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport); return "ok";' -Encoding ASCII
    E $refresh | Out-Null
    U script recompile | Out-Null
    $state = Wait-UnityCompile
    Wait-UnityReady | Out-Null
    $console = Get-UnityConsole
    return "recompilacion=$state errores=$($console.Errors) avisos=$($console.Warnings) compilacionFallida=$($console.CompilationFailed)"
}

function Get-UnityConsole([int]$Tail = 0) {
    $truth = (U console status).groundTruth
    $summary = [pscustomobject]@{ Errors = $truth.consoleErrors; Warnings = $truth.consoleWarnings; CompilationFailed = $truth.compilationFailed }
    if ($Tail -gt 0) {
        (U console get --tail $Tail).entries | ForEach-Object {
            $msg = "$($_.message)"
            Write-Output ("[" + $_.level + "] " + $msg.Substring(0, [Math]::Min(400, $msg.Length)))
        }
    }
    return $summary
}

function Invoke-UnityTests([ValidateSet('EditMode', 'PlayMode')][string]$Mode) {
    $assembly = "Gamespot.Tests.$Mode"
    if ($Mode -eq 'EditMode') {
        $result = U test run --mode EditMode --filter $assembly --filter_type assembly --timeout 300
        $summary = $result.Summary
        $failed = $result.Results | Where-Object { $_.Status -ne 'Passed' }
    } else {
        U test run --mode PlayMode --filter $assembly --filter_type assembly --async_tests true | Out-Null
        $status = $null
        for ($i = 0; $i -lt 300; $i++) {
            Start-Sleep -Seconds 2
            try { $status = U test status } catch { $status = $null }
            if ($status -and $status.status -eq 'completed') { break }
        }
        $summary = $status.summary
        $failed = $status.results | Where-Object { $_.Status -ne 'Passed' }
    }
    Write-Output ("$Mode -> " + ($summary | ConvertTo-Json -Compress))
    foreach ($f in $failed) {
        $msg = "$($f.Message)"
        Write-Output ("  FALLA " + $f.FullName + " :: " + $msg.Substring(0, [Math]::Min(300, $msg.Length)))
    }
}

# Una sola clase de tests PlayMode (por ejemplo ImpostorFlowTests), con el mismo resumen que Invoke-UnityTests
function Invoke-UnityPlayClass([string]$Class) {
    U test run --mode PlayMode --filter "Gamespot.Tests.$Class" --filter_type testName --async_tests true | Out-Null
    $status = $null
    for ($i = 0; $i -lt 300; $i++) {
        Start-Sleep -Seconds 2
        try { $status = U test status } catch { $status = $null }
        if ($status -and $status.status -eq 'completed') { break }
    }
    Write-Output ("$Class -> " + ($status.summary | ConvertTo-Json -Compress))
    foreach ($f in ($status.results | Where-Object { $_.Status -ne 'Passed' })) {
        $msg = "$($f.Message)"
        Write-Output ("  FALLA " + $f.FullName + " :: " + $msg.Substring(0, [Math]::Min(300, $msg.Length)))
    }
}

function Start-UnityPlay {
    # Con el Editor sin foco y "Run In Background" apagado (ajuste de las builds), el bucle del juego no avanza.
    # Se activa solo durante esta sesion de Play. Ojo: en el Editor, Application.runInBackground cambia tambien
    # PlayerSettings.runInBackground y una build lo guardaria. Se sale siempre con Stop-UnityPlay, que lo apaga.
    U editor.playmode start | Out-Null
    Start-Sleep -Seconds 2
    $bg = Join-Path $env:TEMP 'gamespot_run_in_background.cs'
    Set-Content -Path $bg -Encoding ASCII -Value 'UnityEngine.Application.runInBackground = true; return "ok";'
    E $bg | Out-Null
}

function Stop-UnityPlay {
    # Sale de Play y deja "Run In Background" apagado, como en las builds
    U editor.playmode stop | Out-Null
    Start-Sleep -Seconds 2
    $bg = Join-Path $env:TEMP 'gamespot_run_in_background_off.cs'
    Set-Content -Path $bg -Encoding ASCII -Value 'UnityEditor.PlayerSettings.runInBackground = false; return "ok";'
    E $bg | Out-Null
}

function Test-UnityPlay([int]$Seconds = 4) {
    Start-UnityPlay
    Start-Sleep -Seconds $Seconds
    $playing = (U editor status).playMode
    $during = Get-UnityConsole
    Stop-UnityPlay
    Start-Sleep -Seconds 1
    $after = (U editor status).playMode
    return "play=$playing errores=$($during.Errors) avisos=$($during.Warnings) final=$after"
}
