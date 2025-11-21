param(
    [string]$Configuration = "Debug"
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Test FrameworkDetector..."
try {
    $GitCommitVersion = & git log -1 --date=format:"%y%j.%H%M" --format="%ad"

    dotnet test --configuration $Configuration -p:GitCommitVersion="$GitCommitVersion" "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Tests failed!'
    }
} finally {
    Set-Location -Path "$StartingLocation"
}
