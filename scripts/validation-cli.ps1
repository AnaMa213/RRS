<#
# Lecture tolerante du statut de tests pendant un rechargement de domaine.
#
# Dot-source depuis scripts/validate.ps1 :
#   . (Join-Path $PSScriptRoot 'validation-cli.ps1')
#
# Fournit :
#   Invoke-TestStatusQuery        -- une lecture de `unity cmd test_status`, retentee sur indisponibilite passagere
#   Invoke-TestStatusQuerySelfTest -- simulations controlees, sans Editeur (exit 0/1)
#
# Contexte (decision proprietaire du 2026-10-03) : le projet garde le Domain Reload a l'entree en Play Mode
# (ProjectSettings/EditorSettings.asset, m_EnterPlayModeOptions: 2) pour relancer les runs PlayMode sans redemarrer
# l'Editeur. Pendant ce rechargement, le CLI ne repond pas : sortie vide, ou code de sortie 6. Comme les requetes Console
# (Invoke-ConsoleQuery de validate.ps1), la lecture du statut tolere ce silence un nombre borne d'essais, puis echoue
# ferme (AD-8).
#
# Politique :
#   - retentee : sortie vide (aucune reponse), ou code de sortie 6 (CLI temporairement indisponible) ;
#   - immediatement bloquante : tout autre code non nul, une sortie illisible, un refus logique de l'Editeur
#     (success:false avec code 0) ;
#   - un statut de test reel (completed, failed, error, running...) est rendu tel quel : son interpretation reste celle
#     de validate.ps1, inchangee ;
#   - apres epuisement des essais : echec avec diagnostic (nombre d'essais, dernier code, dernier texte).
#>

[CmdletBinding()]
param([switch]$SelfTest)

# Code de sortie du CLI Unity rendu quand l'Editeur ne traite pas la commande (rechargement de domaine en cours).
$script:CliUnavailableExitCode = 6

function Invoke-TestStatusQuery {
    <#
    .SYNOPSIS
    Une lecture du statut de tests. -Invoker rend un objet { ExitCode, Text } (la commande reelle, ou une simulation).
    Rend @{ Ok; Parsed; Message; Attempts; Retries }. Ok faux : Message porte le diagnostic, l'appelant echoue ferme.
    #>
    param(
        [Parameter(Mandatory)][scriptblock]$Invoker,
        [int]$Attempts = 5,
        [int]$DelaySec = 2,
        [scriptblock]$Sleep = { param($seconds) Start-Sleep -Seconds $seconds }
    )

    $lastCode = $null
    $lastText = ''
    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        $response = & $Invoker
        $lastCode = $response.ExitCode
        $lastText = ([string]$response.Text).Trim()
        $unavailable = [string]::IsNullOrWhiteSpace($lastText) -or $lastCode -eq $script:CliUnavailableExitCode

        if (-not $unavailable) {
            if ($lastCode -ne 0) {
                return @{ Ok = $false; Attempts = $attempt; Retries = $attempt - 1
                    Message = "code de sortie $lastCode non reconnu (aucune nouvelle tentative)`n$lastText" }
            }
            $parsed = $null
            try { $parsed = $lastText | ConvertFrom-Json } catch { $parsed = $null }
            if ($null -eq $parsed) {
                return @{ Ok = $false; Attempts = $attempt; Retries = $attempt - 1; Message = "sortie JSON illisible`n$lastText" }
            }
            if (-not $parsed.success) {
                $errText = if ($parsed.errors) { $parsed.errors -join '; ' } else { 'raison inconnue' }
                return @{ Ok = $false; Attempts = $attempt; Retries = $attempt - 1; Message = "refus de l'Editeur : $errText" }
            }
            return @{ Ok = $true; Parsed = $parsed; Attempts = $attempt; Retries = $attempt - 1; Message = '' }
        }

        if ($attempt -lt $Attempts) { & $Sleep $DelaySec }
    }

    $shown = if ([string]::IsNullOrWhiteSpace($lastText)) { 'aucune reponse' } else { $lastText }
    return @{ Ok = $false; Attempts = $Attempts; Retries = $Attempts - 1
        Message = "CLI indisponible apres $Attempts essais (dernier code de sortie $lastCode : $shown). Rechargement de domaine qui ne se termine pas, ou Editeur bloque." }
}

function Invoke-TestStatusQuerySelfTest {
    <#
    .SYNOPSIS
    Simulations controlees de Invoke-TestStatusQuery : chaque cas est une suite de reponses du CLI et le verdict attendu.
    Aucune dependance a l'Editeur ; aucune attente reelle.
    #>
    $mute = @{ ExitCode = 0; Text = '' }
    $down = @{ ExitCode = 6; Text = '{"success": false, "command": "unity command test_status", "data": null, "errors": ["timeout"]}' }
    $running = @{ ExitCode = 0; Text = '{"success": true, "data": {"result": {"status": "running"}}}' }
    $completed = @{ ExitCode = 0; Text = '{"success": true, "data": {"result": {"status": "completed", "summary": {"total": 4, "passed": 4}}}}' }
    $failedRun = @{ ExitCode = 0; Text = '{"success": true, "data": {"result": {"status": "failed", "summary": {"total": 4, "failed": 1}}}}' }
    $refused = @{ ExitCode = 0; Text = '{"success": false, "errors": ["unknown command"]}' }
    $unknownCode = @{ ExitCode = 3; Text = 'erreur de transport' }
    $garbled = @{ ExitCode = 0; Text = 'pas du JSON' }

    $cases = @(
        @{ Name = 'aucune reponse -> aucune reponse -> statut valide'; Responses = @($mute, $mute, $running); Ok = $true; Calls = 3 },
        @{ Name = 'aucune reponse x 5 -> echec ferme'; Responses = @($mute, $mute, $mute, $mute, $mute, $completed); Ok = $false; Calls = 5 },
        @{ Name = 'code 6 -> code 6 -> statut termine'; Responses = @($down, $down, $completed); Ok = $true; Calls = 3 },
        @{ Name = 'code 6 x 5 -> echec ferme'; Responses = @($down, $down, $down, $down, $down); Ok = $false; Calls = 5 },
        @{ Name = 'melange silence et code 6 puis statut'; Responses = @($down, $mute, $down, $mute, $running); Ok = $true; Calls = 5 },
        @{ Name = 'statut failed rendu tel quel (jamais retente)'; Responses = @($failedRun, $completed); Ok = $true; Calls = 1; Status = 'failed' },
        @{ Name = 'code non reconnu -> bloquant immediatement'; Responses = @($unknownCode, $completed); Ok = $false; Calls = 1 },
        @{ Name = 'refus logique (success:false, code 0) -> bloquant'; Responses = @($refused, $completed); Ok = $false; Calls = 1 },
        @{ Name = 'sortie illisible -> bloquante'; Responses = @($garbled, $completed); Ok = $false; Calls = 1 },
        @{ Name = 'silence puis code non reconnu -> bloquant'; Responses = @($mute, $unknownCode, $completed); Ok = $false; Calls = 2 }
    )

    $failed = 0
    foreach ($case in $cases) {
        $queue = [System.Collections.Generic.Queue[object]]::new()
        foreach ($response in $case.Responses) { $queue.Enqueue($response) }
        $calls = [ref]0
        $sleeps = [ref]0
        $outcome = Invoke-TestStatusQuery -Attempts 5 -DelaySec 2 `
            -Invoker { $calls.Value++; $next = $queue.Dequeue(); [pscustomobject]@{ ExitCode = $next.ExitCode; Text = $next.Text } }.GetNewClosure() `
            -Sleep { param($seconds) $sleeps.Value++ }.GetNewClosure()
        $ok = $outcome.Ok -eq $case.Ok -and $calls.Value -eq $case.Calls
        if ($ok -and $case.Status) { $ok = $outcome.Parsed.data.result.status -eq $case.Status }
        if ($ok -and -not $outcome.Ok) { $ok = -not [string]::IsNullOrWhiteSpace($outcome.Message) }
        if ($ok) {
            Write-Host ("[PASS] {0} -> Ok={1}, {2} appel(s), {3} attente(s)" -f $case.Name, $outcome.Ok, $calls.Value, $sleeps.Value)
        }
        else {
            $failed++
            Write-Host ("[FAIL] {0} -> Ok={1}, {2} appel(s) (attendu Ok={3}, {4} appel(s)) {5}" -f $case.Name, $outcome.Ok, $calls.Value,
                $case.Ok, $case.Calls, $outcome.Message)
        }
    }
    Write-Host ("--- SelfTest : {0}/{1} cas conformes ---" -f ($cases.Count - $failed), $cases.Count)
    return $failed
}

if ($SelfTest) {
    $failures = Invoke-TestStatusQuerySelfTest
    exit ([int]($failures -gt 0))
}
