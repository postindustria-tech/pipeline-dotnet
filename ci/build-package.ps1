param(
    [string]$ProjectDir = ".",
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$Name = "Release_x64",
    [string]$Configuration = "CoreRelease",
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [Parameter(Mandatory=$true)]
    [Hashtable]$Keys

)

$Solutions = @("FiftyOne.CloudRequestEngine.sln", "FiftyOne.Pipeline.Elements.sln", "FiftyOne.Pipeline.sln", "FiftyOne.Pipeline.Web.sln")

foreach($Solution in $Solutions){

    ./dotnet/build-package-nuget.ps1 -RepoName $RepoName -Configuration "Release" -Version $Version -SolutionName $Solution -CodeSigningCert $Keys['CodeSigningCert'] -CodeSigningCertPassword $Keys['CodeSigningCertPassword'] -SearchPattern "^Project\(.*csproj"
}

exit $LASTEXITCODE
