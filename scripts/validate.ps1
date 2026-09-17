<#
Chaine de verification RRS (AD-4, AD-5, AD-7, AD-8, AD-9 -- architecture-RRS-devworkflow-2026-09-17).

unity status -> recompile -> recompile_status -> console --level error -> tests cibles -> list_open_scenes + git status
Echoue ferme a chaque etape (AD-8) : `status` != ready, CLI muet, commande inconnue, timeout, resultat
illisible ou absent = echec explicite. Un test en echec est un echec.

Usage :
  scripts\validate.ps1                                    # EditMode complet
  scripts\validate.ps1 -TestMode PlayMode                  # PlayMode complet
  scripts\validate.ps1 -TestMode Both
  scripts\validate.ps1 -TestFilter "RoadRage.Tests.EditMode.RoadRageScaffoldTests"
  scripts\validate.ps1 -Audit                              # + Project Auditor (ADDON-018), informatif, hors gate
#>
[CmdletBinding()]
param(
    [ValidateSet('EditMode', 'PlayMode', 'Both')]
    [string]$TestMode = 'EditMode',

    [string]$TestFilter = '',

    [ValidateSet('testName', 'assembly', 'category')]
    [string]$TestFilterType = 'testName',

    [int]$RecompileTimeoutSec = 120,
    [int]$TestTimeoutSec = 300,

    [switch]$Audit,
    [int]$AuditTimeoutSec = 300
)

$ErrorActionPreference = 'Stop'

# Reference figee dans AGENTS.md : version beta validee avec RRS, aucune montee automatique.
$ExpectedCliVersion = '1.0.0-beta.8'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Fail {
    param([string]$Message)
    Write-Host "ECHEC: $Message" -ForegroundColor Red
    exit 1
}

function Invoke-UnityJson {
    param([Parameter(Mandatory)][string[]]$CliArgs)

    $raw = & unity @CliArgs --format json 2>&1
    $exitCode = $LASTEXITCODE
    $rawText = ($raw | Out-String).Trim()

    if ($exitCode -ne 0 -or [string]::IsNullOrWhiteSpace($rawText)) {
        Fail "unity $($CliArgs -join ' ') : commande muette ou code de sortie $exitCode (AD-8)"
    }

    try {
        $parsed = $rawText | ConvertFrom-Json
    }
    catch {
        Fail "unity $($CliArgs -join ' ') : sortie JSON illisible (AD-8)`n$rawText"
    }

    if (-not $parsed.success) {
        $errText = if ($parsed.errors) { $parsed.errors -join '; ' } else { 'raison inconnue' }
        Fail "unity $($CliArgs -join ' ') : $errText"
    }

    return $parsed
}

function Get-CmdResult {
    param($Parsed)

    $result = $Parsed.data.result
    if ($null -eq $result) {
        Fail "resultat de commande absent (AD-8)"
    }
    if ($result -is [string]) {
        try {
            return $result | ConvertFrom-Json
        }
        catch {
            Fail "resultat de commande illisible (AD-8) : $result"
        }
    }
    return $result
}

# --- Etape 0 : version CLI figee (AGENTS.md, non revalidee automatiquement) ---
Write-Step "Version Unity CLI"
$cliVersion = (& unity --version 2>&1 | Out-String).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($cliVersion)) {
    Fail "unity --version : CLI introuvable ou muet (AD-8)"
}
if ($cliVersion -ne $ExpectedCliVersion) {
    Fail "Unity CLI $cliVersion non revalidee pour RRS (reference : $ExpectedCliVersion). Revalider avant de faire confiance a ce script."
}
Write-Host "  $cliVersion (reference validee)" -ForegroundColor DarkGray

# --- Etape 1 : unity status -> ready pour ce projet ---
Write-Step "unity status"
$statusParsed = Invoke-UnityJson -CliArgs @('status')
$instance = $statusParsed.data.instances | Where-Object {
    (Resolve-Path $_.project -ErrorAction SilentlyContinue).Path -eq $RepoRoot
} | Select-Object -First 1

if (-not $instance) {
    Fail "Aucune instance Unity connectee sur $RepoRoot. L'Editeur doit etre ouvert (AD-5, jamais de batchmode)."
}
if ($instance.state -ne 'ready') {
    Fail "Etat Unity '$($instance.state)' != ready sur le port $($instance.port) (AD-8)."
}
Write-Host "  port $($instance.port), Unity $($instance.version), etat ready" -ForegroundColor DarkGray

# --- Etape 2 : recompile ---
Write-Step "unity cmd recompile"
Invoke-UnityJson -CliArgs @('cmd', 'recompile') | Out-Null

# --- Etape 3 : recompile_status (poll jusqu'a etat terminal) ---
Write-Step "unity cmd recompile_status"
$deadline = (Get-Date).AddSeconds($RecompileTimeoutSec)
$recompileStatus = $null
do {
    Start-Sleep -Seconds 1
    $recompileStatus = (Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'recompile_status'))).status
    if ([string]::IsNullOrWhiteSpace($recompileStatus)) {
        Fail "recompile_status illisible (AD-8)"
    }
} while ($recompileStatus -in @('triggered', 'compiling') -and (Get-Date) -lt $deadline)

if ($recompileStatus -notin @('completed', 'up_to_date', 'idle')) {
    Fail "recompile_status bloque sur '$recompileStatus' apres ${RecompileTimeoutSec}s (AD-8)."
}
Write-Host "  recompile_status: $recompileStatus" -ForegroundColor DarkGray

# --- Etape 4 : console --level error (gate) + warnings hors Assets/Synty (info, AD-7) ---
Write-Step "unity cmd console --level error"
$errorEntries = (Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'console', '--level', 'error'))).entries
if ($errorEntries -and $errorEntries.Count -gt 0) {
    foreach ($entry in $errorEntries) {
        Write-Host "  [ERROR] $($entry.message)" -ForegroundColor Red
    }
    Fail "$($errorEntries.Count) erreur(s) en Console (AD-7)."
}
Write-Host "  0 erreur" -ForegroundColor DarkGray

$warningEntries = (Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'console', '--level', 'warning'))).entries
$relevantWarnings = @($warningEntries | Where-Object { $_.message -notlike '*Assets/Synty/*' })
Write-Host "  $($relevantWarnings.Count) avertissement(s) hors Assets/Synty/ (informatif, ne bloque pas)" -ForegroundColor DarkGray
foreach ($entry in $relevantWarnings) {
    Write-Host "  [WARN] $($entry.message)" -ForegroundColor Yellow
}

# --- Etape 5 : tests cibles (AD-9) ---
$modes = if ($TestMode -eq 'Both') { @('EditMode', 'PlayMode') } else { @($TestMode) }
foreach ($mode in $modes) {
    Write-Step "unity cmd run_tests --mode $mode"
    $runArgs = @('cmd', 'run_tests', '--mode', $mode, '--async_tests', 'true')
    if ($TestFilter) {
        $runArgs += @('--filter', $TestFilter, '--filter_type', $TestFilterType)
    }
    Invoke-UnityJson -CliArgs $runArgs | Out-Null

    Write-Step "unity cmd test_status ($mode)"
    $deadline = (Get-Date).AddSeconds($TestTimeoutSec)
    $testStatus = $null
    do {
        Start-Sleep -Seconds 2
        $testStatus = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'test_status'))
        if (-not $testStatus -or -not $testStatus.status) {
            Fail "test_status illisible (AD-8)"
        }
    } while ($testStatus.status -notin @('completed', 'failed', 'error') -and (Get-Date) -lt $deadline)

    if ($testStatus.status -ne 'completed') {
        Fail "test_status bloque sur '$($testStatus.status)' apres ${TestTimeoutSec}s (AD-8)."
    }
    if (-not $testStatus.summary -or $testStatus.summary.total -eq 0) {
        Fail "Aucun test execute en $mode (filtre '$TestFilter' sans correspondance ?). Une absence de resultat n'est jamais un succes (AD-8)."
    }
    if ($testStatus.summary.failed -gt 0) {
        foreach ($result in ($testStatus.results | Where-Object { $_.Status -ne 'Passed' })) {
            Write-Host "  [FAIL] $($result.FullName) : $($result.Message)" -ForegroundColor Red
        }
        Fail "$($testStatus.summary.failed)/$($testStatus.summary.total) test(s) $mode en echec."
    }
    Write-Host "  $($testStatus.summary.passed)/$($testStatus.summary.total) test(s) $mode passes" -ForegroundColor DarkGray
}

# --- Etape 6 : list_open_scenes + git status (etat final, garde-fou AGENTS.md) ---
Write-Step "unity cmd list_open_scenes + git status"
$scenes = (Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'list_open_scenes'))).scenes
foreach ($scene in $scenes) {
    $dirtyTag = if ($scene.isDirty) { '[isDirty=true]' } else { '[propre]' }
    Write-Host "  $($scene.name) $dirtyTag" -ForegroundColor DarkGray
}
$dirtyScenes = @($scenes | Where-Object { $_.isDirty })
if ($dirtyScenes.Count -gt 0) {
    Write-Host "  ATTENTION : scene(s) isDirty=true -- ne pas sauvegarder sans expliquer l'etat (garde-fou AGENTS.md)." -ForegroundColor Yellow
}

$gitStatus = & git -C $RepoRoot status --short
if ($gitStatus) {
    Write-Host "  git status --short :" -ForegroundColor DarkGray
    $gitStatus | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
}
else {
    Write-Host "  git status --short : propre" -ForegroundColor DarkGray
}

# --- Etape optionnelle : Project Auditor (ADDON-018) ---
# Hors gate volontairement : ~2-3 min sur ce projet, signal domine par le bruit (tests, chemins
# editor-only, allocations generiques hors boucle chaude) -- lecture humaine requise, jamais un
# echec automatique. Voir docs/setup/audit-project-auditor-baseline.md pour la mesure de reference.
if ($Audit) {
    Write-Step "unity cmd audit (ADDON-018, informatif, hors gate)"
    $trigger = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'audit'))
    Write-Host "  scan $($trigger.scanId) declenche" -ForegroundColor DarkGray

    $deadline = (Get-Date).AddSeconds($AuditTimeoutSec)
    $auditState = $null
    do {
        Start-Sleep -Seconds 5
        $auditState = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'audit_status'))
    } while ($auditState.status -eq 'scanning' -and (Get-Date) -lt $deadline)

    if ($auditState.status -eq 'completed') {
        Write-Host "  $($auditState.issueCount) diagnostic(s) -- CSV : $($auditState.csvPath)" -ForegroundColor DarkGray
        Write-Host "  Triage manuel recommande : filtrer Assets/Synty/ et Tests/, ~1-2% du volume brut est reellement actionnable (ADDON-018)." -ForegroundColor DarkGray
    }
    else {
        Write-Host "  Project Auditor non conclusif : statut '$($auditState.status)' (non bloquant, ADDON-018)." -ForegroundColor Yellow
    }
}

Write-Host "OK" -ForegroundColor Green
exit 0
