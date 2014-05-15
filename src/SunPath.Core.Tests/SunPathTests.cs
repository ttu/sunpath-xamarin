using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SunPath.Core.Tests
{
    [TestClass]
    public class SunPathTests
    {
        [TestMethod]
        public void GetData_DateTimeNow()
        {
            var date = DateTime.Now;

            var longitude = 60.2;
            var latitude = 24.76;

            var pos = SunPosition.CalculateSunPosition(date, longitude, latitude);

            Assert.IsNotNull(pos);
        }

        [TestMethod]
        public void GetData_WholeDay_DateTimeNow()
        {
            var date = DateTime.Now;

            var longitude = 60.2;
            var latitude = 24.76;

            var interVal = 10;

            var pathData = SunPath.GetPath(date, longitude, latitude, interVal);

            Assert.IsNotNull(pathData);
        }

        [TestMethod]
        public void GetData_WholeDay_DateTimeFixed()
        {
            var date = new DateTime(2010, 10, 20);

            var longitude = 60.2;
            var latitude = 24.76;
            var interVal = 10;

            var source = SunPath.GetPath(date, longitude, latitude, interVal);

            var minX = 100;
            var maxX = 300;
            var minY = 20;
            var maxY = 220;

            var pathData = SunPath.GetPointsInArea(source, minX, maxX, minY, maxY);

            Assert.IsNotNull(pathData);
        }
        
    }
}