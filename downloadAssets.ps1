# Obtenir le chemin du dossier où se situe le script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# URL de telechargement Dropbox (avec tous les paramètres necessaires)
$dropboxUrl = "https://dl.dropboxusercontent.com/scl/fi/xnxbstujzil7jnr9keqpm/WanulCrazyData.zip?rlkey=22ub1duk6na797dc9gnktcmha&st=jzoefd40&dl=1"

# Nom de fichier de destination dans le dossier 'data'
$outputFile = Join-Path $scriptDir "data.zip"

# Utilisation de Start-BitsTransfer pour telecharger le fichier
Write-Host "Telechargement des textures depuis Dropbox..."
Start-BitsTransfer -Source $dropboxUrl -Destination $outputFile

# Verifier si le fichier a ete telecharge
if (Test-Path $outputFile) {
    Write-Host "Telechargement reussi."
    # Decompresser le fichier ZIP dans le dossier 'data/textures'
    $extractedDir = $scriptDir
    if (-not (Test-Path $extractedDir)) {
        New-Item -ItemType Directory -Path $extractedDir
    }
    try {
        Write-Host "Decompression des textures..."
        Expand-Archive -Path $outputFile -DestinationPath $extractedDir -Force
        Write-Host "Decompression terminee."
    } catch {
        Write-Host "Erreur lors de la decompression : $_"
    }
} else {
    Write-Host "Erreur lors du telechargement."
}

pause
