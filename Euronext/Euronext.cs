using System;

using mbdt.Utils;

namespace mbdt.Euronext
{
    /// <summary>
    /// Encapsulates NYSE EuroNext specific information.
    /// </summary>
    static class Euronext
    {
        #region IsValidMep
        /// <summary>
        /// Checks the validity of a MEP (market entry place).
        /// </summary>
        /// <param name="mep">The market entry place (MEP).</param>
        /// <returns>True if the MEP is valid.</returns>
        public static bool IsValidMep(string mep)
        {
            return "AMS".Equals(mep) || "PAR".Equals(mep) || "BRU".Equals(mep) || "LIS".Equals(mep) || "LON".Equals(mep) || "OTH".Equals(mep) || "OTHER".Equals(mep);
        }
        #endregion

        #region MepToInteger
        /// <summary>
        /// Converts a MEP (market entry place) to a conventional integer value.
        /// </summary>
        /// <param name="mep"></param>
        /// <returns>An integer value of the MEP.</returns>
        public static int MepToInteger(string mep)
        {
            if ("AMS".Equals(mep))
                return 2;
            if ("PAR".Equals(mep))
                return 1;
            if ("BRU".Equals(mep))
                return 3;
            if ("LIS".Equals(mep))
                return 5;
            return 6; // other
        }
        #endregion

        #region IsHoliday
        /// <summary>
        /// Checks if the specified date is a EuroNext holiday (non-trading day).
        /// </summary>
        /// <param name="year">A year (YYYY).</param>
        /// <param name="month">A month (1 - 12).</param>
        /// <param name="day">A day (1 - 31).</param>
        /// <returns>True if the specified date is a EuroNext holiday.</returns>
        /// <remarks>
        /// <para>Sources:</para>
        /// <para>NYSE EuroNext Announces its 2013 Holiday Calendar.</para>
        /// <list type="table">
        /// <item><term>1 January 2013</term><description>New Year's Day</description></item>
        /// <item><term>29 March 2013</term><description>Good Friday</description></item>
        /// <item><term>1 April 2013</term><description>Easter Monday</description></item>
        /// <item><term>1 May 2013</term><description>Labour Day</description></item>
        /// <item><term>25 December 2013</term><description>Christmas Day</description></item>
        /// <item><term>26 December 2013</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>NYSE EuroNext Announces its 2012 Holiday Calendar.</para>
        /// <list type="table">
        /// <item><term>6 April 2012</term><description>Good Friday</description></item>
        /// <item><term>9 April 2012</term><description>Easter Monday</description></item>
        /// <item><term>1 May 2012</term><description>Labour Day</description></item>
        /// <item><term>25 December 2012</term><description>Christmas Day</description></item>
        /// <item><term>26 December 2012</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>NYSE EuroNext Announces its 2011 Holiday Calendar.</para>
        /// <list type="table">
        /// <item><term>22 April 2011</term><description>Good Friday</description></item>
        /// <item><term>25 April 2011</term><description>Easter Monday</description></item>
        /// <item><term>26 December 2011</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>NYSE EuroNext Announces its 2010 Holiday Calendar.</para>
        /// <list type="table">
        /// <item><term>Friday 1 January 2010</term><description>New Year's Day</description></item>
        /// <item><term>Friday 2 April 2010</term><description>Good Friday</description></item>
        /// <item><term>Monday 5 April 2010</term><description>Easter Monday</description></item>
        /// </list>
        /// <para>NYSE EuroNext Announces its 2009 Holiday Calendar.</para>
        /// <list type="table">
        /// <item><term>Thursday 1 January 2009</term><description>New Year's Day</description></item>
        /// <item><term>Friday 10 April 2009</term><description>Good Friday</description></item>
        /// <item><term>Monday 13 April 2009</term><description>Easter Monday</description></item>
        /// <item><term>Friday 1 May 2009 (*)</term><description>Labour Day</description></item>
        /// <item><term>Friday 25 December 2009</term><description>Christmas Day</description></item>
        /// </list>
        /// <para>NYSE EuroNext Announces its 2008 Holiday Calendar.</para>
        /// <list type="table">
        /// <item><term>Tuesday 1 January 2008</term><description>New Year's Day</description></item>
        /// <item><term>Friday 21 March 2008</term><description>Good Friday</description></item>
        /// <item><term>Monday 24 March 2008</term><description>Easter Monday</description></item>
        /// <item><term>Thursday 1 May 2008 (*)</term><description>Labour Day</description></item>
        /// <item><term>Thursday 25 December 2008</term><description>Christmas Day</description></item>
        /// <item><term>Friday 26 December 2008</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>2007 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Monday 1 January 2007</term><description>New Year's Day</description></item>
        /// <item><term>Friday 6 April 2007</term><description>Good Friday</description></item>
        /// <item><term>Monday 9 April 2007</term><description>Easter Monday</description></item>
        /// <item><term>Tuesday 1 May 2007 (*)</term><description>Labour Day</description></item>
        /// <item><term>Tuesday 25 December 2007</term><description>Christmas Day</description></item>
        /// <item><term>Wednesday 26 December 2007</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>2006 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Friday 14 April 2006</term><description>Good Friday</description></item>
        /// <item><term>Monday 17 April 2006</term><description>Easter Monday</description></item>
        /// <item><term>Monday 1 May 2006 (*)</term><description>Labour Day</description></item>
        /// <item><term>Monday 25 December 2006</term><description>Christmas Day</description></item>
        /// <item><term>Tuesday 26 December 2006</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>2005 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Friday 25 March 2005</term><description>Good Friday</description></item>
        /// <item><term>Monday 28 March 2005</term><description>Easter Monday</description></item>
        /// <item><term>Monday 26 December 2005</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>2004 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Thursday 1 January 2004</term><description>New Year's Day</description></item>
        /// <item><term>Friday 9 April 2004</term><description>Good Friday</description></item>
        /// <item><term>Monday 12 April 2004</term><description>Easter Monday</description></item>
        /// </list>
        /// <para>2003 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Wednesday 1 January 2003</term><description>New Year's Day</description></item>
        /// <item><term>Friday 18 April 2003</term><description>Good Friday</description></item>
        /// <item><term>Monday 21 April 2003</term><description>Easter Monday</description></item>
        /// <item><term>Thursday 1 May 2003 (*)</term><description>Labour Day</description></item>
        /// <item><term>Thursday 25 December 2003</term><description>Christmas Day</description></item>
        /// <item><term>Friday 26 December 203</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>2002 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Tuesday 1 January 2002</term><description>New Year's Day</description></item>
        /// <item><term>Friday 29 March 2002</term><description>Good Friday</description></item>
        /// <item><term>Monday 1st April 2002</term><description>Easter Monday</description></item>
        /// <item><term>Wednesday 1st May 2002 (*)</term><description>Labour Day</description></item>
        /// <item><term>Wednesday 25 December 2002</term><description>Christmas Day</description></item>
        /// <item><term>Thursday 26 December 2002</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>2001 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Monday 1 January 2001</term><description>New Year's Day</description></item>
        /// <item><term>Friday 13 April 2001</term><description>Good Friday</description></item>
        /// <item><term>Monday 16 April 2001</term><description>Easter Monday</description></item>
        /// <item><term>Tuesday 1 May 2001 (*)</term><description>Labour Day</description></item>
        /// <item><term>Monday 4 June 2001</term><description>Whit Monday</description></item>
        /// <item><term>Tuesday 25 December 2001</term><description>Christmas Day</description></item>
        /// <item><term>Wednesday 26 December 2001</term><description>Boxing Day</description></item>
        /// <item><term>Monday 31 December 2001</term><description>Last trading day - change over to euro</description></item>
        /// </list>
        /// <para>2000 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Friday 21 April 2000</term><description>Good Friday</description></item>
        /// <item><term>Monday 24 April 2000</term><description>Easter Monday</description></item>
        /// <item><term>Monday 1 May 2000 (*)</term><description>Labour Day</description></item>
        /// <item><term>Monday 12 June 2000</term><description>Whit Monday</description></item>
        /// <item><term>Monday 25 December 2000</term><description>Christmas Day</description></item>
        /// <item><term>Tuesday 26 December 2000</term><description>Boxing Day</description></item>
        /// </list>
        /// <para>1999 calendar for EuroNext markets.</para>
        /// <list type="table">
        /// <item><term>Friday 1 January 1999</term><description>New Year's Day</description></item>
        /// <item><term>Friday 2 April 1999</term><description>Good Friday</description></item>
        /// <item><term>Monday 5 April 1999</term><description>Easter Monday</description></item>
        /// <item><term>Monday 24 May 1999</term><description>Whit Monday</description></item>
        /// </list>
        /// <para>(*) Certain Euronext.liffe contracts will however be open, i.e. interest rate products, UK-based commodity contracts and those derivatives for which the underlying stocks are available for trading.</para>
        /// </remarks>
        private static bool IsHoliday(int year, int month, int day)
        {
            if (2013 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // 1 January 2013
                }
                else if (3 == month)
                {
                    if (29 == day)
                        return true; // 29 March 2013
                }
                else if (4 == month)
                {
                    if (1 == day)
                        return true; // 1 April 2013
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // 1 May 2013
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // 25 December 2013
                    if (26 == day)
                        return true; // 26 December 2013
                }
            }
            else if (2012 == year)
            {
                if (4 == month)
                {
                    if (6 == day)
                        return true; // Friday 6 April 2012
                    if (9 == day)
                        return true; // Monday 9 April 2012
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // 1 May 2012
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // 25 December 2012
                    if (26 == day)
                        return true; // 26 December 2012
                }
            }
            else if (2011 == year)
            {
                if (4 == month)
                {
                    if (22 == day)
                        return true; // Friday 22 April 2011
                    if (25 == day)
                        return true; // Monday 25 April 2011
                }
                else if (12 == month)
                {
                    if (26 == day)
                        return true; // Monday 26 December 2012
                }
            }
            else if (2010 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Friday 1 January 2010
                }
                else if (4 == month)
                {
                    if (2 == day)
                        return true; // Friday 2 April 2010
                    if (5 == day)
                        return true; // Monday 5 April 2010
                }
            }
            else if (2009 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Thursday 1 January 2009
                }
                else if (4 == month)
                {
                    if (10 == day)
                        return true; // Friday 10 April 2009
                    if (13 == day)
                        return true; // Monday 13 April 2009
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Friday 1 May 2009
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Friday 25 December 2009
                }
            }
            else if (2008 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Tuesday 1 January 2008
                }
                else if (3 == month)
                {
                    if (21 == day)
                        return true; // Friday 21 March 2008
                    if (24 == day)
                        return true; // Monday 24 March 2008
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Thursday 1 May 2008
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Thursday 25 December 2008
                    if (26 == day)
                        return true; // Friday 26 December 2008
                }
            }
            else if (2007 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Monday 1 January 2007
                }
                else if (4 == month)
                {
                    if (6 == day)
                        return true; // Friday 6 April 2007
                    if (9 == day)
                        return true; // Monday 9 April 2007
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Tuesday 1 May 2007
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Tuesday 25 December 2007
                    if (26 == day)
                        return true; // Wednesday 26 December 2007
                }
            }
            else if (2006 == year)
            {
                if (4 == month)
                {
                    if (14 == day)
                        return true; // Friday 14 April 2006
                    if (17 == day)
                        return true; // Monday 17 April 2006
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Monday 1 May 2006
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Monday 25 December 2006
                    if (26 == day)
                        return true; // Tuesday 26 December 2006
                }
            }
            else if (2005 == year)
            {
                if (3 == month)
                {
                    if (25 == day)
                        return true; // Friday 25 March 2005
                    if (28 == day)
                        return true; // Monday 28 March 2005
                }
                else if (12 == month)
                {
                    if (26 == day)
                        return true; // Monday 26 December 2005
                }
            }
            else if (2004 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Thursday 1 January 2004
                }
                else if (4 == month)
                {
                    if (9 == day)
                        return true; // Friday 9 April 2004
                    if (12 == day)
                        return true; // Monday 12 April 2004
                }
            }
            else if (2003 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Wednesday 1 January 2003
                }
                else if (4 == month)
                {
                    if (18 == day)
                        return true; // Friday 18 April 2003
                    if (21 == day)
                        return true; // Monday 21 April 2003
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Thursday 1 May 2003
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Thursday 25 December 2003
                    if (26 == day)
                        return true; // Friday 26 December
                }
            }
            else if (2002 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Tuesday 1 January 2002
                }
                else if (3 == month)
                {
                    if (29 == day)
                        return true; // Friday 29 March 2002
                }
                else if (4 == month)
                {
                    if (1 == day)
                        return true; // Monday 1 April 2002
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Wednesday 1 May 2002
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Wednesday 25 December 2002
                    if (26 == day)
                        return true; // Thursday 26 December
                }
            }
            else if (2001 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Monday 1 January 2001
                }
                else if (4 == month)
                {
                    if (13 == day)
                        return true; // Friday 13 April 2001
                    if (16 == day)
                        return true; // Monday 16 April 2001
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Tuesday 1 May 2001
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Tuesday 25 December 2001
                    if (26 == day)
                        return true; // Wednesday 26 December 2001
                    if (31 == day)
                        return true; // Monday 31 December 2001, change over to Euro
                }
            }
            else if (2000 == year)
            {
                if (4 == month)
                {
                    if (21 == day)
                        return true; // Friday 21 April 2000
                    if (24 == day)
                        return true; // Monday 24 April 2000
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true; // Monday 1 May 2000
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true; // Monday 25 December 2000
                    if (26 == day)
                        return true; // Tuesday 26 December 2000
                }
            }
            else if (1999 == year)
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true; // Friday 1 January 1999
                }
                else if (4 == month)
                {
                    if (2 == day)
                        return true; // Friday 2 April 1999
                    if (5 == day)
                        return true; // Monday 5 April 1999
                }
                else if (5 == month)
                {
                    if (24 == day)
                        return true; // Monday 24 May 1999
                }
            }
            else
            {
                if (1 == month)
                {
                    if (1 == day)
                        return true;
                }
                else if (5 == month)
                {
                    if (1 == day)
                        return true;
                }
                else if (12 == month)
                {
                    if (25 == day)
                        return true;
                    if (26 == day)
                        return true;
                }
                int jdn = JulianDayNumber.ToJdn(year, month, day);
                return Computus.IsGoodFriday(jdn) || Computus.IsEasterMonday(jdn);
            }
            return false;
        }

        /// <summary>
        /// Checks if the specified Julian Day Number is a EuroNext holiday (non-trading day).
        /// </summary>
        /// <param name="jdn">A Julian Day Number.</param>
        /// <returns>True if the specified JDN is a EuroNext holiday.</returns>
        public static bool IsHoliday(int jdn)
        {
            int year, month, day;
            JulianDayNumber.ToYmd(jdn, out year, out month, out day);
            return IsHoliday(year, month, day);
        }

        /// <summary>
        /// Checks if the specified date is a EuroNext holiday (non-trading day).
        /// </summary>
        /// <param name="dateTime">A date.</param>
        /// <returns>True if the specified date is a EuroNext holiday.</returns>
        private static bool IsHoliday(DateTime dateTime)
        {
            return IsHoliday(dateTime.Year, dateTime.Month, dateTime.Day);
        }
        #endregion

        #region IsWorkday
        /// <summary>
        /// Checks if the specified Julian Day Number is a EuroNext working (trading) day.
        /// </summary>
        /// <param name="jdn">A Julian Day Number.</param>
        /// <returns>True if the specified JDN is a EuroNext workday.</returns>
        public static bool IsWorkday(int jdn)
        {
            return !JulianDayNumber.IsWeekend(jdn) && !IsHoliday(jdn);
        }

        /// <summary>
        /// Checks if the specified date is a EuroNext working (trading) day.
        /// </summary>
        /// <param name="dateTime">A date.</param>
        /// <returns>True if the specified date is a EuroNext workday.</returns>
        public static bool IsWorkday(DateTime dateTime)
        {
            return !(DayOfWeek.Saturday == dateTime.DayOfWeek || DayOfWeek.Sunday == dateTime.DayOfWeek || IsHoliday(dateTime));
        }
        #endregion
    }
}
