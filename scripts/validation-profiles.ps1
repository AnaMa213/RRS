<#
# Classification des changements pour la selection de profil de validation EditMode.
#
# Dot-source depuis scripts/validate.ps1 :
#   . (Join-Path $PSScriptRoot 'validation-profiles.ps1')
#
# Fournit :
#   Get-ChangedPathsForValidation  -- collecte git (worktree + eventuellement plage -Since)
#   Resolve-EditModeValidationSelection -- classification pure d'une liste de chemins
#   Invoke-ValidationProfileSelfTest -- simulations controlees de selection (exit 0/1)
#
# Contrat de classification (conservateur en cas de doute) :
#   - GEOMETRY : une entree geometrique est touchee -> le profil Auto resout vers Full.
#     Le mapping est derive des entrees reelles des preuves 5.49-5.51 (scene MVP_Run,
#     artefacts du modele de route, prefabs, pipeline Traffic V2, empreintes vehicule,
#     SidewalkDeclarations, ProjectSettings/ProjectSettings.asset, manifeste de packages).
#   - CORE : aucun effet connu sur les preuves geometriques (code runtime, autres scenes,
#     fixtures Core, oracle de trafic, tests PlayMode...).
#   - IGNORED : outillage, documentation, artefacts BMAD, sorties Graphify. Aucune entree
#     de build.
#   - UNKNOWN : toute autre entree de build -> traite comme GEOMETRY (conservateur).
#
# Toute evolution du mapping est un changement de regle de selection : la maintenir ici,
# jamais dans validate.ps1, et couvrir le cas par -SelfTest.
#>

[CmdletBinding()]
param([switch]$SelfTest)

# --- Categories NUnit de la partition EditMode (source unique) ---
$script:CoreCategory = 'Core'
$script:GeometryCategory = 'Geometry'

# --- Classes de chemins (regex, cote 'chemin relatif au depot avec /') ---
$script:IgnoredPathPatterns = @(
    '^_bmad-output/',
    '^_bmad/',
    '^\.agents/',
    '^\.claude/',
    '^\.github/',
    '^docs/',
    '^graphify-out/',
    '^scripts/',
    '^Temp/',
    '^Builds/',
    '^Logs/',
    '^UserSettings/',
    '^\.vscode/',
    '^AGENTS\.md$',
    '^CLAUDE\.md$',
    '^README.*\.md$',
    '^\.gitignore$',
    '^\.gitattributes$',
    '^\.graphifyignore$',
    '^steam_appid\.txt$',
    '^skills-lock\.json$',
    '\.csproj$',
    '\.slnx$'
)

# Entrees geometriques des preuves de Trafic V2 (5.49/5.50/5.51/5.28).
# `Traffic/Routing/` est volontairement EXCLU : le plan de route strategique (Story 5.29) parcourt la
# topologie compilee (corridors, mouvements, portails), ne produit ni courbe, ni collider, ni profil
# et n'alimente aucun artefact mesure -- ses modifications ne doivent pas declencher les preuves
# geometriques. Toute modification du modele de route lui-meme passe par les autres chemins Traffic/**
# ou par les entrees physiques, tous en geometrie.
$script:GeometryPathPatterns = @(
    '^Assets/RoadRage/App/Scenes/MVP_Run\.unity',
    '^Assets/RoadRage/App/Scenes/MVP_Run/',
    '^Assets/RoadRage/Prefabs/',
    '^Assets/RoadRage/Features/Vehicles/Traffic/(?!Routing(/|$))',
    '^Assets/RoadRage/Features/Vehicles/SidewalkDeclarations\.cs$',
    '^Assets/RoadRage/Features/Vehicles/VehicleProfile\.cs$',
    '^Assets/RoadRage/Features/Vehicles/VehicleProfileDef\.cs$',
    '^Assets/RoadRage/Features/Vehicles/VehicleWheel\.cs$',
    '^Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default\.asset$',
    '^ProjectSettings/',
    '^Packages/manifest\.json$',
    '^Packages/packages-lock\.json$'
)

# Chemins explicitement sans effet sur les preuves geometriques (sinon UNKNOWN -> conservateur).
# Liste blanche volontairement limitee aux dossiers dont le role est connu : un chemin RoadRage non
# reconnu (nouveau dossier, nouveau fichier a la racine) reste UNKNOWN, donc conservateur.
$script:CorePathPatterns = @(
    '^Assets/RoadRage/Tests/EditMode/TrafficOracle/',
    '^Assets/RoadRage/Tests/PlayMode/',
    '^Assets/RoadRage/Features/Vehicles/Traffic/Routing/',
    '^Assets/RoadRage/Features/',
    '^Assets/RoadRage/App/',
    '^Assets/RoadRage/DevTools/',
    '^Assets/RoadRage/Editor/',
    '^Assets/RoadRage/ScriptableObjects/',
    '^Assets/Editor/',
    '^Assets/Settings/'
)

function Get-NormalizedPath {
    param([Parameter(Mandatory)][string]$Path)
    return ($Path -replace '\\', '/').TrimStart('/')
}

function Get-PathClassification {
    <#
    .SYNOPSIS
    Classe UN chemin (relatif au depot, / comme separateur) :
    Geometry | Core | Ignored | Unknown.
    .DESCRIPTION
    Les fichiers de fixtures EditMode sont resolus par leur contenu ([Category]),
    car la garantie protegee depend de la fixture, pas du nom : un fichier de test
    sans categorie est traite comme UNKNOWN (conservateur).
    #>
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$RepoRoot
    )

    $normalized = Get-NormalizedPath $Path

    # Un .meta suit son fichier cible : meme classe, meme contenu de reference.
    if ($normalized.EndsWith('.meta', [System.StringComparison]::OrdinalIgnoreCase)) {
        $normalized = $normalized.Substring(0, $normalized.Length - 5)
    }

    foreach ($pattern in $script:IgnoredPathPatterns) {
        if ($normalized -match $pattern) { return 'Ignored' }
    }

    # Fixtures EditMode hors dossier TrafficOracle : la categorie du fichier decide.
    if ($normalized -match '^Assets/RoadRage/Tests/EditMode/' -and $normalized -notmatch '^Assets/RoadRage/Tests/EditMode/TrafficOracle/') {
        $fullPath = Join-Path $RepoRoot $normalized
        if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
            $content = Get-Content -LiteralPath $fullPath -Raw
            $hasGeometry = $content -match '\[Category\("Geometry"\)\]'
            $hasCore = $content -match '\[Category\("Core"\)\]'
            if ($hasGeometry) { return 'Geometry' }   # Geometry prime si les deux sont presents
            if ($hasCore) { return 'Core' }
        }
        # Fichier illisible, absent (suppression) ou sans categorie : conservateur.
        return 'Unknown'
    }

    foreach ($pattern in $script:GeometryPathPatterns) {
        if ($normalized -match $pattern) { return 'Geometry' }
    }

    foreach ($pattern in $script:CorePathPatterns) {
        if ($normalized -match $pattern) { return 'Core' }
    }

    return 'Unknown'
}

function Resolve-EditModeValidationSelection {
    <#
    .SYNOPSIS
    Selection PURE d'un profil a partir d'une liste de chemins.
    .OUTPUTS
    Objet avec : Decision ('Full' | 'Fast'), Geometry[], Core[], Unknown[], Ignored[].
    Decision = Full des qu'une entree Geometry OU Unknown est presente (conservateur) ;
    Fast seulement quand chaque chemin a ete classe, et aucun n'est geometrique.
    #>
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Paths,
        [Parameter(Mandatory)][string]$RepoRoot
    )

    $geometry = New-Object System.Collections.Generic.List[string]
    $core = New-Object System.Collections.Generic.List[string]
    $unknown = New-Object System.Collections.Generic.List[string]
    $ignored = New-Object System.Collections.Generic.List[string]

    foreach ($path in $Paths) {
        if ([string]::IsNullOrWhiteSpace($path)) { continue }
        switch (Get-PathClassification -Path $path -RepoRoot $RepoRoot) {
            'Geometry' { $geometry.Add((Get-NormalizedPath $path)) }
            'Core' { $core.Add((Get-NormalizedPath $path)) }
            'Ignored' { $ignored.Add((Get-NormalizedPath $path)) }
            default { $unknown.Add((Get-NormalizedPath $path)) }
        }
    }

    $decision = if ($geometry.Count -gt 0 -or $unknown.Count -gt 0) { 'Full' } else { 'Fast' }

    return [pscustomobject]@{
        Decision = $decision
        Geometry = $geometry.ToArray()
        Core     = $core.ToArray()
        Unknown  = $unknown.ToArray()
        Ignored  = $ignored.ToArray()
    }
}

function Get-ChangedPathsForValidation {
    <#
    .SYNOPSIS
    Collecte des chemins a classer : travail en cours (git status -z, y compris non suivis)
    + plage -Since fournie, ou + commit HEAD quand l'arbre est propre et sans -Since.
    .DESCRIPTION
    Un arbre propre ne dit pas ce qui a change : sans -Since, seul le dernier commit est
    classable, et la sortie le dit. Pour une livraison multi-commit, passer -Since <base>.
    #>
    param(
        [Parameter(Mandatory)][string]$RepoRoot,
        [string]$Since = '',
        [switch]$WorktreeOnly
    )

    $paths = New-Object System.Collections.Generic.List[string]
    $notes = New-Object System.Collections.Generic.List[string]

    $statusRaw = & git -C $RepoRoot status --porcelain -z -uall 2>$null
    if ($LASTEXITCODE -ne 0) {
        return [pscustomobject]@{ Paths = @(); Notes = @('git status indisponible : classification impossible, profil conservateur.'); GitFailed = $true }
    }

    $entries = @()
    if ($statusRaw) {
        $entries = ($statusRaw -join "`0" -split "`0") | Where-Object { $_ -ne '' }
    }
    $index = 0
    while ($index -lt $entries.Count) {
        $entry = $entries[$index]
        if ($entry.Length -lt 4) { $index++; continue }
        $statusCode = $entry.Substring(0, 2)
        $path = $entry.Substring(3)
        $paths.Add($path) | Out-Null
        if ($statusCode[0] -eq 'R' -or $statusCode[0] -eq 'C') {
            # git status -z : le chemin d'origine suit l'entree, sans code d'etat.
            if ($index + 1 -lt $entries.Count) {
                $paths.Add($entries[$index + 1]) | Out-Null
                $index++
            }
        }
        $index++
    }
    $worktreeCount = $paths.Count

    if ($WorktreeOnly) {
        return [pscustomobject]@{
            Paths = ($paths.ToArray() | Sort-Object -Unique)
            Notes = @()
            GitFailed = $false
        }
    }

    if ($Since) {
        $rangeRaw = & git -C $RepoRoot diff --name-only -z "$Since" HEAD 2>$null
        if ($LASTEXITCODE -ne 0) {
            $notes.Add("git diff $Since HEAD indisponible : plage -Since ignoree, selection conservatrice.")
            return [pscustomobject]@{ Paths = $paths.ToArray(); Notes = $notes.ToArray(); GitFailed = $true }
        }
        foreach ($path in (($rangeRaw -join "`0" -split "`0") | Where-Object { $_ -ne '' })) {
            $paths.Add($path) | Out-Null
        }
        $notes.Add("plage -Since $Since : $(($paths.Count - $worktreeCount)) fichier(s) de commit pris en compte.")
    }
    elseif ($worktreeCount -eq 0) {
        # Arbre propre : classer le dernier commit, et le dire (limite connue).
        $headRaw = & git -C $RepoRoot diff --name-only -z 'HEAD~1' HEAD 2>$null
        if ($LASTEXITCODE -eq 0) {
            foreach ($path in (($headRaw -join "`0" -split "`0") | Where-Object { $_ -ne '' })) {
                $paths.Add($path) | Out-Null
            }
            $headSubject = (& git -C $RepoRoot log -1 --format='%h %s' 2>$null | Out-String).Trim()
            $notes.Add("arbre propre : classification limitee au dernier commit ($headSubject). Pour une livraison multi-commit, passer -Since <base>.")
        }
        else {
            $notes.Add('arbre propre et dernier commit illisible : selection conservatrice.')
            return [pscustomobject]@{ Paths = @(); Notes = $notes.ToArray(); GitFailed = $true }
        }
    }

    return [pscustomobject]@{
        Paths = ($paths.ToArray() | Sort-Object -Unique)
        Notes = $notes.ToArray()
        GitFailed = $false
    }
}

function Invoke-ValidationProfileSelfTest {
    <#
    .SYNOPSIS
    Simulations controlees de selection : chaque cas est une liste de chemins et le
    verdict attendu. Aucune dependance a git ni a l'Editeur.
    #>
    param([Parameter(Mandatory)][string]$RepoRoot)

    $cases = @(
        @{ Name = 'documentation seule (docs + AGENTS)'; Paths = @('docs/setup/build-workflow-rules.md', 'AGENTS.md'); Expect = 'Fast' },
        @{ Name = 'artefacts BMAD et graphify'; Paths = @('_bmad-output/implementation-artifacts/spec-5-52.md', 'graphify-out/graph.json'); Expect = 'Fast' },
        @{ Name = 'code runtime de routage (hors geometrie)'; Paths = @('Assets/RoadRage/Features/Vehicles/LaneGraphRouting.cs', 'Assets/RoadRage/Features/Vehicles/NetworkedAIVehicleDriverController.cs'); Expect = 'Fast' },
        @{ Name = 'fixture Core existante'; Paths = @('Assets/RoadRage/Tests/EditMode/Story52BasicAiRouteFollowingAndRecoveryTests.cs'); Expect = 'Fast' },
        @{ Name = 'fixture PlayMode'; Paths = @('Assets/RoadRage/Tests/PlayMode/Story59ParameterizedDriverModelPlayModeTests.cs'); Expect = 'Fast' },
        @{ Name = 'helper de l''oracle'; Paths = @('Assets/RoadRage/Tests/EditMode/TrafficOracle/TrafficTraceComparer.cs'); Expect = 'Fast' },
        @{ Name = 'scene MVP_Run'; Paths = @('Assets/RoadRage/App/Scenes/MVP_Run.unity'); Expect = 'Full' },
        @{ Name = 'artefact du modele de route'; Paths = @('Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-model.json'); Expect = 'Full' },
        @{ Name = 'sign-off Gate A'; Paths = @('Assets/RoadRage/App/Scenes/MVP_Run/MVP_Run.road-signoff.json'); Expect = 'Full' },
        @{ Name = 'pipeline Traffic V2'; Paths = @('Assets/RoadRage/Features/Vehicles/Traffic/RoadModelCompiler.cs'); Expect = 'Full' },
        @{ Name = 'routage runtime (hors geometrie)'; Paths = @('Assets/RoadRage/Features/Vehicles/Traffic/Routing/RoutePlanner.cs'); Expect = 'Fast' },
        @{ Name = 'meta du dossier Routing'; Paths = @('Assets/RoadRage/Features/Vehicles/Traffic/Routing.meta'); Expect = 'Fast' },
        @{ Name = 'profil vehicule (asset)'; Paths = @('Assets/RoadRage/ScriptableObjects/Vehicles/VehicleProfileDef_Default.asset'); Expect = 'Full' },
        @{ Name = 'profil vehicule (code)'; Paths = @('Assets/RoadRage/Features/Vehicles/VehicleProfileDef.cs'); Expect = 'Full' },
        @{ Name = 'declarations trottoir (semantique)'; Paths = @('Assets/RoadRage/Features/Vehicles/SidewalkDeclarations.cs'); Expect = 'Full' },
        @{ Name = 'prefab'; Paths = @('Assets/RoadRage/Prefabs/Greybox_PlayerCar.prefab'); Expect = 'Full' },
        @{ Name = 'fixture geometrie'; Paths = @('Assets/RoadRage/Tests/EditMode/Story551JunctionClearanceTests.cs'); Expect = 'Full' },
        @{ Name = 'fixture 528 (porte A)'; Paths = @('Assets/RoadRage/Tests/EditMode/Story528AuthoringAndGateATests.cs'); Expect = 'Full' },
        @{ Name = 'ProjectSettings'; Paths = @('ProjectSettings/ProjectSettings.asset'); Expect = 'Full' },
        @{ Name = 'manifeste de packages'; Paths = @('Packages/manifest.json'); Expect = 'Full' },
        @{ Name = 'chemin inconnu (conservateur)'; Paths = @('Assets/RoadRage/Inconnu/NouveauTruc.cs'); Expect = 'Full' }
    )

    # Chemin inconnu HORS Assets/RoadRage : doit aussi etre conservateur.
    $cases += @{ Name = 'chemin inconnu hors RoadRage (conservateur)'; Paths = @('Assets/Inconnu/Mystere.asset'); Expect = 'Full' }
    # Fichier de test EditMode sans categorie (ou supprime) : conservateur.
    $cases += @{ Name = 'fichier EditMode sans categorie (conservateur)'; Paths = @('Assets/RoadRage/Tests/EditMode/NouvelleFixtureSansCategorie.cs'); Expect = 'Full' }
    # Un .meta suit son fichier cible.
    $cases += @{ Name = 'meta d''une fixture Core'; Paths = @('Assets/RoadRage/Tests/EditMode/TestSuiteCategoryPartitionTests.cs.meta'); Expect = 'Fast' }
    $cases += @{ Name = 'meta d''une fixture geometrie'; Paths = @('Assets/RoadRage/Tests/EditMode/Story550PairReviewTests.cs.meta'); Expect = 'Full' }

    # Le cas nominal 'developpement courant' ne doit PAS declencher la geometrie.
    $failed = 0
    foreach ($case in $cases) {
        $selection = Resolve-EditModeValidationSelection -Paths $case.Paths -RepoRoot $RepoRoot
        $ok = $selection.Decision -eq $case.Expect
        if ($ok) {
            Write-Host ("[PASS] {0} -> {1}" -f $case.Name, $selection.Decision)
        }
        else {
            $failed++
            Write-Host ("[FAIL] {0} -> {1} (attendu {2})" -f $case.Name, $selection.Decision, $case.Expect)
        }
    }

    # Couverture : deux categories distinctes, aucun chevauchement avec les helpers.
    if ($script:CoreCategory -eq $script:GeometryCategory) {
        $failed++
        Write-Host '[FAIL] Core et Geometry ne doivent pas etre identiques.'
    }

    Write-Host ("--- SelfTest : {0}/{1} cas conformes ---" -f ($cases.Count - $failed), $cases.Count)
    return $failed
}

if ($SelfTest) {
    $root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
    $failures = Invoke-ValidationProfileSelfTest -RepoRoot $root
    exit ([int]($failures -gt 0))
}
