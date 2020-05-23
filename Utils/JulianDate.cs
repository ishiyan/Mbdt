using System;

namespace mbdt.Utils
{
    static class JulianDate
    {
        #region ToJulianDate
        /// <summary>
        /// Converts a specific date to a fractional Julian Date.
        /// </summary>
        /// <param name="year">A full year (YYYY).</param>
        /// <param name="month">A month (1 - 12).</param>
        /// <param name="day">A fractional day (1 - 31).</param>
        /// <returns>A fractional Julian Date.</returns>
        public static double ToJulianDate(int year, int month, double day)
        {
            if (month < 3)
            {
                year--;
                month += 12;
            }
            // Gregorian Calendar adopted Oct. 15, 1582
            if (year > 1582 || (year == 1582 && (month > 10 || (month == 10 && day >= 15.0))))
            {
                long a = (long)(365.25 * (double)year) + (long)(year / 400) - (long)(year / 100) + (long)(30.59 * (double)(month - 2));
                return (double)a + day + 1721088.5;
            }
            else
            {
                long a = (long)(365.25 * (double)year) + (long)(30.59 * (double)(month - 2));
                return (year < 0) ? ((double)a + day + 1721085.5) : ((double)a + day + 1721086.5);
            }
        }

        /// <summary>
        /// Converts a specific date to a fractional Julian Date.
        /// </summary>
        /// <param name="year">A full year (YYYY).</param>
        /// <param name="month">A month (1 - 12).</param>
        /// <param name="day">A day (1 - 31).</param>
        /// <returns>A fractional Julian Date.</returns>
        public static double ToJulianDate(int year, int month, int day)
        {
            return ToJulianDate(year, month, (double)day);
        }

        /// <summary>
        /// Converts a specific date to a fractional Julian Date.
        /// </summary>
        /// <param name="year">A full year (YYYY).</param>
        /// <param name="month">A month (1 - 12).</param>
        /// <param name="day">A day (1 - 31).</param>
        /// <param name="hour">An hour (0 - 23).</param>
        /// <param name="minute">A minute (0 - 59).</param>
        /// <param name="second">A second (0 - 59).</param>
        /// <returns>A fractional Julian Date.</returns>
        public static double ToJulianDate(int year, int month, int day, int hour, int minute, int second)
        {
            return ToJulianDate(year, month, (double)day) + (double)hour / 24.0 + (double)minute / 1440.0 + second / 86400.0;
        }

        /// <summary>
        /// Converts a specific date to a fractional Julian Date.
        /// </summary>
        /// <param name="dateTime">A date to convert.</param>
        /// <returns>A fractional Julian Date.</returns>
        public static double ToJulianDate(DateTime dateTime)
        {
            return ToJulianDate(dateTime.Year, dateTime.Month, (double)dateTime.Day) + (double)dateTime.Hour / 24.0 + (double)dateTime.Minute / 1440.0 + (double)dateTime.Second / 86400.0 + (double)dateTime.Millisecond / 86400000.0;
        }
        #endregion
    }
}
