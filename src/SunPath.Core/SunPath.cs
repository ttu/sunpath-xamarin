using System;
using System.Collections.Generic;
using System.Linq;

namespace SunPath.Core
{
    public static class SunPath
    {
        public static Dictionary<DateTime, PositionData> GetPath(DateTime date, double longitude, double latitude, int intervalInMin)
        {
            var retVal = new Dictionary<DateTime, PositionData>();

            var dt = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            bool loop = true;

            while (loop)
            {
                dt = dt.AddMinutes(intervalInMin);

                PositionData pos;

                if (dt.Day != date.Day)
                {
                    dt = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    loop = false;
                }

                pos = SunPosition.CalculateSunPosition(dt, longitude, latitude);

                retVal.Add(dt, pos);
            }

            return retVal;
        }

        public static Dictionary<DateTime, PositionData> GetPointsInArea(Dictionary<DateTime, PositionData> source, int minX, int maxX, int minY, int maxY)
        {
            var selected = source.Where(s =>
                s.Value.Azimuth >= minX &&
                s.Value.Azimuth <= maxX &&
                s.Value.Altitude >= minY &&
                s.Value.Altitude <= maxY)
                .ToDictionary(x => x.Key, x => x.Value);

            return selected;
        }
    }
}