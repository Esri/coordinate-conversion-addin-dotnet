# military-planner-application-csharp

This is an Esri ArcGIS Runtime for .NET Application Prototype Template that can be used for demonstrating military operations planning.

![Image of Military Planner Application](ScreenShot.PNG) 

## Features

* Time enabled layers, grouped by Courses of Action and Phases of the Operation
* Order of Battle Widget customizable for individual units
* What You See Is What You Get (WYSIWYG) editing of military symbols and control measures
* Military symbol search
* MIL-STD-2525C symbology

## Sections

* [Requirements](#requirements)
* [Instructions](#instructions)
* [Resources](#resources)
* [Issues](#issues)
* [Contributing](#contributing)
* [Licensing](#licensing)

## Requirements

* Visual Studio 2012 or later
* ArcGIS Runtime SDK for .NET 10.2.6 (Basic License)
	* [ArcGIS Runtime for .NET Requirements](https://developers.arcgis.com/net/desktop/guide/system-requirements.htm)

## Instructions

### General Help

* [New to Github? Get started here.](http://htmlpreview.github.com/?https://github.com/Esri/esri.github.com/blob/master/help/esri-getting-to-know-github.html)

### Getting Started with the Military Planner Application (.NET)

* Building
	* To Build Using Visual Studio
		* Open and build solution file
	* To use MSBuild to build the solution
		* Open a Visual Studio Command Prompt: Start Menu | Visual Studio 2013 | Visual Studio Tools | Developer Command Prompt for VS2013
		* cd military-planner-application-csharp\source
		* msbuild MilitaryPlanner.sln /property:Configuration=Release

* Running
	* Run or debug from Visual Studio
	* To run from a stand-alone deployment
        * A pre-built [application folder](./application) has been provided
		* You may add the runtime deployment folder (ex. `arcgisruntime10.2.6`) to this folder to quickly create a runtime deployment (ex. `./application/arcgisruntime10.2.6`)
        * Run MilitaryPlanner.exe from the deployment folder

## Resources

* [ArcGIS Runtime for .NET Resource Center](https://developers.arcgis.com/net/)
* [ArcGIS Blog](http://blogs.esri.com/esri/arcgis/)
* ![Twitter](https://g.twimg.com/twitter-bird-16x16.png)[@EsriDefense](http://twitter.com/EsriDefense)
* [ArcGIS Solutions Website](http://solutions.arcgis.com/military/)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an [issue](https://github.com/ArcGIS/military-planner-application-csharp/issues).

## Contributing

Anyone and everyone is welcome to contribute. Please see our [guidelines for contributing](https://github.com/esri/contributing).

## Licensing

Copyright 2015 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](license.txt) file.

[](Esri Tags: Military Defense ArcGIS Runtime .NET Planning WPF ArcGISSolutions)
[](Esri Language: C#) 
