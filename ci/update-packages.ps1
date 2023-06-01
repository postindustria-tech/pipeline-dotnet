
param(
    [string]$ProjectDir = ".",
    [string]$Name,
    [Parameter(Mandatory=$true)]
    [string]$RepoName
)
$Solutions = @("FiftyOne.CloudRequestEngine.sln", "FiftyOne.Pipeline.Elements.sln", "FiftyOne.Pipeline.sln", "FiftyOne.Pipeline.Web.sln")

foreach($Solution in $Solutions){

./dotnet/run-update-dependencies.ps1 -RepoName $RepoName -ProjectDir $Solution -Name $Name

}

exit $LASTEXITCODE