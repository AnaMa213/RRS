<#
Precheck de symbole nomme -- AGENTS.md AD-3, docs/setup/build-workflow-rules.md section 1.

But : rendre deterministe et non ambigue la verification d'existence d'un symbole avant toute
requete Graphify. `rg` n'est PAS garanti present dans la session de l'agent (verifie absent le
2026-09-18) : une commande inconnue produit une sortie vide, qu'un agent peut lire comme
« symbole absent » et conclure STOP a tort. Ce script essaie `rg`, retombe sur `Select-String`
natif, et surtout distingue trois issues par un code de sortie dedie.

Codes de sortie :
  0  le symbole existe (les occurrences sont affichees)
  1  le symbole est absent -> STOP, ne pas interroger Graphify, corriger le nom
  2  la recherche elle-meme a echoue -> le resultat est inconnu, jamais « absent »

La distinction 1 / 2 est le point important : un outil de recherche casse ne doit jamais produire
la meme conclusion qu'une absence reelle.

Usage :
  .\scripts\find-symbol.ps1 DriverProfileDef
  .\scripts\find-symbol.ps1 NetworkedAIVehicleDriverController
  .\scripts\find-symbol.ps1 SomeType -Path Assets\RoadRage\Features\Vehicles
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)][string]$Symbol,
    [string]$Path = 'Assets/RoadRage',
    [string]$Include = '*.cs'
)

$ErrorActionPreference = 'Stop'
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$searchRoot = Join-Path $RepoRoot $Path

if (-not (Test-Path $searchRoot)) {
    Write-Host "ECHEC RECHERCHE: chemin introuvable : $searchRoot (resultat inconnu, pas 'absent')" -ForegroundColor Red
    exit 2
}

$pattern = "\b$([regex]::Escape($Symbol))\b"
# Les correspondances sont gardees comme objets (chemin + ligne + texte) : une decoupe sur ':'
# casserait sur les chemins Windows (D:\...), ce qui annoncait 1 fichier au lieu de 4.
$hits = @()
$tool = ''

if (Get-Command rg -ErrorAction SilentlyContinue) {
    $tool = 'rg'
    $raw = & rg --line-number --no-heading --color never --glob $Include $pattern $searchRoot 2>&1
    $rgExit = $LASTEXITCODE
    if ($rgExit -gt 1) {
        Write-Host "ECHEC RECHERCHE: rg a retourne le code $rgExit (resultat inconnu, pas 'absent')" -ForegroundColor Red
        $raw | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        exit 2
    }
    $hits = @($raw | Where-Object { $_ } | ForEach-Object {
        $parts = "$_" -split ':', 3
        [pscustomobject]@{ Path = $parts[0]; LineNumber = $parts[1]; Text = $parts[2] }
    })
}
else {
    # Repli natif : aucune dependance externe, disponible dans toute session PowerShell.
    $tool = 'Select-String (rg indisponible)'
    $hits = @(
        Get-ChildItem -Path $searchRoot -Recurse -Filter $Include -File |
            Select-String -Pattern $pattern |
            ForEach-Object { [pscustomobject]@{ Path = $_.Path; LineNumber = $_.LineNumber; Text = $_.Line.Trim() } }
    )
}

if ($hits.Count -eq 0) {
    Write-Host "ABSENT: aucun symbole '$Symbol' sous $Path [$tool]" -ForegroundColor Red
    Write-Host "STOP : ne pas interroger Graphify, corriger le nom (AD-3)." -ForegroundColor Red
    exit 1
}

$fileCount = @($hits | Select-Object -ExpandProperty Path -Unique).Count
Write-Host "TROUVE: '$Symbol' -- $($hits.Count) occurrence(s) dans $fileCount fichier(s) [$tool]" -ForegroundColor Green
$hits | Select-Object -First 40 | ForEach-Object { Write-Host "  $($_.Path):$($_.LineNumber): $($_.Text)" -ForegroundColor DarkGray }
if ($hits.Count -gt 40) {
    Write-Host "  ... $($hits.Count - 40) occurrence(s) de plus" -ForegroundColor DarkGray
}
Write-Host "Graphify peut etre interroge si une analyse d'impact est reellement necessaire." -ForegroundColor DarkGray
exit 0
