define([
  'intern!object',
  'intern/chai!assert',
  'dojo/dom-construct',
  'dojo/_base/window',
  'esri/map',
  'esri/geometry/Extent',
  'CC/Widget',
  'CC/CoordinateControl',
  'CC/util',
  'dojo/promise/all',
  'dojo/_base/lang',
  'dojo/_base/Deferred'
], function(registerSuite, assert, domConstruct, win, Map, Extent, CoordinateConversion, CoordinateControl, CCUtil, dojoAll, lang, Deferred) {
  // local vars scoped to this module
  var map, ccUtil, coordinateConversion, cc;
  var dms2,dms3,ds,ds2,dp,ns,pLat,pLon,pss,ms,ss;
  var notations;
  var totalTestCount = 0;
  var latDDArray = [];
  var lonDDArray = [];
  var latDDMArray = [];
  var lonDDMArray = [];
  var latDMSArray = [];
  var lonDMSArray = [];  
   
  registerSuite({
    name: 'Coordinate Conversion Widget',
     // before the suite starts
    setup: function() {
      // load claro and esri css, create a map div in the body, and create map and Coordinate Conversion objects for our tests
      domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.19/esri/css/esri.css">', win.doc.getElementsByTagName("head")[0], 'last');
      domConstruct.place('<link rel="stylesheet" type="text/css" href="//js.arcgis.com/3.19/dijit/themes/claro/claro.css">', win.doc.getElementsByTagName("head")[0], 'last');
      domConstruct.place('<script src="http://js.arcgis.com/3.19/"></script>', win.doc.getElementsByTagName("head")[0], 'last');
      domConstruct.place('<div id="map" style="width:300px;height:200px;" class="claro"></div>', win.body(), 'only');
      domConstruct.place('<div id="ccNode" style="width:300px;" class="claro"></div>', win.body(), 'last');

      map = new Map("map", {
        basemap: "topo",
        center: [-122.45, 37.75],
        zoom: 13,
        sliderStyle: "small",
        extent: new Extent({xmin:-180,ymin:-90,xmax:180,ymax:90,spatialReference:{wkid:4326}})
      });
      
      coordinateConversion = new CoordinateConversion({
            appConfig: {geomService: {url: "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer"},
                        geometryService: "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer"},
            parentWidget: this,
            map: map,
            input: true,
            type: 'DD',
            config: {
              coordinateconversion: {
                zoomScale: 50000,
                initialCoords: ["DDM", "DMS", "MGRS", "UTM"],
                geometryService: {
                   url: "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer"
                }
              }   
            }         
          }, domConstruct.create("div")).placeAt("ccNode");
      
      
      cc = new CoordinateControl({                
                parentWidget: coordinateConversion,
                input: true,
                type: 'DD'
            });
       
      ccUtil = new CCUtil({appConfig: {
        geometryService: "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer",
        coordinateconversion: {                  
          geometryService: {url: "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Utilities/Geometry/GeometryServer"}
        }   
      }});
      
      notations = ccUtil.getNotations();
       
      //populate the arrays that will be used in the tests       
      //dms2 = degrees/minutes/seconds two figures
      dms2 = ['0','00'];
      //dms3 = degrees/minutes/seconds three figures
      dms3 = ['0','00','000'];
      //ds = degree symbol      
      ds = ['','°','˚','º','^','~','*'];
      //there has to be some seperator between degrees and minute values
      ds2 = [' ','°','˚','º','^','~','*','-','_']; 
      //ms = minute symbol      
      ms = ["","'","′"];       
      //there has to be some seperator between minute and second values
      ms2 = [' ',"'","′"];      
      //ms = second symbol
      ss = ['"','¨','˝'];            
      //dp = decimal place 
      //just test a single decimal place using both comma and decimal point
      dp = ['','.0',',0'];
      //ns = number seperator
      //we know that a comma seperator used with a comma for decimal degrees will fail so do not test for this
      ns = [' ',':',';','|','/','\\'];
      //pLat = prefix / suffix latitude - test lower and upper case
      pLat = ['','n','S','+','-'];
      //pLon = prefix / suffix longitude
      pLon = ['','E','w','+','-'];
      //pss = prefix / suffix spacer
      pss = ['',' '];

       
      //set up an array of each combination of DD latitude values
      for (var a = 0; a < dms2.length; a++) {
        for (var b = 0; b < dp.length; b++) {
          for (var c = 0; c < ds.length; c++) {
            latDDArray.push(dms2[a] + dp[b] + ds[c]);            
          }
        }                   
      }
      //set up an array of each combination of DD longitude values
      for (var a = 0; a < dms3.length; a++) {
        for (var b = 0; b < dp.length; b++) {
          for (var c = 0; c < ds.length; c++) {
            lonDDArray.push(dms3[a] + dp[b] + ds[c]);            
          }
        }                   
      }
      
      //set up an array of each combination of DDM latitude values
      for (var a = 0; a < dms2.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < dp.length; d++) {
              for (var e = 0; e < ms.length; e++) {
                latDDMArray.push(dms2[a] + ds2[b] + dms2[c] + dp[d] + ms[e]);                
              }
            }                   
          }
        }
      }

      //set up an array of each combination of DDM longitude values
      for (var a = 0; a < dms3.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < dp.length; d++) {
              for (var e = 0; e < ms.length; e++) {
                lonDDMArray.push(dms3[a] + ds2[b] + dms2[c] + dp[d] + ms[e]);                
              }
            }                   
          }
        }
      }
      
      //set up an array of each combination of DMS latitude values
      for (var a = 0; a < dms2.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < ms2.length; d++) {
              for (var e = 0; e < dms2.length; e++) {
                for (var f = 0; f < dp.length; f++) {
                  for (var g = 0; g < ss.length; g++) {
                    latDMSArray.push(dms2[a] + ds2[b] + dms2[c] + ms2[d] + dms2[e] + dp[f] + ss[g]);                
                  }
                }
              }
            }                   
          }
        }
      }
      
      //set up an array of each combination of DMS longitude values
      for (var a = 0; a < dms3.length; a++) {
        for (var b = 0; b < ds2.length; b++) {
          for (var c = 0; c < dms2.length; c++) {
            for (var d = 0; d < ms2.length; d++) {
              for (var e = 0; e < dms2.length; e++) {
                for (var f = 0; f < dp.length; f++) {
                  for (var g = 0; g < ss.length; g++) {
                    lonDMSArray.push(dms3[a] + ds2[b] + dms2[c] + ms2[d] + dms2[e] + dp[f] + ss[g]);                
                  }
                }
              }
            }                   
          }
        }
      }
      
      jsonLoader = function loadTests(file, callback) {
          var rawFile = new XMLHttpRequest();
          rawFile.overrideMimeType("application/json");
          rawFile.open("GET", file, false);
          rawFile.onreadystatechange = function() {
              if (rawFile.readyState === 4 && rawFile.status == "200") {
                  callback(rawFile.responseText);
              }
          }
          rawFile.send(null);
      }
      
      roundNumber = function round(value, decimals) {
        return Number(Math.round(value+'e'+decimals)+'e-'+decimals);
      }
    },

    // before each test executes
    beforeEach: function() {
      
    },

    // after the suite is done (all tests)
    teardown: function() {
      if (map.loaded) {
        map.destroy();   
      }
      if (coordinateConversion) {
        coordinateConversion.destroy();
      }
      console.log("Total number of tests conducted is: " + totalTestCount);      
    },
    
    'Test Manual Input: Convert DDM to Lat/Long': function() {
      //test to ensure inputed DDM is converted correctly to Lat/Long (4 Decimal Places)
      //tests held in file: toGeoFromDDM.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var DDM2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromDDM.json", lang.hitch(this,function(response){
        DDM2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < DDM2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(DDM2geo.tests[i].testString,'DDM'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],4), roundNumber(DDM2geo.tests[i].lon,4), DDM2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],4), roundNumber(DDM2geo.tests[i].lat,4), DDM2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert DDM to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up Array
        DDM2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert DMS to Lat/Long': function() {
      //test to ensure inputed DMS is converted correctly to Lat/Long (2 Decimal Places)
      //tests held in file: toGeoFromDMS.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var DMS2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromDMS.json", lang.hitch(this,function(response){
        DMS2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < DMS2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(DMS2geo.tests[i].testString,'DMS'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],2), roundNumber(DMS2geo.tests[i].lon,2), DMS2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],2), roundNumber(DMS2geo.tests[i].lat,2), DMS2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert DMS to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up Array
        DMS2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert UTM (Band) to Lat/Long': function() {
      //test to ensure inputed UTM (using band letters) is converted correctly to Lat/Long (3 Decimal Places)
      //tests held in file: toGeoFromUTMBand.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var UTM2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromUTMBand.json", lang.hitch(this,function(response){
        UTM2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < UTM2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(UTM2geo.tests[i].testString,'UTM'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],3), roundNumber(UTM2geo.tests[i].lon,3), UTM2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],3), roundNumber(UTM2geo.tests[i].lat,3), UTM2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert UTM (Band) to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up UTM2geo Array
        UTM2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert UTM (Hemisphere) to Lat/Long': function() {
      //test to ensure inputed UTM (using hemisphere letters) is converted correctly to Lat/Long (3 Decimal Places)
      //tests held in file: toGeoFromUTMHem.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var UTMHem2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromUTMHem.json", lang.hitch(this,function(response){
        UTMHem2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < UTMHem2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(UTMHem2geo.tests[i].testString,'UTM (H)'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],3), roundNumber(UTMHem2geo.tests[i].lon,3), UTMHem2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],3), roundNumber(UTMHem2geo.tests[i].lat,3), UTMHem2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert UTM (Hemisphere) to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up UTMHem2geo Array
        UTMHem2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert GEOREF to Lat/Long': function() {
      //test to ensure inputed GEOREF is converted correctly to Lat/Long (3 Decimal Places)
      //tests held in file: toGeoFromGEOREF.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var GEOREF2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromGEOREF.json", lang.hitch(this,function(response){
        GEOREF2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < GEOREF2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(GEOREF2geo.tests[i].testString,'GEOREF'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],3), roundNumber(GEOREF2geo.tests[i].lon,3), GEOREF2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],3), roundNumber(GEOREF2geo.tests[i].lat,3), GEOREF2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert GEOREF to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up GEOREF2geo Array
        GEOREF2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert GARS to Lat/Long': function() {
      //test to ensure inputed GARS is converted correctly to Lat/Long
      //tests held in file: toGeoFromGARS.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var GARS2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromGARS.json", lang.hitch(this,function(response){
        GARS2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < GARS2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(GARS2geo.tests[i].testString,'GARS'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i][0][0], GARS2geo.tests[i].lon, GARS2geo.tests[i].testNumber + " Failed");
          assert.equal(itm[i][0][1], GARS2geo.tests[i].lat, GARS2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert GARS to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up GARS2geo Array
        GARS2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert MGRS to Lat/Long': function() {
      //test to ensure inputed MGRS is converted correctly to Lat/Long (6 Decimal Places)
      //tests held in file: toGeoFromMGRS.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var MGRS2geo = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/toGeoFromMGRS.json", lang.hitch(this,function(response){
        MGRS2geo = JSON.parse(response);
      }));
       
      for (var i = 0; i < MGRS2geo.tests.length; i++) {
        returnArray.push(ccUtil.getXYNotation(MGRS2geo.tests[i].testString,'MGRS'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(roundNumber(itm[i][0][0],6), roundNumber(MGRS2geo.tests[i].lon,6), MGRS2geo.tests[i].testNumber + " Failed");
          assert.equal(roundNumber(itm[i][0][1],6), roundNumber(MGRS2geo.tests[i].lat,6), MGRS2geo.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert MGRS to Lat/Long conducted was: " + count);
        totalTestCount = totalTestCount + count;
        //clean up MGRS2geo Array
        MGRS2geo = null;
      }));  
    },
    
    'Test Manual Input: Convert Lat/Long to DDM': function() {
      //test to ensure inputed Lat/Long is converted correctly to DDM
      //tests held in file: geo2DDM.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2DDM = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2DDM.json", lang.hitch(this,function(response){
        geo2DDM = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2DDM.tests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2DDM.tests[i].testString,'DDM',4));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2DDM.tests[i].OUTPUT, geo2DDM.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to DDM conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up Array
        geo2DDM = null;
      }));  
    },

    'Test Manual Input: Convert Lat/Long to DMS': function() {
      //test to ensure inputed Lat/Long is converted correctly to DMS      
      //tests held in file: geo2DMS.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2DMS = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2DMS.json", lang.hitch(this,function(response){
        geo2DMS = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2DMS.tests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2DMS.tests[i].testString,'DMS',2));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2DMS.tests[i].OUTPUT, geo2DMS.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to DMS conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up Array
        geo2DMS = null;
      }));  
    },
    
    'Test Manual Input: Convert Lat/Long to UTM (Band)': function() {
      //test to ensure inputed Lat/Long is converted correctly to UTM (Band)      
      //tests held in file: geo2UTMBand.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2UTMBand = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2UTMBand.json", lang.hitch(this,function(response){
        geo2UTMBand = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2UTMBand.tests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2UTMBand.tests[i].testString,'UTM'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2UTMBand.tests[i].OUTPUT, geo2UTMBand.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to UTM (Band) conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up Array
        geo2UTMBand = null;
      }));  
    },
    
    'Test Manual Input: Convert Lat/Long to UTM (Hemisphere)': function() {
      //test to ensure inputed Lat/Long is converted correctly to UTM (Hemisphere)      
      //tests held in file: geo2UTMHem.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2UTMHem = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2UTMHem.json", lang.hitch(this,function(response){
        geo2UTMHem = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2UTMHem.tests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2UTMHem.tests[i].testString,'UTM (H)'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2UTMHem.tests[i].OUTPUT, geo2UTMHem.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to UTM (Hemisphere) conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up Array
        geo2UTMHem = null;
      }));  
    },
    
    'Test Manual Input: Convert Lat/Long to GEOREF': function() {
      //test to ensure inputed Lat/Long is converted correctly to GEOREF     
      //tests held in file: geo2GEOREF.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2GEOREF = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2GEOREF.json", lang.hitch(this,function(response){
        geo2GEOREF = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2GEOREF.tests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2GEOREF.tests[i].testString,'GEOREF'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2GEOREF.tests[i].OUTPUT, geo2GEOREF.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to GEOREF conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up Array
        geo2GEOREF = null;
      }));  
    },
    
    'Test Manual Input: Convert Lat/Long to GARS': function() {
      //test to ensure inputed Lat/Long is converted correctly to GARS     
      //tests held in file: geo2GARS.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2GARS = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2GARS.json", lang.hitch(this,function(response){
        geo2GARS = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2GARS.tests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2GARS.tests[i].testString,'GARS'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2GARS.tests[i].OUTPUT, geo2GARS.tests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to GARS conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up Array
        geo2GARS = null;
      }));  
    },
    
    'Test Manual Input: Convert Lat/Long to MGRS': function() {
      //test to ensure inputed Lat/Long is converted correctly to MGRS
      
      //tests held in file: geo2mgrs.json
      
      //this.skip('Skip test for now');  
      var count = 0;
      var geo2MGRStests = null;
      var dfd = this.async();
      var returnArray = [];
      
      //read in tests from the json file
      jsonLoader("../../widgets/CoordinateConversion/tests/fromGeo2mgrs.json", lang.hitch(this,function(response){
        geo2MGRStests = JSON.parse(response);
      }));
       
      for (var i = 0; i < geo2MGRStests.MGRSTests.length; i++) {
        returnArray.push(ccUtil.getCoordValues(geo2MGRStests.MGRSTests[i].testString,'MGRS'));          
      }
      
      dojoAll(returnArray).then(dfd.callback(function (itm) {
        for (var i = 0; i < itm.length; i++) {
          assert.equal(itm[i], geo2MGRStests.MGRSTests[i].OUTPUT, geo2MGRStests.MGRSTests[i].testNumber + " Failed");
          count++;          
        }
        console.log("The number of manual tests conducted for Convert Lat/Long to MGRS conducted was: " + count);
        totalTestCount = totalTestCount + count;        
        //clean up MGRStests Array
        geo2MGRStests = null;
      }));  
    },    
    
    'Test Auto Input: Identify Input as DD - Lat / Long': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;       
    
      //test each the DD latitude and longitude array items against each other using each of the seperators
      for (var a = 0; a < latDDArray.length; a++) {
        for (var b = 0; b < lonDDArray.length; b++) {
          for (var c = 0; c < ns.length; c++) {
            ccUtil.getCoordinateType(latDDArray[a] + ns[c] + lonDDArray[b]).then(function(itm){
             /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
             ** https://theintern.github.io/intern/#async-tests
             ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
             ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
             */
             itm && itm[0].name == 'DD'?passed=true:passed=false;
             //execute the reg ex and store in the variable match
             match = itm[0].pattern.exec(latDDArray[a].toUpperCase() + ns[c] + lonDDArray[b].toUpperCase());    
            });         
            //test to see if the regular expression identified the input as a valid input and identified it as DD (for decimal degrees)
            assert.isTrue(passed, latDDArray[a] + ns[c] + lonDDArray[b] + ' did not validate as DD Lat/Long');
            //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
            assert.equal(latDDArray[a].toUpperCase(), match[1], latDDArray[a] + ns[c] + lonDDArray[b] + " Failed");
            assert.equal(lonDDArray[b].toUpperCase(), match[9], latDDArray[a] + ns[c] + lonDDArray[b] + " Failed");
            //test to see if the regular expression has correctly identified the seperator
            assert.equal(ns[c], match[8], "Matching the seperator failed");
            //reset passed
            passed = false;
            count++;
          }
        }                   
      }
      
      //we have tested each combination of numbers so lets just test a single combination with each possible prefix/suffix combo
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000.0" + pss[e] + pLon[f].toUpperCase();                  
                  ccUtil.getCoordinateType(tempLat + (" ") + tempLon).then(function(itm){
                  /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
                  ** https://theintern.github.io/intern/#async-tests
                  ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
                  ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
                  */
                  itm && itm[0].name == 'DD'?passed=true:passed=false;
                  //execute the reg ex and store in the variable match
                  match = itm[0].pattern.exec(tempLat.toUpperCase() + (" ") + tempLon.toUpperCase());    
                  });         
                  //test to see if the regular expression identified the input as a valid input and identified it as DDM (for degrees decimal minutes)
                  assert.isTrue(passed, tempLat + (" ") + tempLon + ' did not validate as DD Lat/Long');
                  //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
                  assert.equal(tempLat.toUpperCase(), match[1], tempLat + (" ") + tempLon + " Failed ");
                  assert.equal(tempLon.toUpperCase(), match[9], tempLat + (" ") + tempLon + " Failed ");
                  //test to see if the regular expression has correctly identified the seperator
                  assert.equal(" ", match[8], "Matching the seperator failed");
                  //reset passed
                  passed = false;
                  count++;                                 
                }
              }
            }
          }
        }
      }
      console.log("The number of Auto tests conducted for Identify Input as DD - Lat / Long tests conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
     
    'Test Auto Input: Identify Input as DD - Long / Lat': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;       
      
      //test each the DD latitude and longitude array items against each other using each of the seperators
      for (var a = 0; a < latDDArray.length; a++) {
        for (var b = 0; b < lonDDArray.length; b++) {
          for (var c = 0; c < ns.length; c++) {
          ccUtil.getCoordinateType(lonDDArray[b] + ns[c] + latDDArray[a]).then(function(itm){
           /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
           ** https://theintern.github.io/intern/#async-tests
           ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
           ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
           */
           if (itm.length == 1) {
            itm && itm[0].name == 'DDrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[0].pattern.exec(lonDDArray[b].toUpperCase() + ns[c] + latDDArray[a].toUpperCase());
           } else {
            itm && itm[1].name == 'DDrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[1].pattern.exec(lonDDArray[b].toUpperCase() + ns[c] + latDDArray[a].toUpperCase());
           }
          });         
          //test to see if the regular expression identified the input as a valid input and identified it as DD (for decimal degrees)
          assert.isTrue(passed, lonDDArray[b] + ns[c] + latDDArray[a] + ' did not validate as DD Long/Lat');
          //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
          assert.equal(lonDDArray[b].toUpperCase(), match[1], lonDDArray[b] + ns[c] + latDDArray[a] + " Failed");
          assert.equal(latDDArray[a].toUpperCase(), match[10], lonDDArray[b] + ns[c] + latDDArray[a] + " Failed");
          //test to see if the regular expression has correctly identified the seperator
          assert.equal(ns[c], match[9], "Matching the seperator failed");
          //reset passed
          passed = false;
          count++;
          }
        }                   
      }
      
      //we have tested each combination of numbers so lets just test a single combination with each possible prefix/suffix combo
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {
                  var tempLon = pLon[d].toUpperCase() + "000.0" + pss[e] + pLon[f].toUpperCase();                 
                  var tempLat = pLat[a].toUpperCase() + "00.0" + pss[b] + pLat[c].toUpperCase(); 
                  ccUtil.getCoordinateType(tempLon + (" ") + tempLat).then(function(itm){
                  /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
                  ** https://theintern.github.io/intern/#async-tests
                  ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
                  ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
                  */
                  itm && itm[0].name == 'DDrev'?passed=true:passed=false;
                  //execute the reg ex and store in the variable match
                  match = itm[0].pattern.exec(tempLon.toUpperCase() + (" ") + tempLat.toUpperCase());    
                  });         
                  //test to see if the regular expression identified the input as a valid input and identified it as DD (for decimal degrees)
                  assert.isTrue(passed, tempLon + (" ") + tempLat + ' did not validate as DD Lat/Long');
                  //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
                  assert.equal(tempLon.toUpperCase(), match[1], tempLon + (" ") + tempLat + " Failed ");
                  assert.equal(tempLat.toUpperCase(), match[10], tempLon + (" ") + tempLat + " Failed ");
                  //test to see if the regular expression has correctly identified the seperator
                  assert.equal(" ", match[9], "Matching the seperator failed");
                  //reset passed
                  passed = false;
                  count++;                  
                }
              }
            }
          }
        }
      }
      console.log("The number of Auto tests conducted for Identify Input as DD - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Identify Input as DDM - Lat / Long': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;       
    
      //test each the DD latitude and longitude array items against each other using each of the seperators
      for (var a = 0; a < latDDMArray.length; a++) {
        for (var b = 0; b < lonDDMArray.length; b++) {
          for (var c = 0; c < ns.length; c++) {
            ccUtil.getCoordinateType(latDDMArray[a] + ns[c] + lonDDMArray[b]).then(function(itm){
             /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
             ** https://theintern.github.io/intern/#async-tests
             ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
             ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
             */
             itm && itm[0].name == 'DDM'?passed=true:passed=false;
             //execute the reg ex and store in the variable match
             match = itm[0].pattern.exec(latDDMArray[a].toUpperCase() + ns[c] + lonDDMArray[b].toUpperCase());    
            });         
            //test to see if the regular expression identified the input as a valid input and identified it as DDM (for degrees decimal minutes)
            assert.isTrue(passed, latDDMArray[a] + ns[c] + lonDDMArray[b] + ' did not validate as DDM Lat/Long');
            //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
            assert.equal(latDDMArray[a].toUpperCase(), match[1], latDDMArray[a] + ns[c] + lonDDMArray[b] + " Failed");
            assert.equal(lonDDMArray[b].toUpperCase(), match[9], latDDMArray[a] + ns[c] + lonDDMArray[b] + " Failed");
            //test to see if the regular expression has correctly identified the seperator
            assert.equal(ns[c], match[8], "Matching the seperator failed");
            //reset passed
            passed = false;
            count++;
          }
        }                   
      }
      
      //we have tested each combination of numbers so lets just test a single combination with each possible prefix/suffix combo
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00 00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000 00.0" + pss[e] + pLon[f].toUpperCase();
                  ccUtil.getCoordinateType(tempLat + (" ") + tempLon).then(function(itm){
                  /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
                  ** https://theintern.github.io/intern/#async-tests
                  ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
                  ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
                  */
                  itm && itm[0].name == 'DDM'?passed=true:passed=false;
                  //execute the reg ex and store in the variable match
                  match = itm[0].pattern.exec(tempLat.toUpperCase() + (" ") + tempLon.toUpperCase());    
                  });         
                  //test to see if the regular expression identified the input as a valid input and identified it as DDM (for degrees decimal minutes)
                  assert.isTrue(passed, tempLat + (" ") + tempLon + ' did not validate as DDM Lat/Long');
                  //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
                  assert.equal(tempLat.toUpperCase(), match[1], tempLat + (" ") + tempLon + " Failed ");
                  assert.equal(tempLon.toUpperCase(), match[9], tempLat + (" ") + tempLon + " Failed ");
                  //test to see if the regular expression has correctly identified the seperator
                  assert.equal(" ", match[8], "Matching the seperator failed");
                  //reset passed
                  passed = false;
                  count++;                                 
                }
              }
            }
          }
        }
      }
      console.log("The number of Auto tests conducted for Identify Input as DDM - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Identify Input as DDM - Long / Lat': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;       
      
      //test each the DD latitude and longitude array items against each other using each of the seperators
      for (var a = 0; a < latDDMArray.length; a++) {
        for (var b = 0; b < lonDDMArray.length; b++) {
          for (var c = 0; c < ns.length; c++) {
          ccUtil.getCoordinateType(lonDDMArray[b] + ns[c] + latDDMArray[a]).then(function(itm){
           /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
           ** https://theintern.github.io/intern/#async-tests
           ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
           ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
           */
           if (itm.length == 1) {
            itm && itm[0].name == 'DDMrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[0].pattern.exec(lonDDMArray[b].toUpperCase() + ns[c] + latDDMArray[a].toUpperCase());
           } else {
            itm && itm[1].name == 'DDMrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[1].pattern.exec(lonDDMArray[b].toUpperCase() + ns[c] + latDDMArray[a].toUpperCase());
           }
          });         
          //test to see if the regular expression identified the input as a valid input and identified it as DDM (for degrees decimal minutes)
          assert.isTrue(passed, lonDDArray[b] + ns[c] + latDDArray[a] + ' did not validate as DDM Long/Lat');
          //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
          assert.equal(lonDDMArray[b].toUpperCase(), match[1], lonDDMArray[b] + ns[c] + latDDMArray[a] + " Failed");
          assert.equal(latDDMArray[a].toUpperCase(), match[9], lonDDMArray[b] + ns[c] + latDDMArray[a] + " Failed");
          //test to see if the regular expression has correctly identified the seperator
          assert.equal(ns[c], match[8], "Matching the seperator failed");
          //reset passed
          passed = false;
          count++;
          }
        }                   
      }
      
      //we have tested each combination of numbers so lets just test a single combination with each possible prefix/suffix combo
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {
                  var tempLon = pLon[d].toUpperCase() + "000 00.0" + pss[e] + pLon[f].toUpperCase();                 
                  var tempLat = pLat[a].toUpperCase() + "00 00.0" + pss[b] + pLat[c].toUpperCase();                      
                  ccUtil.getCoordinateType(tempLon + (" ") + tempLat).then(function(itm){
                  /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
                  ** https://theintern.github.io/intern/#async-tests
                  ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
                  ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
                  */
                  itm && itm[0].name == 'DDMrev'?passed=true:passed=false;
                  //execute the reg ex and store in the variable match
                  match = itm[0].pattern.exec(tempLon.toUpperCase() + (" ") + tempLat.toUpperCase());    
                  });         
                  //test to see if the regular expression identified the input as a valid input and identified it as DDM (for degrees decimal minutes))
                  assert.isTrue(passed, tempLon + (" ") + tempLat + ' did not validate as DDM Long/lat');
                  //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
                  assert.equal(tempLon.toUpperCase(), match[1], tempLon + (" ") + tempLat + " Failed ");
                  assert.equal(tempLat.toUpperCase(), match[9], tempLon + (" ") + tempLat + " Failed ");
                  //test to see if the regular expression has correctly identified the seperator
                  assert.equal(" ", match[8], "Matching the seperator failed");
                  //reset passed
                  passed = false;
                  count++;                                 
                }
              }
            }
          }
        }
      }
      console.log("The number of Auto tests conducted for Identify Input as DDM - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Identify Input as DMS - Lat / Long': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;       
    
      //The arrays are too large to test each of the DMS latitude and longitude array items against each other using each of the seperators
      //So just test using the space seperator we will check the seperator in the next test
      for (var a = 0; a < latDMSArray.length; a++) {
        for (var b = 0; b < lonDMSArray.length; b++) {
          ccUtil.getCoordinateType(latDMSArray[a] + " " + lonDMSArray[b]).then(function(itm){
           /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
           ** https://theintern.github.io/intern/#async-tests
           ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
           ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
           */
           itm && itm[0].name == 'DMS'?passed=true:passed=false;
           //execute the reg ex and store in the variable match
           match = itm[0].pattern.exec(latDMSArray[a].toUpperCase() + " " + lonDMSArray[b].toUpperCase());    
          });         
          //test to see if the regular expression identified the input as a valid input and identified it as DMS (for degrees, minutes, seconds)
          assert.isTrue(passed, latDMSArray[a] + " " + lonDMSArray[b] + ' did not validate as DMS Lat/Long');
          //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
          assert.equal(latDMSArray[a].toUpperCase(), match[1], latDMSArray[a] + " " + lonDMSArray[b] + " Failed");
          assert.equal(lonDMSArray[b].toUpperCase(), match[10], latDMSArray[a] + " " + lonDMSArray[b] + " Failed");
          //test to see if the regular expression has correctly identified the seperator
          assert.equal(" ", match[9], "Matching the seperator failed");
          //reset passed
          passed = false;
          count++;
        }                   
      }
      
      //we have tested each combination of numbers so lets just test a single combination with each possible prefix/suffix and seperator combo
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00 00 00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000 00 00.0" + pss[e] + pLon[f].toUpperCase();               
                  ccUtil.getCoordinateType(tempLat + ns[e] + tempLon).then(function(itm){
                  /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
                  ** https://theintern.github.io/intern/#async-tests
                  ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
                  ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
                  */
                  itm && itm[0].name == 'DMS'?passed=true:passed=false;
                  //execute the reg ex and store in the variable match
                  match = itm[0].pattern.exec(tempLat.toUpperCase() + ns[e] + tempLon.toUpperCase());    
                  });         
                  //test to see if the regular expression identified the input as a valid input and identified it as DMS (for degrees, minutes, seconds)
                  assert.isTrue(passed, tempLat + ns[e] + tempLon + ' did not validate as DMS Lat/Long');
                  //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
                  assert.equal(tempLat.toUpperCase(), match[1], tempLat + ns[e] + tempLon + " Failed ");
                  assert.equal(tempLon.toUpperCase(), match[10], tempLat + ns[e] + tempLon + " Failed ");
                  //test to see if the regular expression has correctly identified the seperator
                  assert.equal(ns[e], match[9], "Matching the seperator failed");
                  //reset passed
                  passed = false;
                  count++;                              
                }
              }
            }
          }
        }
      }
      console.log("The number of Auto tests conducted for Identify Input as DMS - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Identify Input as DMS - Long / Lat': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;       
      
      //test each the DD latitude and longitude array items against each other using each of the seperators
      for (var a = 0; a < latDMSArray.length; a++) {
        for (var b = 0; b < lonDMSArray.length; b++) {          
          ccUtil.getCoordinateType(lonDMSArray[b] + " " + latDMSArray[a]).then(function(itm){
           /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
           ** https://theintern.github.io/intern/#async-tests
           ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
           ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
           */
           if (itm.length == 1) {
            itm && itm[0].name == 'DMSrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[0].pattern.exec(lonDMSArray[b].toUpperCase() + " " + latDMSArray[a].toUpperCase());
           } else {
            itm && itm[1].name == 'DMSrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[1].pattern.exec(lonDMSArray[b].toUpperCase() + " " + latDMSArray[a].toUpperCase());
           }
          });         
          //test to see if the regular expression identified the input as a valid input and identified it as DMS (for degrees, minutes, seconds)
          assert.isTrue(passed, lonDMSArray[b].toUpperCase() + " " + latDMSArray[a].toUpperCase() + ' did not validate as DMS Long/Lat');
          //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
          assert.equal(lonDMSArray[b].toUpperCase(), match[1], lonDMSArray[b] + " " + latDMSArray[a] + " Failed");
          assert.equal(latDMSArray[a].toUpperCase(), match[10], lonDMSArray[b] + " " + latDMSArray[a] + " Failed");
          //test to see if the regular expression has correctly identified the seperator
          assert.equal(" ", match[9], "Matching the seperator failed");
          //reset passed
          passed = false;
          count++;        
        }                   
      }
      
      //we have tested each combination of numbers so lets just test a single combination with each possible prefix/suffix combo
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {
                  var tempLon = pLon[d].toUpperCase() + "000 00 00.0" + pss[e] + pLon[f].toUpperCase();                 
                  var tempLat = pLat[a].toUpperCase() + "00 00 00.0" + pss[b] + pLat[c].toUpperCase();
                  ccUtil.getCoordinateType(tempLon + ns[e] + tempLat).then(function(itm){
                  /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
                  ** https://theintern.github.io/intern/#async-tests
                  ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
                  ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
                  */
                  itm && itm[0].name == 'DMSrev'?passed=true:passed=false;
                  //execute the reg ex and store in the variable match
                  match = itm[0].pattern.exec(tempLon.toUpperCase() + ns[e] + tempLat.toUpperCase());    
                  });         
                  //test to see if the regular expression identified the input as a valid input and identified it as DMS (for degrees, minutes, seconds)
                  assert.isTrue(passed, tempLon + (" ") + tempLat + ' did not validate as DMS Long/lat');
                  //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
                  assert.equal(tempLon.toUpperCase(), match[1], tempLon + ns[e] + tempLat + " Failed ");
                  assert.equal(tempLat.toUpperCase(), match[10], tempLon + ns[e] + tempLat + " Failed ");
                  //test to see if the regular expression has correctly identified the seperator
                  assert.equal(ns[e], match[9], "Matching the seperator failed");
                  //reset passed
                  passed = false;
                  count++;
                }
              }
            }
          }
        }
      }
      console.log("The number of Auto tests conducted for Identify Input as DMS - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Manual Input: Identify Input as DD - Lat / Long': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '001', testString: '89.999 179.999', lat: '89.999', lon: '179.999', testSeperator: ' '},
        {testNumber: '002', testString: '90.000 180.000', lat: '90.000', lon: '180.000', testSeperator: ' '}
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          itm && itm[0].name == 'DD'?passed=true:passed=false;
          //execute the reg ex and store in the variable match
          match = itm[0].pattern.exec(validEntries[i].testString);            
          
          //split the input string by its seperator
          latLongArray = validEntries[i].testString.split(validEntries[i].testSeperator);
        });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DD (for decimal degrees)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' did not validate as DD Lat/Long');
        
        //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
        assert.equal(validEntries[i].lat, match[1], validEntries[i].testString + " Failed");
        assert.equal(validEntries[i].lon, match[9], validEntries[i].testString + " Failed");
                
        //test to see if the regular expression has correctly identified the seperator
        assert.equal(validEntries[i].testSeperator, match[8], "Matching the seperator failed");
        
        //reset passed
        passed = false;
        count++;                        
      }
      console.log("The number of manual tests conducted for Identify Input as DD - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;      
    },
    
    'Test Manual Input: Identify Input as DD - Long / Lat': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '001', testString: '179.999 89.999', lat: '89.999', lon: '179.999', testSeperator: ' '},
        {testNumber: '002', testString: '180.000 90.000', lat: '90.000', lon: '180.000', testSeperator: ' '}
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          if (itm.length == 1) {
            itm && itm[0].name == 'DDrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[0].pattern.exec(validEntries[i].testString);            
          } else {
            itm && itm[1].name == 'DDrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[1].pattern.exec(validEntries[i].testString);            
          }
          
          //split the input string by its seperator
          latLongArray = validEntries[i].testString.split(validEntries[i].testSeperator);
        });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DD (for decimal degrees)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' did not validate as DD Long/Lat');
        
        //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
        assert.equal(validEntries[i].lon, match[1], validEntries[i].testString + " Failed");
        assert.equal(validEntries[i].lat, match[10], validEntries[i].testString + " Failed");
                
        //test to see if the regular expression has correctly identified the seperator
        assert.equal(validEntries[i].testSeperator, match[9], "Matching the seperator failed");
        
        //reset passed
        passed = false;
        count++;                        
      }
      console.log("The number of manual tests conducted for Identify Input as DD - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;      
    },
    
    'Test Manual Input: Identify Input as DDM - Lat / Long': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '001', testString: '89 59.999 179 59.999', lat: '89 59.999', lon: '179 59.999', testSeperator: ' '},
        {testNumber: '002', testString: '90 00.000 180 00.000', lat: '90 00.000', lon: '180 00.000', testSeperator: ' '}
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          itm && itm[0].name == 'DDM'?passed=true:passed=false;
          //execute the reg ex and store in the variable match
          match = itm[0].pattern.exec(validEntries[i].testString);            
          
          //split the input string by its seperator
          latLongArray = validEntries[i].testString.split(validEntries[i].testSeperator);
        });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DDM (for degrees decimal minutes)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' did not validate as DDM Lat/Long');
        
        //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
        assert.equal(validEntries[i].lat, match[1], validEntries[i].testString + " Failed");
        assert.equal(validEntries[i].lon, match[9], validEntries[i].testString + " Failed");
                
        //test to see if the regular expression has correctly identified the seperator
        assert.equal(validEntries[i].testSeperator, match[8], "Matching the seperator failed");
        
        //reset passed
        passed = false;
        count++;                        
      }
      console.log("The number of manual tests conducted for Identify Input as DDM - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;     
    },
    
    'Test Manual Input: Identify Input as DDM - Long / Lat': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '001', testString: '179 59.999 89 59.999', lat: '89 59.999', lon: '179 59.999', testSeperator: ' '},
        {testNumber: '002', testString: '180 00.000 90 00.000', lat: '90 00.000', lon: '180 00.000', testSeperator: ' '}
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          if (itm.length == 1) {
            itm && itm[0].name == 'DDMrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[0].pattern.exec(validEntries[i].testString);            
          } else {
            itm && itm[1].name == 'DDMrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[1].pattern.exec(validEntries[i].testString);            
          }
          
          //split the input string by its seperator
          latLongArray = validEntries[i].testString.split(validEntries[i].testSeperator);
        });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DDM (for degrees decimal minutes)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' did not validate as DDM Long/Lat');
        
        //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
        assert.equal(validEntries[i].lon, match[1], validEntries[i].testString + " Failed");
        assert.equal(validEntries[i].lat, match[9], validEntries[i].testString + " Failed");
                
        //test to see if the regular expression has correctly identified the seperator
        assert.equal(validEntries[i].testSeperator, match[8], "Matching the seperator failed");
        
        //reset passed
        passed = false;
        count++;                        
      }
      console.log("The number of manual tests conducted for Identify Input as DDM - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;      
    },
    
    'Test Manual Input: Identify Input as DMS - Lat / Long': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '001', testString: '00 59 59.666 000 59 59.666', lat: '00 59 59.666', lon: '000 59 59.666', testSeperator: ' '},
        {testNumber: '002', testString: '00 00 59.666|000 00 59.666', lat: '00 00 59.666', lon: '000 00 59.666', testSeperator: '|'},
        {testNumber: '003', testString: '00 59 00.666:000 59 00.666', lat: '00 59 00.666', lon: '000 59 00.666', testSeperator: ':'},
        {testNumber: '004', testString: '89 59 59.666 179 59 59.666', lat: '89 59 59.666', lon: '179 59 59.666', testSeperator: ' '},
        {testNumber: '005', testString: '90 00 00.000 180 00 00.000', lat: '90 00 00.000', lon: '180 00 00.000', testSeperator: ' '},
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          itm && itm[0].name == 'DMS'?passed=true:passed=false;
          //execute the reg ex and store in the variable match
          match = itm[0].pattern.exec(validEntries[i].testString);            
          
          //split the input string by its seperator
          latLongArray = validEntries[i].testString.split(validEntries[i].testSeperator);
        });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DMS (for degrees, minutes, seconds)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' did not validate as DMS Lat/Long');
        
        //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
        assert.equal(validEntries[i].lat, match[1], validEntries[i].testString + " Failed");
        assert.equal(validEntries[i].lon, match[10], validEntries[i].testString + " Failed");
                
        //test to see if the regular expression has correctly identified the seperator
        assert.equal(validEntries[i].testSeperator, match[9], "Matching the seperator failed");
        
        //reset passed
        passed = false;
        count++;                        
      }
      console.log("The number of manual tests conducted for Identify Input as DMS - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Manual Input: Identify Input as DMS - Long / Lat': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '001', testString: '000 59 59.666 00 59 59.666', lat: '00 59 59.666', lon: '000 59 59.666', testSeperator: ' '},
        {testNumber: '002', testString: '000 00 59.666|00 00 59.666', lat: '00 00 59.666', lon: '000 00 59.666', testSeperator: '|'},
        {testNumber: '003', testString: '000 59 00.666:00 59 00.666', lat: '00 59 00.666', lon: '000 59 00.666', testSeperator: ':'},
        {testNumber: '004', testString: '179 59 59.666 89 59 59.666', lat: '89 59 59.666', lon: '179 59 59.666', testSeperator: ' '},
        {testNumber: '005', testString: '180 00 00.000 90 00 00.000', lat: '90 00 00.000', lon: '180 00 00.000', testSeperator: ' '}
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          if (itm.length == 1) {
            itm && itm[0].name == 'DMSrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[0].pattern.exec(validEntries[i].testString);            
          } else {
            itm && itm[1].name == 'DMSrev'?passed=true:passed=false;
            //execute the reg ex and store in the variable match
            match = itm[1].pattern.exec(validEntries[i].testString);            
          }
          
          //split the input string by its seperator
          latLongArray = validEntries[i].testString.split(validEntries[i].testSeperator);
        });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DMS (for degrees, minutes, seconds)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' did not validate as DMS Long/Lat');
        
        //test to see if the regular expression has correctly identified the Lat / long values by comparing them against the original string
        assert.equal(validEntries[i].lon, match[1], validEntries[i].testString + " Failed");
        assert.equal(validEntries[i].lat, match[10], validEntries[i].testString + " Failed");
                
        //test to see if the regular expression has correctly identified the seperator
        assert.equal(validEntries[i].testSeperator, match[9], "Matching the seperator failed");
        
        //reset passed
        passed = false;
        count++;                        
      }
      
      console.log("The number of manual tests conducted for Identify Input as DMS - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Manual Input: Identify Correct Non-Geographic Formats': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;
      
      // if you want to add specific tests that are not that you think will not be test with the automatic testing functions
      // add entries to the array below, including test number, testString, lat, long and seperator. Ensure there is no comma after your last array entry 
      var validEntries = [
        {testNumber: '1', testString: '49Q GV 3527397324', correctNotation: 'MGRS'},
        {testNumber: '2', testString: '49Q GV 35273 97324', correctNotation: 'MGRS'},
        {testNumber: '3', testString: '49Q GV 3527 9732', correctNotation: 'MGRS'},
        {testNumber: '4', testString: '49Q GV 352 397', correctNotation: 'MGRS'},
        {testNumber: '5', testString: '49Q GV 35 39', correctNotation: 'MGRS'},
        {testNumber: '6', testString: '49Q GV 3 3', correctNotation: 'MGRS'},
        {testNumber: '7', testString: '49QGV 3527397324', correctNotation: 'MGRS'},
        {testNumber: '8', testString: '49QGV 35273 97324', correctNotation: 'MGRS'},
        {testNumber: '9', testString: '49QGV 3527 9732', correctNotation: 'MGRS'},
        {testNumber: '10', testString: '49QGV 352 397', correctNotation: 'MGRS'},
        {testNumber: '11', testString: '49QGV 35 39', correctNotation: 'MGRS'},
        {testNumber: '12', testString: '49QGV 3 3', correctNotation: 'MGRS'},
        {testNumber: '13', testString: '49QGV3527397324', correctNotation: 'MGRS'},
        {testNumber: '14', testString: '49QGV35273 97324', correctNotation: 'MGRS'},
        {testNumber: '15', testString: '49QGV3527 9732', correctNotation: 'MGRS'},
        {testNumber: '16', testString: '49QGV352 397', correctNotation: 'MGRS'},
        {testNumber: '17', testString: '49QGV35 39', correctNotation: 'MGRS'},
        {testNumber: '18', testString: '49QGV3 3', correctNotation: 'MGRS'},
        {testNumber: '19', testString: '02D 456100 2516654', correctNotation: 'UTM'},
        {testNumber: '20', testString: '02D4561002516654', correctNotation: 'UTM'},
        {testNumber: '21', testString: 'ABFH2111042', correctNotation: 'GEOREF'},
        {testNumber: '22', testString: 'ABBN0677780', correctNotation: 'GEOREF'},
        {testNumber: '23', testString: 'ACAF0357515', correctNotation: 'GEOREF'},
        {testNumber: '24', testString: 'ACFF5642385', correctNotation: 'GEOREF'},
        {testNumber: '25', testString: 'ADAE3811872', correctNotation: 'GEOREF'},
        {testNumber: '26', testString: '011BW25', correctNotation: 'GARS'},
        {testNumber: '27', testString: '003CG35', correctNotation: 'GARS'},
        {testNumber: '28', testString: '001CZ14', correctNotation: 'GARS'},
        {testNumber: '29', testString: '012CZ26', correctNotation: 'GARS'},
        {testNumber: '30', testString: '002EC15', correctNotation: 'GARS'},
        {testNumber: '31', testString: '004EM25', correctNotation: 'GARS'},
        {testNumber: '32', testString: '009FF14', correctNotation: 'GARS'}
      ];

      for (var i = 0; i < validEntries.length; i++) {
        ccUtil.getCoordinateType(validEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
            itm && itm[0].name == validEntries[i].correctNotation?passed=true:passed=false;
         });
        
        //test to see if the regular expression identified the input as a valid inpout and identified it as DMS (for degrees, minutes, seconds)
        assert.isTrue(passed, 'Test Number: ' + validEntries[i].testNumber + " String: " + validEntries[i].testString + ' notation was not identified.');
                
        //reset passed
        passed = false;
        count++;                        
      }
      
      console.log("The number of manual tests conducted for Identify Correct Non-Geographic Formats conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Process Input as DD - Lat / Long': function() {
      //test to ensure that coordinates are processed correctly before handing off to the geometry service
      
      //this.skip('Skip test for now');  
      var count = 0;      
      //var notations = ccUtil.getNotations();
      
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000.0" + pss[e] + pLon[f].toUpperCase();                      
                  var outLatPrefix = '';                  
                  if(pLat[a] != '' && pLat[c] != '') {
                    new RegExp(/[Ss-]/).test(pLat[a])?outLatPrefix = '-':outLatPrefix = '+';
                  } else {
                    if(pLat[a] && new RegExp(/[Ss-]/).test(pLat[a])){
                      outLatPrefix = '-';
                    } else {
                      if(pLat[c] && new RegExp(/[Ss-]/).test(pLat[c])){
                        outLatPrefix = '-';
                      } else {
                        outLatPrefix = '+';
                      }
                    }
                  }                  
                  var outLonPrefix = '';                  
                  if(pLon[d] != '' && pLon[f] != '') {
                    new RegExp(/[Ww-]/).test(pLon[d])?outLonPrefix = '-':outLonPrefix = '+';
                  } else {
                    if(pLon[d] && new RegExp(/[Ww-]/).test(pLon[d])){
                      outLonPrefix = '-';
                  } else {
                      if(pLon[f]  && new RegExp(/[Ww-]/).test(pLon[f])){
                        outLonPrefix = '-';
                      } else {
                        outLonPrefix = '+';
                      }
                    }
                  }                  
                  var expectedOutput = outLatPrefix + "00.0," + outLonPrefix + "000.0";
                  var testString = (tempLat + " " + tempLon);
                  var returnString = cc.processCoordTextInput(testString, notations[0], true);                  
                  assert.equal(returnString, expectedOutput, expectedOutput + "  Failed");
                  count++;     
                }
              }
            }
          }
        }
      }   
      console.log("The number of Auto tests conducted for Process Input as DD - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Process Input as DD - Long / Lat': function() {
      //test to ensure that coordinates are processed correctly before handing off to the geometry service
      
      //this.skip('Skip test for now');  
      var count = 0; 
      
           
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000.0" + pss[e] + pLon[f].toUpperCase();                      
                  var outLatPrefix = '';                  
                  if(pLat[a] != '' && pLat[c] != '') {
                    new RegExp(/[Ss-]/).test(pLat[a])?outLatPrefix = '-':outLatPrefix = '+';
                  } else {
                    if(pLat[a] && new RegExp(/[Ss-]/).test(pLat[a])){
                      outLatPrefix = '-';
                    } else {
                      if(pLat[c] && new RegExp(/[Ss-]/).test(pLat[c])){
                        outLatPrefix = '-';
                      } else {
                        outLatPrefix = '+';
                      }
                    }
                  }                  
                  var outLonPrefix = '';                  
                  if(pLon[d] != '' && pLon[f] != '') {
                    new RegExp(/[Ww-]/).test(pLon[d])?outLonPrefix = '-':outLonPrefix = '+';
                  } else {
                    if(pLon[d] && new RegExp(/[Ww-]/).test(pLon[d])){
                      outLonPrefix = '-';
                  } else {
                      if(pLon[f]  && new RegExp(/[Ww-]/).test(pLon[f])){
                        outLonPrefix = '-';
                      } else {
                        outLonPrefix = '+';
                      }
                    }
                  }                  
                  var expectedOutput = outLatPrefix + "00.0," + outLonPrefix + "000.0";
                  var testString = (tempLon + " " + tempLat);
                  var returnString = cc.processCoordTextInput(testString, notations[1], true);                                    
                  assert.equal(returnString, expectedOutput, expectedOutput + "  Failed");
                  count++;     
                }
              }
            }
          }
        }
      }   
      console.log("The number of Auto tests conducted for Process Input as DD - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Process Input as DDM - Lat / Long': function() {
      //test to ensure that coordinates are processed correctly before handing off to the geometry service
      
      //this.skip('Skip test for now');  
      var count = 0;      
      
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00 00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000 00.0" + pss[e] + pLon[f].toUpperCase();                      
                  var outLatPrefix = '';                  
                  if(pLat[a] != '' && pLat[c] != '') {
                    new RegExp(/[Ss-]/).test(pLat[a])?outLatPrefix = '-':outLatPrefix = '+';
                  } else {
                    if(pLat[a] && new RegExp(/[Ss-]/).test(pLat[a])){
                      outLatPrefix = '-';
                    } else {
                      if(pLat[c] && new RegExp(/[Ss-]/).test(pLat[c])){
                        outLatPrefix = '-';
                      } else {
                        outLatPrefix = '+';
                      }
                    }
                  }                  
                  var outLonPrefix = '';                  
                  if(pLon[d] != '' && pLon[f] != '') {
                    new RegExp(/[Ww-]/).test(pLon[d])?outLonPrefix = '-':outLonPrefix = '+';
                  } else {
                    if(pLon[d] && new RegExp(/[Ww-]/).test(pLon[d])){
                      outLonPrefix = '-';
                  } else {
                      if(pLon[f]  && new RegExp(/[Ww-]/).test(pLon[f])){
                        outLonPrefix = '-';
                      } else {
                        outLonPrefix = '+';
                      }
                    }
                  }                  
                  var expectedOutput = outLatPrefix + "00 00.0," + outLonPrefix + "000 00.0";
                  var testString = (tempLat + " " + tempLon);
                  var returnString = cc.processCoordTextInput(testString, notations[2], true);                  
                  assert.equal(returnString, expectedOutput, expectedOutput + "  Failed");
                  count++;     
                }
              }
            }
          }
        }
      }   
      console.log("The number of Auto tests conducted for Process Input as DDM - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Process Input as DDM - Long / Lat': function() {
      //test to ensure that coordinates are processed correctly before handing off to the geometry service
      
      //this.skip('Skip test for now');  
      var count = 0;      
           
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00 00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000 00.0" + pss[e] + pLon[f].toUpperCase();                      
                  var outLatPrefix = '';                  
                  if(pLat[a] != '' && pLat[c] != '') {
                    new RegExp(/[Ss-]/).test(pLat[a])?outLatPrefix = '-':outLatPrefix = '+';
                  } else {
                    if(pLat[a] && new RegExp(/[Ss-]/).test(pLat[a])){
                      outLatPrefix = '-';
                    } else {
                      if(pLat[c] && new RegExp(/[Ss-]/).test(pLat[c])){
                        outLatPrefix = '-';
                      } else {
                        outLatPrefix = '+';
                      }
                    }
                  }                  
                  var outLonPrefix = '';                  
                  if(pLon[d] != '' && pLon[f] != '') {
                    new RegExp(/[Ww-]/).test(pLon[d])?outLonPrefix = '-':outLonPrefix = '+';
                  } else {
                    if(pLon[d] && new RegExp(/[Ww-]/).test(pLon[d])){
                      outLonPrefix = '-';
                  } else {
                      if(pLon[f]  && new RegExp(/[Ww-]/).test(pLon[f])){
                        outLonPrefix = '-';
                      } else {
                        outLonPrefix = '+';
                      }
                    }
                  }                  
                  var expectedOutput = outLatPrefix + "00 00.0," + outLonPrefix + "000 00.0";
                  var testString = (tempLon + " " + tempLat);
                  var returnString = cc.processCoordTextInput(testString, notations[3], true);                                    
                  assert.equal(returnString, expectedOutput, expectedOutput + "  Failed");
                  count++;     
                }
              }
            }
          }
        }
      }   
      console.log("The number of Auto tests conducted for Process Input as DDM - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Process Input as DMS - Lat / Long': function() {
      //test to ensure that coordinates are processed correctly before handing off to the geometry service
      
      //this.skip('Skip test for now');  
      var count = 0;      
      
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00 00 00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000 00 00.0" + pss[e] + pLon[f].toUpperCase();                      
                  var outLatPrefix = '';                  
                  if(pLat[a] != '' && pLat[c] != '') {
                    new RegExp(/[Ss-]/).test(pLat[a])?outLatPrefix = '-':outLatPrefix = '+';
                  } else {
                    if(pLat[a] && new RegExp(/[Ss-]/).test(pLat[a])){
                      outLatPrefix = '-';
                    } else {
                      if(pLat[c] && new RegExp(/[Ss-]/).test(pLat[c])){
                        outLatPrefix = '-';
                      } else {
                        outLatPrefix = '+';
                      }
                    }
                  }                  
                  var outLonPrefix = '';                  
                  if(pLon[d] != '' && pLon[f] != '') {
                    new RegExp(/[Ww-]/).test(pLon[d])?outLonPrefix = '-':outLonPrefix = '+';
                  } else {
                    if(pLon[d] && new RegExp(/[Ww-]/).test(pLon[d])){
                      outLonPrefix = '-';
                  } else {
                      if(pLon[f]  && new RegExp(/[Ww-]/).test(pLon[f])){
                        outLonPrefix = '-';
                      } else {
                        outLonPrefix = '+';
                      }
                    }
                  }                  
                  var expectedOutput = outLatPrefix + "00 00 00.0," + outLonPrefix + "000 00 00.0";
                  var testString = (tempLat + " " + tempLon);
                  var returnString = cc.processCoordTextInput(testString, notations[4], true);                  
                  assert.equal(returnString, expectedOutput, expectedOutput + "  Failed");
                  count++;     
                }
              }
            }
          }
        }
      }   
      console.log("The number of Auto tests conducted for Process Input as DMS - Lat / Long conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Auto Input: Process Input as DMS - Long / Lat': function() {
      //test to ensure that coordinates are processed correctly before handing off to the geometry service
      
      //this.skip('Skip test for now');  
      var count = 0;      
      
      for (var a = 0; a < pLat.length; a++) {
        for (var b = 0; b < pss.length; b++) {
          for (var c = 0; c < pLat.length; c++) { 
            for (var d = 0; d < pLon.length; d++) {
              for (var e = 0; e < pss.length; e++) {
                for (var f = 0; f < pLon.length; f++) {         
                  var tempLat = pLat[a].toUpperCase() + "00 00 00.0" + pss[b] + pLat[c].toUpperCase();
                  var tempLon = pLon[d].toUpperCase() + "000 00 00.0" + pss[e] + pLon[f].toUpperCase();                      
                  var outLatPrefix = '';                  
                  if(pLat[a] != '' && pLat[c] != '') {
                    new RegExp(/[Ss-]/).test(pLat[a])?outLatPrefix = '-':outLatPrefix = '+';
                  } else {
                    if(pLat[a] && new RegExp(/[Ss-]/).test(pLat[a])){
                      outLatPrefix = '-';
                    } else {
                      if(pLat[c] && new RegExp(/[Ss-]/).test(pLat[c])){
                        outLatPrefix = '-';
                      } else {
                        outLatPrefix = '+';
                      }
                    }
                  }                  
                  var outLonPrefix = '';                  
                  if(pLon[d] != '' && pLon[f] != '') {
                    new RegExp(/[Ww-]/).test(pLon[d])?outLonPrefix = '-':outLonPrefix = '+';
                  } else {
                    if(pLon[d] && new RegExp(/[Ww-]/).test(pLon[d])){
                      outLonPrefix = '-';
                  } else {
                      if(pLon[f]  && new RegExp(/[Ww-]/).test(pLon[f])){
                        outLonPrefix = '-';
                      } else {
                        outLonPrefix = '+';
                      }
                    }
                  }                  
                  var expectedOutput = outLatPrefix + "00 00 00.0," + outLonPrefix + "000 00 00.0";
                  var testString = (tempLon + " " + tempLat);
                  var returnString = cc.processCoordTextInput(testString, notations[5], true);                                    
                  assert.equal(returnString, expectedOutput, expectedOutput + "  Failed");
                  count++;     
                }
              }
            }
          }
        }
      }   
      console.log("The number of Auto tests conducted for Process Input as DMS - Long / Lat conducted was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
    'Test Manual Input: Check invalid input is not identified as a valid entry': function() {
      //this.skip('Skip test for now');  
      var passed = false;
      var match = '';      
      var count = 0;      
       
      var invalidEntries = [
        {testNumber: '1', testString: '00 00 00 000N 00 00'},  //cannot have a north suffix on a longitude value
        {testNumber: '2', testString: 'W00 00 00 000 00 00'},  //cannot have a west prefix on a latitude value
        {testNumber: '3', testString: 'A random string'},  //random values
        {testNumber: '4', testString: '009FF141'}, //incorrect GARS entry - an extra digit on the end
        {testNumber: '5', testString: '41RPR1'}, //incorrect MGRS only 1 digit
        {testNumber: '6', testString: '41RPR 1'}, //incorrect MGRS only 1 digit
        {testNumber: '7', testString: '41RPR 123'}, //incorrect MGRS 3 digits
        {testNumber: '8', testString: '41RPR 12 3'}, //incorrect MGRS 3 digits
        {testNumber: '9', testString: '41RPR 12345'}, //incorrect MGRS 5 digits
        {testNumber: '10', testString: '41RPR 12 345'}, //incorrect MGRS 5 digits
        {testNumber: '11', testString: '41RPR 1234567'}, //incorrect MGRS 7 digits
        {testNumber: '12', testString: '41RPR 1234 567'}, //incorrect MGRS 7 digits
        {testNumber: '13', testString: '41RPR 123456789'}, //incorrect MGRS 9 digits
        {testNumber: '14', testString: '41RPR 12345 6789'}, //incorrect MGRS 9 digits
        {testNumber: '15', testString: '41RPR 12345678900'}, //incorrect MGRS 11 digits
        {testNumber: '16', testString: '41RPR 123456 78900'} //incorrect MGRS 11 digits
      ];

      for (var i = 0; i < invalidEntries.length; i++) {
        ccUtil.getCoordinateType(invalidEntries[i].testString).then(function(itm){
          /* as the getCoordinateType function returns a promise and resolving the promise indicates a passing test:
          ** https://theintern.github.io/intern/#async-tests
          ** we need check whats in the promise return and set the passed boolean to true or false accordinaly
          ** we can the use the passed boolean to perform an assert.isTrue outside of the promise
          */
          
          //if no match is made itm should be null
            itm == null?passed=true:passed=false;
        });
        
        assert.isTrue(passed, 'Test Number: ' + invalidEntries[i].testNumber + " String: " + invalidEntries[i].testString + ' was identified as a valid input when it should have failed');
                
        //reset passed
        passed = false;
        count++;                        
      }      
      console.log("The number of manual tests conducted to check invalid input is not identified as a valid entry was: " + count);
      totalTestCount = totalTestCount + count;
    },
    
  });
});