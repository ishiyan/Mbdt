
namespace mbdt.Utils
{
    /// <summary>
    /// ISIN (International Securities Identifying Number) utilities.
    /// </summary>
    static class Isin
    {
        /// <summary>
        /// Checks for the validity of the ISIN.
        /// </summary>
        /// <param name="isin">The ISIN.</param>
        /// <returns>True is ISIN is valid.</returns>
        /// <remarks>
        /// <para>
        /// An International Securities Identifying Number (ISIN) uniquely identifies a
        /// security. Its structure is defined in ISO 6166. Securities for which ISINs
        /// are issued include bonds, commercial paper, equities and warrants. The ISIN
        /// code is a 12-character alpha-numerical code that does not contain information
        /// characterizing financial instruments but serves for uniform identification of
        /// a security at trading and settlement.
        /// </para><para>
        /// Securities with which ISINs can be used include debt securities, shares,
        /// options, derivatives and futures. The ISIN identifies the security, not the
        /// exchange (if any) on which it trades; it is not a ticker symbol. For
        /// instance, DaimlerChrysler stock trades on twenty-two different stock
        /// exchanges worldwide, and is priced in five different currencies; it has the
        /// same ISIN on each, though not the same ticker symbol. ISIN cannot specify a
        /// particular trade in this case, and another identifier, typically the
        /// three-letter exchange code, will have to be specified in addition to the ISIN.
        /// </para><para>
        /// ISINs consist of three parts: a two letter country code, a nine character
        /// alpha-numeric national security identifier, and a single check digit. The
        /// country code is the ISO 3166-1 alpha-2 code for the country of issue, which
        /// is not necessarily the country in which the issuing company is domiciled.
        /// International securities cleared through Clearstream or Euroclear, which are
        /// Europe-wide, use "XS" as the country code.
        /// </para><para>
        /// The nine-digit security identifier is the National Securities Identifying
        /// Number, or NSIN, assigned by governing bodies in each country, known as the
        /// national numbering agency (NNA). In North America the NNA is the CUSIP
        /// organization, meaning that CUSIPs can easily be converted into ISINs by
        /// adding the US or CA country code to the beginning of the existing CUSIP code
        /// and adding an additional check digit at the end. In the United Kingdom and
        /// Ireland the NNA is the London Stock Exchange and the NSIN is the SEDOL,
        /// converted in a similar fashion after padding the SEDOL number out with
        /// leading zeros. Most other countries use similar conversions, but if no
        /// country NNA exists then regional NNAs are used instead.
        /// </para><para>
        /// ISIN check digits are based on the same "Modulus 10 Double Add Double"
        /// technique used in CUSIPs. To calculate the check digit, first convert any
        /// letters to numbers by adding their ordinal position in the alphabet to 9,
        /// such that A = 10 and M = 22. Starting with the right most digit, every other
        /// digit is multiplied by two. The resulting string of digits (numbers greater
        /// than 9 becoming two separate digits) are added up. Subtract this sum from the
        /// smallest number ending with zero that is greater than or equal to it: this
        /// gives the check digit which is also known as the ten's complement of the sum
        /// modulo 10.
        /// </para>
        /// </remarks>
        public static bool IsValid(string isin)
        {
		    char[] input = isin.ToCharArray();
		    int number = input.Length;
		    if (12 != number)
			    return false;
		    number = input[11];
		    if (number < '0' || number > '9'/*|| !Nna.IsValid(isin)*/)
			    return false;
		    int sum = 0;
		    bool multiply = true;
		    for (int i = 10; i > -1; --i)
            {
			    number = input[i];
			    if (i < 2)
                {
				    if (number >= 'A' && number <= 'Z')
					    number = number - 'A' + 10;
				    else
					    return false;
			    }
                else
                {
				    if (number >= 'A' && number <= 'Z')
					    number = number - 'A' + 10;
				    else if (number >= '0' && number <= '9')
					    number -= '0';
				    else
					    return false;
			    }
			    if (number < 10)
                {
				    number *= (multiply ? 2 : 1);
				    multiply = !multiply;
			    }
                else
                {
				    int d = number / 10;
				    number %= 10;
				    number *= (multiply ? 2 : 1);
				    d *= (multiply ? 1 : 2);
				    sum += (d % 10) + (d / 10);
			    }
			    sum += (number % 10) + (number / 10);
		    }
		    sum = (10 - (sum % 10)) % 10;
		    return sum == (input[11] - '0');
	    }
    }
}
