param(
    [boolean]$Clean = $True
)

[string] $RepoRoot = Resolve-Path "$PSScriptRoot\.."

[string] $OutputRoot = "bld"

$StartingLocation = Get-Location
Set-Location -Path $RepoRoot

$TargetOutputDirectory = "FrameworkDetector"

if ($Clean -and (Test-Path "$OutputRoot\$TargetOutputDirectory")) {
    Write-Host "Clean output folder..."
    Remove-Item "$OutputRoot\$TargetOutputDirectory" -Recurse | Out-Null
}

Write-Host "Build release..."
try
{
    New-Item -Path "$OutputRoot\$TargetOutputDirectory" -ItemType "directory" | Out-Null
    dotnet msbuild -target:Publish -p:RuntimeIdentifier=win-x64 -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:SelfContained=true -restore -p:Configuration=Release -p:PublishDir="$RepoRoot\$OutputRoot\$TargetOutputDirectory" "$RepoRoot\src\FrameworkDetector.sln"
    if (!$?) {
    	throw 'Build failed!'
    }
}
finally
{
    Set-Location -Path "$StartingLocation"
}