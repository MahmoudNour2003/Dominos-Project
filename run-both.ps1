Param(
    [switch]$Build,
    [string]$Configuration = "Debug"
)

# Determine script and repo directories
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot = Resolve-Path -Path $scriptDir

# Local dotnet installed by dotnet-install.ps1
$dotnet = Join-Path $scriptDir 'local-dotnet\dotnet.exe'

if (!(Test-Path $dotnet)) {
    Write-Error "Local dotnet not found at: $dotnet`nRun dotnet-install.ps1 first or install an SDK and set DOTNET_ROOT."
    exit 1
}

if ($Build) {
    Write-Host "Building solution (Configuration=$Configuration)..."
    & $dotnet build (Join-Path $repoRoot 'Final Project.slnx') -c $Configuration
}

$serverProject = Join-Path $repoRoot 'DominoServer\DominoServer.csproj'
$clientProject = Join-Path $repoRoot 'DominoClient\DominoClient.csproj'

$serverCmd = "cd `"$repoRoot`"; `"$dotnet`" run --project `"$serverProject`" -c $Configuration"
$clientCmd = "cd `"$repoRoot`"; `"$dotnet`" run --project `"$clientProject`" -c $Configuration"

Write-Host "Starting server in a new PowerShell window..."
Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoExit','-Command',$serverCmd) -WindowStyle Normal
Start-Sleep -Milliseconds 500
Write-Host "Starting client in a new PowerShell window..."
Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoExit','-Command',$clientCmd) -WindowStyle Normal

Write-Host "Launched server and client (Configuration=$Configuration)."
