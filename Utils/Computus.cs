using System;

namespace mbdt.Utils
{
    /// <summary>
    /// Computus (Latin for computation) is the calculation of the date of Easter in the Christian calendar.
    /// <para>
    /// The name has been used for this procedure since the early Middle Ages, as it was one of the most important computations of the age.
    /// </para>
    /// </summary>
    static class Computus
    {
        #region EasterSundayFromYear
        /// <summary>
        /// Compute the day of the year that Easter falls on.
        /// <para>Reference: Knuth, volume 1, page 155.</para>
        /// </summary>
        /// <param name="year">A year.</param>
        /// <returns>The Julian day number of the Easter Sunday.</returns>
        public static int EasterSundayFromYear(int year)
        {
		    if (1583 > year)
			    throw new ArgumentException("Algorithm is invalid before April 1583");
		    int golden = (year % 19) + 1; // E1: metonic cycle
		    int century = (year / 100) + 1; // E2: e.g. 1984 was in 20th century
		    int x = (3 * century / 4) - 12; // E3: leap year correction
		    int z = ((8 * century + 5) / 25) -5; // E3: sync with moon's orbit
		    int d = (5 * year / 4) - x - 10;
		    int epact = (11 * golden + 20 + z - x) % 30; // E5: epact
		    if ((25 == epact && 11 < golden) || 24 == epact)
			    epact++;
		    int n = 44 - epact;
		    n += 30 * (n < 21 ? 1 : 0); // E6:
		    n += 7 - ((d + n) % 7);
		    if (31 < n) //E7:
			    return JulianDayNumber.ToJdn(year, 4, n - 31); // April
            return JulianDayNumber.ToJdn(year, 3, n); // March
        }

        /// <summary>
        /// Compute the day of the year that Easter falls on.
        /// <para>Reference: Knuth, volume 1, page 155.</para>
        /// </summary>
        /// <param name="year">A year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        public static void EasterSundayFromYear(int year, out int month, out int day)
        {
		    if (1583 > year)
			    throw new ArgumentException("Algorithm is invalid before April 1583");
		    int golden = (year % 19) + 1; // E1: metonic cycle
		    int century = (year / 100) + 1; // E2: e.g. 1984 was in 20th century
		    int x = (3 * century / 4) - 12; // E3: leap year correction
		    int z = ((8 * century + 5) / 25) -5; // E3: sync with moon's orbit
		    int d = (5 * year / 4) - x - 10;
		    int epact = (11 * golden + 20 + z - x) % 30; // E5: epact
		    if ((25 == epact && 11 < golden) || 24 == epact)
			    epact++;
		    day = 44 - epact;
		    day += 30 * (day < 21 ? 1 : 0); // E6:
		    day += 7 - ((d + day) % 7);
		    if (31 < day) // E7:
            {
                month = 4; // April
                day -= 31;
            }
            else
                month = 3; // March
	    }
        #endregion

        #region EasterSundayFromJdn
        /// <summary>
        /// Compute the day of the year that Easter falls on.
        /// </summary>
        /// <param name="jdn">A reference Julian day number to extract a year.</param>
        /// <returns>The Julian day number of the Easter Sunday.</returns>
	    public static int EasterSundayFromJdn(int jdn)
        {
            int year, month, day;
		    JulianDayNumber.ToYmd(jdn, out year, out month, out day);
		    return EasterSundayFromYear(year);
	    }

        /// <summary>
        /// Compute the day of the year that Easter falls on.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        public static void EasterSundayFromJdn(int jdn, out int month, out int day)
        {
            int year;
            JulianDayNumber.ToYmd(jdn, out year, out month, out day);
        }
        #endregion

        #region IsShroveTuesday
        /// <summary>
        /// Checks if a Julian day number is Shrove Tuesday, aka Mardi Gras (47 days before Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Shrove Tuesday.</returns>
	    public static bool IsShroveTuesday(int jdn)
        {
		    return (EasterSundayFromJdn(jdn) - 47) == jdn;
	    }
        #endregion

        #region IsAshWednesday
        /// <summary>
        /// Checks if a Julian day number is Ash Wednesday, start of Lent (46 days before Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Ash Wednesday.</returns>
        public static bool IsAshWednesday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) - 46) == jdn;
        }
        #endregion

        #region IsPalmSunday
        /// <summary>
        /// Checks if a Julian day number is Palm Sunday (7 days before Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Palm Sunday.</returns>
        public static bool IsPalmSunday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) - 7) == jdn;
        }
        #endregion

        #region IsMaundyThursday
        /// <summary>
        /// Checks if a Julian day number is Maundy Thursday (3 days before Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Maundy Thursday.</returns>
        public static bool IsMaundyThursday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) - 3) == jdn;
        }
        #endregion

        #region IsGoodFriday
        /// <summary>
        /// Checks if a Julian day number is Good Friday (Karfreitag, 2 days before Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Good Friday.</returns>
        public static bool IsGoodFriday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) - 2) == jdn;
        }
        #endregion

        #region IsEasterSunday
        /// <summary>
        /// Checks if a Julian day number is Easter Sunday.
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Easter Sunday.</returns>
        public static bool IsEasterSunday(int jdn)
        {
            return EasterSundayFromJdn(jdn) == jdn;
        }
        #endregion

        #region IsEasterMonday
        /// <summary>
        /// Checks if a Julian day number is Easter Monday (Ostermontag, 1 day after Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Easter Monday.</returns>
        public static bool IsEasterMonday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) + 1) == jdn;
        }
        #endregion

        #region IsAscensionThursday
        /// <summary>
        /// Checks if a Julian day number is Ascension Thursday (Christi Himmelfahrt, 39 days after Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Ascension Thursday.</returns>
        public static bool IsAscensionThursday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) + 39) == jdn;
        }
        #endregion

        #region IsWhitSunday
        /// <summary>
        /// Checks if a Julian day number is Pentecost, Whit Sunday (49 days after Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Whit Sunday (Pentecost).</returns>
        public static bool IsWhitSunday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) + 49) == jdn;
        }
        #endregion

        #region IsWhitMonday
        /// <summary>
        /// Checks if a Julian day number is Whit Monday (Pfingstmontag, 50 days after Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Whit Monday.</returns>
        public static bool IsWhitMonday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) + 50) == jdn;
        }
        #endregion

        #region IsTrinitySunday
        /// <summary>
        /// Checks if a Julian day number is Trinity Sunday (59 days after Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Trinity Sunday.</returns>
        public static bool IsTrinitySunday(int jdn)
        {
            return (EasterSundayFromJdn(jdn) + 59) == jdn;
        }
        #endregion

        #region IsCorpusChristi
        /// <summary>
        /// Checks if a Julian day number is Corpus Christi (60 days after Easter).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Corpus Christi.</returns>
        public static bool IsCorpusChristi(int jdn)
        {
            return (EasterSundayFromJdn(jdn) + 60) == jdn;
        }
        #endregion

        #region IsChristmasDay
        /// <summary>
        /// Checks if a Julian day number is Christmas Day (25 December).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Christmas Day.</returns>
        public static bool IsChristmasDay(int jdn)
        {
            int year, month, day;
		    JulianDayNumber.ToYmd(jdn, out year, out month, out day);
			return (12 == month) && (25 == day);
        }
        #endregion

        #region IsBoxingDay
        /// <summary>
        /// Checks if a Julian day number is Boxing Day (26 December).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is Boxing Day.</returns>
        public static bool IsBoxingDay(int jdn)
        {
            int year, month, day;
		    JulianDayNumber.ToYmd(jdn, out year, out month, out day);
			return (12 == month) && (26 == day);
        }
        #endregion

        #region IsNewYearDay
        /// <summary>
        /// Checks if a Julian day number is New Year Day (1 January).
        /// </summary>
        /// <param name="jdn">A Julian day number.</param>
        /// <returns>True if the <paramref name="jdn"/> is New Year Day.</returns>
        public static bool IsNewYearDay(int jdn)
        {
            int year, month, day;
		    JulianDayNumber.ToYmd(jdn, out year, out month, out day);
			return (1 == month) && (1 == day);
        }
        #endregion
    }
}
