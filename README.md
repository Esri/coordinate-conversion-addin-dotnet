# coordinate-tool-addin-dotnet

This is an Esri ArcGIS Prototype Addin for ArcMAP 10.3 that can be used for converting coordinates.

![Image of Coordinate Tool Addin](ScreenShot.PNG) 

## Features

* Parses input coordinates and outputs multiple formats of the input coordinate
* Addin for ArcMAP 10.3
* Works with DD, DMS, GARS, MGRS, USNG, UTM

## Sections

* [Requirements](#requirements)
* [Instructions](#instructions)
* [Resources](#resources)
* [Issues](#issues)
* [Contributing](#contributing)
* [Licensing](#licensing)

## Requirements

* Visual Studio 2013
* ArcGIS Desktop SDK for .NET 10.3
	* [ArcGIS Runtime for .NET Requirements](https://desktop.arcgis.com/en/desktop/latest/get-started/system-requirements/arcobjects-sdk-system-requirements.htm)

## Instructions

### General Help

* [New to Github? Get started here.](http://htmlpreview.github.com/?https://github.com/Esri/esri.github.com/blob/master/help/esri-getting-to-know-github.html)

### Getting Started with the Coordinate Tool Addin (.NET)

* Building
	* To Build Using Visual Studio
		* Open and build solution file
	* To use MSBuild to build the solution
		* Open a Visual Studio Command Prompt: Start Menu | Visual Studio 2013 | Visual Studio Tools | Developer Command Prompt for VS2013
		* cd coordinate-tool-addin-dotnet\source\CoordinateTool
		* msbuild CoordinateTool.sln /property:Configuration=Release

* Running
	* Run or debug from Visual Studio
	* To run from a stand-alone deployment
		* ArcMAP
			* Install the add-in from the application folder by double clicking it
			* Optional : copy the "CoordinateToolLibrary.dll.config" file to the following directory
				* C:\Users\YOURUSERNAME\AppData\Local\ESRI\Desktop10.3\AssemblyCache\{19B92973-746A-4114-9232-3467CA1FC631}
			* Add the add-in command to a toolbar via menu option 
				* "Customize -> Customize mode"
				* Select "Commands" Tab
				* Select "Add-In Controls"
				* Drag/Drop "CoordinateTool" command onto a toolbar
				* Close customize mode
				* open tool by clicking the "Coordinate Tool" command you just added
				* dockable coordinate tool appears
		* Pro
			* Install the add-in from the application folder by double clicking it
			* Optional : copy the "CoordinateToolLibrary.dll.config" file to the following directory
				* C:\Users\YOURUSERNAME\AppData\Local\ESRI\ArcGISPro\AssemblyCache\{dfc85d8b-d2c1-405b-bd03-9a26740d842c}
			* The ADD-IN appears under the "ADD-IN" tab in Pro	
				* Click the "Show Coordinate Tool Dock Pane" button and the tool will appear
				
* Custom Formatters
	* Each coordinate type has its own custom formatter.  The following guide will help build a custom format string for each particular coordinate type.  If a character is not part of the formatting code it is simply transferred to the output string.
	* Formatting numbers
		* Numbers found in DD, DDM and DMS can be formatted using the first three ("0" Zero placeholder, "#" digit placeholder and "." decimal point) characters found here [Custom Numeric Format Strings](https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx)
		* Numbers found in GARS, MGRS, USNG and UTM can be formatted using the first two ("0" Zero placeholder and "#" digit placeholder) characters found here [Custom Numeric Format Strings](https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx)
	* Formatting string output for each coordinate type
		* DD Decimal Degrees
			* "X" = Longitude
			* "Y" = Latitude
			* "+" = add "+" prefix if number is positive
			* "-" = add "-" prefix if number is negative
			* "N" or "S" = add "N" or "S" direction based on latitude
			* "E" or "W" = add "E" or "W" direction based on longitude
			* Example : "+-Y0.0 +-X0.0" will output "+41.1 -78.2"
	
## Resources

* [ArcGIS 10.3 Help](http://resources.arcgis.com/en/help/)
* [ArcGIS Blog](http://blogs.esri.com/esri/arcgis/)
* ![Twitter](https://g.twimg.com/twitter-bird-16x16.png)[@EsriDefense](http://twitter.com/EsriDefense)
* [ArcGIS Solutions Website](http://solutions.arcgis.com/military/)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an [issue](https://github.com/ArcGIS/coordinate-tool-addin-dotnet/issues).

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

[](Esri Tags: Military Defense ArcGIS ArcObjects .NET Planning WPF ArcGISSolutions)
[](Esri Language: C#) 
