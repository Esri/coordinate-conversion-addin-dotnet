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
        [TestMethod]
        public void ParseDD()
        {
            CoordinateDD coord;
            Assert.IsTrue(CoordinateDD.TryParse("40.273048 -78.847427", out coord));
            Assert.AreEqual(40.273048, coord.Lat);
            Assert.AreEqual(-78.847427, coord.Lon);

            Assert.IsFalse(CoordinateDD.TryParse("", out coord));

            Assert.IsTrue(CoordinateDD.TryParse("40.273048N 78.847427W", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N,78.847427W", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N78.847427W", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N;78.847427W", out coord));
            Assert.IsTrue(CoordinateDD.TryParse("40.273048N:78.847427W", out coord));

            Assert.IsFalse(CoordinateDD.TryParse("This is not a coordinate", out coord));
        }

        [TestMethod]
        public void ParseDDM()
        {
            CoordinateDDM coord;
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288', -78°50.84562'", out coord));
            Assert.IsTrue(CoordinateDDM.TryParse("40°16.38288', -078°50.84562'", out coord));

            Assert.IsFalse(CoordinateDDM.TryParse("This is not a coordinate", out coord));
        }

        [TestMethod]
        public void ParseDMS()
        {
            CoordinateDMS coord;
            Assert.IsTrue(CoordinateDMS.TryParse("40°16'22.9728\", -78°50'50.7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40°16'22.9728\", -078°50'50.7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 16' 22.9728\", -78° 50' 50.7372\"", out coord));
            Assert.IsTrue(CoordinateDMS.TryParse("40° 7' 22.8\"N 77° 32' 38.4W\"", out coord));

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
        }

        [TestMethod]
        public void FormatterDD()
        {
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
        }

        [TestMethod]
        public void FormatterDDM()
        {
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
        }

        [TestMethod]
        public void FormatterDMS()
        {
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
            var coord = new CoordinateUTM(17, "N", 683016, 4460286);
            var temp = coord.ToString("", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17N 683016 4460286");

            temp = coord.ToString("Z+-# X0m Y0m", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "+17 683016m 4460286m");

            temp = coord.ToString("Z#H X0m E Y0m N", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17N 683016m E 4460286m N");
            
            temp = coord.ToString("Z#H X0 E Y0 N", new CoordinateUTMFormatter());
            Assert.AreEqual(temp, "17N 683016 E 4460286 N");
        }

        //[TestMethod]
        //public void CCViewModel()
        //{
        //    var ctvm = new CoordinateConversionViewModel();

        //    //Assert.IsNotNull(ctvm.OCView);
        //}
    }
}
