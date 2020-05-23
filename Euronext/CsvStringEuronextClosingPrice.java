package mbmr.data.provider;

import mbmr.util.CsvString;
import mbmr.util.Isin;
import mbmr.util.Iso4217CurrencyCode;
import mbmr.util.JulianDayNumber;
import mbmr.util.XmlErrorHandler;

/**
 * EuroNext ETF price search CSV line parser. Filename pattern: unknown.
 * <p/>Field numbers per format type:
 * <table>
 * <tr><td></td>        <td>Stock</td><td>Stock(ALTX)</td><td>ETF</td><td>Fund</td><td>Index</td></tr>
 * <tr><td># fields</td><td>   34</td><td>         35</td><td> 40</td><td>  31</td><td>   25</td></tr>
 * <tr><td>Date</td>    <td>   10</td><td>         11</td><td> 17</td><td>   9</td><td>   11</td></tr>
 * <tr><td>Name</td>    <td>    0</td><td>          0</td><td>  0</td><td>   0</td><td>    0</td></tr>
 * <tr><td>ISIN</td>    <td>    1</td><td>          1</td><td>  1</td><td>   1</td><td>    1</td></tr>
 * <tr><td>MEP</td>     <td>    3</td><td>          3</td><td>  3</td><td>   3</td><td>    2</td></tr>
 * <tr><td>Symbol</td>  <td>    4</td><td>          4</td><td>  4</td><td>   4</td><td>    3</td></tr>
 * <tr><td>Currency</td><td>    6</td><td>          7</td><td>  6</td><td>   5</td><td>    -</td></tr>
 * <tr><td>Open</td>    <td>   15</td><td>         16</td><td> 21</td><td>  12</td><td>    4</td></tr>
 * <tr><td>High</td>    <td>   16</td><td>         17</td><td> 22</td><td>  13</td><td>    5</td></tr>
 * <tr><td>Low</td>     <td>   18</td><td>         19</td><td> 24</td><td>  15</td><td>    7</td></tr>
 * <tr><td>Close</td>   <td>    7</td><td>          8</td><td> 11</td><td>   6</td><td>    9</td></tr>
 * <tr><td>Volume</td>  <td>    8</td><td>          9</td><td> 12</td><td>   7</td><td>    -</td></tr>
 * </table>
 * <p/>ETF CSV format:
 * <table>
 * <tr><td>Instrument's name;</td>                 <td> 0</td><td>ISHARES CHINA 25;</td></tr>
 * <tr><td>ISIN;</td>                              <td> 1</td><td>IE00B02KXK85;</td></tr>
 * <tr><td>EuroNext code;</td>                     <td> 2</td><td>IE00B02KXK85;</td></tr>
 * <tr><td>MEP;</td>                               <td> 3</td><td>AMS;</td></tr>
 * <tr><td>Symbol;</td>                            <td> 4</td><td>FXC;</td></tr>
 * <tr><td>Underlying;</td>                        <td> 5</td><td>;</td></tr>
 * <tr><td>Trading currency;</td>                  <td> 6</td><td>EUR;</td></tr>
 * <tr><td>Bid Date - time (CET);</td>             <td> 7</td><td>04/01/08 17:44 CET;</td></tr>
 * <tr><td>Bid;</td>                               <td> 8</td><td>104.04;</td></tr>
 * <tr><td>Ask;</td>                               <td> 9</td><td>104.95;</td></tr>
 * <tr><td>Ask Date - time (CET);</td>             <td>10</td><td>04/01/08 17:44 CET;</td></tr>
 * <tr><td>Last;</td>                              <td>11</td><td>104.69;</td></tr>
 * <tr><td>Volume;</td>                            <td>12</td><td>22148;</td></tr>
 * <tr><td>D/D-1 (%);</td>                         <td>13</td><td>-2;</td></tr>
 * <tr><td>Last Date - time (CET);</td>            <td>14</td><td>04/01/08 17:28;</td></tr>
 * <tr><td>Turnover;</td>                          <td>15</td><td>2341976;</td></tr>
 * <tr><td>Indicative value;</td>                  <td>16</td><td>104.44;</td></tr>
 * <tr><td>Indicative value Date - time (CET);</td><td>17</td><td>04/01/08 18:47 CET;</td></tr>
 * <tr><td>Trading mode;</td>                      <td>18</td><td>null;</td></tr>
 * <tr><td>Underlying price;</td>                  <td>19</td><td>;</td></tr>
 * <tr><td>Underlying price Date - time (CET);</td><td>20</td><td>;</td></tr>
 * <tr><td>Day First;</td>                         <td>21</td><td>109.13;</td></tr>
 * <tr><td>Day High;</td>                          <td>22</td><td>110.65;</td></tr>
 * <tr><td>Day High / Date - time (CET);</td>      <td>23</td><td>04/01/08 11:24;</td></tr>
 * <tr><td>Day Low;</td>                           <td>24</td><td>103.7;</td></tr>
 * <tr><td>Day Low / Date - time (CET);</td>       <td>25</td><td>04/01/08 17:27;</td></tr>
 * <tr><td>31-12/Change (%);</td>                  <td>26</td><td>-5.68;</td></tr>
 * <tr><td>31-12/High;</td>                        <td>27</td><td>111.6;</td></tr>
 * <tr><td>31-12/High/Date;</td>                   <td>28</td><td>02/01/08;</td></tr>
 * <tr><td>31-12/Low;</td>                         <td>29</td><td>103.7;</td></tr>
 * <tr><td>31-12/Low/Date;</td>                    <td>30</td><td>04/01/08;</td></tr>
 * <tr><td>52 weeks/Change (%);</td>               <td>31</td><td>-;</td></tr>
 * <tr><td>52 weeks/High;</td>                     <td>32</td><td>-;</td></tr>
 * <tr><td>52 weeks/High/Date;</td>                <td>33</td><td>-;</td></tr>
 * <tr><td>52 weeks/Low;</td>                      <td>34</td><td>-;</td></tr>
 * <tr><td>52 weeks/Low/Date;</td>                 <td>35</td><td>-;</td></tr>
 * <tr><td>Suspended;</td>                         <td>36</td><td>-;</td></tr>
 * <tr><td>Suspended / Date - time (CET);</td>     <td>37</td><td>-;</td></tr>
 * <tr><td>Reserved;</td>                          <td>38</td><td>-;</td></tr>
 * <tr><td>Reserved / Date - time (CET)</td>       <td>39</td><td>-</td></tr>
 * <tr><td></td>                                   <td>40</td><td></td></tr>
 * </table>
 * <p/>Stock CSV format:
 * <table>
 * <tr><td>Instrument's name;</td>            <td> 0</td><td>AIR FRANCE -KLM;</td></tr>
 * <tr><td>ISIN;</td>                         <td> 1</td><td>FR0000031122;</td></tr>
 * <tr><td>EuroNext code;</td>                <td> 2</td><td>NSCNL000AFA1;</td></tr>
 * <tr><td>MEP;</td>                          <td> 3</td><td>AMS;</td></tr>
 * <tr><td>Symbol;</td>                       <td> 4</td><td>AFA;</td></tr>
 * <tr><td>ICB Sector (Level 4);</td>         <td> 5</td><td>5751 Airlines;</td></tr>
 * <tr><td>Trading currency;</td>             <td> 6</td><td>EUR;</td></tr>
 * <tr><td>Last;</td>                         <td> 7</td><td>22.37;</td></tr>
 * <tr><td>Volume;</td>                       <td> 8</td><td>29121;</td></tr>
 * <tr><td>D/D-1 (%);</td>                    <td> 9</td><td>-4.15;</td></tr>
 * <tr><td>Date - time (CET);</td>            <td>10</td><td>04/01/08 17:28;</td></tr>
 * <tr><td>Turnover;</td>                     <td>11</td><td>658200;</td></tr>
 * <tr><td>Total number of shares;</td>       <td>12</td><td>300,219,278;</td></tr>
 * <tr><td>Capitalization;</td>               <td>13</td><td>6,715,905,024;</td></tr>
 * <tr><td>Trading mode;</td>                 <td>14</td><td>Continuous;</td></tr>
 * <tr><td>Day First;</td>                    <td>15</td><td>23.23;</td></tr>
 * <tr><td>Day High;</td>                     <td>16</td><td>23.24;</td></tr>
 * <tr><td>Day High / Date - time (CET);</td> <td>17</td><td>04/01/08 09:31;</td></tr>
 * <tr><td>Day Low;</td>                      <td>18</td><td>22.1;</td></tr>
 * <tr><td>Day Low / Date - time (CET);</td>  <td>19</td><td>04/01/08 15:40;</td></tr>
 * <tr><td>31-12/Change (%);</td>             <td>20</td><td>-7.56;</td></tr>
 * <tr><td>31-12/High;</td>                   <td>21</td><td>24.55;</td></tr>
 * <tr><td>31-12/High/Date;</td>              <td>22</td><td>02/01/08;</td></tr>
 * <tr><td>31-12/Low;</td>                    <td>23</td><td>22.1;</td></tr>
 * <tr><td>31-12/Low/Date;</td>               <td>24</td><td>04/01/08;</td></tr>
 * <tr><td>52 weeks/Change (%);</td>          <td>25</td><td>-31.76;</td></tr>
 * <tr><td>52 weeks/High;</td>                <td>26</td><td>39.33;</td></tr>
 * <tr><td>52 weeks/High/Date;</td>           <td>27</td><td>04/06/07;</td></tr>
 * <tr><td>52 weeks/Low;</td>                 <td>28</td><td>22.05;</td></tr>
 * <tr><td>52 weeks/Low/Date;</td>            <td>29</td><td>21/11/07;</td></tr>
 * <tr><td>Suspended;</td>                    <td>30</td><td>-;</td></tr>
 * <tr><td>Suspended / Date - time (CET);</td><td>31</td><td>-;</td></tr>
 * <tr><td>Reserved;</td>                     <td>32</td><td>-;</td></tr>
 * <tr><td>Reserved / Date - time (CET)</td>  <td>33</td><td>-</td></tr>
 * <tr><td></td>                              <td>34</td><td></td></tr>
 * </table>
 * <p/>Stock CSV format (ALTX):
 * <table>
 * <tr><td>Instrument's name;</td>            <td> 0</td><td>AIR FRANCE -KLM;</td></tr>
 * <tr><td>ISIN;</td>                         <td> 1</td><td>FR0000031122;</td></tr>
 * <tr><td>EuroNext code;</td>                <td> 2</td><td>NSCNL000AFA1;</td></tr>
 * <tr><td>MEP;</td>                          <td> 3</td><td>AMS;</td></tr>
 * <tr><td>Symbol;</td>                       <td> 4</td><td>AFA;</td></tr>
 * <tr><td>Prices & features;</td>            <td> 5</td><td>Public offer;</td></tr>
 * <tr><td>ICB Sector (Level 4);</td>         <td> 6</td><td>5751 Airlines;</td></tr>
 * <tr><td>Trading currency;</td>             <td> 7</td><td>EUR;</td></tr>
 * <tr><td>Last;</td>                         <td> 8</td><td>22.37;</td></tr>
 * <tr><td>Volume;</td>                       <td> 9</td><td>29121;</td></tr>
 * <tr><td>D/D-1 (%);</td>                    <td>10</td><td>-4.15;</td></tr>
 * <tr><td>Date - time (CET);</td>            <td>11</td><td>04/01/08 17:28;</td></tr>
 * <tr><td>Turnover;</td>                     <td>12</td><td>658200;</td></tr>
 * <tr><td>Total number of shares;</td>       <td>13</td><td>300,219,278;</td></tr>
 * <tr><td>Capitalization;</td>               <td>14</td><td>6,715,905,024;</td></tr>
 * <tr><td>Trading mode;</td>                 <td>15</td><td>Continuous;</td></tr>
 * <tr><td>Day First;</td>                    <td>16</td><td>23.23;</td></tr>
 * <tr><td>Day High;</td>                     <td>17</td><td>23.24;</td></tr>
 * <tr><td>Day High / Date - time (CET);</td> <td>18</td><td>04/01/08 09:31;</td></tr>
 * <tr><td>Day Low;</td>                      <td>19</td><td>22.1;</td></tr>
 * <tr><td>Day Low / Date - time (CET);</td>  <td>20</td><td>04/01/08 15:40;</td></tr>
 * <tr><td>31-12/Change (%);</td>             <td>21</td><td>-7.56;</td></tr>
 * <tr><td>31-12/High;</td>                   <td>22</td><td>24.55;</td></tr>
 * <tr><td>31-12/High/Date;</td>              <td>23</td><td>02/01/08;</td></tr>
 * <tr><td>31-12/Low;</td>                    <td>24</td><td>22.1;</td></tr>
 * <tr><td>31-12/Low/Date;</td>               <td>25</td><td>04/01/08;</td></tr>
 * <tr><td>52 weeks/Change (%);</td>          <td>26</td><td>-31.76;</td></tr>
 * <tr><td>52 weeks/High;</td>                <td>27</td><td>39.33;</td></tr>
 * <tr><td>52 weeks/High/Date;</td>           <td>28</td><td>04/06/07;</td></tr>
 * <tr><td>52 weeks/Low;</td>                 <td>29</td><td>22.05;</td></tr>
 * <tr><td>52 weeks/Low/Date;</td>            <td>30</td><td>21/11/07;</td></tr>
 * <tr><td>Suspended;</td>                    <td>31</td><td>-;</td></tr>
 * <tr><td>Suspended / Date - time (CET);</td><td>32</td><td>-;</td></tr>
 * <tr><td>Reserved;</td>                     <td>33</td><td>-;</td></tr>
 * <tr><td>Reserved / Date - time (CET)</td>  <td>34</td><td>-</td></tr>
 * <tr><td></td>                              <td>35</td><td></td></tr>
 * </table>
 * <p/>Fund CSV format:
 * <table>
 * <tr><td>Instrument's name;</td>            <td> 0</td><td>DELTA LLOYD DOLLAR;</td></tr>
 * <tr><td>ISIN;</td>                         <td> 1</td><td>NL0000442010;</td></tr>
 * <tr><td>EuroNext code;</td>                <td> 2</td><td>NL0000442010;</td></tr>
 * <tr><td>MEP;</td>                          <td> 3</td><td>AMS;</td></tr>
 * <tr><td>Symbol;</td>                       <td> 4</td><td>DLDF;</td></tr>
 * <tr><td>Trading currency;</td>             <td> 5</td><td>EUR;</td></tr>
 * <tr><td>Last;</td>                         <td> 6</td><td>8.47;</td></tr>
 * <tr><td>Volume;</td>                       <td> 7</td><td>1737;</td></tr>
 * <tr><td>D/D-1 (%);</td>                    <td> 8</td><td>-0.35;</td></tr>
 * <tr><td>Date - time (CET);</td>            <td> 9</td><td>04/01/08 10:00;</td></tr>
 * <tr><td>Turnover;</td>                     <td>10</td><td>14712;</td></tr>
 * <tr><td>Trading mode;</td>                 <td>11</td><td>null;</td></tr>
 * <tr><td>Day First;</td>                    <td>12</td><td>8.47;</td></tr>
 * <tr><td>Day High;</td>                     <td>13</td><td>8.47;</td></tr>
 * <tr><td>Day High / Date - time (CET);</td> <td>14</td><td>04/01/08 10:00;</td></tr>
 * <tr><td>Day Low;</td>                      <td>15</td><td>8.47;</td></tr>
 * <tr><td>Day Low / Date - time (CET);</td>  <td>16</td><td>04/01/08 10:00;</td></tr>
 * <tr><td>31-12/Change (%);</td>             <td>17</td><td>0.71;</td></tr>
 * <tr><td>31-12/High;</td>                   <td>18</td><td>8.52;</td></tr>
 * <tr><td>31-12/High/Date;</td>              <td>19</td><td>02/01/08;</td></tr>
 * <tr><td>31-12/Low;</td>                    <td>20</td><td>8.47;</td></tr>
 * <tr><td>31-12/Low/Date;</td>               <td>21</td><td>04/01/08;</td></tr>
 * <tr><td>52 weeks/Change (%);</td>          <td>22</td><td>-;</td></tr>
 * <tr><td>52 weeks/High;</td>                <td>23</td><td>-;</td></tr>
 * <tr><td>52 weeks/High/Date;</td>           <td>24</td><td>-;</td></tr>
 * <tr><td>52 weeks/Low;</td>                 <td>25</td><td>-;</td></tr>
 * <tr><td>52 weeks/Low/Date;</td>            <td>26</td><td>-;</td></tr>
 * <tr><td>Suspended;</td>                    <td>27</td><td>-;</td></tr>
 * <tr><td>Suspended / Date - time (CET);</td><td>28</td><td>-;</td></tr>
 * <tr><td>Reserved;</td>                     <td>29</td><td>-;</td></tr>
 * <tr><td>Reserved / Date - time (CET)</td>  <td>30</td><td>-;</td></tr>
 * <tr><td></td>                              <td>31</td><td></td></tr>
 * </table>
 * <p/>Index CSV format:
 * <table>
 * <tr><td>Instrument's name;</td>           <td> 0</td><td>AEX-INDEX;</td></tr>
 * <tr><td>ISIN;</td>                        <td> 1</td><td>NL0000000107;</td></tr>
 * <tr><td>MEP;</td>                         <td> 2</td><td>AMS;</td></tr>
 * <tr><td>Symbol;</td>                      <td> 3</td><td>AEX;</td></tr>
 * <tr><td>Day First;</td>                   <td> 4</td><td>507.7;</td></tr>
 * <tr><td>Day High;</td>                    <td> 5</td><td>511.16;</td></tr>
 * <tr><td>Day High / Date - time (CET);</td><td> 6</td><td>04/01/08 11:14;</td></tr>
 * <tr><td>Day Low;</td>                     <td> 7</td><td>498.95;</td></tr>
 * <tr><td>Day Low / Date - time (CET);</td> <td> 8</td><td>04/01/08 17:08;</td></tr>
 * <tr><td>Last;</td>                        <td> 9</td><td>500.6;</td></tr>
 * <tr><td>D/D-1 (%);</td>                   <td>10</td><td>-1.59;</td></tr>
 * <tr><td>Date - time (CET);</td>           <td>11</td><td>04/01/08 18:07;</td></tr>
 * <tr><td>31-12/Change (%);</td>            <td>12</td><td>-2.94;</td></tr>
 * <tr><td>31-12/High;</td>                  <td>13</td><td>518.27;</td></tr>
 * <tr><td>31-12/High/Date;</td>             <td>14</td><td>02/01/08;</td></tr>
 * <tr><td>31-12/Low;</td>                   <td>15</td><td>498.95;</td></tr>
 * <tr><td>31-12/Low/Date;</td>              <td>16</td><td>04/01/08;</td></tr>
 * <tr><td>52 weeks/Change (%);</td>         <td>17</td><td>-;</td></tr>
 * <tr><td>52 weeks/High;</td>               <td>18</td><td>-;</td></tr>
 * <tr><td>52 weeks/High/Date;</td>          <td>19</td><td>-;</td></tr>
 * <tr><td>52 weeks/Low;</td>                <td>20</td><td>-;</td></tr>
 * <tr><td>52 weeks/Low/Date;</td>           <td>21</td><td>-;</td></tr>
 * <tr><td>Risers;</td>                      <td>22</td><td>4;</td></tr>
 * <tr><td>Fallers;</td>                     <td>23</td><td>18;</td></tr>
 * <tr><td>Neutrals</td>                     <td>24</td><td>1</td></tr>
 * <tr><td></td>                             <td>25</td><td></td></tr>
 * </table>
 */
public class CsvStringEuronextClosingPrice implements EodProviderQuote {
	private CsvString parser = new CsvString(';');
	private String mep = null;
	private String type = null;
	private int jdn = 0;
	private String datestamp = null;
	private String symbol = null;
	private String name = null;
	private String currencyCode = null;
	private String isin = null;
	private double open = Double.NaN;
	private double low = Double.NaN;
	private double high = Double.NaN;
	private double close = Double.NaN;
	private int volume = Integer.MAX_VALUE;

	private void clean() {
		mep = null;
		type = null;
		jdn = 0;
		datestamp = null;
		symbol = null;
		name = null;
		currencyCode = null;
		isin = null;
		open = Double.NaN;
		low = Double.NaN;
		high = Double.NaN;
		close = Double.NaN;
		volume = Integer.MAX_VALUE;
	}

	public static final String STOCK = "stock";
	public static final String ETF = "etf";
	public static final String FUND = "fund";
	public static final String INDEX = "index";

	/**
	 * Parses a CSV string.
	 * @param csv a string to parse.
	 * @throws IllegalArgumentException if string format is not valid.
	 */
	public void parse(String csv, XmlErrorHandler eh) {
		clean();
		parser.parse(csv);
		int fieldCount = parser.getFieldCount();
		int dateField, symbolField, currencyField, mepField, openField, highField, lowField, closeField, volumeField;
		int[] dateFields = null;
		if (34 == fieldCount) { // Stock
			dateField = 10;
			symbolField = 4;
			currencyField = 6;
			mepField = 3;
			openField = 15;
			highField = 16;
			lowField = 18;
			closeField = 7;
			volumeField = 8;
			int[] fields = {10, 17, 19};
			dateFields = fields;
			type = STOCK;
		} else if (35 == fieldCount) { // Stock (ALTX)
			dateField = 11;
			symbolField = 4;
			currencyField = 7;
			mepField = 3;
			openField = 16;
			highField = 17;
			lowField = 19;
			closeField = 8;
			volumeField = 9;
			int[] fields = {11, 18, 20};
			dateFields = fields;
			type = STOCK;
		} else if (40 == fieldCount) { // ETF
			dateField = 17;
			symbolField = 4;
			currencyField = 6;
			mepField = 3;
			openField = 21;
			highField = 22;
			lowField = 24;
			closeField = 11;
			volumeField = 12;
			int[] fields = {7, 10, 14, /*17,*/ 23, 25};
			dateFields = fields;
			type = ETF;
		} else if (25 == fieldCount) { // Index
			dateField = 11;
			symbolField = 3;
			currencyField = -1;
			mepField = 2;
			openField = 4;
			highField = 5;
			lowField = 7;
			closeField = 9;
			volumeField = -1;
			int[] fields = {6, 8, 11};
			dateFields = fields;
			type = INDEX;
		} else if (31 == fieldCount) { // Fund
			dateField = 9;
			symbolField = 4;
			currencyField = 5;
			mepField = 3;
			openField = 12;
			highField = 13;
			lowField = 15;
			closeField = 6;
			volumeField = 7;
			int[] fields = {9, 14, 16};
			dateFields = fields;
			type = FUND;
		} else {
			StringBuffer buffer = new StringBuffer(1024);
			buffer.append("invalid number of fields, expected 34 (stock), 40 (etf), 25 (index) or 31 (fund); got ");
			buffer.append(fieldCount);
			buffer.append(": ");
			buffer.append(csv);
			throw new IllegalArgumentException(buffer.toString());
		}
		// Date
		try {
			/*try {
				jdn = JulianDayNumber.fromDDsMMsYY(parser.getField(dateField));									
			} catch (IllegalArgumentException e) {
				jdn = 0;
			}
			if (0 == jdn) {*/
				int jdnmax = 0, length = dateFields.length;
				for (int i = 0; i < length; i++) {
					try {
						dateField = dateFields[i];
						jdn = JulianDayNumber.fromDDsMMsYY(parser.getField(dateField));
					} catch (IllegalArgumentException e) {
						jdn = 0;
					} finally {
						if (jdnmax < jdn) {
							jdnmax = jdn;
						}
					}
				}
				jdn = jdnmax;
				dateField =  -1;
				if (0 == jdn) {
					throw new IllegalArgumentException("cannot find a valid date");
				}				
			//}
			datestamp = JulianDayNumber.toYYYYMMDD(jdn);
		} catch (IllegalArgumentException e) {
			clean();
			StringBuffer buffer = new StringBuffer(1024);
			buffer.append("invalid date (field ");
			buffer.append(dateField);
			buffer.append("), ");
			buffer.append(e.getMessage());
			buffer.append(": ");
			buffer.append(csv);
			throw new IllegalArgumentException(buffer.toString());
		}
		// Symbol
		symbol = parser.getField(symbolField);
		if (null == symbol || 0 == symbol.length()) {
			if (null != eh) {
				StringBuffer buffer = new StringBuffer(1024);
				buffer.append("invalid symbol (field ");
				buffer.append(symbolField);
				buffer.append("), replacing with null: ");
				buffer.append(csv);
				eh.error(buffer.toString());					
			}
			symbol = "null";
			/*
			clean();
			StringBuffer buffer = new StringBuffer(1024);
			buffer.append("invalid symbol (field ");
			buffer.append(symbolField);
			buffer.append("): ");
			buffer.append(csv);
			throw new IllegalArgumentException(buffer.toString());
			*/
		}
		// Name
		name = parser.getField(0);
		if (null == name || 0 == name.length()) {
			clean();
			StringBuffer buffer = new StringBuffer(1024);
			buffer.append("invalid name (field 0): ");
			buffer.append(csv);
			throw new IllegalArgumentException(buffer.toString());
		}
		// ISIN
		isin = parser.getField(1);
		if (null == isin || 0 == isin.length()) {
			if (null != eh) {
				StringBuffer buffer = new StringBuffer(1024);
				buffer.append("invalid isin (field ");
				buffer.append(mepField);
				buffer.append(", ");
				buffer.append(isin);
				buffer.append("), trying EuroNext code: ");
				buffer.append(csv);
				eh.error(buffer.toString());					
			}
			isin = parser.getField(2);
		}
		if (null == isin || 0 == isin.length() || !Isin.isValid(isin)) {
			clean();
			StringBuffer buffer = new StringBuffer(1024);
			buffer.append("invalid isin (field 1): ");
			buffer.append(csv);
			throw new IllegalArgumentException(buffer.toString());
		}
		// MEP
		mep = parser.getField(mepField);
		if (null == mep || 0 == mep.length() || !Euronext.validMep(mep)) {
			clean();
			StringBuffer buffer = new StringBuffer(1024);
			buffer.append("invalid mep (field ");
			buffer.append(mepField);
			buffer.append("): ");
			buffer.append(csv);
			throw new IllegalArgumentException(buffer.toString());
		}
		// Currency
		if (0 < currencyField) {
			currencyCode = parser.getField(currencyField);
			if ("%".equals(currencyCode)) {
				if (null != eh) {
					StringBuffer buffer = new StringBuffer(1024);
					buffer.append("invalid currency (field ");
					buffer.append(mepField);
					buffer.append(", ");
					buffer.append(currencyCode);
					buffer.append("), corrected to EUR: ");
					buffer.append(csv);
					eh.error(buffer.toString());					
				}
				currencyCode = "EUR";
			}
			if (!Iso4217CurrencyCode.isValid(currencyCode)) {
				clean();
				StringBuffer buffer = new StringBuffer(1024);
				buffer.append("invalid currency (field ");
				buffer.append(mepField);
				buffer.append("): ");
				buffer.append(csv);
				throw new IllegalArgumentException(buffer.toString());
			}			
		} else {
			currencyCode = "EUR";
		}
		// Open
		open = convert(parser.getField(openField), "open", openField, csv);
		// High
		high = convert(parser.getField(highField), "high", highField, csv);
		// Low
		low = convert(parser.getField(lowField), "low", lowField, csv);
		// Close
		close = convert(parser.getField(closeField), "close", closeField, csv);
		// Volume
		if (0 < volumeField) {
			volume = (int)convert(parser.getField(volumeField), "volume", volumeField, csv);
		} else {
			volume = 0;
		}
	}

	private double convert(String s, String name, int field, String csv) {
		if (null == s || 0 == s.length() || "-".equals(s)) {
			return 0;
		} else {
			StringBuffer buffer = null;
			int comma = s.indexOf(',');
			while (-1 < comma) {
				if (null == buffer) {
					buffer = new StringBuffer(128);
				}
				buffer.append(s.substring(0, comma));
				buffer.append(s.substring(comma + 1));
				s = buffer.toString();
				comma = s.indexOf(',');
			}
			double value;
			try {
				value = Double.parseDouble(s);
			} catch (NumberFormatException e) {
				clean();
				if (null == buffer) {
					buffer = new StringBuffer(1024);
				}
				buffer.append("invalid ");
				buffer.append(name);
				buffer.append(" (field ");
				buffer.append(field);
				buffer.append("): ");
				buffer.append(csv);
				throw new IllegalArgumentException(buffer.toString());
			}
			return value;
		}
	}

	@Override
	public String getType() {
		return type;
	}

	@Override
	public double getClose() {
		return close;
	}

	@Override
	public String getCurrency() {
		return currencyCode;
	}

	@Override
	public String getDate() {
		return datestamp;
	}

	@Override
	public double getHigh() {
		return high;
	}

	@Override
	public String getIsin() {
		return isin;
	}

	@Override
	public int getJulianDayNumber() {
		return jdn;
	}

	@Override
	public double getLow() {
		return low;
	}

	@Override
	public String getName() {
		return name;
	}

	@Override
	public double getOpen() {
		return open;
	}

	@Override
	public String getSymbol() {
		return symbol;
	}

	@Override
	public int getVolume() {
		return volume;
	}

	@Override
	public String getMep() {
		return mep;
	}

	@Override
	public boolean hasIsin() {
		return true;
	}

	@Override
	public boolean hasMep() {
		return true;
	}

	@Override
	public boolean hasName() {
		return true;
	}

	@Override
	public boolean hasType() {
		return true;
	}

	@Override
	public String getTime() {
		return null;
	}

	@Override
	public int getSeconds() {
		return 0;
	}

	@Override
	public double getPrice() {
		return 0.;
	}

}
