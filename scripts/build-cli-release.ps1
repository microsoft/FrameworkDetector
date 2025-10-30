param(
    [string] $OutputRoot = ".\bld\FrameworkDetector",
    [boolean]$Clean = $True
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

Write-Host "Build FrameworkDetector.CLI release..."
try {

    $OutputRoot = Resolve-Path $OutputRoot

    if ($Clean -and (Test-Path "$OutputRoot")) {
        Write-Host "Clean output folder..."
        Remove-Item "$OutputRoot" -Recurse | Out-Null
    }

    $GitCommitVersion = & git log -1 --date=format:"%y%j.%H%M" --format="%ad"

    New-Item -Path "$OutputRoot" -ItemType "directory" | Out-Null
    dotnet msbuild -target:Publish -p:RuntimeIdentifier=win-x64 -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:SelfContained=true -restore -p:Configuration=Release -p:PublishDir="$OutputRoot" -p:GitCommitVersion="$GitCommitVersion" "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Build failed!'
    }
} finally {
    Set-Location -Path "$StartingLocation"
}
