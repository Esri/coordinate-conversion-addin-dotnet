Coordinate Conversion Widget: Unit testing
==========================================

The Coordinate Conversion Widget uses [Intern](http://theintern.io/) as its test runner.  Unit tests can be
executed directly from a browser.  To run functional tests, you will have to use either a hosted environment such as
[Sauce Labs](https://saucelabs.com/).

## Instructions

### Getting Intern

Install the latest version of Intern 2.x using npm at the web folder level.

```
npm install intern
```

The following sections assume that the lib, test, and node_modules(intern) folders all exist within the same parent
folder(web).

### Running the units tests

#### From a browser

1. Create a test WAB app using Web AppBuilder
2. Download the test WAB app and extract to a folder
3. Create a virtual directory pointing to the location of the test WAB app
4. Deploy the CoordinateConversion widget to the test WAB app widgets folder 
5. Install Intern components to the root folder to a web server. Note: NPM needs to be installed before calling
	```
	npm install intern
	```
6. Copy the test folder to the root folder of the test WAB app
7. Open a browser and navigate to http://hostname/path-to-distance-and-direction-folder/node_modules/intern/client.html?config=widgets/CoordinateConversion/tests/intern
8. View the results in the browser console window. 

#### Through a hosted unit test environment

The included intern configuration is not setup to use in a hosted environment.  To use a hosted environment such as
Sauce Labs see the [Configuring Intern](https://github.com/theintern/intern/wiki/Configuring-Intern) wiki page.

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Esri welcomes contributions from anyone and everyone. Please see our
[guidelines for contributing](https://github.com/esri/contributing).

All web components produced follow [Esri's tailcoat](http://arcgis.github.io/tailcoat/styleguides/css/) style guide.

If you are using [JS Hint](http://http://www.jshint.com/) there is a .jshintrc file included in the root folder which
enforces this style.

## Licensing

Copyright 2014 Esri

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance
with the License.  You may obtain a copy of the License at
[http://www.apache.org/licenses/LICENSE-2.0](http://www.apache.org/licenses/LICENSE-2.0)

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
specific language governing permissions and limitations under the License.