param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64",
    [string]$BuildMethod = "msbuild"
)

exit 0

$Solutions = @("FiftyOne.CloudRequestEngine.sln", "FiftyOne.Pipeline.Elements.sln", "FiftyOne.Pipeline.sln", "FiftyOne.Pipeline.Web.sln")

foreach($Solution in $Solutions){
    if ($BuildMethod -eq "dotnet"){

        ./dotnet/build-project-core.ps1 -RepoName $RepoName -ProjectDir $Solution -Name $Name -Configuration $Configuration -Arch $Arch
    
    }
    else{
    
        ./dotnet/build-project-framework.ps1 -RepoName $RepoName -ProjectDir $Solution -Name $Name -Configuration $Configuration -Arch $Arch
    }
}


exit $LASTEXITCODE