using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CoordinateToolLibrary.Models;
using CoordinateToolLibrary.ViewModels;

namespace CoordinateToolLibrary.Tests
{
    [TestClass]
    public class CoordinateToolLibraryTests
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

    }
}
