# 51Degrees Pipeline API

![51Degrees](https://51degrees.com/DesktopModules/FiftyOne/Distributor/Logo.ashx?utm_source=github&utm_medium=repository&utm_content=readme_main&utm_campaign=dotnet-open-source "Data rewards the curious") **Pipeline API**


[Reference Documentation](https://51degrees.github.io/ "Reference documentation")

# Introduction
This repository contains all the projects required to build the .NET implementation of the Pipeline API.
Individual engines (For example, device detection) are in separate repositories.

## Pre-requesites

Visual Studio 2017 or later is recommended. Although Visual Studio Code can be used for working with most of the projects.

The Pipeline projects are written in C# and target .NET Standard 2.0.3 and .NET Core 2.1.

## Solutions and projects

- **FiftyOne.Pipeline** - The core projects that comprise the Pipeline API.
  - *FiftyOne.Pipeline.Core* - The core Pipeline classes such as Pipeline, FlowData, FlowElement and Evidence.
  - *FiftyOne.Pipeline.Engines* - Functionality for AspectEngines, a specialized FlowElement with additional features. 
  - *FiftyOne.Pipeline.Engines.FiftyOne* - Functionality that is specific to 51Degrees aspect engines.
- **FiftyOne.Pipeline.Web** - Projects that are relevant to the Pipeline API ASP.NET integration.
  - *FiftyOne.Pipeline.Web* - ASP.NET Core integration.
  - *FiftyOne.Pipeline.Web.Framework* - ASP.NET Framework integration.
  - *FiftyOne.Pipeline.Web.Shared* - Shared code that is used by both Core and Framework ASP.NET integrations.
- **FiftyOne.Pipeline.Elements** - Projects for various common Flow Elements that are used by multiple other solutions.
  - *FiftyOne.Pipeline.JavaScriptBuilder* - An element that packages values from all 'JavaScript' properties from all engines into a single JavaScript function.
  - *FiftyOne.Pipeline.JsonBuilder* - An element that serializes all properties from all engines into JSON format.
- **FiftyOne.CloudRequestEngine** - Projects related to making general requests to the 51Degrees cloud.
  - *FiftyOne.Pipeline.CloudRequestEngine* - An engine that makes requests to the 51Degrees cloud service.

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
  - *Example Website* - Shows how to use the Pipeline ASP.NET Core integration.
  - *Example Website Framework* - Shows how to use the Pipeline ASP.NET integration.

## Project documentation

For complete documentation on the Pipeline API and associated engines, see the [51Degrees documentation site][Documenation].

## Enable debugging of NuGet packages

In order to debug into NuGet packages, you must be using packages that reference debug symbols. By default, this includes all alpha packages but not beta or final versions.
If you have a debuggable package then you will need to configure Visual Studio to allow you to step into it:

- In tools -> options -> debugging -> symbols, add the Azure DevOps symbol server: 
![Visual Studio 2017 screenshot with symbol server added][ImageAddSymbolServer]
- Select the ‘Load only specified modules’ option at the bottom and configure it to only load Symbols for 51Degrees modules as shown below:
![Visual Studio 2017 configured to only load external symbols for 51Degrees libraries][ImageLoadOnlyFiftyone]
- In tools -> options -> debugging -> general, ensure that:
  - Enable Just My Code is off. Having this on will prevent VS stepping into any NuGet packages.
  - Enable source server support is on.
  - Example Source Link support is on.
![Visual Studio 2017 configured for debugging external packages][ImageConfigureDebugger]

When stepping into a method from a relevant NuGet package, you should now see the following warning message:
![Visual Studio 2017 Source Link download warning][ImageSourceLinkDownload]


[Documentation]: https://51degrees.github.io
[ImageAddSymbolServer]: file://Images/vs2017-add-symbol-server.png
[ImageConfigureDebugger]: file://Images/vs2017-configure-debugger.png
[ImageLoadOnlyFiftyone]: file://Images/vs2017-load-only-fiftyone.png
[ImageSourceLinkDownload]: file://Images/vs2017-source-link-download.png