$RepoRoot = $PSScriptRoot

if(!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "Docker is not installed. Please install Docker to build the project."
    exit 1
}

Write-Host "Building proxy docker image..."
docker build -t ghcr.io/analogrelay/cosmos-proxy:local -f $RepoRoot/Dockerfile $RepoRoot
