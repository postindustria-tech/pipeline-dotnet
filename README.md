# 51Degrees Pipeline API

![51Degrees](https://51degrees.com/img/logo.png?utm_source=github&utm_medium=repository&utm_content=readme_main&utm_campaign=dotnet-open-source "Data rewards the curious") **Pipeline API**


[Developer Documentation](https://51degrees.com/documentation/4.1/index.html?utm_source=github&utm_medium=repository&utm_content=documentation&utm_campaign=dotnet-open-source "developer documentation")

# Introduction
This repository contains all the projects required to build the .NET implementation of the Pipeline API.
Individual engines (For example, device detection) are in separate repositories.

## Pre-requesites

Visual Studio 2019 or later is recommended. Although Visual Studio Code can be used for working with most of the projects.

The Pipeline projects are written in C# and target .NET Standard 2.0.3
The Web integration multi-targets the following:
    - .NET Core 2.1
    - .NET Core 3.1
    - .NET Framework 4.6.1
Test and example projects target .NET Core 3.1

## Solutions and projects

- **FiftyOne.Pipeline** - The core projects that comprise the Pipeline API.
  - *FiftyOne.Pipeline.Core* - The core Pipeline classes such as Pipeline, FlowData, FlowElement and Evidence.
  - *FiftyOne.Pipeline.Engines* - Functionality for AspectEngines, a specialized FlowElement with additional features. 
  - *FiftyOne.Pipeline.Engines.FiftyOne* - Functionality that is specific to 51Degrees aspect engines.
- **FiftyOne.Pipeline.Web** - Projects that are relevant to the Pipeline API ASP.NET integration.
  - *FiftyOne.Pipeline.Web* - ASP.NET Core integration.
  - *FiftyOne.Pipeline.Web.Framework* - ASP.NET Framework integration.
  - *FiftyOne.Pipeline.Web.Minify* - FlowElement which takes the JavaScript function from the JavaScriptBundler element and minifies it.
  - *FiftyOne.Pipeline.Web.Shared* - Shared code that is used by both Core and Framework ASP.NET integrations.
- **FiftyOne.Pipeline.Elements** - Projects for various common Flow Elements that are used by multiple other solutions.
  - *FiftyOne.Pipeline.JavaScriptBuilder* - An element that packages values from all 'JavaScript' properties from all engines into a single JavaScript function.
  - *FiftyOne.Pipeline.JsonBuilder* - An element that serializes all properties from all engines into JSON format.
- **FiftyOne.CloudRequestEngine** - Projects related to making general requests to the 51Degrees cloud.
  - *FiftyOne.Pipeline.CloudRequestEngine* - An engine that makes requests to the 51Degrees cloud service.

## Installation

You can either clone this repository and reference the projects locally or you can reference the [NuGet][nuget] packages directly.

```
Install-Package FiftyOne.Pipeline.Core
Install-Package FiftyOne.Pipeline.Engines
Install-Package FiftyOne.Pipeline.Engines.FiftyOne
Install-Package FiftyOne.Pipeline.Web
Install-Package FiftyOne.Pipeline.Web.Minify
Install-Package FiftyOne.Pipeline.JsonBuilder
Install-Package FiftyOne.Pipeline.JavaScriptBuilder
Install-Package FiftyOne.Pipeline.CloudRequestEngine
```

Note that the packages have dependencies on each other so you'll never need to install all of them individually.
For example, Installing `FiftyOne.Pipeline.Engines.FiftyOne` will automatically add `FiftyOne.Pipeline.Engines` and `FiftyOne.Pipeline.Core`.

## Examples

### Pipeline Examples

- **In FfityOne.Pipeline.sln**
  - *FiftyOne.Pipeline.Examples* - Shared code library that contains base classes that are used by the examples.
  - *FiftyOne.Pipeline.Examples.Caching* - Example that shows how the results caching feature of engines can be used.

- **In DeveloperExamples.sln**
  - *SimpleFlowElement* - Shows how to create a custom flow element that returns star sign based on a supplied date of birth.
  - *SimpleOnPremiseEngine* - Shows how to modify SimpleFlowElement to make use of the 'engine' functionality and use a custom data file to map dates to star signs rather than relying on hard coded data.
  - *SimpleClientSideElement* - Shows how to modify SimpleFlowElement to request the data of birth from the user using client-side JavaScript.
  - *SimpleClientSideElementMVC* - An example project showing how to use the code from SimpleClientSideElement in an ASP.NET Core web application.
  - *SimpleCloudEngine* - Shows how to modify SimpleFlowElement to perform the star sign lookup via a cloud service rather than locally.

- **In FiftyOne.Pipeline.Web.sln**
  - *AspNetCore 2.1 Example* - Shows how to use the Pipeline ASP.NET Core 2.1 integration.
  - *AspNetCore 3.1 Example* - Shows how to use the Pipeline ASP.NET Core 3.1 integration.
  - *Example Website Framework* - Shows how to use the Pipeline ASP.NET integration.

## Tests

- **FiftyOne.Pipeline.CloudRequestEngine.Tests** - Tests for the CloudRequestEngine and builder.
- **FiftyOne.Pipeline.Core.Tests** - Tests for FlowElement and FlowData base classes.
- **FiftyOne.Pipeline.Engines.Tests** - Tests for AspectEngines and AspectData base classes.
- **FiftyOne.Pipeline.Engines.FiftyOne.Tests** - Tests for 51Degrees specific aspect engines.
- **FiftyOne.Pipeline.Examples.Tests** - Tests for developer examples. This will automatically run all the examples and ensure they do not crash.
- **FiftyOne.Pipeline.Web.Tests** - Tests for web integration functionality.

The tests can be run from within Visual Studio or (in most cases) by using the `dotnet` command line tool. 

## Project documentation

For complete documentation on the Pipeline API and associated engines, see the [51Degrees documentation site][Documentation].

[Documentation]: https://51degrees.com/documentation/4.1/index.html
[nuget]: https://www.nuget.org/profiles/51Degrees