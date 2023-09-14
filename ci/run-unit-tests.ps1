param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$BuildMethod = "dotnet"
)

./dotnet/run-unit-tests.ps1 -RepoName $RepoName -ProjectDir $ProjectDir -Name $Name -Configuration $Configuration -Arch $Arch -BuildMethod $BuildMethod -Filter ".*Tests(|\.Core|\.Web)\.dll"

$result=$LASTEXITCODE

$RepoPath = [IO.Path]::Combine($pwd, $RepoName)
$TestName = "FiftyOne.Pipeline.Web.Framework.Tests"
$ArtifactsLocation = [IO.Path]::Combine($RepoPath, "artifacts", $OutputFolder, $Name)
$zip_uuid = New-Guid
$ArtifactPath = [IO.Path]::Combine($ArtifactsLocation, "${TestName}_bin_${Configuration}_${zip_uuid}.zip")
$MyBinPath = [IO.Path]::Combine($RepoPath, "Web Integration", "Tests", $TestName, "bin", $Configuration)

try {
    mkdir -p $ArtifactsLocation
    Compress-Archive -Path $MyBinPath -DestinationPath $ArtifactPath
}
finally {
}

exit $result
