<#
# Chaine de verification RRS (AD-5, AD-7, AD-8, AD-9 -- architecture-RRS-devworkflow-2026-09-17).
# AD-4 (verification declenchee par l'utilisateur) abrogee le 2026-09-18 : l'agent execute ce script
# directement.
#
# unity status -> stabilisation -> curseur Console -> recompile -> recompile_status -> console --since
# -> list_tests -> tests cibles -> console --since -> list_open_scenes + git status
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
  scripts\validate.ps1                                    # EditMode complet (profil Full)
  scripts\validate.ps1 -TestMode PlayMode                  # PlayMode complet
  scripts\validate.ps1 -TestMode Both
  scripts\validate.ps1 -TestFilter "RoadRage.Tests.EditMode.RoadRageScaffoldTests"
  scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode EditMode  # tests de la story
  scripts\validate.ps1 -Profile Story -Story 5.31 -TestMode PlayMode  # tests de la story
  scripts\validate.ps1 -TestMode PlayMode -TestFilter Story531Campaign -TestFilterType category -IncludeExplicit
                                                              # campagne [Explicit] ciblee
  scripts\validate.ps1 -Profile Fast                       # EditMode hors geometrie (developpement)
  scripts\validate.ps1 -Profile Geometry                   # preuves geometriques seules
  scripts\validate.ps1 -Profile Auto                       # selection selon les fichiers modifies
  scripts\validate.ps1 -Audit                              # + Project Auditor (ADDON-018), informatif, hors gate

Profils (partition EditMode par categories NUnit Core / Geometry, voir
docs/setup/build-workflow-rules.md) :
  Story           categorie Story<epic><story>, -Story X.Y obligatoire, EditMode/PlayMode/Both.
                  Aucun test [Explicit]. Suites completes reservees a la fin d'epic.
  Full            suite complete, aucune exclusion. Fin d'epic seulement ; defaut inchange.
  Fast            categorie Core uniquement ; diagnostic EditMode.
  FullSansGeometry meme selection que Fast ; diagnostic avec classification Auto jointe.
  Geometry        categorie Geometry uniquement ; diagnostic des preuves.
  Auto            classifie les fichiers modifies (travail en cours + dernier commit, ou plage
                  -Since) : entree geometrique ou inconnue -> Full ; sinon -> Core (libelle
                  "complet hors geometrie"). Conservateur par construction.
#>
[CmdletBinding()]
param(
    [ValidateSet('EditMode', 'PlayMode', 'Both')]
    [string]$TestMode = 'EditMode',

    [string]$TestFilter = '',

    [ValidateSet('testName', 'assembly', 'category')]
    [string]$TestFilterType = 'testName',

    [switch]$IncludeExplicit,

    [ValidateSet('Full', 'Fast', 'Geometry', 'FullSansGeometry', 'Auto', 'Story')]
    [string]$Profile = 'Full',

    [string]$Story = '',

    [string]$Since = '',

    [int]$RecompileTimeoutSec = 120,

    # La suite EditMode complete dure ~430 s de tests (mesure du 2026-09-28, 925 tests) : l'ancien
    # defaut de 300 s declarait le profil Full en echec par timeout avant la fin. 3600 s laisse
    # tourner la suite complete et ne borne que les blocages anormaux.
    [int]$TestTimeoutSec = 3600,

    [switch]$Audit,
    [int]$AuditTimeoutSec = 300
)

$ErrorActionPreference = 'Stop'

# Reference figee dans AGENTS.md : version beta validee avec RRS, aucune montee automatique.
$ExpectedCliVersion = '1.0.0-beta.8'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path

# Classification des changements pour le profil Auto (source unique du mapping : le script
# lui-meme, jamais duplique ici). Voir scripts/validation-profiles.ps1 -SelfTest.
. (Join-Path $PSScriptRoot 'validation-profiles.ps1')
# Lecture du statut de tests tolerante au Domain Reload (decision proprietaire du 2026-10-03). Voir
# scripts/validation-cli.ps1 -SelfTest.
. (Join-Path $PSScriptRoot 'validation-cli.ps1')

$script:ProfileLabel = 'Full (suite complete, aucune exclusion)'
$script:ProfileCategoryFilter = $null
$script:ExpectedEditModeTests = $null
$script:ExpectedPlayModeTests = $null
$script:NonExecutedBySelection = $null

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
    if ($Profile -eq 'Story') { Write-Host "  VALIDATION STORY ($($script:ProfileCategoryFilter))" -ForegroundColor Yellow }
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

# --- Gardes de profil (avant tout appel Unity : un usage incoherent echoue tout de suite) ---
# Seul Story utilise le filtre de categorie en PlayMode/Both. Les autres profils restent EditMode.
if ($Profile -ne 'Full' -and $Profile -ne 'Story' -and $TestMode -ne 'EditMode') {
    Fail "-Profile $Profile ne s'applique qu'a -TestMode EditMode. PlayMode/Both se lancent avec -Profile Full (aucune exclusion)."
}
if ($Profile -ne 'Full' -and $PSBoundParameters.ContainsKey('TestFilter')) {
    Fail "-Profile $Profile et -TestFilter sont exclusifs : le filtrage par categorie et le filtrage par nom sont deux selections differentes (une seule passe par le CLI). Utiliser l'un ou l'autre."
}
if ($IncludeExplicit -and [string]::IsNullOrWhiteSpace($TestFilter)) {
    Fail "-IncludeExplicit exige -TestFilter non vide : aucune suite [Explicit] complete ne peut etre lancee par erreur."
}
if ($Profile -eq 'Story' -and $Story -cnotmatch '^\d+\.\d+$') {
    Fail "-Profile Story exige -Story au format X.Y (exemple : 5.31)."
}
if ($Profile -ne 'Story' -and $PSBoundParameters.ContainsKey('Story')) {
    Fail "-Story ne s'utilise qu'avec -Profile Story."
}
if ($Profile -eq 'Story') {
    $script:ProfileCategoryFilter = 'Story' + ($Story -replace '\.', '')
    $script:ProfileLabel = "Story ($($script:ProfileCategoryFilter))"
    $script:ExecutedStoryTests = 0
    $script:Summary['Profil'] = $script:ProfileLabel
    $script:Summary['Tests executes'] = '0 (validation interrompue avant tests)'
    $script:Summary['Suites completes'] = "non executees ; reservees a la fin d'epic"
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

# --- Etape 8a : selection apres recompile et stabilisation (list_tests voit les nouveaux tests) ---
# Par defaut (aucun nouveau parametre) : profil Full, aucune exclusion, comportement inchange.
# Auto : la classification des fichiers modifies (voir scripts/validation-profiles.ps1) decide.
# Une entree geometrique OU inconnue -> Full (conservateur) ; tout le reste classe Core -> Fast.
# Un profil partiel explicite (Fast/FullSansGeometry/Geometry) reste un choix de developpement :
# si des entrees geometriques sont modifiees dans l'arbre de travail, il le dit a voix haute.
if ($Profile -ne 'Full') {
    Write-Step "profil de validation ($Profile)"

    if ($Profile -eq 'Story') {
        Write-Host "  categorie : $($script:ProfileCategoryFilter)" -ForegroundColor DarkGray
    }
    elseif ($Profile -eq 'Auto') {
        $changed = Get-ChangedPathsForValidation -RepoRoot $RepoRoot -Since $Since
        foreach ($note in $changed.Notes) { Write-Host "  note : $note" -ForegroundColor DarkYellow }

        if ($changed.GitFailed) {
            $script:ProfileLabel = 'Full (Auto : etat git non classable, conservateur)'
            Write-Host "  decision : Full -- etat git non classable, aucune preuve ne peut etre exclue (conservateur)." -ForegroundColor Yellow
        }
        else {
            $selection = Resolve-EditModeValidationSelection -Paths $changed.Paths -RepoRoot $RepoRoot
            Write-Host "  fichiers pris en compte : $($changed.Paths.Count) (classee(s) : $($selection.Geometry.Count) geometrie, $($selection.Core.Count) core, $($selection.Unknown.Count) inconnue(s), $($selection.Ignored.Count) ignoree(s))" -ForegroundColor DarkGray
            if ($selection.Decision -eq 'Full') {
                $script:ProfileLabel = 'Full (Auto : entree geometrique ou inconnue touchee)'
                foreach ($path in $selection.Geometry) { Write-Host "    [GEOMETRIE] $path" -ForegroundColor Yellow }
                foreach ($path in $selection.Unknown) { Write-Host "    [INCONNU -> conservateur] $path" -ForegroundColor Yellow }
                Write-Host "  decision : Full -- entree(s) geometrique(s) ou inconnue(s) touchee(s) : les preuves 5.49/5.50/5.51/5.28 sont incluses." -ForegroundColor Yellow
            }
            else {
                $script:ProfileCategoryFilter = $script:CoreCategory
                $script:ProfileLabel = 'Auto -> Core (complet hors geometrie : aucune entree geometrique touchee)'
                Write-Host "  decision : Core -- aucune entree geometrique ni inconnue parmi les fichiers classes." -ForegroundColor DarkGray
                Write-Host "  rappel : ce profil n'est pas la suite complete ; consulter les regles de fin d'epic." -ForegroundColor DarkGray
            }
        }
    }
    else {
        switch ($Profile) {
            'Fast' { $script:ProfileCategoryFilter = $script:CoreCategory; $script:ProfileLabel = 'Fast (categorie Core : hors geometrie)' }
            'FullSansGeometry' { $script:ProfileCategoryFilter = $script:CoreCategory; $script:ProfileLabel = 'FullSansGeometry (complet hors geometrie)' }
            'Geometry' { $script:ProfileCategoryFilter = $script:GeometryCategory; $script:ProfileLabel = 'Geometry (preuves geometriques seules)' }
        }

        # Avertissement quand des entrees geometriques sont modifiees dans l'arbre de travail alors que
        # le profil demande ne les couvre pas : jamais un contournement silencieux.
        if ($script:ProfileCategoryFilter -eq $script:CoreCategory) {
            $worktree = Get-ChangedPathsForValidation -RepoRoot $RepoRoot -WorktreeOnly
            if (-not $worktree.GitFailed) {
                $worktreeSelection = Resolve-EditModeValidationSelection -Paths $worktree.Paths -RepoRoot $RepoRoot
                $sensitive = @($worktreeSelection.Geometry) + @($worktreeSelection.Unknown)
                if ($sensitive.Count -gt 0) {
                    foreach ($path in $sensitive) { Write-Host "    [hors perimetre du profil] $path" -ForegroundColor Yellow }
                    Write-Host "  AVERTISSEMENT : $($sensitive.Count) entree(s) geometrique(s) ou inconnue(s) modifiee(s) dans l'arbre de travail ; ce profil ne les couvre pas." -ForegroundColor Yellow
                }
            }
        }
    }

    # Comptes de selection : list_tests donne le contenu reellement porteur des categories, donc la
    # couverture que ce profil peut revendiquer. La partition doit etre complete : un test sans
    # categorie serait exclu des deux profils partiels sans que personne ne le voie -> echec ferme.
    if ($Profile -ne 'Story' -or $TestMode -ne 'PlayMode') {
        Write-Step "unity cmd list_tests --mode editor (comptes et partition)"
        $listResult = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'list_tests', '--mode', 'editor'))
        $allTests = @($listResult.tests)
        if ($allTests.Count -eq 0 -and $listResult.result) {
            $allTests = @(($listResult.result | ConvertFrom-Json).tests)
        }
        if ($allTests.Count -eq 0) {
            Fail "list_tests n'a rendu aucun test EditMode : comptes de selection impossibles (AD-8)."
        }

        $runnable = @($allTests | Where-Object { -not $_.explicit })
        $explicitTests = @($allTests | Where-Object { $_.explicit })
        $coreTests = @($runnable | Where-Object { $_.categories -contains $script:CoreCategory })
        $geometryTests = @($runnable | Where-Object { $_.categories -contains $script:GeometryCategory })
        $partitioned = @($runnable | Where-Object { $_.categories -contains $script:CoreCategory -or $_.categories -contains $script:GeometryCategory })
        $uncategorized = @($runnable | Where-Object { $_.categories -notcontains $script:CoreCategory -and $_.categories -notcontains $script:GeometryCategory })

        if ($uncategorized.Count -gt 0) {
            foreach ($test in ($uncategorized | Select-Object -First 10)) { Write-Host "  [SANS CATEGORIE] $($test.fullName)" -ForegroundColor Red }
            Fail "$($uncategorized.Count) test(s) EditMode sans categorie Core/Geometry : la partition de selection est incomplete, ce profil ne peut pas certifier ce qu'il couvre (garde TestSuiteCategoryPartitionTests). Tagger la ou les fixtures concernees."
        }

        $expected = if ($Profile -eq 'Story') { @($runnable | Where-Object { $_.categories -contains $script:ProfileCategoryFilter }).Count }
                    elseif ($script:ProfileCategoryFilter -eq $script:CoreCategory) { $coreTests.Count } else { $geometryTests.Count }
        if ($expected -eq 0) { Fail "Aucun test EditMode pour la categorie $($script:ProfileCategoryFilter) (AD-8)." }
        $script:ExpectedEditModeTests = $expected
        $script:NonExecutedBySelection = $runnable.Count - $expected
        $explicitNote = if ($explicitTests.Count -gt 0) { " ; $($explicitTests.Count) test(s) [Explicit] hors suite par defaut" } else { '' }
        $script:Summary['Selection'] = "$expected test(s) attendus (categorie $($script:ProfileCategoryFilter)) ; $($script:NonExecutedBySelection) non executes par selection$explicitNote"
        Write-Host "  $expected test(s) attendus (categorie $($script:ProfileCategoryFilter)) ; $($script:NonExecutedBySelection) non executes par selection$explicitNote" -ForegroundColor DarkGray
        if ($Profile -eq 'Story') { $storyEditTests = @($allTests | Where-Object { $_.categories -contains $script:ProfileCategoryFilter }) }
    }
}
$script:Summary['Profil'] = $script:ProfileLabel

# Le pipeline exclut [Explicit] quand include_explicit=false (defaut), y compris au niveau
# fixture. list_tests signale a tort Explicit=false pour les campagnes PlayMode : controler
# egalement les sources des fixtures selectionnees et echouer ferme si elles sont illisibles.
if ($Profile -eq 'Story') {
    $storyTestsByMode = @{}
    if ($TestMode -in @('EditMode', 'Both')) { $storyTestsByMode['EditMode'] = $storyEditTests }
    if ($TestMode -in @('PlayMode', 'Both')) {
        Write-Step "unity cmd list_tests --mode playmode (compte Story)"
        $playList = Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'list_tests', '--mode', 'playmode'))
        $playTests = @($playList.tests)
        if ($playTests.Count -eq 0) { Fail "list_tests n'a rendu aucun test PlayMode : compte Story impossible (AD-8)." }
        $storyTestsByMode['PlayMode'] = @($playTests | Where-Object { $_.categories -contains $script:ProfileCategoryFilter })
        $script:ExpectedPlayModeTests = $storyTestsByMode['PlayMode'].Count
        $script:Summary['Selection PlayMode'] = "$($script:ExpectedPlayModeTests) test(s) attendus (categorie $($script:ProfileCategoryFilter))"
        Write-Host "  $($script:ExpectedPlayModeTests) test(s) PlayMode attendus (categorie $($script:ProfileCategoryFilter))" -ForegroundColor DarkGray
    }

    foreach ($mode in $storyTestsByMode.Keys) {
        $selected = @($storyTestsByMode[$mode])
        if ($selected.Count -eq 0) { Fail "Aucun test $mode pour la categorie $($script:ProfileCategoryFilter) (AD-8)." }
        if (@($selected | Where-Object { $_.explicit }).Count -gt 0) {
            Fail "La categorie $($script:ProfileCategoryFilter) contient un test $mode [Explicit] (AD-8)."
        }
        $fixtures = @($selected | ForEach-Object {
            if ($_.fullName -notmatch "^RoadRage\.Tests\.$mode\.([^.]+)\.") {
                Fail "Fixture $mode illisible pour '$($_.fullName)' : garde [Explicit] impossible (AD-8)."
            }
            $Matches[1]
        } | Sort-Object -Unique)
        foreach ($fixture in $fixtures) {
            $sources = @(Get-ChildItem -LiteralPath (Join-Path $RepoRoot "Assets/RoadRage/Tests/$mode") -Filter "$fixture.cs" -Recurse -File)
            if ($sources.Count -ne 1) { Fail "Source de fixture $mode '$fixture' introuvable ou ambigue : garde [Explicit] impossible (AD-8)." }
            if ((Get-Content -LiteralPath $sources[0].FullName -Raw) -match '\[Explicit(?:Attribute)?\s*(?:\(|\])') {
                Fail "Fixture $mode '$fixture' porte [Explicit] : hors profil Story (AD-8)."
            }
        }
    }
}

# --- Etape 8 : tests cibles (AD-9) ---
$modes = if ($TestMode -eq 'Both') { @('EditMode', 'PlayMode') } else { @($TestMode) }
foreach ($mode in $modes) {
    $explicitExpected = @()
    if ($IncludeExplicit -and $TestFilterType -eq 'category') {
        $listMode = if ($mode -eq 'EditMode') { 'editor' } else { 'playmode' }
        Write-Step "unity cmd list_tests --mode $listMode (selection [Explicit] ciblee)"
        $listed = @( (Get-CmdResult (Invoke-UnityJson -CliArgs @('cmd', 'list_tests', '--mode', $listMode))).tests )
        $explicitExpected = @($listed | Where-Object { $_.categories -contains $TestFilter } | ForEach-Object { $_.fullName } | Sort-Object)
        if ($explicitExpected.Count -eq 0) { Fail "Aucun test $mode ne porte la categorie '$TestFilter' (AD-8)." }
        Write-Host "  $($explicitExpected.Count) test(s) attendus (categorie $TestFilter)" -ForegroundColor DarkGray
        $script:Summary['Selection explicite'] = "$($explicitExpected.Count) test(s) attendus (categorie $TestFilter, $mode)"
    }
    Write-Step "unity cmd run_tests --mode $mode"
    $runArgs = @('cmd', 'run_tests', '--mode', $mode, '--async_tests', 'true')
    if ($IncludeExplicit) { $runArgs += @('--include_explicit', 'true') }
    if ($TestFilter) {
        $runArgs += @('--filter', $TestFilter, '--filter_type', $TestFilterType)
    }
    elseif ($script:ProfileCategoryFilter -and ($mode -eq 'EditMode' -or $Profile -eq 'Story')) {
        # Le CLI compare les categories par egalite (pas par regex) : Story531 n'inclut pas Story531Campaign.
        $runArgs += @('--filter', $script:ProfileCategoryFilter, '--filter_type', 'category')
    }
    Invoke-UnityJson -CliArgs $runArgs | Out-Null

    Write-Step "unity cmd test_status ($mode)"
    $deadline = (Get-Date).AddSeconds($TestTimeoutSec)
    $testStatus = $null
    $statusRetries = 0
    do {
        Start-Sleep -Seconds 2
        # Domain Reload a l'entree et a la sortie du Play Mode : le CLI peut se taire un instant (sortie vide ou code 6).
        # Lecture retentee 5 fois au plus, a 2 s d'intervalle, puis echec ferme ; tout autre code, une sortie illisible ou un
        # refus de l'Editeur reste bloquant immediatement. L'interpretation du statut ci-dessous est inchangee.
        $query = Invoke-TestStatusQuery -Attempts 5 -DelaySec 2 -Invoker {
            $raw = & unity cmd test_status --format json 2>&1
            [pscustomobject]@{ ExitCode = $LASTEXITCODE; Text = ($raw | Out-String) }
        }
        if (-not $query.Ok) {
            Fail "unity cmd test_status : $($query.Message) (AD-8)"
        }
        $statusRetries += $query.Retries
        $testStatus = Get-CmdResult $query.Parsed
        if (-not $testStatus -or -not $testStatus.status) {
            Fail "test_status illisible (AD-8)"
        }
    } while ($testStatus.status -notin @('completed', 'failed', 'error') -and (Get-Date) -lt $deadline)
    if ($statusRetries -gt 0) {
        Write-Host "  test_status : $statusRetries lecture(s) retentee(s) pendant une indisponibilite passagere du CLI (Domain Reload)" -ForegroundColor DarkGray
    }

    if ($testStatus.status -ne 'completed') {
        Fail "test_status bloque sur '$($testStatus.status)' apres ${TestTimeoutSec}s (AD-8)."
    }
    if (-not $testStatus.summary -or $testStatus.summary.total -eq 0) {
        $activeFilter = if ($Profile -eq 'Story') { $script:ProfileCategoryFilter } else { $TestFilter }
        Fail "Aucun test execute en $mode (filtre '$activeFilter' sans correspondance ?). Une absence de resultat n'est jamais un succes (AD-8)."
    }
    if ($IncludeExplicit -and $TestFilterType -eq 'category') {
        $actualNames = @($testStatus.results | ForEach-Object { $_.FullName } | Sort-Object)
        $nameDiff = @(Compare-Object -ReferenceObject $explicitExpected -DifferenceObject $actualNames)
        if ($testStatus.summary.total -ne $explicitExpected.Count -or $actualNames.Count -ne $explicitExpected.Count -or $nameDiff.Count -gt 0) {
            Fail "Selection [Explicit] divergente : $($testStatus.summary.total) resultat(s), $($explicitExpected.Count) attendu(s) pour '$TestFilter' (AD-8)."
        }
    }
    if ($Profile -eq 'Story') {
        $script:ExecutedStoryTests += [int]$testStatus.summary.total
        $script:Summary['Tests executes'] = "$($script:ExecutedStoryTests) au total ($($testStatus.summary.total) en $mode)"
        if ([int]$testStatus.summary.passed + [int]$testStatus.summary.failed -eq 0) {
            Fail "Aucun test effectivement execute en $mode : tous les resultats sont ignores ou inconclusifs (AD-8)."
        }
    }
    if ($testStatus.summary.failed -gt 0) {
        foreach ($result in ($testStatus.results | Where-Object { $_.Status -ne 'Passed' })) {
            Write-Host "  [FAIL] $($result.FullName) : $($result.Message)" -ForegroundColor Red
        }
        Fail "$($testStatus.summary.failed)/$($testStatus.summary.total) test(s) $mode en echec."
    }

    # Garde de selection : ce qui a ete execute doit correspondre exactement a ce que le profil a
    # annonce (compte list_tests). Un ecart = selection et execution divergent, donc la couverture
    # revendiquee n'est pas prouvee -> echec ferme plutot qu'un compte rendu trompeur.
    if ($script:ProfileCategoryFilter) {
        $expectedModeTests = if ($mode -eq 'EditMode') { $script:ExpectedEditModeTests } else { $script:ExpectedPlayModeTests }
        if ($testStatus.summary.total -ne $expectedModeTests) {
            Fail "Selection et execution divergent : $($testStatus.summary.total) test(s) $mode executes, $expectedModeTests attendus (categorie $($script:ProfileCategoryFilter)). Verifier list_tests et la partition des categories."
        }
    }

    $skipped = 0
    $inconclusive = 0
    if ($testStatus.summary.PSObject.Properties.Name -contains 'skipped') { $skipped = [int]$testStatus.summary.skipped }
    if ($testStatus.summary.PSObject.Properties.Name -contains 'inconclusive') { $inconclusive = [int]$testStatus.summary.inconclusive }
    Write-Host "  $($testStatus.summary.passed)/$($testStatus.summary.total) test(s) $mode passes" -ForegroundColor DarkGray
    if ($skipped -gt 0 -or $inconclusive -gt 0) {
        Write-Host "  ignores (reels) : $skipped skipped, $inconclusive inconclusive" -ForegroundColor DarkGray
    }
    $script:Summary["Tests $mode"] = "$($testStatus.summary.passed)/$($testStatus.summary.total) passes"
    $script:Summary["Ignores $mode"] = "$skipped skipped, $inconclusive inconclusive (reels, dans l'execution)"
    if ($IncludeExplicit -and ($skipped -gt 0 -or $inconclusive -gt 0)) {
        Fail "Campagne [Explicit] incomplete : $skipped skipped, $inconclusive inconclusive (AD-8)."
    }
    if ($Profile -eq 'Story' -and ($skipped -gt 0 -or $inconclusive -gt 0)) {
        Fail "Validation Story incomplete : $skipped skipped, $inconclusive inconclusive (AD-8)."
    }
    if ($script:ProfileCategoryFilter -and $mode -eq 'EditMode') {
        $script:Summary['Non executes (selection)'] = "$($script:NonExecutedBySelection) test(s) de la suite EditMode, hors profil $Profile"
    }
    elseif ($Profile -eq 'Story') {
        $script:Summary['Couverture PlayMode'] = "$($testStatus.summary.total) test(s) de la story ; suite complete non executee"
    }
    elseif ($TestFilter) {
        $script:Summary['Couverture'] = "ciblage -TestFilter '$TestFilter' : la suite complete n'a pas ete executee"
    }
    elseif ($mode -eq 'EditMode' -and $Profile -eq 'Full') {
        $script:Summary['Non executes (selection)'] = 'aucun (profil Full : suite complete)'
    }
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

# Verdict lisible : le profil Full est le seul a valoir "validation complete". Un profil partiel
# s'affiche comme tel, avec le compte de ce qui n'a PAS ete execute -- un test exclu par selection
# ne doit jamais pouvoir se lire comme "reussi". Auto peut resoudre vers Full : la decision et sa
# justification sont alors rappelees, sans etiqueter "partiel" une suite qui a bien tout execute.
# Un ciblage -TestFilter est lui aussi partiel par construction (une seule passe du CLI).
if ($Profile -eq 'Story') {
    Write-Host "VALIDATION STORY ($($script:ProfileCategoryFilter)) : tests de la story seulement. Suites completes non executees ; elles relevent de la fin d'epic. Cette sortie ne vaut pas validation complete." -ForegroundColor Yellow
}
elseif ($TestFilter) {
    Write-Host "CIBLAGE (-TestFilter '$TestFilter') : la suite complete n'a pas ete executee ; cette sortie ne vaut pas validation complete." -ForegroundColor Yellow
}
elseif ($script:ProfileCategoryFilter) {
    Write-Host "VALIDATION PARTIELLE ($($script:ProfileLabel)) : $($script:NonExecutedBySelection) test(s) EditMode non executes par selection. Une validation complete exige -Profile Full." -ForegroundColor Yellow
}
elseif ($Profile -ne 'Full') {
    Write-Host "Validation complete ($($script:ProfileLabel)) : suite EditMode entiere, aucune exclusion." -ForegroundColor DarkGray
}
else {
    Write-Host "Validation complete : profil Full, aucune exclusion." -ForegroundColor DarkGray
}
exit 0
