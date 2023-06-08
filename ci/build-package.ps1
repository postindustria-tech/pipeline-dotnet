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

$Solutions = @("FiftyOne.CloudRequestEngine.sln", "FiftyOne.Pipeline.Elements.sln", "FiftyOne.Pipeline.sln")

foreach($Solution in $Solutions){

    ./dotnet/build-package-nuget.ps1 -RepoName $RepoName -Configuration "Release" -Version $Version -SolutionName $Solution -CodeSigningCert $Keys['CodeSigningCert'] -CodeSigningCertPassword $Keys['CodeSigningCertPassword'] -SearchPattern "^Project\(.*csproj"
}

# Now build the web package using the NuSpec file, as this is handled differently.

$NuspecPath = [IO.Path]::Combine($pwd, $RepoName, "Web Integration", "FiftyOne.Pipeline.Web.nuspec")
$CorePath = [IO.Path]::Combine($pwd, $RepoName, "Web Integration", "FiftyOne.Pipeline.Web")
$WebSolutionPath = [IO.Path]::Combine($pwd, $RepoName, "FiftyOne.Pipeline.Web.sln")

./environments/setup-msbuild.ps1
./dotnet/build-project-core.ps1 -RepoName $RepoName -ProjectDir $CorePath -Name $Name -Configuration "Release"
./dotnet/build-project-framework.ps1 -RepoName $RepoName -ProjectDir $WebSolutionPath -Name $Name -Configuration "Release" -Arch "Any CPU"
./dotnet/build-package-nuspec.ps1 -RepoName $RepoName -Configuration "Release" -Version $Version -NuspecPath  $NuspecPath -CodeSigningCert $Keys['CodeSigningCert'] -CodeSigningCertPassword $Keys['CodeSigningCertPassword']

exit $LASTEXITCODE
