# API Specific CI/CD Approach
This API complies with the `common-ci` approach with the following exceptions:

## General
The project contains multiple solutions, and these are hardcoded in the scripts, allowing iteration by passing the solution name to the script through the ProjectDir parameter.

## Performance Tests

The script sets up the required directories to store the test results and builds the performance tests project using publish command to produce an executable file that is then passed to the script. If the operating system is Linux, it installs the APR library needed for the tests. It then proceeds to build the performance tests using CMake and generates the necessary executable.

From the generated "summary.json" file, relevant data is extracted to create a performance summary in JSON format. This summary includes DetectionsPerSecond and MsPerDetection metrics.
