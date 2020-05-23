using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Globalization;
using System.Threading;

using mbdt.Utils;

namespace mbdt.Euronext
{
    /// <summary>
    /// Euronext endofday closing price utilities.
    /// </summary>
    /// <remarks>
    /// EuroNext ETF price search CSV line parser. Filename pattern: unknown.
    /// <p/>Field numbers per format type:
    /// <table>
    /// <tr><td></td>        <td>Stock</td><td>Stock(ALTX)</td><td>ETF</td><td>Fund</td><td>Index</td></tr>
    /// <tr><td># fields</td><td>   34</td><td>         35</td><td> 40</td><td>  31</td><td>   25</td></tr>
    /// <tr><td>Date</td>    <td>   10</td><td>         11</td><td> 17</td><td>   9</td><td>   11</td></tr>
    /// <tr><td>Name</td>    <td>    0</td><td>          0</td><td>  0</td><td>   0</td><td>    0</td></tr>
    /// <tr><td>ISIN</td>    <td>    1</td><td>          1</td><td>  1</td><td>   1</td><td>    1</td></tr>
    /// <tr><td>MEP</td>     <td>    3</td><td>          3</td><td>  3</td><td>   3</td><td>    2</td></tr>
    /// <tr><td>Symbol</td>  <td>    4</td><td>          4</td><td>  4</td><td>   4</td><td>    3</td></tr>
    /// <tr><td>Currency</td><td>    6</td><td>          7</td><td>  6</td><td>   5</td><td>    -</td></tr>
    /// <tr><td>Open</td>    <td>   15</td><td>         16</td><td> 21</td><td>  12</td><td>    4</td></tr>
    /// <tr><td>High</td>    <td>   16</td><td>         17</td><td> 22</td><td>  13</td><td>    5</td></tr>
    /// <tr><td>Low</td>     <td>   18</td><td>         19</td><td> 24</td><td>  15</td><td>    7</td></tr>
    /// <tr><td>Close</td>   <td>    7</td><td>          8</td><td> 11</td><td>   6</td><td>    9</td></tr>
    /// <tr><td>Volume</td>  <td>    8</td><td>          9</td><td> 12</td><td>   7</td><td>    -</td></tr>
    /// </table>
    /// <p/>ETF CSV format:
    /// <table>
    /// <tr><td>Instrument's name;</td>                 <td> 0</td><td>ISHARES CHINA 25;</td></tr>
    /// <tr><td>ISIN;</td>                              <td> 1</td><td>IE00B02KXK85;</td></tr>
    /// <tr><td>EuroNext code;</td>                     <td> 2</td><td>IE00B02KXK85;</td></tr>
    /// <tr><td>MEP;</td>                               <td> 3</td><td>AMS;</td></tr>
    /// <tr><td>Symbol;</td>                            <td> 4</td><td>FXC;</td></tr>
    /// <tr><td>Underlying;</td>                        <td> 5</td><td>;</td></tr>
    /// <tr><td>Trading currency;</td>                  <td> 6</td><td>EUR;</td></tr>
    /// <tr><td>Bid Date - time (CET);</td>             <td> 7</td><td>04/01/08 17:44 CET;</td></tr>
    /// <tr><td>Bid;</td>                               <td> 8</td><td>104.04;</td></tr>
    /// <tr><td>Ask;</td>                               <td> 9</td><td>104.95;</td></tr>
    /// <tr><td>Ask Date - time (CET);</td>             <td>10</td><td>04/01/08 17:44 CET;</td></tr>
    /// <tr><td>Last;</td>                              <td>11</td><td>104.69;</td></tr>
    /// <tr><td>Volume;</td>                            <td>12</td><td>22148;</td></tr>
    /// <tr><td>D/D-1 (%);</td>                         <td>13</td><td>-2;</td></tr>
    /// <tr><td>Last Date - time (CET);</td>            <td>14</td><td>04/01/08 17:28;</td></tr>
    /// <tr><td>Turnover;</td>                          <td>15</td><td>2341976;</td></tr>
    /// <tr><td>Indicative value;</td>                  <td>16</td><td>104.44;</td></tr>
    /// <tr><td>Indicative value Date - time (CET);</td><td>17</td><td>04/01/08 18:47 CET;</td></tr>
    /// <tr><td>Trading mode;</td>                      <td>18</td><td>null;</td></tr>
    /// <tr><td>Underlying price;</td>                  <td>19</td><td>;</td></tr>
    /// <tr><td>Underlying price Date - time (CET);</td><td>20</td><td>;</td></tr>
    /// <tr><td>Day First;</td>                         <td>21</td><td>109.13;</td></tr>
    /// <tr><td>Day High;</td>                          <td>22</td><td>110.65;</td></tr>
    /// <tr><td>Day High / Date - time (CET);</td>      <td>23</td><td>04/01/08 11:24;</td></tr>
    /// <tr><td>Day Low;</td>                           <td>24</td><td>103.7;</td></tr>
    /// <tr><td>Day Low / Date - time (CET);</td>       <td>25</td><td>04/01/08 17:27;</td></tr>
    /// <tr><td>31-12/Change (%);</td>                  <td>26</td><td>-5.68;</td></tr>
    /// <tr><td>31-12/High;</td>                        <td>27</td><td>111.6;</td></tr>
    /// <tr><td>31-12/High/Date;</td>                   <td>28</td><td>02/01/08;</td></tr>
    /// <tr><td>31-12/Low;</td>                         <td>29</td><td>103.7;</td></tr>
    /// <tr><td>31-12/Low/Date;</td>                    <td>30</td><td>04/01/08;</td></tr>
    /// <tr><td>52 weeks/Change (%);</td>               <td>31</td><td>-;</td></tr>
    /// <tr><td>52 weeks/High;</td>                     <td>32</td><td>-;</td></tr>
    /// <tr><td>52 weeks/High/Date;</td>                <td>33</td><td>-;</td></tr>
    /// <tr><td>52 weeks/Low;</td>                      <td>34</td><td>-;</td></tr>
    /// <tr><td>52 weeks/Low/Date;</td>                 <td>35</td><td>-;</td></tr>
    /// <tr><td>Suspended;</td>                         <td>36</td><td>-;</td></tr>
    /// <tr><td>Suspended / Date - time (CET);</td>     <td>37</td><td>-;</td></tr>
    /// <tr><td>Reserved;</td>                          <td>38</td><td>-;</td></tr>
    /// <tr><td>Reserved / Date - time (CET)</td>       <td>39</td><td>-</td></tr>
    /// <tr><td></td>                                   <td>40</td><td></td></tr>
    /// </table>
    /// <p/>Stock CSV format:
    /// <table>
    /// <tr><td>Instrument's name;</td>            <td> 0</td><td>AIR FRANCE -KLM;</td></tr>
    /// <tr><td>ISIN;</td>                         <td> 1</td><td>FR0000031122;</td></tr>
    /// <tr><td>EuroNext code;</td>                <td> 2</td><td>NSCNL000AFA1;</td></tr>
    /// <tr><td>MEP;</td>                          <td> 3</td><td>AMS;</td></tr>
    /// <tr><td>Symbol;</td>                       <td> 4</td><td>AFA;</td></tr>
    /// <tr><td>ICB Sector (Level 4);</td>         <td> 5</td><td>5751 Airlines;</td></tr>
    /// <tr><td>Trading currency;</td>             <td> 6</td><td>EUR;</td></tr>
    /// <tr><td>Last;</td>                         <td> 7</td><td>22.37;</td></tr>
    /// <tr><td>Volume;</td>                       <td> 8</td><td>29121;</td></tr>
    /// <tr><td>D/D-1 (%);</td>                    <td> 9</td><td>-4.15;</td></tr>
    /// <tr><td>Date - time (CET);</td>            <td>10</td><td>04/01/08 17:28;</td></tr>
    /// <tr><td>Turnover;</td>                     <td>11</td><td>658200;</td></tr>
    /// <tr><td>Total number of shares;</td>       <td>12</td><td>300,219,278;</td></tr>
    /// <tr><td>Capitalization;</td>               <td>13</td><td>6,715,905,024;</td></tr>
    /// <tr><td>Trading mode;</td>                 <td>14</td><td>Continuous;</td></tr>
    /// <tr><td>Day First;</td>                    <td>15</td><td>23.23;</td></tr>
    /// <tr><td>Day High;</td>                     <td>16</td><td>23.24;</td></tr>
    /// <tr><td>Day High / Date - time (CET);</td> <td>17</td><td>04/01/08 09:31;</td></tr>
    /// <tr><td>Day Low;</td>                      <td>18</td><td>22.1;</td></tr>
    /// <tr><td>Day Low / Date - time (CET);</td>  <td>19</td><td>04/01/08 15:40;</td></tr>
    /// <tr><td>31-12/Change (%);</td>             <td>20</td><td>-7.56;</td></tr>
    /// <tr><td>31-12/High;</td>                   <td>21</td><td>24.55;</td></tr>
    /// <tr><td>31-12/High/Date;</td>              <td>22</td><td>02/01/08;</td></tr>
    /// <tr><td>31-12/Low;</td>                    <td>23</td><td>22.1;</td></tr>
    /// <tr><td>31-12/Low/Date;</td>               <td>24</td><td>04/01/08;</td></tr>
    /// <tr><td>52 weeks/Change (%);</td>          <td>25</td><td>-31.76;</td></tr>
    /// <tr><td>52 weeks/High;</td>                <td>26</td><td>39.33;</td></tr>
    /// <tr><td>52 weeks/High/Date;</td>           <td>27</td><td>04/06/07;</td></tr>
    /// <tr><td>52 weeks/Low;</td>                 <td>28</td><td>22.05;</td></tr>
    /// <tr><td>52 weeks/Low/Date;</td>            <td>29</td><td>21/11/07;</td></tr>
    /// <tr><td>Suspended;</td>                    <td>30</td><td>-;</td></tr>
    /// <tr><td>Suspended / Date - time (CET);</td><td>31</td><td>-;</td></tr>
    /// <tr><td>Reserved;</td>                     <td>32</td><td>-;</td></tr>
    /// <tr><td>Reserved / Date - time (CET)</td>  <td>33</td><td>-</td></tr>
    /// <tr><td></td>                              <td>34</td><td></td></tr>
    /// </table>
    /// <p/>Stock CSV format (ALTX):
    /// <table>
    /// <tr><td>Instrument's name;</td>            <td> 0</td><td>AIR FRANCE -KLM;</td></tr>
    /// <tr><td>ISIN;</td>                         <td> 1</td><td>FR0000031122;</td></tr>
    /// <tr><td>EuroNext code;</td>                <td> 2</td><td>NSCNL000AFA1;</td></tr>
    /// <tr><td>MEP;</td>                          <td> 3</td><td>AMS;</td></tr>
    /// <tr><td>Symbol;</td>                       <td> 4</td><td>AFA;</td></tr>
    /// <tr><td>Prices & features;</td>            <td> 5</td><td>Public offer;</td></tr>
    /// <tr><td>ICB Sector (Level 4);</td>         <td> 6</td><td>5751 Airlines;</td></tr>
    /// <tr><td>Trading currency;</td>             <td> 7</td><td>EUR;</td></tr>
    /// <tr><td>Last;</td>                         <td> 8</td><td>22.37;</td></tr>
    /// <tr><td>Volume;</td>                       <td> 9</td><td>29121;</td></tr>
    /// <tr><td>D/D-1 (%);</td>                    <td>10</td><td>-4.15;</td></tr>
    /// <tr><td>Date - time (CET);</td>            <td>11</td><td>04/01/08 17:28;</td></tr>
    /// <tr><td>Turnover;</td>                     <td>12</td><td>658200;</td></tr>
    /// <tr><td>Total number of shares;</td>       <td>13</td><td>300,219,278;</td></tr>
    /// <tr><td>Capitalization;</td>               <td>14</td><td>6,715,905,024;</td></tr>
    /// <tr><td>Trading mode;</td>                 <td>15</td><td>Continuous;</td></tr>
    /// <tr><td>Day First;</td>                    <td>16</td><td>23.23;</td></tr>
    /// <tr><td>Day High;</td>                     <td>17</td><td>23.24;</td></tr>
    /// <tr><td>Day High / Date - time (CET);</td> <td>18</td><td>04/01/08 09:31;</td></tr>
    /// <tr><td>Day Low;</td>                      <td>19</td><td>22.1;</td></tr>
    /// <tr><td>Day Low / Date - time (CET);</td>  <td>20</td><td>04/01/08 15:40;</td></tr>
    /// <tr><td>31-12/Change (%);</td>             <td>21</td><td>-7.56;</td></tr>
    /// <tr><td>31-12/High;</td>                   <td>22</td><td>24.55;</td></tr>
    /// <tr><td>31-12/High/Date;</td>              <td>23</td><td>02/01/08;</td></tr>
    /// <tr><td>31-12/Low;</td>                    <td>24</td><td>22.1;</td></tr>
    /// <tr><td>31-12/Low/Date;</td>               <td>25</td><td>04/01/08;</td></tr>
    /// <tr><td>52 weeks/Change (%);</td>          <td>26</td><td>-31.76;</td></tr>
    /// <tr><td>52 weeks/High;</td>                <td>27</td><td>39.33;</td></tr>
    /// <tr><td>52 weeks/High/Date;</td>           <td>28</td><td>04/06/07;</td></tr>
    /// <tr><td>52 weeks/Low;</td>                 <td>29</td><td>22.05;</td></tr>
    /// <tr><td>52 weeks/Low/Date;</td>            <td>30</td><td>21/11/07;</td></tr>
    /// <tr><td>Suspended;</td>                    <td>31</td><td>-;</td></tr>
    /// <tr><td>Suspended / Date - time (CET);</td><td>32</td><td>-;</td></tr>
    /// <tr><td>Reserved;</td>                     <td>33</td><td>-;</td></tr>
    /// <tr><td>Reserved / Date - time (CET)</td>  <td>34</td><td>-</td></tr>
    /// <tr><td></td>                              <td>35</td><td></td></tr>
    /// </table>
    /// <p/>Fund CSV format:
    /// <table>
    /// <tr><td>Instrument's name;</td>            <td> 0</td><td>DELTA LLOYD DOLLAR;</td></tr>
    /// <tr><td>ISIN;</td>                         <td> 1</td><td>NL0000442010;</td></tr>
    /// <tr><td>EuroNext code;</td>                <td> 2</td><td>NL0000442010;</td></tr>
    /// <tr><td>MEP;</td>                          <td> 3</td><td>AMS;</td></tr>
    /// <tr><td>Symbol;</td>                       <td> 4</td><td>DLDF;</td></tr>
    /// <tr><td>Trading currency;</td>             <td> 5</td><td>EUR;</td></tr>
    /// <tr><td>Last;</td>                         <td> 6</td><td>8.47;</td></tr>
    /// <tr><td>Volume;</td>                       <td> 7</td><td>1737;</td></tr>
    /// <tr><td>D/D-1 (%);</td>                    <td> 8</td><td>-0.35;</td></tr>
    /// <tr><td>Date - time (CET);</td>            <td> 9</td><td>04/01/08 10:00;</td></tr>
    /// <tr><td>Turnover;</td>                     <td>10</td><td>14712;</td></tr>
    /// <tr><td>Trading mode;</td>                 <td>11</td><td>null;</td></tr>
    /// <tr><td>Day First;</td>                    <td>12</td><td>8.47;</td></tr>
    /// <tr><td>Day High;</td>                     <td>13</td><td>8.47;</td></tr>
    /// <tr><td>Day High / Date - time (CET);</td> <td>14</td><td>04/01/08 10:00;</td></tr>
    /// <tr><td>Day Low;</td>                      <td>15</td><td>8.47;</td></tr>
    /// <tr><td>Day Low / Date - time (CET);</td>  <td>16</td><td>04/01/08 10:00;</td></tr>
    /// <tr><td>31-12/Change (%);</td>             <td>17</td><td>0.71;</td></tr>
    /// <tr><td>31-12/High;</td>                   <td>18</td><td>8.52;</td></tr>
    /// <tr><td>31-12/High/Date;</td>              <td>19</td><td>02/01/08;</td></tr>
    /// <tr><td>31-12/Low;</td>                    <td>20</td><td>8.47;</td></tr>
    /// <tr><td>31-12/Low/Date;</td>               <td>21</td><td>04/01/08;</td></tr>
    /// <tr><td>52 weeks/Change (%);</td>          <td>22</td><td>-;</td></tr>
    /// <tr><td>52 weeks/High;</td>                <td>23</td><td>-;</td></tr>
    /// <tr><td>52 weeks/High/Date;</td>           <td>24</td><td>-;</td></tr>
    /// <tr><td>52 weeks/Low;</td>                 <td>25</td><td>-;</td></tr>
    /// <tr><td>52 weeks/Low/Date;</td>            <td>26</td><td>-;</td></tr>
    /// <tr><td>Suspended;</td>                    <td>27</td><td>-;</td></tr>
    /// <tr><td>Suspended / Date - time (CET);</td><td>28</td><td>-;</td></tr>
    /// <tr><td>Reserved;</td>                     <td>29</td><td>-;</td></tr>
    /// <tr><td>Reserved / Date - time (CET)</td>  <td>30</td><td>-;</td></tr>
    /// <tr><td></td>                              <td>31</td><td></td></tr>
    /// </table>
    /// <p/>Index CSV format:
    /// <table>
    /// <tr><td>Instrument's name;</td>           <td> 0</td><td>AEX-INDEX;</td></tr>
    /// <tr><td>ISIN;</td>                        <td> 1</td><td>NL0000000107;</td></tr>
    /// <tr><td>MEP;</td>                         <td> 2</td><td>AMS;</td></tr>
    /// <tr><td>Symbol;</td>                      <td> 3</td><td>AEX;</td></tr>
    /// <tr><td>Day First;</td>                   <td> 4</td><td>507.7;</td></tr>
    /// <tr><td>Day High;</td>                    <td> 5</td><td>511.16;</td></tr>
    /// <tr><td>Day High / Date - time (CET);</td><td> 6</td><td>04/01/08 11:14;</td></tr>
    /// <tr><td>Day Low;</td>                     <td> 7</td><td>498.95;</td></tr>
    /// <tr><td>Day Low / Date - time (CET);</td> <td> 8</td><td>04/01/08 17:08;</td></tr>
    /// <tr><td>Last;</td>                        <td> 9</td><td>500.6;</td></tr>
    /// <tr><td>D/D-1 (%);</td>                   <td>10</td><td>-1.59;</td></tr>
    /// <tr><td>Date - time (CET);</td>           <td>11</td><td>04/01/08 18:07;</td></tr>
    /// <tr><td>31-12/Change (%);</td>            <td>12</td><td>-2.94;</td></tr>
    /// <tr><td>31-12/High;</td>                  <td>13</td><td>518.27;</td></tr>
    /// <tr><td>31-12/High/Date;</td>             <td>14</td><td>02/01/08;</td></tr>
    /// <tr><td>31-12/Low;</td>                   <td>15</td><td>498.95;</td></tr>
    /// <tr><td>31-12/Low/Date;</td>              <td>16</td><td>04/01/08;</td></tr>
    /// <tr><td>52 weeks/Change (%);</td>         <td>17</td><td>-;</td></tr>
    /// <tr><td>52 weeks/High;</td>               <td>18</td><td>-;</td></tr>
    /// <tr><td>52 weeks/High/Date;</td>          <td>19</td><td>-;</td></tr>
    /// <tr><td>52 weeks/Low;</td>                <td>20</td><td>-;</td></tr>
    /// <tr><td>52 weeks/Low/Date;</td>           <td>21</td><td>-;</td></tr>
    /// <tr><td>Risers;</td>                      <td>22</td><td>4;</td></tr>
    /// <tr><td>Fallers;</td>                     <td>23</td><td>18;</td></tr>
    /// <tr><td>Neutrals</td>                     <td>24</td><td>1</td></tr>
    /// <tr><td></td>                             <td>25</td><td></td></tr>
    /// </table>
    /// </remarks>
    class EuronextEodPrice
    {
        #region Quote
        private class Quote
        {
            #region Members and accessors
            #region Line
            /// <summary>
            /// The quote string in xml form.
            /// </summary>
            public string Line;
            #endregion

            #region Jdn
            /// <summary>
            /// The Julian day number of this quote.
            /// </summary>
            public int Jdn;
            #endregion
            #endregion

            #region Constructor
            /// <summary>
            /// Constructs a new instance of the class.
            /// </summary>
            /// <param name="line">The quote string in xml form.</param>
            /// <param name="jdn">The Julian day number of this quote.</param>
            public Quote(string line, int jdn)
            {
                Line = line;
                Jdn = jdn;
            }
            #endregion
        }
        #endregion

        #region Constants
        private const string instrumentFormat = "<instrument vendor=\"Euronext\" isin=\"{0}\" mep=\"{1}\" name=\"{2}\" symbol=\"{3}\">";
        private const string quoteFormat = "<q c=\"{0}\" d=\"{1}\" h=\"{2}\" j=\"{3}\" l=\"{4}\" o=\"{5}\" v=\"{6}\"/>";
        private const string quoteEnd = "</q>";
        private const string instrumentEnd = "</instrument>";
        private const string instrumentsBegin = "<instruments>";
        private const string instrumentsEnd = "</instruments>";
        private const string endofdayBegin = "<endofday>";
        private const string endofdayEnd = "</endofday>";
        private const string endofday = "endofday";
        private const string history = "closingPrice";
        #endregion

        #region SplitLine
        private static string[] SplitLine(string line)
        {
            line = line.Replace("Â ", "");
            line = line.Replace("Â", "");
            line = line.Replace(" ", "");
            line = line.Replace("á", "");
            line = line.Replace(",", "");
            return line.Split(';');
        }
        #endregion

        #region Extract
        private static string Extract(string line, int fromIndex)
        {
            char[] chars = line.ToCharArray(fromIndex, line.Length - fromIndex);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in chars)
            {
                if ('<' == c)
                    break;
                else
                    stringBuilder.Append(c);
            }
            line = stringBuilder.ToString();
            line = line.Replace("á", "");
            line = line.Replace(" ", "");
            line = line.Replace(" ", "");
            return line;
        }
        #endregion

        #region Convert
        private static double Convert(string s, string name, int lineNumber, string line, EuronextInstrumentContext context)
        {
            if (string.IsNullOrEmpty(s) || "-" == s)
                return 0;
            else
            {
                double value;
                try
                {
                    value = double.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                }
                catch (Exception)
                {
                    Trace.TraceError("invalid endofday history csv {0}, line {1} [{2}] file {3}", name, lineNumber, line, Path.GetFileName(context.DownloadedPath));
                    value = 0;
                }
                return value;
            }
        }
        #endregion

        #region PickOne
        private static double PickOne(double value1, double value2, double value3)
        {
            return (0 != value1 ? value1 : (0 != value2 ? value2 : (0 != value3 ? value3 : 0)));
        }
        #endregion

        #region ImportCsv
        /// <summary>
        /// Imports a downloded Euronext endofday history csv file into a list containing quote strings in XML format.
        /// </summary>
        /// <param name="context">A EuronextInstrumentContext.</param>
        /// <returns>A list containing imported quote strings in XML format.</returns>
        private static List<Quote> ImportCsv(EuronextInstrumentContext context)
        {
            List<Quote> quoteList = new List<Quote>(1024);
            using (StreamReader csvStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                string line, date;
                const string errorFormat = "invalid endofday history csv{0}, line {1} [{2}] file {3}, skipping";
                string[] splitted;
                int lineNumber = 4, jdn;
                double open, high, low, close, volume;
                csvStreamReader.ReadLine();        // ICompany name;ISIN;MEP;Symbol;Segment;Date
                csvStreamReader.ReadLine();        // AP ALTERNAT ASSETS;GB00B15Y0C52;AMS;AAA;-;03/07/08 16:09 CET
                csvStreamReader.ReadLine();        // Empty line
                line = csvStreamReader.ReadLine(); // Date;opening;High;Low;closing;Volume03/05/08;13.00;13.00;13.00;13.00;1000
                                                   // 03/06/08;12.89;13.50;12.87;13.50;346120
                const string pattern = "Date;opening;High;Low;closing;Volume";
                if (!string.IsNullOrEmpty(line) && line.StartsWith(pattern) && !string.IsNullOrEmpty((line = line.Substring(pattern.Length))))
                {
                    do
                    {
                        splitted = SplitLine(line);
                        if (6 == splitted.Length)
                        {
                            jdn = JulianDayNumber.FromMMsDDsYY(splitted[0]);
                            date = JulianDayNumber.ToYYYYMMDD(jdn);
                            open = Convert(splitted[1], "open", lineNumber, line, context);
                            high = Convert(splitted[2], "high", lineNumber, line, context);
                            low = Convert(splitted[3], "low", lineNumber, line, context);
                            close = Convert(splitted[4], "close", lineNumber, line, context);
                            volume = Convert(splitted[5], "volume", lineNumber, line, context);
                            if (0 == open)
                                open = PickOne(close, high, low);
                            if (0 == close)
                                close = PickOne(open, high, low);
                            if (0 == high)
                                high = PickOne(open, close, low);
                            if (0 == low)
                                low = PickOne(open, close, high);
                            quoteList.Add(new Quote(string.Format(quoteFormat,
                                close.ToString(CultureInfo.InvariantCulture.NumberFormat), date,
                                high.ToString(CultureInfo.InvariantCulture.NumberFormat), jdn,
                                low.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                open.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                volume.ToString(CultureInfo.InvariantCulture.NumberFormat)), jdn));
                        }
                        else
                            Trace.TraceError(errorFormat, "", lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    } while (null != (line = csvStreamReader.ReadLine()));
                }
                else
                {
                    Trace.TraceError(errorFormat, " header", lineNumber, "", Path.GetFileName(context.DownloadedPath));
                    return null;
                }
            }
            if (1 > quoteList.Count)
            {
                Trace.TraceError("no historical data found in csv file {0}, skipping", Path.GetFileName(context.DownloadedPath));
                return null;
            }
            return quoteList;
        }
        #endregion

        #region ImportCsvh
        /// <summary>
        /// Imports a downloded Euronext endofday history csv file into a list containing quote strings in XML format.
        /// </summary>
        /// <param name="context">A EuronextInstrumentContext.</param>
        /// <returns>A list containing imported quote strings in XML format.</returns>
        private static List<Quote> ImportCsvh(EuronextInstrumentContext context)
        {
            List<Quote> quoteList = new List<Quote>(1024);
            using (StreamReader csvhStreamReader = new StreamReader(context.DownloadedPath, Encoding.UTF8))
            {
                string line, date;
                const string errorFormat = "invalid endofday history csvh, line {0} [{1}] file {2}, skipping";
                string[] splitted;
                int lineNumber = 1, jdn;
                double open, high, low, close, volume;
                // dd/mm/yy;open;high;low;close;volume
                line = csvhStreamReader.ReadLine(); // ´╗┐16/10/06;27.21;27.91;27.16;27.61;12288608
                if (null != line)
                {
                    //line = line.Replace("´╗┐", "");
                    do
                    {
                        splitted = SplitLine(line);
                        if (6 == splitted.Length)
                        {
                            jdn = JulianDayNumber.FromMMsDDsYY(splitted[0]);
                            date = JulianDayNumber.ToYYYYMMDD(jdn);
                            open = Convert(splitted[1], "open", lineNumber, line, context);
                            high = Convert(splitted[2], "high", lineNumber, line, context);
                            low = Convert(splitted[3], "low", lineNumber, line, context);
                            close = Convert(splitted[4], "close", lineNumber, line, context);
                            volume = Convert(splitted[5], "volume", lineNumber, line, context);
                            if (0 == open)
                                open = PickOne(close, high, low);
                            if (0 == close)
                                close = PickOne(open, high, low);
                            if (0 == high)
                                high = PickOne(open, close, low);
                            if (0 == low)
                                low = PickOne(open, close, high);
                            quoteList.Add(new Quote(string.Format(quoteFormat,
                                close.ToString(CultureInfo.InvariantCulture.NumberFormat), date,
                                high.ToString(CultureInfo.InvariantCulture.NumberFormat), jdn,
                                low.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                open.ToString(CultureInfo.InvariantCulture.NumberFormat),
                                volume.ToString(CultureInfo.InvariantCulture.NumberFormat)), jdn));
                        }
                        else
                            Trace.TraceError(errorFormat, lineNumber, line, Path.GetFileName(context.DownloadedPath));
                        lineNumber++;
                    } while (null != (line = csvhStreamReader.ReadLine()));
                }
                else
                    Trace.TraceError("no endofday historical data found in csvh file {0}, skipping", Path.GetFileName(context.DownloadedPath));
            }
            return quoteList;
        }
        #endregion

        #region ParseJulianDayNumber
        private static int ParseJulianDayNumber(string line)
        {
            int jdn = 0, j = line.IndexOf(" j=\"") + 4;
            char c = line[j++];
            while ('0' <= c && c <= '9')
            {
                jdn *= 10;
                jdn += c - '0';
                c = line[j++];
            }
            return jdn;
        }
        #endregion

        #region DownloadHtml
        /// <summary>
        /// Downloads sequence of html pages extracting intraday quotes end writing them to a file.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="filePath">The file name to write quotes to.</param>
        /// <param name="minimalLength">A minimal length of the file in bytes.</param>
        /// <param name="overwrite">If the file already exists, overwrite it.</param>
        /// <param name="retries">The number of download retries.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns>True if download was successful.</returns>
        private static bool DownloadHtml(string uri, string filePath, long minimalLength, bool overwrite, int retries, int timeout)
        {
            Debug.WriteLine(string.Concat("downloading endofday history html ", filePath, " from ", uri));
            FileInfo fileInfo = new FileInfo(filePath);
            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (!directoryInfo.Exists)
                directoryInfo.Create();
            if (!overwrite)
            {
                if (fileInfo.Exists)
                {
                    if (fileInfo.Length > minimalLength)
                    {
                        Trace.TraceWarning("file {0} already exists, skipping", Path.GetFileName(filePath));
                        return true;
                    }
                    Trace.TraceWarning("file {0} already exists but length {1} is smaller than the minimal length {2}, overwriting", Path.GetFileName(filePath), fileInfo.Length, minimalLength);
                }
            }
            const int bufferSize = 0x1000;
            byte[] buffer = new byte[bufferSize];
            const string pattern1 = "<td class=\"tableDateStamp\" style=\"white-space: nowrap\">";
            int pattern1Length = pattern1.Length;
            const string pattern2 = "&nbsp;";
            int pattern2Length = pattern2.Length;
            const string pattern3 = "<td>";
            int pattern3Length = pattern3.Length;
            const string pattern4 = "</td>";
            int pattern4Length = pattern4.Length;
            bool downloaded = false, found = false;
            int i, page = 0;
            string line, date, open, high, low, close, volume;
            StreamReader streamReader = null;
            StreamWriter streamWriter = null;
            while (0 < retries)
            {
                try
                {
                    WebRequest webRequest = HttpWebRequest.Create(uri);
                    //webRequest.Headers.Set(HttpRequestHeader.UserAgent, "foobar");
                    webRequest.Proxy = WebRequest.DefaultWebProxy;
                    // DefaultCredentials represents the system credentials for the current 
                    // instrument context in which the application is running. For a client-side 
                    // application, these are usually the Windows credentials 
                    // (user name, password, and domain) of the user running the application. 
                    webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                    webRequest.Timeout = timeout;
                    webRequest.Headers.Add(HttpRequestHeader.Upgrade, "1");
                    // Skip validation of SSL/TLS certificate
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                           | SecurityProtocolType.Tls11
                                                           | SecurityProtocolType.Tls12
                                                           | SecurityProtocolType.Ssl3;
                    page = 1;
                    streamWriter = new StreamWriter(filePath, false, Encoding.UTF8, bufferSize);
                    streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                    found = false;
                    line = streamReader.ReadLine();
                    while (null != line)
                    {
                        i = line.IndexOf(pattern1);
                        if (-1 < i)
                        {
                            //  <tr>
                            //    <td class="tableHeader" >Date</td>
                            //    <td class="tableHeader" >opening</td>
                            //    <td class="tableHeader" >High</td>
                            //    <td class="tableHeader" >Low</td>
                            //    <td class="tableHeader" >closing</td>
                            //    <td class="tableHeader" >Volume</td>
                            //..</tr>
                            //  <tr class=bgColor7>
                            //    <td class="tableDateStamp" style="white-space: nowrap">16/10/06</td>
                            //    <td class="tableDateStamp" style="white-space: nowrap">27.21</td>
                            //    <td>27.91</td>
                            //    <td>27.16</td>
                            //    <td>27.61</td>
                            //    <td>12 288 608</td>
                            //  </tr>
                            //  <tr class=bgColor1>
                            //    <td class="tableDateStamp" style="white-space: nowrap">17/10/06</td>
                            found = true;
                            i += pattern1Length;
                            date = line.Substring(i, 8);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern1) + pattern1Length;
                            open = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            high = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            low = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            close = Extract(line, i);
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern3) + pattern3Length;
                            volume = Extract(line, i);
                            streamWriter.WriteLine("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume);
                            Debug.WriteLine(string.Format("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume));
                        }
                        line = streamReader.ReadLine();
                    }
                    while (found)
                    {
                        page++;
                        streamReader.Close();
                        streamReader.Dispose();
                        line = string.Concat(uri, "&pageIndex=", page.ToString());
                        Debug.WriteLine(string.Concat("downloading endofday history html page ", page, " from ", line));
                        webRequest = HttpWebRequest.Create(line);
                        webRequest.Proxy = WebRequest.DefaultWebProxy;
                        webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                        webRequest.Timeout = timeout;
                        streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                        found = false;
                        line = streamReader.ReadLine();
                        while (null != line)
                        {
                            i = line.IndexOf(pattern1);
                            if (-1 < i)
                            {
                                found = true;
                                i += pattern1Length;
                                date = line.Substring(i, 8);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern1) + pattern1Length;
                                open = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                high = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                low = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                close = Extract(line, i);
                                line = streamReader.ReadLine();
                                i = line.IndexOf(pattern3) + pattern3Length;
                                volume = Extract(line, i);
                                streamWriter.WriteLine("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume);
                                Debug.WriteLine(string.Format("{0};{1};{2};{3};{4};{5}", date, open, high, low, close, volume));
                            }
                            line = streamReader.ReadLine();
                        }
                    }
                    streamReader.Close();
                    streamWriter.Close();
                    streamReader.Dispose();
                    streamWriter.Dispose();
                    fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists)
                    {
                        if (fileInfo.Length > minimalLength)
                        {
                            downloaded = true;
                            retries = 0;
                        }
                        else
                        {
                            if (1 < retries)
                                Trace.TraceError("endofday history html file {0}: downloaded length {1} is smaller than the minimal length {2}, retrying ({3})", Path.GetFileName(filePath), fileInfo.Length, minimalLength, retries);
                            else
                            {
                                Trace.TraceError("endofday history html file {0}: downloaded length {1} is smaller than the minimal length {2}, giving up ({3})", Path.GetFileName(filePath), fileInfo.Length, minimalLength, retries);
                                File.Delete(filePath);
                            }
                            retries--;
                        }
                    }
                    else
                        retries--;
                }
                catch (Exception e)
                {
                    if (1 < retries)
                        Trace.TraceError("endofday history html file {0} page {1}: download failed [{2}], retrying ({3})", Path.GetFileName(filePath), page, e.Message, retries);
                    else
                        Trace.TraceError("endofday history html file {0} page {1}: download failed [{2}], giving up ({3})", Path.GetFileName(filePath), page, e.Message, retries);
                    retries--;
                    if (null != streamReader)
                    {
                        streamReader.Close();
                        streamReader.Dispose();
                        streamReader = null;
                    }
                    if (null != streamWriter)
                    {
                        streamWriter.Close();
                        streamWriter.Dispose();
                        streamWriter = null;
                    }
                }
            }
            return downloaded;
        }
        #endregion

        #region Merge
        /// <summary>
        /// Merges the downloaded csv file with the repository xml file.
        /// </summary>
        /// <param name="xmlPath">The repository xml file.</param>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and a path to the csv file.</param>
        /// <returns>True if merged, false otherwise.</returns>
        public static bool Merge(string xmlPath, EuronextInstrumentContext context)
        {
            int jdn;
            List<Quote> quoteList = context.DownloadedPath.EndsWith(".csvh") ?
                ImportCsvh(context) : ImportCsv(context);
            if (null == quoteList || 0 == quoteList.Count)
                return false;
            const int bufferSize = 0x1000;
            string line, xmlPathMerged = string.Concat(xmlPath, ".merged");
            using (StreamWriter xmlStreamWriter = new StreamWriter(xmlPathMerged, false, Encoding.UTF8, bufferSize))
            {
                if (File.Exists(xmlPath))
                {
                    using (StreamReader xmlStreamReader = new StreamReader(xmlPath, Encoding.UTF8))
                    {
                        bool notMerged = true;
                        while (null != (line = xmlStreamReader.ReadLine()))
                        {
                            if (line.StartsWith("<instrument "))
                            {
                                xmlStreamWriter.WriteLine(line);
                                if (line.Contains(string.Concat(" isin=\"", context.Isin, "\"")) &&
                                    line.Contains(string.Concat(" mep=\"", context.Mep, "\"")) &&
                                    line.Contains(string.Concat(" symbol=\"", context.Symbol, "\"")))
                                {
                                    notMerged = false;
                                    while (null != (line = xmlStreamReader.ReadLine()))
                                    {
                                        if (line.StartsWith("<q "))
                                        {
                                            if (0 < quoteList.Count)
                                            {
                                                Quote quote;
                                                jdn = ParseJulianDayNumber(line);
                                                do
                                                {
                                                    quote = quoteList[0];
                                                    if (jdn < quote.Jdn)
                                                    {
                                                        xmlStreamWriter.WriteLine(line);
                                                        break;
                                                    }
                                                    else if (jdn == quote.Jdn)
                                                    {
                                                        xmlStreamWriter.WriteLine(quote.Line);
                                                        quoteList.RemoveAt(0);
                                                        break;
                                                    }
                                                    else // if (jdn > quote.Jdn)
                                                    {
                                                        xmlStreamWriter.WriteLine(quote.Line);
                                                        quoteList.RemoveAt(0);
                                                        if (0 == quoteList.Count)
                                                            xmlStreamWriter.WriteLine(line);
                                                    }
                                                } while (0 < quoteList.Count);
                                            }
                                            else
                                            {
                                                xmlStreamWriter.WriteLine(line);
                                            }
                                        }
                                        else if (line.StartsWith(endofdayEnd))
                                        {
                                            quoteList.ForEach(quote => xmlStreamWriter.WriteLine(quote.Line));
                                            quoteList.Clear();
                                            xmlStreamWriter.WriteLine(line);
                                            break;
                                        }
                                        else
                                            xmlStreamWriter.WriteLine(line);
                                    }
                                }
                                else // copy non-matched instrument
                                {
                                    while (null != (line = xmlStreamReader.ReadLine()))
                                    {
                                        xmlStreamWriter.WriteLine(line);
                                        if (line.StartsWith(instrumentEnd))
                                            break;
                                    }
                                }
                            }
                            else if (line.StartsWith(instrumentsEnd))
                            {
                                if (notMerged)
                                {
                                    xmlStreamWriter.WriteLine(instrumentFormat, context.Isin, context.Mep, context.Name, context.Symbol);
                                    xmlStreamWriter.WriteLine(endofdayBegin);
                                    quoteList.ForEach(quote => xmlStreamWriter.WriteLine(quote.Line));
                                    quoteList.Clear();
                                    xmlStreamWriter.WriteLine(endofdayEnd);
                                    xmlStreamWriter.WriteLine(instrumentEnd);
                                    notMerged = false;
                                }
                                xmlStreamWriter.WriteLine(line);
                                break;
                            }
                            else
                                xmlStreamWriter.WriteLine(line);
                        }
                    }
                }
                else
                {
                    // Just write new output stream and copy the csv endofday history data.
                    xmlStreamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");
                    xmlStreamWriter.WriteLine(instrumentsBegin);
                    xmlStreamWriter.WriteLine(instrumentFormat, context.Isin, context.Mep, context.Name, context.Symbol);
                    xmlStreamWriter.WriteLine(endofdayBegin);
                    quoteList.ForEach(quote => xmlStreamWriter.WriteLine(quote.Line));
                    xmlStreamWriter.WriteLine(endofdayEnd);
                    xmlStreamWriter.WriteLine(instrumentEnd);
                    xmlStreamWriter.WriteLine(instrumentsEnd);
                }
            }
            if (File.Exists(xmlPathMerged))
            {
                if (File.Exists(xmlPath))
                    File.Replace(xmlPathMerged, xmlPath, null);
                else
                    File.Move(xmlPathMerged, xmlPath);
            }
            return true;
        }
        #endregion

        #region Download
        /// <summary>
        /// Downloads endofday history data to a file.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> containing instrument specification and download options.</param>
        /// <param name="downloadDir">The download directory with a traling separator.</param>
        /// <param name="allowHtml">Try to download html if csv download fails.</param>
        /// <param name="days">The number of last history days to download or 0 to download all available history data.</param>
        /// <returns>True if downloaded, false otherwise.</returns>
        public static bool Download(EuronextInstrumentContext context, string downloadDir, bool allowHtml, int days)
        {
            string s = string.Concat(downloadDir,
                string.Format("{0}_{1}_{2}_{3}_eoh.csv", context.Mep, context.Symbol, context.Isin, context.Yyyymmdd));
            EuronextInstrumentContext.VerifyFile(s);
            context.DownloadedPath = s;
            bool succeded = true;
            const string requestDateFormat = "dd/MM/yyyy";
            DateTime dateTime = DateTime.Now;
            string dateTo = dateTime.ToString(requestDateFormat, DateTimeFormatInfo.InvariantInfo);
            string dateFrom = "01/01/1950";
            if (0 < days)
                dateFrom = dateTime.AddDays(-days).ToString(requestDateFormat, DateTimeFormatInfo.InvariantInfo);
            //string uri = string.Format("http://www.euronext.com/tools/datacentre/dataCentreDownload.jcsv?lan=EN&quote=&time=&dayvolume=&indexCompo=&opening=on&high=on&low=on&closing=on&volume=on&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha=2634&dateFrom={0}&dateTo={1}&isinCode={2}&selectedMep={3}&isin={2}&mep={3}",
            string uri = string.Format("http://160.92.106.167/tools/datacentre/dataCentreDownload.jcsv?lan=EN&quote=&time=&dayvolume=&indexCompo=&opening=on&high=on&low=on&closing=on&volume=on&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha=2634&dateFrom={0}&dateTo={1}&isinCode={2}&selectedMep={3}&isin={2}&mep={3}",
                dateFrom, dateTo, context.Isin, Euronext.MepToInteger(context.Mep).ToString());
            if (!Downloader.Download(uri, s, EuronextInstrumentContext.IntradayDownloadMinimalLength, EuronextInstrumentContext.IntradayDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout, null))
            {
                if (!allowHtml)
                    return false;
                //uri = string.Format("http://www.euronext.com/tools/datacentre/dataCentreDownloadHTML-2783-EN.html?&volume=on&opening=on&low=on&high=on&closing=on&indexCompo=&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha=2783&dateFrom={0}&dateTo={1}&isinCode={2}&selectedMep={3}&isin={2}&mep={3}",
                uri = string.Format("http://160.92.106.167/tools/datacentre/dataCentreDownloadHTML-2783-EN.html?&volume=on&opening=on&low=on&high=on&closing=on&indexCompo=&format=txt&formatDate=dd/MM/yy&formatDecimal=&formatValue=txt&formatDateValue=dd/MM/yy&formatDecimalValue=&typeDownload=2&choice=2&cha=2783&dateFrom={0}&dateTo={1}&isinCode={2}&selectedMep={3}&isin={2}&mep={3}",
                    dateFrom, dateTo, context.Isin, Euronext.MepToInteger(context.Mep).ToString());
                s = string.Concat(context.DownloadedPath, "h");
                context.DownloadedPath = s;
                if (!DownloadHtml(uri, s, EuronextInstrumentContext.HistoryDownloadMinimalLength, EuronextInstrumentContext.HistoryDownloadOverwriteExisting, EuronextInstrumentContext.DownloadRetries, EuronextInstrumentContext.DownloadTimeout))
                    succeded = false;
            }
            return succeded;
        }
        #endregion

        #region Zip
        /// <summary>
        /// Makes a zip file from a directory of downloaded files and deletes this directory.
        /// </summary>
        /// <param name="context">The <see cref="EuronextInstrumentContext"/> to get the download repository suffix and the datestamp in YYYYMMDD format.</param>
        public static void Zip(EuronextInstrumentContext context)
        {
            string directory = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, history, context.DownloadRepositorySuffix);
            string separator = Path.DirectorySeparatorChar.ToString();
            string parent = directory;
            if (directory.EndsWith(separator))
                parent = directory.TrimEnd(Path.DirectorySeparatorChar);
            if (directory.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                parent = directory.TrimEnd(Path.AltDirectorySeparatorChar);
            parent = string.Concat(Directory.GetParent(parent).FullName, separator);
            Packager.ZipCsvDirectory(string.Concat(parent, context.Yyyymmdd, "_eoh.zip"), directory, true);
        }
        #endregion

        #region UpdateTask
        /// <summary>
        /// Performs a daily update task.
        /// </summary>
        /// <param name="days">The number of history days to download.</param>
        public static void UpdateTask(int days)
        {
            object notDownloadedListLock = new object();
            List<EuronextInstrumentContext> approvedNotDownloadedList = new List<EuronextInstrumentContext>(1024);
            List<EuronextInstrumentContext> discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);
            List<EuronextInstrumentContext> approvedFailedList = new List<EuronextInstrumentContext>(1024);
            List<EuronextInstrumentContext> discoveredFailedList = new List<EuronextInstrumentContext>(1024);

            Trace.TraceInformation("Preparing: {0}", DateTime.Now);
            List<List<EuronextExecutor.Instrument>> approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
            List<List<EuronextExecutor.Instrument>> discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            long downloadedApprovedInstruments = 0, mergedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, mergedDiscoveredInstruments = 0, discoveredInstruments = 0;
            string downloadDir = string.Concat(EuronextInstrumentContext.DownloadRepositoryPath, history);

            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved: {0}", DateTime.Now);
            /*EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, esc =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false, days))
                {
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                    string s = string.Concat(EuronextInstrumentContext.EndofdayRepositoryPath, esc.RelativePath);
                    EuronextInstrumentContext.VerifyFile(s);
                    if (Merge(s, esc))
                        Interlocked.Increment(ref mergedApprovedInstruments);
                }
                else
                {
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            });*/
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered: {0}", DateTime.Now);
            /*EuronextExecutor.Iterate(discoveredList, esc =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), false, days))
                {
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                    string s = string.Concat(EuronextInstrumentContext.EndofdayDiscoveredRepositoryPath, esc.RelativePath);
                    EuronextInstrumentContext.VerifyFile(s);
                    if (Merge(s, esc))
                        Interlocked.Increment(ref mergedDiscoveredInstruments);
                }
                else
                {
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            });*/
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instruments (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc =>
                    {
                        if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), pass == passCount, days))
                        {
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                            string s = string.Concat(EuronextInstrumentContext.EndofdayRepositoryPath, esc.RelativePath);
                            EuronextInstrumentContext.VerifyFile(s);
                            if (Merge(s, esc))
                                Interlocked.Increment(ref mergedApprovedInstruments);
                        }
                        else
                        {
                            approvedFailedList.Add(esc);
                        }
                    });
                    List<EuronextInstrumentContext> list = approvedNotDownloadedList;
                    approvedNotDownloadedList = approvedFailedList;
                    approvedFailedList = list;
                    approvedFailedList.Clear();
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instruments (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc =>
                    {
                        if (Download(esc, string.Concat(downloadDir, esc.DownloadRepositorySuffix), pass == passCount, days))
                        {
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                            string s = string.Concat(EuronextInstrumentContext.IntradayRepositoryPath, esc.RelativePath);
                            EuronextInstrumentContext.VerifyFile(s);
                            if (Merge(s, esc))
                                Interlocked.Increment(ref mergedDiscoveredInstruments);
                        }
                        else
                        {
                            discoveredFailedList.Add(esc);
                        }
                    });
                    List<EuronextInstrumentContext> list = discoveredNotDownloadedList;
                    discoveredNotDownloadedList = discoveredFailedList;
                    discoveredFailedList = list;
                    discoveredFailedList.Clear();
                }
                pass++;
            }
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedFailedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} approved instruments: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedFailedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
                if (0 < discoveredFailedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} discovered instruments: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredFailedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
            }
            /*Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History {0} approved   instruments: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments, mergedApprovedInstruments);
            Trace.TraceInformation("History {0} discovered instruments: total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments, mergedDiscoveredInstruments);
            Trace.TraceInformation("History {0} both                 : total {1}, downloaded {2}, merged {3}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments, mergedApprovedInstruments + mergedDiscoveredInstruments);
            Zip(context);*/
        }
        #endregion

        #region DownloadTask
        /// <summary>
        /// Performs a download task.
        /// </summary>
        /// <param name="days">The number of history days to download.</param>
        public static void DownloadTask(string downloadPath, int days)
        {
            if (string.IsNullOrEmpty(downloadPath))
                downloadPath = "";
            else
            {
                if (!downloadPath.EndsWith(Path.DirectorySeparatorChar.ToString()) && !downloadPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                    downloadPath = string.Concat(downloadPath, Path.DirectorySeparatorChar.ToString());
                if (!Directory.Exists(downloadPath))
                    Directory.CreateDirectory(downloadPath);
            }

            object notDownloadedListLock = new object();
            List<EuronextInstrumentContext> approvedNotDownloadedList = new List<EuronextInstrumentContext>(1024);
            List<EuronextInstrumentContext> discoveredNotDownloadedList = new List<EuronextInstrumentContext>(1024);
            List<EuronextInstrumentContext> approvedFailedList = new List<EuronextInstrumentContext>(1024);
            List<EuronextInstrumentContext> discoveredFailedList = new List<EuronextInstrumentContext>(1024);

            Trace.TraceInformation("Preparing: {0}", DateTime.Now);
            List<List<EuronextExecutor.Instrument>> approvedList = EuronextExecutor.Split(EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.WorkerThreads);
            List<List<EuronextExecutor.Instrument>> discoveredList = EuronextExecutor.Split(EuronextInstrumentContext.DiscoveredIndexPath, EuronextInstrumentContext.WorkerThreads);
            long downloadedApprovedInstruments = 0, approvedInstruments = 0;
            long downloadedDiscoveredInstruments = 0, discoveredInstruments = 0;
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading approved to {0}: {1}", downloadPath, DateTime.Now);
            /*EuronextInstrumentContext context = EuronextExecutor.Iterate(approvedList, esc =>
            {
                Interlocked.Increment(ref approvedInstruments);
                if (Download(esc, downloadPath, false, days))
                    Interlocked.Increment(ref downloadedApprovedInstruments);
                else
                {
                    lock (notDownloadedListLock)
                    {
                        approvedNotDownloadedList.Add(esc);
                    }
                }
            });*/
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Downloading discovered to {0}: {1}", downloadPath, DateTime.Now);
            /*EuronextExecutor.Iterate(discoveredList, esc =>
            {
                Interlocked.Increment(ref discoveredInstruments);
                if (Download(esc, downloadPath, false, days))
                    Interlocked.Increment(ref downloadedDiscoveredInstruments);
                else
                {
                    lock (notDownloadedListLock)
                    {
                        discoveredNotDownloadedList.Add(esc);
                    }
                }
            });*/
            int passCount = EuronextInstrumentContext.DownloadPasses, pass = 1;
            while (pass <= passCount && (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count))
            {
                if (0 < approvedNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} approved instruments (pass {1}): {2}", approvedNotDownloadedList.Count, pass, DateTime.Now);
                    approvedNotDownloadedList.ForEach(esc =>
                    {
                        if (Download(esc, downloadPath, pass == passCount, days))
                            Interlocked.Increment(ref downloadedApprovedInstruments);
                        else
                        {
                            approvedFailedList.Add(esc);
                        }
                    });
                    List<EuronextInstrumentContext> list = approvedNotDownloadedList;
                    approvedNotDownloadedList = approvedFailedList;
                    approvedFailedList = list;
                    approvedFailedList.Clear();
                }
                if (0 < discoveredNotDownloadedList.Count)
                {
                    Trace.TraceInformation("---------------------------------------------------------------------------------------");
                    Trace.TraceInformation("Retrying to download {0} discovered instruments (pass {1}): {2}", discoveredNotDownloadedList.Count, pass, DateTime.Now);
                    discoveredNotDownloadedList.ForEach(esc =>
                    {
                        if (Download(esc, downloadPath, pass == passCount, days))
                            Interlocked.Increment(ref downloadedDiscoveredInstruments);
                        else
                        {
                            discoveredFailedList.Add(esc);
                        }
                    });
                    List<EuronextInstrumentContext> list = discoveredNotDownloadedList;
                    discoveredNotDownloadedList = discoveredFailedList;
                    discoveredFailedList = list;
                    discoveredFailedList.Clear();
                }
                pass++;
            }
            if (0 < approvedNotDownloadedList.Count || 0 < discoveredNotDownloadedList.Count)
            {
                Trace.TraceInformation("---------------------------------------------------------------------------------------");
                if (0 < approvedFailedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} approved instruments: {1}", approvedNotDownloadedList.Count, DateTime.Now);
                    approvedFailedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
                if (0 < discoveredFailedList.Count)
                {
                    Trace.TraceInformation("Failed to download {0} discovered instruments: {1}", discoveredNotDownloadedList.Count, DateTime.Now);
                    discoveredFailedList.ForEach(esc =>
                    {
                        Trace.TraceInformation("{0}_{1}_{2}_{3}_eoi.csv(h)", esc.Mep, esc.Symbol, esc.Isin, esc.Yyyymmdd);
                    });
                }
            }
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            /*Trace.TraceInformation("History {0} approved   instruments: total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments, downloadedApprovedInstruments);
            Trace.TraceInformation("History {0} discovered instruments: total {1}, downloaded {2}", context.Yyyymmdd, discoveredInstruments, downloadedDiscoveredInstruments);
            Trace.TraceInformation("History {0} both                 : total {1}, downloaded {2}", context.Yyyymmdd, approvedInstruments + discoveredInstruments, downloadedApprovedInstruments + downloadedDiscoveredInstruments);
             */
        }
        #endregion

        #region ImportTask
        /// <summary>
        /// Performs an import task.
        /// </summary>
        /// <param name="importPath">A path to an import directory or an import file.</param>
        public static void ImportTask(string importPath)
        {
            Trace.TraceInformation("Scanning {0} and {1}: {2}", EuronextInstrumentContext.ApprovedIndexPath, EuronextInstrumentContext.DiscoveredIndexPath, DateTime.Now);
            Dictionary<string, EuronextExecutor.Instrument> dictionaryApproved = EuronextExecutor.ScanIndex(EuronextInstrumentContext.ApprovedIndexPath);
            Dictionary<string, EuronextExecutor.Instrument> dictionaryDiscovered = EuronextExecutor.ScanIndex(EuronextInstrumentContext.DiscoveredIndexPath);
            Trace.TraceInformation("Splitting {0}: {1}", importPath, DateTime.Now);
            List<string> orphaned;
            List<List<string>> list = EuronextExecutor.Split(importPath, dictionaryApproved, dictionaryDiscovered, EuronextInstrumentContext.WorkerThreads, out orphaned);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("Merging: {0}", DateTime.Now);
            long totalInstruments = 0, mergedInstruments = 0;
            string yyyymmdd = "";
            EuronextExecutor.Iterate(list, dictionaryApproved, dictionaryDiscovered, yyyymmdd, (xml, esc) =>
            {
                Interlocked.Increment(ref totalInstruments);
                EuronextInstrumentContext.VerifyFile(xml);
                if (Merge(xml, esc))
                    Interlocked.Increment(ref mergedInstruments);
            }, false);
            Trace.TraceInformation("---------------------------------------------------------------------------------------");
            Trace.TraceInformation("History imported instruments: total {0}, merged {1}", totalInstruments, mergedInstruments);
            orphaned.ForEach(file => Trace.TraceInformation("Orphaned import file [{0}], skipped", file));
        }
        #endregion

    }
}
