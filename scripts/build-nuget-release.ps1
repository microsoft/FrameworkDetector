param(
    [string]$OutputRoot = ".\bld\FrameworkDetector",
    [switch]$Clean = $False
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Build FrameworkDetector NuGet..."
try {

    $OutputRoot = [System.IO.Path]::Combine($RepoRoot, $OutputRoot)

    if ($Clean -and (Test-Path "$OutputRoot")) {
        Write-Host "Clean output folder..."
        Remove-Item "$OutputRoot" -Recurse | Out-Null
    }

    if (-not (Test-Path "$OutputRoot")) {
        Write-Host "Create output folder..."
        New-Item -Path "$OutputRoot" -ItemType "directory" | Out-Null
    }

    $GitCommitVersion = & git log -1 --date=format:"%y%j.%H%M" --format="%ad"

    dotnet build -restore -target:pack -p:PackageOutputPath="$OutputRoot" -p:GitCommitVersion="$GitCommitVersion" "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Build failed!'
    }
} finally {
    Set-Location -Path "$StartingLocation"
}
