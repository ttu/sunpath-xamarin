using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
