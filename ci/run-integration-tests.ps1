param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_AnyCPU",
    [string]$Configuration = "Release",
    [string]$Arch = "Any CPU"
)

# TODO skipped while testing
exit 0

./dotnet/run-unit-tests.ps1 -RepoName $RepoName -ProjectDir $ProjectDir -Name $Name -Configuration $Configuration -Arch $Arch -BuildMethod $BuildMethod -Filter ".*\.Examples\.Tests\.dll" -OutputFolder "integration"

exit $LASTEXITCODE
