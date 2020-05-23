﻿
namespace mbdt.Utils
{
    /// <summary>
    /// SEDOL (Stock Exchange Daily Official List) utilities.
    /// </summary>
    static class Sedol
    {
        /// <summary>
        /// Checks for the validity of the SEDOL.
        /// </summary>
        /// <param name="isin">The SEDOL.</param>
        /// <returns>True is SEDOL is valid.</returns>
        /// <remarks>
        /// <para>
        /// SEDOL stands for Stock Exchange Daily Official List, a list of security       
        /// identifiers used in the United Kingdom and Ireland for clearing purposes. The 
        /// numbers are assigned by the London Stock Exchange, on request by the security 
        /// issuer. SEDOLs serve as the NSIN for all securities issued in the United      
        /// Kingdom and are therefore part of the security's ISIN as well.                
        /// </para><para>                                                                              
        /// SEDOLs are seven characters in length, consisting of two parts: a six-place   
        /// alphanumeric code and a trailing check digit. SEDOLs issued prior to January  
        /// 26, 2004 were composed only of numbers. For those older SEDOLs, those from    
        /// Asia and Africa typically begin with 6, those from the UK and Ireland (until  
        /// Ireland joined the EU) typically begin with 0 or 3 those from Europe          
        /// typically began with 4, 5 or 7 and those from the Americas began with 2.      
        /// After January 26, 2004, SEDOLs were changed to be alpha-numeric and are       
        /// issued sequentially, beginning with B000009. At each character position       
        /// numbers precede letters and vowels are never used. All new SEDOLs, therefore, 
        /// begin with a letter. Ranges beginning with 9 are reserved for end user        
        /// allocation.                                                                   
        /// </para><para>                                                                              
        /// The check digit for a SEDOL is chosen to make the total weighted sum of all   
        /// seven characters a multiple of 10. The check digit is computed using a        
        /// weighted sum of the first six characters. Letters are converted to numbers by 
        /// adding their ordinal position in the alphabet to 9, such that B = 11 and Z =  
        /// 35. While vowels are never used in SEDOLs, they are not ignored when          
        /// computing this weighted sum (e.g. H = 17 and J = 19, even though I is not     
        /// used), simplifying code to compute this sum. The resulting string of numbers  
        /// is then multiplied by the weighting factor as follows:                        
        /// </para><para>                                                                              
        /// First 1 Second 3 Third 1 Fourth 7 Fifth 3 Sixth 9 Seventh 1 (the check digit) 
        /// </para><para>                                                                              
        /// The character values are multiplied by the weights. The check digit is chosen 
        /// to make the total sum, including the check digit, a multiple of 10, which can 
        /// be calculated from the weighted sum of the first six characters as (10 -      
        /// (this sum modulo 10) modulo 10.                                               
        /// </para><para>                                                                              
        /// For British and Irish securities, SEDOLs are converted to ISINs by padding    
        /// the front with two zeros, then adding the country code on the front and the   
        /// ISIN check digit at the end.                                                  
        /// </para><para>
        /// See <a href="https://www.cusip.com/static/html/cusipaccess/CUSIPIntro_%207.26.2007.pdf">www.cusip.com</a>
        /// </<para>
        /// </remarks>
        public static bool IsValid(string sedol)
        {
		    char[] input = sedol.ToCharArray();
		    int number = input.Length;
		    if (7 != number)
			    return false;
		    number = input[6];
		    if (number < '0' || number > '9')
			    return false;
		    int sum = 0;
		    for (int i = 0; i < 6; i++)
            {
			    number = input[i];
				if (number >= '0' && number <= '9')
					number -= '0';
				else if (number >= 'A' && number <= 'Z')
					number = number - 'A' + 10;
				else
					return false;
				switch (i)
				{
					case 1:
					case 4:
						number *= 3; break;
					case 3:
						number *= 7; break;
					case 5:
						number *= 9; break;
				}
				sum += number;
		    }
			sum = (10 - (sum % 10)) % 10;
			number = input[6];
    		if (number >= 'A' && number <= 'Z')
    			number = number - 'A' + 10;
        	else if (number >= '0' && number <= '9')
            	number -= '0';
        	else
            	return false;
			return sum == number;
	    }
    }
}
