<#
# Chaine de verification RRS (AD-5, AD-7, AD-8, AD-9 -- architecture-RRS-devworkflow-2026-09-17).
# AD-4 (verification declenchee par l'utilisateur) abrogee le 2026-09-18 : l'agent execute ce script
# directement.
#
# unity status -> stabilisation -> curseur Console -> recompile -> recompile_status -> console --since
# -> tests cibles -> console --since -> list_open_scenes + git status
#
# AD-7 ne porte que sur la FENETRE DE VALIDATION : le curseur Console est capture a l'ouverture
# (editeur stabilise, avant `recompile`) et seules les entrees de sequence STRICTEMENT superieures a ce
# curseur comptent. Une erreur de compilation d'un etat intermediaire deja corrige, ou une erreur
# laissee par une commande CLI rejetee, ne rougit donc plus la porte pour le reste de la session
# d'Editeur ; une erreur produite pendant la fenetre (recompilation de ce run, tests cibles, sortie de
# Play Mode) la rougit toujours. Rien ne depend de `clear_console`, qui ne vide pas la memoire tampon
# du pipeline.
#
# La fenetre rend muettes les erreurs ANTERIEURES, pas un build encore casse : `recompile_status`
# rapporte la derniere DEMANDE et un `recompile` sans changement l'ecrase (`failed:true` devient
# `up_to_date, failed:false`, mesure le 2026-09-23). L'etat courant est donc lu a sa source,
# `EditorUtility.scriptCompilationFailed` : vrai tant que la compilation des scripts est en echec, il
# reste bloquant quel que soit l'age de l'entree Console correspondante.
#
# Echoue ferme a chaque etape (AD-8) : `status` != ready, CLI muet, commande inconnue, timeout, resultat
# illisible ou absent = echec explicite. Un test en echec est un echec.

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

# Recapitulatif collecte au fil des etapes et affiche en fin de course (resume lisible exige par
# docs/setup/build-workflow-rules.md). Aucune valeur de ce recapitulatif ne participe a une decision
# de gate : les gates restent dans les etapes elles-memes (AD-8).
$script:Summary = [ordered]@{}

function Write-Step {
    param([string]$Message)
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Summary {
    if (-not $script:Summary -or $script:Summary.Count -eq 0) {
        return
    }
    Write-Host ""
    Write-Host "---- Recapitulatif de validation ----" -ForegroundColor Cyan
    foreach ($key in $script:Summary.Keys) {
        Write-Host ("  {0,-24} {1}" -f $key, $script:Summary[$key])
    }
    Write-Host "-------------------------------------" -ForegroundColor Cyan
}

function Fail {
    param([string]$Message)
    Write-Host "ECHEC: $Message" -ForegroundColor Red
    Write-Summary
    Write-Host "VALIDATION FAILED / INCOMPLETE" -ForegroundColor Red
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

function Wait-EditorSettled {
    # Attend la fin d'une compilation ou d'un rechargement de domaine avant d'ouvrir ou d'interroger la
    # fenetre de validation. `editor_status` est main-thread requis : pendant un rechargement il peut ne
    # pas repondre, et c'est l'etat a attendre, pas un echec. Budget borne, puis echec ferme (AD-8) : un
    # editeur qui ne se stabilise pas ne valide rien.
    param(
        [Parameter(Mandatory)][string]$Reason,
        [Parameter(Mandatory)][int]$TimeoutSec
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    do {
        $state = $null
        $raw = & unity cmd editor_status --format json 2>&1
        if ($LASTEXITCODE -eq 0) {
            try {
                $state = ((($raw | Out-String).Trim()) | ConvertFrom-Json).data.result
            }
            catch {
                $state = $null
            }
        }
        # Egalite explicite a $false : un champ absent n'est pas une stabilisation (fail-closed).
        if ($state -and $state.compiling -eq $false -and $state.domainReloadInProgress -eq $false) {
            Write-Host "  editeur stabilise (compiling=false, domainReloadInProgress=false)" -ForegroundColor DarkGray
            return
        }
        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    Fail "Editeur toujours en compilation ou rechargement de domaine apres ${TimeoutSec}s ($Reason, AD-8)."
}

function Invoke-ConsoleQuery {
    # Requete Console de la fenetre de validation. La doc du package pipeline demande au client de
    # tolerer les erreurs de connexion pendant un rechargement de domaine ("The client must tolerate
    # connection errors during a domain reload (recompile, target switch)") : une reponse muette ou
    # illisible est retentee un nombre borne d'essais, puis l'echec est ferme (AD-8). Un refus logique
    # de l'editeur (`success:false`) n'est jamais retente : il n'est pas transitoire.
    param(
        [Parameter(Mandatory)][string[]]$CliArgs,
        [int]$Attempts = 5,
        [int]$DelaySec = 2
    )

    $exitCode = $null
    for ($attempt = 1; $attempt -le $Attempts; $attempt++) {
        $raw = & unity @CliArgs --format json 2>&1
        $exitCode = $LASTEXITCODE
        $rawText = ($raw | Out-String).Trim()

        if ($exitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($rawText)) {
            $parsed = $null
            try {
                $parsed = $rawText | ConvertFrom-Json
            }
            catch {
                $parsed = $null
            }
            if ($null -ne $parsed) {
                if ($parsed.success) {
                    return (Get-CmdResult $parsed)
                }
                $errText = if ($parsed.errors) { $parsed.errors -join '; ' } else { 'raison inconnue' }
                Fail "unity $($CliArgs -join ' ') : $errText"
            }
        }
        if ($attempt -lt $Attempts) {
            Start-Sleep -Seconds $DelaySec
        }
    }
    Fail "unity $($CliArgs -join ' ') : commande muette ou code de sortie $exitCode apres $Attempts essais (AD-8)."
}

function Assert-NoConsoleErrorInWindow {
    # Gate AD-7. Seules comptent les entrees de sequence STRICTEMENT superieures au curseur ("only
    # return entries newer than this seq"). Une erreur anterieure a l'ouverture de la fenetre
    # appartient a l'histoire de la session d'Editeur, pas a l'etat du code : elle est ignoree, jamais
    # reparee. Une erreur de la fenetre, quelle qu'en soit la source, est un echec.
    param([Parameter(Mandatory)][string]$Phase)

    $entries = (Invoke-ConsoleQuery -CliArgs @('cmd', 'console', '--level', 'error', '--since', "$($script:ConsoleBaseline)")).entries
    if ($entries -and $entries.Count -gt 0) {
        foreach ($entry in $entries) {
            Write-Host "  [ERROR] seq $($entry.seq) $($entry.message)" -ForegroundColor Red
        }
        Fail "$($entries.Count) erreur(s) en Console depuis le curseur $($script:ConsoleBaseline) ($Phase, AD-7)."
    }
    $script:Summary['Erreurs Console'] = "0 depuis le curseur $($script:ConsoleBaseline)"
    Write-Host "  0 erreur depuis le curseur $($script:ConsoleBaseline)" -ForegroundColor DarkGray
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
$script:Summary['Unity CLI'] = $cliVersion
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
$script:Summary['Editeur'] = "port $($instance.port), Unity $($instance.version), ready"
Write-Host "  port $($instance.port), Unity $($instance.version), etat ready" -ForegroundColor DarkGray

# --- Etape 2 : stabilisation de l'editeur (compilation / rechargement de domaine) ---
# La fenetre ne s'ouvre qu'une fois l'editeur stabilise : une compilation encore en cours appartient a
# l'etat anterieur de la session, pas a ce run. C'est le contraire du comportement d'avant, ou la
# Console etait relue depuis le debut de la session d'Editeur.
Write-Step "editeur stabilise (compiling / domainReloadInProgress)"
Wait-EditorSettled -Reason 'ouverture de la fenetre de validation' -TimeoutSec $RecompileTimeoutSec

# --- Etape 3 : curseur Console, borne basse unique du gate AD-7 ---
# `console` rend la tete du journal capture quel que soit --level : les entrees rendues ici sont donc
# toutes hors fenetre. Elles servent de mesure informative (jamais de gate) et prouvent l'attribution.
Write-Step "unity cmd console (curseur de fenetre)"
$openProbe = Invoke-ConsoleQuery -CliArgs @('cmd', 'console', '--level', 'error')
if ($null -eq $openProbe.cursor) {
    Fail "curseur Console absent de la reponse d'ouverture (AD-8)."
}
$script:ConsoleBaseline = [int64]$openProbe.cursor
$historicalErrors = @($openProbe.entries)
$script:Summary['Curseur Console'] = "$($script:ConsoleBaseline) ($($historicalErrors.Count) erreur(s) anterieure(s) ignoree(s))"
Write-Host "  curseur $($script:ConsoleBaseline) ; $($historicalErrors.Count) erreur(s) anterieure(s) hors fenetre" -ForegroundColor DarkGray

# --- Etape 4 : recompile ---
Write-Step "unity cmd recompile"
Invoke-UnityJson -CliArgs @('cmd', 'recompile') | Out-Null

# --- Etape 5 : recompile_status (poll jusqu'a etat terminal) ---
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
$script:Summary['Recompilation'] = $recompileStatus
Write-Host "  recompile_status: $recompileStatus" -ForegroundColor DarkGray

# --- Etape 6 : gate AD-7 (fenetre ouverte a l'etape 3) + warnings hors Assets/Synty (info) ---
# `unity cmd recompile` peut declencher un rechargement de domaine meme quand rien n'a change : on
# attend sa fin avant d'interroger la Console, sinon la requete part dans le vide (doc du package :
# "The client must tolerate connection errors during a domain reload").
Write-Step "stabilisation apres recompile + unity cmd console --level error --since $($script:ConsoleBaseline)"
Wait-EditorSettled -Reason 'gate Console apres recompile' -TimeoutSec $RecompileTimeoutSec
Assert-NoConsoleErrorInWindow -Phase 'apres recompile'

# --- Etape 7 : etat de compilation courant (complement du gate, jamais un remplacement) ---
# Un build casse dont l'erreur a ete produite AVANT la fenetre (compilation deja tentee et echouee,
# rien de modifie depuis) est invisible pour `--since` : c'est precisement l'etat que `recompile`
# n'essaie plus et que `recompile_status` ne rapporte plus. Sans ce controle, cet etat deviendrait
# muet alors que la version precedente du script rougissait dessus (AD-7 ne doit pas faiblir).
# Les entrees Console affichees ici sont du CONTEXTE, jamais une decision : le gate reste la fenetre.
Write-Step "etat de compilation courant (EditorUtility.scriptCompilationFailed)"
$compileState = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'eval', '--code', 'return UnityEditor.EditorUtility.scriptCompilationFailed;'))
if ($compileState.result -ne $false) {
    $contextEntries = (Invoke-ConsoleQuery -CliArgs @('cmd', 'console', '--level', 'error', '--tail', '5')).entries
    foreach ($entry in $contextEntries) {
        $firstLine = ($entry.message -split "`n")[0]
        Write-Host "  [historique, contexte] seq $($entry.seq) $firstLine" -ForegroundColor Yellow
    }
    Fail "La compilation des scripts est en echec (EditorUtility.scriptCompilationFailed) : l'etat courant ne peut pas etre valide, meme si l'erreur Console est anterieure a la fenetre. Corriger, laisser l'editeur recompiler, puis relancer."
}
$script:Summary['Etat de compilation'] = 'sain (scriptCompilationFailed=false)'
Write-Host "  scriptCompilationFailed=false" -ForegroundColor DarkGray

# Les avertissements restent volontairement lus sur toute la session (informatif, hors gate) : le
# curseur ne borne que les erreurs.
$warningEntries = (Invoke-ConsoleQuery -CliArgs @('cmd', 'console', '--level', 'warning')).entries
$relevantWarnings = @($warningEntries | Where-Object { $_.message -notlike '*Assets/Synty/*' })
$script:Summary['Avertissements hors Synty'] = "$($relevantWarnings.Count) (informatif, hors gate)"
Write-Host "  $($relevantWarnings.Count) avertissement(s) hors Assets/Synty/ (informatif, ne bloque pas)" -ForegroundColor DarkGray

# Les avertissements repetes sont regroupes par message, les plus frequents d'abord (AD-7) : sur ce
# projet 100 fois le meme message chassaient toute information utile de l'ecran. Rien n'est supprime
# ni desactive -- le total reste affiche au-dessus, et chaque groupe montre son nombre d'occurrences.
# Un groupe est marque [projet] quand le message nomme Assets/RoadRage : c'est un test de contenu,
# pas une deduction. Un message sans chemin reste non marque.
$warningGroups = @($relevantWarnings | Group-Object message | Sort-Object Count -Descending)
$maxWarningGroups = 12
$shownGroups = 0
foreach ($group in $warningGroups) {
    if ($shownGroups -ge $maxWarningGroups) {
        Write-Host "  ... $($warningGroups.Count - $shownGroups) autre(s) message(s) distinct(s) non affiche(s) ; total : $($relevantWarnings.Count)" -ForegroundColor Yellow
        break
    }
    $origin = if ($group.Name -like '*Assets/RoadRage*') { ' [projet]' } else { '' }
    $repeat = if ($group.Count -gt 1) { " x$($group.Count)" } else { '' }
    Write-Host "  [WARN]$origin$repeat $($group.Name)" -ForegroundColor Yellow
    $shownGroups++
}

# --- Etape 8 : tests cibles (AD-9) ---
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
    $script:Summary["Tests $mode"] = "$($testStatus.summary.passed)/$($testStatus.summary.total) passes"
}

# --- Etape 9 : gate AD-7 apres tests (la fenetre court jusqu'ici) ---
# Une erreur produite par la suite de tests, ou par la sortie de Play Mode (rechargement de domaine),
# appartient a la fenetre : elle rougit la porte exactement comme une erreur de compilation de ce run.
Write-Step "stabilisation apres tests + unity cmd console --level error --since $($script:ConsoleBaseline)"
Wait-EditorSettled -Reason 'gate Console apres tests' -TimeoutSec $RecompileTimeoutSec
Assert-NoConsoleErrorInWindow -Phase 'apres tests'

# --- Etape 10 : list_open_scenes + git status (etat final, garde-fou AGENTS.md) ---
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
$sceneState = if ($dirtyScenes.Count -gt 0) { "$($dirtyScenes.Count) scene(s) isDirty=true" } else { 'aucune scene modifiee' }
$script:Summary['Scenes ouvertes'] = $sceneState

$gitStatus = & git -C $RepoRoot status --short
if ($gitStatus) {
    Write-Host "  git status --short :" -ForegroundColor DarkGray
    $gitStatus | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    $script:Summary['Arbre de travail'] = "$(@($gitStatus).Count) entree(s) modifiee(s) -- voir git status"
}
else {
    Write-Host "  git status --short : propre" -ForegroundColor DarkGray
    $script:Summary['Arbre de travail'] = 'propre'
}

# --- Etape optionnelle : Project Auditor (ADDON-018) ---
# Hors gate volontairement : ~2-3 min sur ce projet, signal domine par le bruit (tests, chemins
# editor-only, allocations generiques hors boucle chaude) -- lecture humaine requise, jamais un
# echec automatique. Voir docs/setup/audit-project-auditor-baseline.md pour la mesure de reference.
if ($Audit) {
    Write-Step "unity cmd audit (ADDON-018, informatif, hors gate)"
    $auditClock = [System.Diagnostics.Stopwatch]::StartNew()
    $trigger = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'audit'))
    Write-Host "  scan $($trigger.scanId) declenche" -ForegroundColor DarkGray

    $deadline = (Get-Date).AddSeconds($AuditTimeoutSec)
    $auditState = $null
    do {
        Start-Sleep -Seconds 5
        $auditState = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'audit_status'))
    } while ($auditState.status -eq 'scanning' -and (Get-Date) -lt $deadline)
    $auditClock.Stop()
    $auditDuration = [math]::Round($auditClock.Elapsed.TotalSeconds, 1)

    if ($auditState.status -eq 'completed') {
        Write-Host "  $($auditState.issueCount) diagnostic(s) en ${auditDuration}s -- CSV : $($auditState.csvPath)" -ForegroundColor DarkGray
        Write-Host "  Triage manuel recommande : filtrer Assets/Synty/ et Tests/, ~1-2% du volume brut est reellement actionnable (ADDON-018)." -ForegroundColor DarkGray
        $script:Summary['Project Auditor'] = "$($auditState.issueCount) diagnostic(s) en ${auditDuration}s (hors gate)"
    }
    else {
        Write-Host "  Project Auditor non conclusif : statut '$($auditState.status)' apres ${auditDuration}s (non bloquant, ADDON-018)." -ForegroundColor Yellow
        $script:Summary['Project Auditor'] = "non conclusif : '$($auditState.status)' apres ${auditDuration}s (hors gate)"
    }
}

Write-Summary
Write-Host "OK" -ForegroundColor Green
exit 0
