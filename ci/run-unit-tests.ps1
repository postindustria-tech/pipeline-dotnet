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
$q = "FiftyOne.Pipeline.Web.Framework.Tests"
$TestResultPath = [IO.Path]::Combine($RepoPath, "test-results", $OutputFolder, $Name, "${q}_bin_${Configuration}")
$MyBinPath = [IO.Path]::Combine($RepoPath, "Web Integration", "Tests", $q, "bin", $Configuration)

Compress-Archive -Path $MyBinPath -DestinationPath $TestResultPath
Write-Output "Created $TestResultPath from $MyBinPath"

exit $result
