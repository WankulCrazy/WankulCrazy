# Obtenir le chemin du dossier où se situe le script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# URL de telechargement Dropbox (avec tous les paramètres necessaires)
$dropboxUrl = "https://dl.dropboxusercontent.com/scl/fi/g2q7x17kiflfm5nucfc7s/WankulCrazyData-v0.0.7.zip?rlkey=0jwm9bzee5an76jbytp4m3g6z&st=qe9syuqx&dl=1"
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
