using System;
using System.Globalization;
using System.Text;

namespace mbdt.Utils
{
    /// <summary>
    /// The Julian day number associated with the solar day is the number assigned to
    /// a day in a continuous count of days beginning with the Julian day number 0
    /// assigned to the day starting at Greenwich mean noon on 1 January 4713 BC,
    /// Julian proleptic calendar -4712. Julian day 0 is Monday.
    /// <para>
    /// The algorithms are from Press et al., Numerical Recipes in C, 2nd ed., Cambridge University Press 1992.
    /// </para>
    /// </summary>
    static class JulianDayNumber
    {
        #region ToJdn
        /// <summary>
        /// Converts a current date to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <returns>The Julian day number.</returns>
        public static int ToJdn()
        {
            return ToJdn(DateTime.Now);
        }

        /// <summary>
        /// Converts a date to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <param name="dateTime">A date</param>
        /// <returns>The Julian day number.</returns>
        public static int ToJdn(DateTime dateTime)
        {
            return ToJdn(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        /// Converts a date to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <param name="year">A full year (YYYY); positive year signifies AD, negative year BC. Note that the year after 1BC was 1AD.</param>
        /// <param name="month">A month (1 -12).</param>
        /// <param name="day">A day (1-31).</param>
        /// <returns>The Julian day number.</returns>
        public static int ToJdn(int year, int month, int day)
        {
            if (1 > month || month > 12)
                throw new ArgumentException(string.Concat("illegal month value ", month.ToString(CultureInfo.InvariantCulture)));
            if (1 > day || day > DaysInMonth(year, month))
                throw new ArgumentException(string.Concat("illegal day value ", day.ToString(CultureInfo.InvariantCulture), " for a month ", month.ToString(CultureInfo.InvariantCulture)));
            if (1582 == year && 10 == month && 4 < day && day < 15)
                throw new ArgumentException("the dates 5 through 14 October, 1582, do not exist in the Gregorian Calendar");
            int jy = year;
            if (jy < 0)
                jy++;
            int jm = month;
            if (month > 2)
                jm++;
            else
            {
                jy--;
                jm += 13;
            }
            var jul = (int)(Math.Floor(365.25 * jy) + Math.Floor(30.6001 * jm) + day + 1720995);
            // Gregorian Calendar adopted Oct. 15, 1582
            // 15 + 31 * (10 + 12 * 1582)
            if ((day + 31 * (month + 12 * year)) >= 588829)
            {
                var ja = (int)(0.01 * jy);
                jul += 2 - ja + (int)(0.25 * ja);
            }
            return jul;
        }
        #endregion

        #region ToYmd
        /// <summary>
        /// Converts a Julian day number to a year, month, day.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <param name="year">A year.</param>
        /// <param name="month">A month.</param>
        /// <param name="day">A day.</param>
        public static void ToYmd(int jdn, out int year, out int month, out int day)
        {
            int ja = jdn;
            // The JDN of the adoption of the Gregorian calendar
            if (jdn >= 2299161)
            {
                // Cross-over to Gregorian Calendar produces this correction
                var jalpha = (int)((jdn - 1867216 - 0.25) / 36524.25);
                ja += 1 + jalpha - (int)(0.25 * jalpha);
            }
            int jb = ja + 1524;
            var jc = (int)(6680.0 + (jb - 2439870 - 122.1) / 365.25);
            var jd = (int)(365 * jc + (0.25 * jc));
            var je = (int)((jb - jd) / 30.6001);
            day = jb - jd - (int)(30.6001 * je);
            month = je - 1;
            if (month > 12)
                month -= 12;
            year = jc - 4715;
            if (month > 2)
                --year;
            if (year <= 0)
                --year;
        }
        #endregion

        #region FromYyyymmdd
        /// <summary>
        /// Converts an YYYYMMDD date stamp to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <param name="yyyymmdd">A date stamp.</param>
        /// <returns>The Julian day number.</returns>
        public static int FromYyyymmdd(string yyyymmdd)
        {
            if (yyyymmdd.Length > 7)
            {
                char c = yyyymmdd[0];
                if ('0' <= c && c <= '9')
                {
                    int year = 1000 * (c - '0');
                    c = yyyymmdd[1];
                    if ('0' <= c && c <= '9')
                    {
                        year += 100 * (c - '0');
                        c = yyyymmdd[2];
                        if ('0' <= c && c <= '9')
                        {
                            year += 10 * (c - '0');
                            c = yyyymmdd[3];
                            if ('0' <= c && c <= '9')
                            {
                                year += c - '0';
                                c = yyyymmdd[4];
                                if ('0' <= c && c <= '9')
                                {
                                    int month = 10 * (c - '0');
                                    c = yyyymmdd[5];
                                    if ('0' <= c && c <= '9')
                                    {
                                        month += c - '0';
                                        c = yyyymmdd[6];
                                        if ('0' <= c && c <= '9')
                                        {
                                            int day = 10 * (c - '0');
                                            c = yyyymmdd[7];
                                            if ('0' <= c && c <= '9')
                                            {
                                                day += c - '0';
                                                try
                                                {
                                                    return ToJdn(year, month, day);
                                                }
                                                catch (ArgumentException e)
                                                {
                                                    throw new ArgumentException(InvalidDatestampMessage(yyyymmdd, e));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException(InvalidDatestampMessage(yyyymmdd));
        }
        #endregion

        #region FromDdsMmsYy
        /// <summary>
        /// Converts a DD/MM/YY date stamp to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <param name="ddSmmSyy">A date stamp.</param>
        /// <returns>The Julian day number.</returns>
        public static int FromDdsMmsYy(string ddSmmSyy)
        {
            if (ddSmmSyy.Length > 7)
            {
                char c = ddSmmSyy[0];
                if ('0' <= c && c <= '9')
                {
                    int day = 10 * (c - '0');
                    c = ddSmmSyy[1];
                    if ('0' <= c && c <= '9')
                    {
                        day += c - '0';
                        c = ddSmmSyy[2];
                        if ('/' == c)
                        {
                            c = ddSmmSyy[3];
                            if ('0' <= c && c <= '9')
                            {
                                int month = 10 * (c - '0');
                                c = ddSmmSyy[4];
                                if ('0' <= c && c <= '9')
                                {
                                    month += c - '0';
                                    c = ddSmmSyy[5];
                                    if ('/' == c)
                                    {
                                        c = ddSmmSyy[6];
                                        if ('0' <= c && c <= '9')
                                        {
                                            int year = 10 * (c - '0');
                                            c = ddSmmSyy[7];
                                            if ('0' <= c && c <= '9')
                                            {
                                                year += c - '0';
                                                year += (50 > year) ? 2000 : 1900;
                                                try
                                                {
                                                    return ToJdn(year, month, day);
                                                }
                                                catch (ArgumentException e)
                                                {
                                                    throw new ArgumentException(InvalidDatestampMessage(ddSmmSyy, e));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException(InvalidDatestampMessage(ddSmmSyy));
        }
        #endregion

        #region FromMmsDdsYy
        /// <summary>
        /// Converts a MM/DD/YY date stamp to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <param name="mmSddSyy">A date stamp.</param>
        /// <returns>The Julian day number.</returns>
        public static int FromMmsDdsYy(string mmSddSyy)
        {
            if (mmSddSyy.Length > 7)
            {
                char c = mmSddSyy[0];
                if ('0' <= c && c <= '9')
                {
                    int month = 10 * (c - '0');
                    c = mmSddSyy[1];
                    if ('0' <= c && c <= '9')
                    {
                        month += c - '0';
                        c = mmSddSyy[2];
                        if ('/' == c)
                        {
                            c = mmSddSyy[3];
                            if ('0' <= c && c <= '9')
                            {
                                int day = 10 * (c - '0');
                                c = mmSddSyy[4];
                                if ('0' <= c && c <= '9')
                                {
                                    day += c - '0';
                                    c = mmSddSyy[5];
                                    if ('/' == c)
                                    {
                                        c = mmSddSyy[6];
                                        if ('0' <= c && c <= '9')
                                        {
                                            int year = 10 * (c - '0');
                                            c = mmSddSyy[7];
                                            if ('0' <= c && c <= '9')
                                            {
                                                year += c - '0';
                                                year += (50 > year) ? 2000 : 1900;
                                                try
                                                {
                                                    return ToJdn(year, month, day);
                                                }
                                                catch (ArgumentException e)
                                                {
                                                    throw new ArgumentException(InvalidDatestampMessage(mmSddSyy, e));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException(InvalidDatestampMessage(mmSddSyy));
        }
        #endregion

        #region FromMmsDdsYyyy
        /// <summary>
        /// Converts a MM/DD/YYYY date stamp to an integer Julian Day Number (JDN) that begins at noon of this day.
        /// </summary>
        /// <param name="mmSddSyyyy">A date stamp.</param>
        /// <returns>The Julian day number.</returns>
        public static int FromMmsDdsYyyy(string mmSddSyyyy)
        {
            if (mmSddSyyyy.Length > 9)
            {
                char c = mmSddSyyyy[0];
                if ('0' <= c && c <= '9')
                {
                    int month = 10 * (c - '0');
                    c = mmSddSyyyy[1];
                    if ('0' <= c && c <= '9')
                    {
                        month += c - '0';
                        c = mmSddSyyyy[2];
                        if ('/' == c)
                        {
                            c = mmSddSyyyy[3];
                            if ('0' <= c && c <= '9')
                            {
                                int day = 10 * (c - '0');
                                c = mmSddSyyyy[4];
                                if ('0' <= c && c <= '9')
                                {
                                    day += c - '0';
                                    c = mmSddSyyyy[5];
                                    if ('/' == c)
                                    {
                                        c = mmSddSyyyy[6];
                                        if ('0' <= c && c <= '9')
                                        {
                                            int year = 1000 * (c - '0');
                                            c = mmSddSyyyy[7];
                                            if ('0' <= c && c <= '9')
                                            {
                                                year += 100 * (c - '0');
                                                c = mmSddSyyyy[8];
                                                if ('0' <= c && c <= '9')
                                                {
                                                    year += 10 * (c - '0');
                                                    c = mmSddSyyyy[9];
                                                    if ('0' <= c && c <= '9')
                                                    {
                                                        year += c - '0';
                                                        try
                                                        {
                                                            return ToJdn(year, month, day);
                                                        }
                                                        catch (ArgumentException e)
                                                        {
                                                            throw new ArgumentException(InvalidDatestampMessage(mmSddSyyyy, e));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            throw new ArgumentException(InvalidDatestampMessage(mmSddSyyyy));
        }
        #endregion

        #region ToYyyymmdd
        /// <summary>
        /// Converts a Julian day number to a YYYYMMDD date stamp string.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>The YYYYMMDD string.</returns>
        public static string ToYyyymmdd(int jdn)
        {
            int year, month, day;
            ToYmd(jdn, out year, out month, out day);
            var sb = new StringBuilder(8);
            if (year < 1000)
                sb.Append('0');
            if (year < 100)
                sb.Append('0');
            if (year < 10)
                sb.Append('0');
            sb.Append(year);
            if (month < 10)
                sb.Append('0');
            sb.Append(month);
            if (day < 10)
                sb.Append('0');
            sb.Append(day);
            return sb.ToString();
        }
        #endregion

        #region ToDateTime
        /// <summary>
        /// Converts a Julian day number to a DateTime instance.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>The DateTime instance.</returns>
        public static DateTime ToDateTime(int jdn)
        {
            int year, month, day;
            ToYmd(jdn, out year, out month, out day);
            return new DateTime(year, month, day);
        }
        #endregion

        #region IsSunday
        /// <summary>
        /// Checks if a Julian day number is Sunday. Based on the fact that Julian day number 0 is Monday.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Sunday.</returns>
        public static bool IsSunday(int jdn)
        {
            return 6 == (jdn % 7);
        }
        #endregion

        #region IsSaturday
        /// <summary>
        /// Checks if a Julian day number is Saturday. Based on the fact that Julian day number 0 is Monday.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Saturday.</returns>
        public static bool IsSaturday(int jdn)
        {
            return 5 == (jdn % 7);
        }
        #endregion

        #region IsWeekend
        /// <summary>
        /// Checks if a Julian day number is a weekend. Based on the fact that Julian day number 0 is Monday.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is a weekend.</returns>
        public static bool IsWeekend(int jdn)
        {
            return 4 < (jdn % 7);
        }
        #endregion

        #region DaysInMonth
        /// <summary>
        /// Calculates a number of days in the month.
        /// </summary>
        /// <param name="year">A year.</param>
        /// <param name="month">A month.</param>
        /// <returns>The number of days in the month.</returns>
        private static int DaysInMonth(int year, int month)
        {
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    return 31;
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;
                //case 2:
                default:
                    return (year % 4 == 0 && (year % 100 != 0 || year % 400 == 0)) ? 29 : 28;
            }
        }
        #endregion

        #region InvalidDatestampMessage
        /// <summary>
        /// A helper to format a invalid datestamp exception message.
        /// </summary>
        /// <param name="stamp">A date stamp.</param>
        /// <returns>A message text.</returns>
        private static string InvalidDatestampMessage(string stamp)
        {
            return string.Concat("invalid datestamp ", stamp);
        }

        /// <summary>
        /// A helper to format a invalid datestamp exception message.
        /// </summary>
        /// <param name="stamp">A date stamp.</param>
        /// <param name="e">A catched exception.</param>
        /// <returns>A message text.</returns>
        private static string InvalidDatestampMessage(string stamp, Exception e)
        {
            return string.Concat("invalid datestamp ", stamp, ", ", e.Message);
        }
        #endregion
    }
}
