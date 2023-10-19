param(
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [string]$ProjectDir = ".",
    [string]$Name = "Release_x64",
    [string]$Configuration = "Release",
    [string]$Arch = "x64"
)

# workaround, see https://github.com/actions/runner-images/issues/8598
$env:PATH = $env:PATH -replace "C:\\Strawberry\\c\\bin;"

$RepoPath = [IO.Path]::Combine($pwd, $RepoName)
$CommonPath = $pwd
$PerfPath = [IO.Path]::Combine($RepoPath, "performance-tests")
$PerfResultsFile = [IO.Path]::Combine($RepoPath, "test-results", "performance-summary", "results_$Name.json")

Write-Output "Entering '$RepoPath'"
Push-Location $RepoPath

try {

    # Create the output directories if they don't already exist.
    if ($(Test-Path -Path "test-results") -eq  $False) {
        mkdir test-results
    }
    if ($(Test-Path -Path "test-results/performance") -eq  $False) {
        mkdir test-results/performance
    }
    if ($(Test-Path -Path "test-results/performance-summary") -eq  $False) {
        mkdir test-results/performance-summary
    }

}
finally {

    Write-Output "Leaving '$RepoPath'"
    Pop-Location

}


./dotnet/build-project-core.ps1 -RepoName $RepoName -ProjectDir "$PerfPath" -Name $Name -Configuration "Release" -Arch $Arch

if($IsLinux){
    #install APR library for linux
    sudo apt-get update
    sudo apt-get install apache2-dev libapr1-dev libaprutil1-dev
}

Write-Output "Entering '$PerfPath'"
Push-Location $PerfPath

try {
    mkdir build
    Push-Location build
    try {

        # Build the performance tests
        Write-Output "Building performance test"
        cmake ..
        cmake --build .

        Write-Output "Running performance test"
        if($IsLinux){
            sh ./runPerf.sh 
        }
        else{
            ./runPerf.ps1
        }
        

        Get-ChildItem -Path $PerfPath -Filter "summary.json" -File -Recurse | ForEach-Object {
            $destinationPath = Join-Path -Path $PerfPath/build -ChildPath $_.Name
            if (-not (Test-Path -Path $destinationPath)) {
                Copy-Item -Path $_.FullName -Destination $destinationPath -Force
                Write-Host "Copied $($_.Name) to $destinationPath"
            } else {
                Write-Host "File $($_.Name) already exists in $destinationPath. Skipping..."
            }
        }


        # Write out the results for comparison
        Write-Output "Writing performance test results"
        $Results = Get-Content ./summary.json | ConvertFrom-Json
        Write-Output "{
            'HigherIsBetter': {
                'DetectionsPerSecond': $(1/($Results.overhead_ms / 1000))
            },
            'LowerIsBetter': {
                'MsPerDetection': $($Results.overhead_ms)
            }
        }" > $PerfResultsFile

    }
    finally {

        Write-Output "Leaving build"
        Pop-Location

    }
}
finally {

    Write-Output "Leaving '$PerfPath'"
    Pop-Location

}
