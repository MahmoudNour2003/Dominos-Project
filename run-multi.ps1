Param(
    [int]$Clients = 2,
    [switch]$Build,
    [string]$Configuration = "Debug"
)

# Resolve repo and dotnet
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Resolve-Path -Path $scriptDir
$dotnet = Join-Path $scriptDir 'local-dotnet\\dotnet.exe'

if (!(Test-Path $dotnet)) {
    Write-Error "Local dotnet not found at: $dotnet`nRun dotnet-install.ps1 first or install an SDK and set DOTNET_ROOT."
    exit 1
}

if ($Build) {
    Write-Host "Building solution (Configuration=$Configuration)..."
    & $dotnet build (Join-Path $repoRoot 'Final Project.slnx') -c $Configuration
}

$serverProject = Join-Path $repoRoot 'DominoServer\\DominoServer.csproj'
$clientProject = Join-Path $repoRoot 'DominoClient\\DominoClient.csproj'

$serverCmd = "cd `"$repoRoot`"; `"$dotnet`" run --project `"$serverProject`" -c $Configuration"

Write-Host "Starting server in a new PowerShell window..."
Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoExit','-Command',$serverCmd) -WindowStyle Normal
Start-Sleep -Milliseconds 500

for ($i = 1; $i -le $Clients; $i++) {
    $clientCmd = "cd `"$repoRoot`"; `"$dotnet`" run --project `"$clientProject`" -c $Configuration"
    Write-Host "Starting client #$i in a new PowerShell window..."
    Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoExit','-Command',$clientCmd) -WindowStyle Normal
    Start-Sleep -Milliseconds 300
}

Write-Host "Launched server and $Clients clients (Configuration=$Configuration)."
