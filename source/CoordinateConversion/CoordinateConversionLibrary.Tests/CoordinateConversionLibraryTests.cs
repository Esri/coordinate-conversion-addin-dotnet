/******************************************************************************* 
  * Copyright 2015 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoordinateConversionLibrary.Models;
using CoordinateConversionLibrary.ViewModels;

namespace CoordinateConversionLibrary.Tests
{
    [TestClass]
    public class CoordinateConversionLibraryTests
    {
        [ClassInitialize()]
        public static void TestClassInitialize(TestContext testContext)
        {
            // disable this dialog for tests
            CoordinateConversionLibraryConfig.AddInConfig.DisplayAmbiguousCoordsDlg = false;
        }

        [TestMethod]
        public void ParseDD()
        {
            double v;
            double.TryParse("3", out v);
            CoordinateDD coord;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");    
            Assert.IsFalse(CoordinateDD.TryParse("", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048 -78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N 78.847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("78.847427W 40.273048N", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N,78.847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsFalse(CoordinateDD.TryParse("40.273048N78.847427W", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N;78.847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N:78.847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048, 78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048,78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048;78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048:78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048n 78.847427w", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("n40.273048 w78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsFalse(CoordinateDD.TryParse("40.273048N|-78.847427W", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N/78.847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N\\78.847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45.0°N 100.0°W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45.0˚N 100.0˚W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45.0ºN 100.0ºW", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45.0^N 100.0^W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45.0~N 100.0~W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45.0*N 100.0*W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100.0°W 45.0°N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100.0˚W 45.0˚N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100.0ºW 45.0ºN", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100.0^W 45.0^N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100.0~W 45.0~N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100.0*W 45.0*N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100*W 45*N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100W 45N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100- 45+", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("-100 +45", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("-100 45N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("+100 45S", out coord));
            Assert.AreEqual(-45, coord.Lat);
            Assert.AreEqual(100, coord.Lon);
            Assert.IsFalse(CoordinateDD.TryParse("+100+ 45S", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("-100- 45S", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("100- +45-", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("-100- +45+", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("+100W -45S", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("+100E +45N", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("-100W -45S", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("100N 45", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("100S 45", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("100 45E", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("100 45W", out coord));
            Assert.IsFalse(CoordinateDD.TryParse("This is not a coordinate", out coord));

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
            Assert.IsTrue(CoordinateDD.TryParse("40,273048 -78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N 78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N,78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N   78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N;78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N:78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048, 78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048,78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048;78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048:78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048n 78,847427w", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("n40,273048 w78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N|78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N/78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048N\\78,847427W", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048-\\78,847427-", out coord));
            Assert.AreEqual(-40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048+\\78,847427+", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("+40,273048\\+78,847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("40,273048-\\78,847427-", out coord));
            Assert.AreEqual(-40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45,0°N 100,0°W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45,0˚N 100,0˚W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45,0ºN 100,0ºW", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45,0^N 100,0^W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45,0~N 100,0~W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("45,0*N 100,0*W", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0°W 45,0°N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0˚W 45,0˚N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0ºW 45,0ºN", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0^W 45,0^N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0~W 45,0~N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0*W 45,0*N", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0*- 45,0*+", out coord));
            Assert.AreEqual(45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("100,0*+ 45,0*-", out coord));
            Assert.AreEqual(-45, coord.Lat);
            Assert.AreEqual(100, coord.Lon);
            Assert.IsTrue(CoordinateDD.TryParse("-100,0 -45,0", out coord));
            Assert.AreEqual(-45, coord.Lat);
            Assert.AreEqual(-100, coord.Lon);

            Assert.IsFalse(CoordinateDD.TryParse("This is not a coordinate", out coord));
        }

        [TestMethod]
        public void ParseDDM()
        {
            CoordinateDDM coord;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288',78°50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288', -78°50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288', -078°50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288, -078°50.84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288, -078 50.84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288',-078°50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288,-078 50.84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288' -078°50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288 -078 50.84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288N 078 50.84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288n 078 50.84562w", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N40 16.38288 W078 50.84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("n40 16.38288 w078 50.84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288N|078 50.84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288N/078 50.84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16.38288N\\078 50.84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40-16.38288' -078-50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40_16.38288' -078_50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16N 078 50W", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.IsTrue(CoordinateDDM.TryParse("N40 16.38288 W078 50.84562", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16.38288, coord.LatMinutes);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50.84562, coord.LonMinutes);
            Assert.IsTrue(CoordinateDDM.TryParse("W078 50.84562 N40 16.38288", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16.38288, coord.LatMinutes);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50.84562, coord.LonMinutes);
            Assert.IsTrue(CoordinateDDM.TryParse("45 0n/100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0n|100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0n\\100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100-0.0W 45-0.0N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("+45 0.0 +100 0.0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45-0N 100-0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("-45 0.0 +100 0.0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("-100 0.0 +45 0.0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45_0.0N 100_0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N45 0, W100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N45    0   W100   0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N45 0 W100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0n\\100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0 100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0N 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 08N 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45° 0.0N 100° 0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45˚ 0.0N 100˚ 0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45º 0.0N 100º 0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45^ 0.0N 100^ 0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45~ 0.0N 100~ 0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45* 0.0N 100* 0.0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0.0′N 100 0.0′W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0.0'N 100 0.0'W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100° 0.0′W 45° 0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100° 0.0'W 45° 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100˚ 0.0′W 45˚ 0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100˚ 0.0'W 45˚ 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100º 0.0′W 45º 0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100º 0.0'W 45º 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100^ 0.0′W 45^ 0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100^ 0.0'W 45^ 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100~0.0′W 45~0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100~0.0'W 45~0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0.0′W 45* 0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0.0'W 45* 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0N 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0N, 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0, 100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100~0.0'W 45~0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0.0′W 45* 0.0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0.0'W 45* 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100 0.0'- 45 0.0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100 0.0'W 45 0.0'+", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("-100 0.0' -45 0.0'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("+100 0.0' +45 0.0'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("+100 0.0' 45 0.0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("+100 0.0'+ 45 0.0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0.0'+ 45 0.0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0.0'- 45 0.0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0.0' -45 0.0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0.0' -45 0.0'+", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0.0' N45 0.0'+", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0.0' S45 0.0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("1234567.1234 1234567.1234", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("This is not a coordinate", out coord));

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
            Assert.IsTrue(CoordinateDDM.TryParse("40°16,38288',78°50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16,38288', -78°50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16,38288', -078°50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16,38288, -078°50,84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288, -078 50,84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16,38288',-078°50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288,-078 50,84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16,38288' -078°50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288 -078 50,84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288N 078 50,84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288n 078 50,84562w", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N40 16,38288 W078 50,84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("n40 16,38288 w078 50,84562", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288N|078 50,84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288N/078 50,84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40 16,38288N\\078 50,84562W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40-16,38288' -078-50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40_16,38288' -078_50,84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100-0,0W 45-0,0N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("+45 0,0 +100 0,0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45-0N 100-0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("-45 0,0 +100 0,0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("-100 0,0 +45 0,0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45_0,0N 100_0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N45 0, W100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N45 0, W100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("N45 0 W100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0n\\100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0 100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0N 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 08N 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45° 0,0N 100° 0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45˚ 0,0N 100˚ 0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45º 0,0N 100º 0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45^ 0,0N 100^ 0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45~ 0,0N 100~ 0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45* 0,0N 100* 0,0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0,0′N 100 0,0′W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0,0'N 100 0,0'W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100° 0,0′W 45° 0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100° 0,0'W 45° 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100˚ 0,0′W 45˚ 0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100˚ 0,0'W 45˚ 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100º 0,0′W 45º 0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100º 0,0'W 45º 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100^ 0,0′W 45^ 0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100^ 0,0'W 45^ 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100~0,0′W 45~0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100~0,0'W 45~0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0,0′W 45* 0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0,0'W 45* 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0N 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0N, 100 0W", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("45 0, 100 0", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100~0,0'W 45~0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0,0′W 45* 0,0′N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100* 0,0'W 45* 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100 0,0'- 45 0,0'N", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("100 0,0'W 45 0,0'+", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("-100 0,0' -45 0,0'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("+100 0,0' +45 0,0'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("+100 0,0' 45 0,0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("+100 0,0'+ 45 0,0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0,0'+ 45 0,0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0,0'- 45 0,0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0,0' -45 0,0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0,0' -45 0,0'+", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0,0' N45 0,0'+", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("E100 0,0' S45 0,0'-", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("1234567,1234 1234567,1234", out coord));
            Assert.IsFalse(CoordinateDDM.TryParse("This is not a coordinate", out coord));
        }

        [TestMethod]
        public void ParseDMS()
        {
            CoordinateDMS coord;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
            Assert.IsTrue(CoordinateDMS.TryParse("40°16'22.9728\", -78°50'50.7372\"", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22.9728, coord.LatSeconds);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50.7372, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("W78°50'50.7372\", N40°16'22.9728\"", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22.9728, coord.LatSeconds);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50.7372, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("N40°16'22.9728\", W78°50'50.7372\"", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22.9728, coord.LatSeconds);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50.7372, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("40°16'22.9728\", -078°50'50.7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 16' 22.9728\", -78° 50' 50.7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22.8\"N 77° 32' 38.4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22.8\"n 77° 32' 38.4\"w", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40° 7' 22.8\" W77° 32' 38.4\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("n40° 7' 22.8\" w77° 32' 38.4\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22.8\"N|77° 32' 38.4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22.8\"N/77° 32' 38.4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22.8\"N\\77° 32' 38.4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40-7-22.8 W77-32-38.4", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40_7_22.8 W77_32_38.4", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40 16 22, W78 50 50", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22, coord.LatSeconds);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("+40 16 22, -78 50 50", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22, coord.LatSeconds);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("40 16 22+, 78 50 50-", out coord));
            Assert.AreEqual(40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22, coord.LatSeconds);
            Assert.AreEqual(-78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("-40 16 22, +78 50 50", out coord));
            Assert.AreEqual(-40, coord.LatDegrees);
            Assert.AreEqual(16, coord.LatMinutes);
            Assert.AreEqual(22, coord.LatSeconds);
            Assert.AreEqual(78, coord.LonDegrees);
            Assert.AreEqual(50, coord.LonMinutes);
            Assert.AreEqual(50, coord.LonSeconds);
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0N 100 0 0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45  1     2, 100 3 4", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45°  1 0.0N 100° 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45˚  1 0.0N 100˚ 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45º   1 0.0N 100º 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45^ 1 0.0N 100^ 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45~ 1 0.0N 100~ 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45* 1 0.0N 100* 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1' 0.0\"N 100 1' 0.0\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1 0.0N 100 1 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45˚ 1′ 0.0N 100˚ 1′ 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("+45 1 0.0 +100 1 0.0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("-45 1 0.0 +100 1 0.0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("+45 1 0.0 -100 1 0.0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("-45 1 0.0 -100 1 0.0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45-0-0.0N 100-0-0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45-0-0N 100-0-0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45_0_0.0N 100_0_0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0.0N|100 0 0.0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0N|100 0 0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0, 100 0 0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45_0_0N 100_0_0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0N 100 0 0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N45 0 0 W100 0 0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("-45 1 0.0 +100 1 0.0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1 0.0- +100 1 0.0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1 0.0- 100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0.0- 100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0.0+ 100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("45 1 0.0+ -100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0.0+ +100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("N45 1 0.0+ 100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("45 1 0.0+ S100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("N45 1 0.0N 100 1 0.0", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0.0S 100 1 0.0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("1234567.1234 1234567.1234", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("This is not a coordinate", out coord));

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
            Assert.IsTrue(CoordinateDMS.TryParse("40°16'22,9728\", -78°50'50,7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40°16'22,9728\", -078°50'50,7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 16' 22,9728\", -78° 50' 50,7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22,8\"N 77° 32' 38,4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22,8\"n 77° 32' 38,4\"w", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40° 7' 22,8\" W77° 32' 38,4\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("n40° 7' 22,8\" w77° 32' 38,4\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22,8\"N|77° 32' 38,4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22,8\"N/77° 32' 38,4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22,8\"N\\77° 32' 38,4\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40-7-22,8 W77-32-38,4", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N40_7_22,8 W77_32_38,4", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45  1     2, 100 3 4", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45°  1 0,0N 100° 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45˚  1 0,0N 100˚ 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45º   1 0,0N 100º 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45^ 1 0,0N 100^ 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45~ 1 0,0N 100~ 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45* 1 0,0N 100* 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1' 0,0\"N 100 1' 0,0\"W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1 0,0N 100 1 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45˚ 1′ 0,0N 100˚ 1′ 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("+45 1 0,0 +100 1 0,0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("-45 1 0,0 +100 1 0,0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("+45 1 0,0 -100 1 0,0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("-45 1 0,0 -100 1 0,0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45-0-0,0N 100-0-0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45-0-0N 100-0-0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45_0_0,0N 100_0_0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0,0N|100 0 0,0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0N|100 0 0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0, 100 0 0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45_0_0N 100_0_0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 0 0N 100 0 0W", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("N45 0 0 W100 0 0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("-45 1 0,0 +100 1 0,0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1 0,0- +100 1 0,0", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("45 1 0,0- 100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0,0- 100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0,0+ 100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("45 1 0,0+ -100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0,0+ +100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("N45 1 0,0+ 100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("45 1 0,0+ S100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("N45 1 0,0N 100 1 0,0", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("-45 1 0,0S 100 1 0,0-", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("1234567,1234 1234567,1234", out coord));
            Assert.IsFalse(CoordinateDMS.TryParse("This is not a coordinate", out coord));
        }

        [TestMethod]
        public void ParseGARS()
        {
            CoordinateGARS coord;
            Assert.IsTrue(CoordinateGARS.TryParse("203LW18", out coord));
            Assert.IsTrue(CoordinateGARS.TryParse("203 LW 1 8", out coord));
            Assert.IsTrue(CoordinateGARS.TryParse("203,LW,1,8", out coord));
            Assert.IsTrue(CoordinateGARS.TryParse("203-LW-1-8", out coord));

            Assert.IsFalse(CoordinateGARS.TryParse("This is not a coordinate", out coord));
            // test some out of range coordinates
            Assert.IsFalse(CoordinateGARS.TryParse("203RZ18", out coord));
            Assert.IsFalse(CoordinateGARS.TryParse("800AA18", out coord));
            Assert.IsFalse(CoordinateGARS.TryParse("000AA18", out coord));
            Assert.IsFalse(CoordinateGARS.TryParse("203AA08", out coord));
            Assert.IsFalse(CoordinateGARS.TryParse("203AA58", out coord));
            Assert.IsFalse(CoordinateGARS.TryParse("203AA10", out coord));
        }

        [TestMethod]
        public void ParseUTM()
        {
            CoordinateUTM coord;
            Assert.IsTrue(CoordinateUTM.TryParse("17N683016m4460286m", out coord));
            Assert.IsTrue(CoordinateUTM.TryParse("17N 683016m 4460286m", out coord));
            Assert.IsTrue(CoordinateUTM.TryParse("17N,683016m,4460286m", out coord));
            Assert.IsTrue(CoordinateUTM.TryParse("17N-683016m-4460286m", out coord));
            Assert.IsTrue(CoordinateUTM.TryParse("17N;683016m;4460286m", out coord));
            Assert.IsTrue(CoordinateUTM.TryParse("17N:683016m:4460286m", out coord));

            Assert.IsFalse(CoordinateUTM.TryParse("This is not a coordinate", out coord));
            Assert.IsFalse(CoordinateUTM.TryParse("00N683016m4460286m", out coord));
            Assert.IsFalse(CoordinateUTM.TryParse("61N683016m4460286m", out coord));
        }

        [TestMethod]
        public void ParseMGRS()
        {
            CoordinateMGRS coord;
            Assert.IsTrue(CoordinateMGRS.TryParse("17TPE8301660286", out coord));
            Assert.IsTrue(CoordinateMGRS.TryParse("17T PE 8301660286", out coord));
            Assert.IsTrue(CoordinateMGRS.TryParse("17T-PE-8301660286", out coord));
            Assert.IsTrue(CoordinateMGRS.TryParse("17T,PE,8301660286", out coord));
            Assert.IsTrue(CoordinateMGRS.TryParse("17T:PE:8301660286", out coord));
            Assert.IsTrue(CoordinateMGRS.TryParse("17T;PE;8301660286", out coord));

            Assert.IsFalse(CoordinateMGRS.TryParse("This is not a coordinate", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("00TPE8301660286", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("61TPE8301660286", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("17APE8301660286", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("17ZPE8301660286", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("17TPW8301660286", out coord));

            //Test base MGRS:11SMT9915302431 for 117 0 32.8W 33 27 40.8N
            Assert.IsTrue(CoordinateMGRS.TryParse("11SMT", out coord));
            Assert.AreEqual(0, coord.Easting);
            Assert.AreEqual(0, coord.Northing);

            Assert.IsTrue(CoordinateMGRS.TryParse("11SMT90", out coord));
            Assert.AreEqual(90000, coord.Easting);
            Assert.AreEqual(0, coord.Northing);


            Assert.IsTrue(CoordinateMGRS.TryParse("11SMT9902", out coord));
            Assert.AreEqual(99000, coord.Easting);
            Assert.AreEqual(2000, coord.Northing);

            Assert.IsTrue(CoordinateMGRS.TryParse("11SMT991024", out coord));
            Assert.AreEqual(99100, coord.Easting);
            Assert.AreEqual(2400, coord.Northing);

            Assert.IsTrue(CoordinateMGRS.TryParse("11SMT99150243", out coord));
            Assert.AreEqual(99150, coord.Easting);
            Assert.AreEqual(2430, coord.Northing);

            Assert.IsTrue(CoordinateMGRS.TryParse("11SMT9915302431", out coord));
            Assert.AreEqual(99153, coord.Easting);
            Assert.AreEqual(2431, coord.Northing);

            // test bad MGRS with odd number northings/eastings
            Assert.IsFalse(CoordinateMGRS.TryParse("11SMT991530243", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("11SMT9915024", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("11SMT99102", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("11SMT992", out coord));
            Assert.IsFalse(CoordinateMGRS.TryParse("11SMT9", out coord));

            // Test lowercase MGRS
            Assert.IsTrue(CoordinateMGRS.TryParse("11smt8154368826", out coord));
        }

        [TestMethod]
        public void ParseUSNG()
        {
            CoordinateUSNG coord;
            Assert.IsTrue(CoordinateUSNG.TryParse("17TPE8301660286", out coord));
            Assert.IsTrue(CoordinateUSNG.TryParse("17T PE 8301660286", out coord));
            Assert.IsTrue(CoordinateUSNG.TryParse("17T-PE-8301660286", out coord));
            Assert.IsTrue(CoordinateUSNG.TryParse("17T,PE,8301660286", out coord));
            Assert.IsTrue(CoordinateUSNG.TryParse("17T:PE:8301660286", out coord));
            Assert.IsTrue(CoordinateUSNG.TryParse("17T;PE;8301660286", out coord));

            Assert.IsFalse(CoordinateUSNG.TryParse("This is not a coordinate", out coord));

            //Test base USNG 13SBB4476129499 as 107 52 43.6W 37 16 37.6N

            //enter "13SBB" should get "13SBB0000000000"
            Assert.IsTrue(CoordinateUSNG.TryParse("13SBB", out coord));
            Assert.AreEqual(0, coord.Northing);
            Assert.AreEqual(0, coord.Easting);

            //In actual conversion, we should bget a different case in a zipper region
            //"13SBB" should end up as "12SYF3393697916"
            //Assert.AreEqual("12S", coord.GZD);
            //Assert.AreEqual("YF", coord.GS);
            //Assert.AreEqual(33936, coord.Northing);
            //Assert.AreEqual(97916, coord.Easting);

            Assert.IsTrue(CoordinateUSNG.TryParse("13SBB42", out coord));
            Assert.AreEqual(40000, coord.Easting);
            Assert.AreEqual(20000, coord.Northing);

            Assert.IsTrue(CoordinateUSNG.TryParse("13SBB4429", out coord));
            Assert.AreEqual(44000, coord.Easting);
            Assert.AreEqual(29000, coord.Northing);

            Assert.IsTrue(CoordinateUSNG.TryParse("13SBB447294", out coord));
            Assert.AreEqual(44700, coord.Easting);
            Assert.AreEqual(29400, coord.Northing);

            Assert.IsTrue(CoordinateUSNG.TryParse("13SBB44762949", out coord));
            Assert.AreEqual(44760, coord.Easting);
            Assert.AreEqual(29490, coord.Northing);

            Assert.IsTrue(CoordinateUSNG.TryParse("13SBB4476129499", out coord));
            Assert.AreEqual(44761, coord.Easting);
            Assert.AreEqual(29499, coord.Northing);

            // test bad USNG with odd number northings/eastings
            Assert.IsFalse(CoordinateUSNG.TryParse("13SBB447612949", out coord));
            Assert.IsFalse(CoordinateUSNG.TryParse("13SBB4476294", out coord));
            Assert.IsFalse(CoordinateUSNG.TryParse("13SBB44729", out coord));
            Assert.IsFalse(CoordinateUSNG.TryParse("13SBB442", out coord));
            Assert.IsFalse(CoordinateUSNG.TryParse("13SBB4", out coord));

        }

        [TestMethod]
        public void FormatterDD()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
            var coord = new CoordinateDD(40.273048, -78.847427);
            var temp = coord.ToString("Y0.0#N X0.0#E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40.27N 78.85W");

            temp = coord.ToString("Y0.0#S X0.0#W", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40.27N 78.85W");

            temp = coord.ToString("Y+-0.##N X+-0.##E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "+40.27N -78.85W");

            temp = coord.ToString("Y+-0.0# X+-0.0#", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "+40.27 -78.85");

            temp = coord.ToString("Y0.0# N, X0.0# E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40.27 N, 78.85 W");

            temp = coord.ToString("Y0.0#° N, X0.0#° E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40.27° N, 78.85° W");

            temp = coord.ToString("N Y0.0#°, E X0.0#°", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "N 40.27°, W 78.85°");

            // test the default
            temp = coord.ToString("", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40.273048 -78.847427");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
            temp = coord.ToString("Y0.0#N X0.0#E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40,27N 78,85W");

            temp = coord.ToString("Y0.0#S X0.0#W", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40,27N 78,85W");

            temp = coord.ToString("Y+-0.##N X+-0.##E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "+40,27N -78,85W");

            temp = coord.ToString("Y+-0.0# X+-0.0#", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "+40,27 -78,85");

            temp = coord.ToString("Y0.0# N, X0.0# E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40,27 N, 78,85 W");

            temp = coord.ToString("Y0.0#° N, X0.0#° E", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40,27° N, 78,85° W");

            temp = coord.ToString("N Y0.0#°, E X0.0#°", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "N 40,27°, W 78,85°");

            // test the default
            temp = coord.ToString("", new CoordinateDDFormatter());
            Assert.AreEqual(temp, "40,273048 -78,847427");
        }

        [TestMethod]
        public void FormatterMGRS()
        {
            var coord = new CoordinateMGRS("17T", "PE", 83016, 60286);
            var temp = coord.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17TPE8301660286");

            temp = coord.ToString("Z S X00000 Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17T PE 83016 60286");

            temp = coord.ToString("Z,S,X00000,Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17T,PE,83016,60286");
            
            temp = coord.ToString("Z-S-X00000-Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17T-PE-83016-60286");
            
            temp = coord.ToString("ZS X00000Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17TPE 8301660286");
            
            temp = coord.ToString("ZS X00000 Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17TPE 83016 60286");

            // test the default
            temp = coord.ToString("", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "17TPE8301660286");

            // test 11SMT9902
            coord = new CoordinateMGRS("11S", "MT", 99000, 2000);
            temp = coord.ToString("ZSX00000Y00000", new CoordinateMGRSFormatter());
            Assert.AreEqual(temp, "11SMT9900002000");

        }

        [TestMethod]
        public void FormatterDDM()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
            var coord = new CoordinateDDM(40, 16.38288, -78, 50.84562);
            var temp = coord.ToString("", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40°16.3829' -78°50.8456'");

            temp = coord.ToString("A0°B0.0#####'N X0°Y0.0#####'E", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40°16.38288'N 78°50.84562'W");

            temp = coord.ToString("A+-0°B0.0#####' X+-0°Y0.0#####'", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "+40°16.38288' -78°50.84562'");

            temp = coord.ToString("NA0°B0.0#####' EX0°Y0.0#####'", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "N40°16.38288' W78°50.84562'");
            
            temp = coord.ToString("A0°B0.0#####'N, X0°Y0.0#####'E", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40°16.38288'N, 78°50.84562'W");
            
            temp = coord.ToString("A0° B0.0#####'N X0° Y0.0#####'E", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40° 16.38288'N 78° 50.84562'W");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
            temp = coord.ToString("", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40°16,3829' -78°50,8456'");

            temp = coord.ToString("A0°B0.0#####'N X0°Y0.0#####'E", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40°16,38288'N 78°50,84562'W");

            temp = coord.ToString("A+-0°B0.0#####' X+-0°Y0.0#####'", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "+40°16,38288' -78°50,84562'");

            temp = coord.ToString("NA0°B0.0#####' EX0°Y0.0#####'", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "N40°16,38288' W78°50,84562'");

            temp = coord.ToString("A0°B0.0#####'N, X0°Y0.0#####'E", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40°16,38288'N, 78°50,84562'W");

            temp = coord.ToString("A0° B0.0#####'N X0° Y0.0#####'E", new CoordinateDDMFormatter());
            Assert.AreEqual(temp, "40° 16,38288'N 78° 50,84562'W");
        }

        [TestMethod]
        public void FormatterDMS()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
            var coord = new CoordinateDMS(40, 16, 22.9728, -78, 50, 50.7372);
            var temp = coord.ToString("", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40°16'22.97\"N 78°50'50.74\"W");

            temp = coord.ToString("A0°B0'C0.0##\"N X0°Y0'Z0.0##\"E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40°16'22.973\"N 78°50'50.737\"W");

            temp = coord.ToString("NA0°B0'C0.0##\" EX0°Y0'Z0.0##\"", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "N40°16'22.973\" W78°50'50.737\"");

            temp = coord.ToString("A0° B0' C0.0##\" N X0° Y0' Z0.0##\" E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40° 16' 22.973\" N 78° 50' 50.737\" W");

            temp = coord.ToString("A+-#°B0'C0.0##\" X+-#°Y0'Z0.0##\"", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "+40°16'22.973\" -78°50'50.737\"");

            temp = coord.ToString("A0 B0 C0.0## N X0 Y0 Z0.0## E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40 16 22.973 N 78 50 50.737 W");

            temp = coord.ToString("A0°B0'C0.0##\"N, X0°Y0'Z0.0##\"E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40°16'22.973\"N, 78°50'50.737\"W");

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("es-ES");
            temp = coord.ToString("", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40°16'22,97\"N 78°50'50,74\"W");

            temp = coord.ToString("A0°B0'C0.0##\"N X0°Y0'Z0.0##\"E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40°16'22,973\"N 78°50'50,737\"W");

            temp = coord.ToString("NA0°B0'C0.0##\" EX0°Y0'Z0.0##\"", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "N40°16'22,973\" W78°50'50,737\"");

            temp = coord.ToString("A0° B0' C0.0##\" N X0° Y0' Z0.0##\" E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40° 16' 22,973\" N 78° 50' 50,737\" W");

            temp = coord.ToString("A+-#°B0'C0.0##\" X+-#°Y0'Z0.0##\"", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "+40°16'22,973\" -78°50'50,737\"");

            temp = coord.ToString("A0 B0 C0.0## N X0 Y0 Z0.0## E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40 16 22,973 N 78 50 50,737 W");

            temp = coord.ToString("A0°B0'C0.0##\"N, X0°Y0'Z0.0##\"E", new CoordinateDMSFormatter());
            Assert.AreEqual(temp, "40°16'22,973\"N, 78°50'50,737\"W");
        }

        [TestMethod]
        public void FormatterGARS()
        {
            var coord = new CoordinateGARS(203, "LW", 1, 8);
            var temp = coord.ToString("", new CoordinateGARSFormatter());
            Assert.AreEqual(temp, "203LW18");

            temp = coord.ToString("X# Y QK", new CoordinateGARSFormatter());
            Assert.AreEqual(temp, "203 LW 18");
            
            temp = coord.ToString("X# Y Q K", new CoordinateGARSFormatter());
            Assert.AreEqual(temp, "203 LW 1 8");
            
            temp = coord.ToString("X#-Y-QK", new CoordinateGARSFormatter());
            Assert.AreEqual(temp, "203-LW-18");
            
            temp = coord.ToString("X#-Y-Q-K", new CoordinateGARSFormatter());
            Assert.AreEqual(temp, "203-LW-1-8");

            temp = coord.ToString("X#,Y,Q,K", new CoordinateGARSFormatter());
            Assert.AreEqual(temp, "203,LW,1,8");
        }

        [TestMethod]
        public void FormatterUTM()
        {
            var coord = new CoordinateUTM(17, "P", 683016, 4460286);
            var temp = coord.ToString("", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17P 683016 4460286");

            temp = coord.ToString("Z+-# X0m Y0m", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "+17 683016m 4460286m");

            temp = coord.ToString("Z#H X0m E Y0m N", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17N 683016m E 4460286m N");
            
            temp = coord.ToString("Z#H X0 E Y0 N", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17N 683016 E 4460286 N");

            var coord2 = new CoordinateUTM(17, "C", 683016, 4460286);
            temp = coord2.ToString("", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17C 683016 4460286");

            temp = coord2.ToString("Z#H X0 Y0", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17S 683016 4460286");
        }

        [TestMethod]
        public void CoordDD()
        {
            var dms = new CoordinateDDM(38, 53.2788, -76, 56.907);
            var dd = new CoordinateDD(dms);
            Assert.AreEqual(38.88798, dd.Lat);
            Assert.AreEqual(-76.94845, dd.Lon);
        }
        //[TestMethod]
        //public void CCViewModel()
        //{
        //    var ctvm = new CoordinateConversionViewModel();

        //    //Assert.IsNotNull(ctvm.OCView);
        //}
    }
}
