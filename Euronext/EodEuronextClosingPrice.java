package mbmr.data.provider;

import java.io.BufferedReader;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.GregorianCalendar;
import java.util.List;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.transform.Transformer;
import javax.xml.xpath.XPath;

import mbmr.data.repository.EodQuoteRepository;
import mbmr.data.repository.SecurityInfoRepository;
import mbmr.data.repository.XmlElements;
import mbmr.util.Downloader;
import mbmr.util.JulianDayNumber;
import mbmr.util.LockedRandomAccessFile;
import mbmr.util.Logger;
import mbmr.util.LoggerHtml;
import mbmr.util.ParentDir;
import mbmr.util.SharedDocument;
import mbmr.util.XmlErrorHandler;
import mbmr.util.XmlFactory;
import mbmr.util.Zipper;

import org.w3c.dom.Comment;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.xml.sax.SAXException;

/**
 *
 */
public class EodEuronextClosingPrice extends EodDiscoveringProvider {
	/**
	 * Reads in all the CSV lines from the {@code reader}. For every CSV line, retrieves the security file path from the
	 * {@link SecurityInfoRepository SecurityInfoRepository} and updates it using the {@link EodQuoteRepository EodQuoteRepository}.
	 * Skips several header lines at the beginning of the input stream.
	 * <p/>Stock price search:
	 * <table>
	 * <tr><td>1</td><td>Stocks</td><tr>
	 * <tr><td>2</td><td>Amsterdam/Brussels/Paris/Lisbon - Euronext</td><tr>
	 * <tr><td>3</td><td></td><tr>
	 * <tr><td>4</td><td>Instrument's name;ISIN;Euronext code;MEP;Symbol;ICB Sector (Level 4);Trading currency;Last;Volume;D/D-1 (%);Date - time (CET);Turnover;Total number of shares;Capitalisation;Trading mode;Day First;Day High;Day High / Date - time (CET);Day Low;Day Low / Date - time (CET); 31-12/Change (%); 31-12/High; 31-12/High/Date; 31-12/Low; 31-12/Low/Date; 52 weeks/Change (%); 52 weeks/High; 52 weeks/High/Date; 52 weeks/Low; 52 weeks/Low/Date;Suspended;Suspended / Date - time (CET);Reserved;Reserved / Date - time (CET)</td><tr>
	 * </table>
	 * <p/>ETF price search:
	 * <table>
	 * <tr><td>1</td><td>Trackers-structured fnds;</td><tr>
	 * <tr><td>2</td><td>ETF/Trackers;</td><tr>
	 * <tr><td>3</td><td>Price search;</td><tr>
	 * <tr><td>4</td><td></td><tr>
	 * <tr><td>5</td><td>Instrument's name;ISIN;Euronext code;MEP;Symbol;Underlying;Trading currency;Bid Date - time (CET);Bid;Ask;Ask Date - time (CET);Last;Volume;D/D-1 (%);Last Date - time (CET);Turnover;Indicative value;Indicative value Date - time (CET);Trading mode;Underlying price;Underlying price Date - time (CET);Day First;Day High;Day High / Date - time (CET);Day Low;Day Low / Date - time (CET); 31-12/Change (%); 31-12/High; 31-12/High/Date; 31-12/Low; 31-12/Low/Date; 52 weeks/Change (%); 52 weeks/High; 52 weeks/High/Date; 52 weeks/Low; 52 weeks/Low/Date;Suspended;Suspended / Date - time (CET);Reserved;Reserved / Date - time (CET)</td><tr>
	 * </table>
	 * <p/>Fund price search:
	 * <table>
	 * <tr><td>1</td><td>Funds;</td><tr>
	 * <tr><td>2</td><td>null;</td><tr>
	 * <tr><td>3</td><td>Price search;</td><tr>
	 * <tr><td>4</td><td></td><tr>
	 * <tr><td>5</td><td>Instrument's name;ISIN;Euronext code;MEP;Symbol;Trading currency;Last;Volume;D/D-1 (%);Date - time (CET);Turnover;Trading mode;Day First;Day High;Day High / Date - time (CET);Day Low;Day Low / Date - time (CET); 31-12/Change (%); 31-12/High; 31-12/High/Date; 31-12/Low; 31-12/Low/Date; 52 weeks/Change (%); 52 weeks/High; 52 weeks/High/Date; 52 weeks/Low; 52 weeks/Low/Date;Suspended;Suspended / Date - time (CET);Reserved;Reserved / Date - time (CET)</td><tr>
	 * </table>
	 * <p/>Index price search:
	 * <table>
	 * <tr><td>1</td><td>Indices;GLOBAL/NATIONALINDICES/SECTORIALINDICES;</td><tr>
	 * <tr><td>2</td><td></td><tr>
	 * <tr><td>3</td><td>Instrument's name;ISIN;MEP;Symbol;Day First;Day High;Day High / Date - time (CET);Day Low;Day Low / Date - time (CET);Last;D/D-1 (%);Date - time (CET); 31-12/Change (%); 31-12/High; 31-12/High/Date; 31-12/Low; 31-12/Low/Date; 52 weeks/Change (%); 52 weeks/High; 52 weeks/High/Date; 52 weeks/Low; 52 weeks/Low/Date;Risers;Fallers;Neutrals</td><tr>
	 * </table>
	 *
	 * @param reader the input source reader.
	 * @param approvedRepositoryFilePath a path to the read-only repository XML file containing approved securities.
	 * @param discoveredRepositoryFilePath a path to the read-write repository XML file containing newly discovered securities. A not-existing file will be created.
	 * @param log the {@linkplain Logger logger}.
	 */
	public void process(BufferedReader reader, String approvedRepositoryFilePath, String discoveredRepositoryFilePath, Logger log,
		SharedDocument sharedDocument, boolean verbose, int daysBack) {
		StringBuffer buffer = new StringBuffer(1024);
        String csv = null, securityFilePath;
        boolean proceed;
		CsvStringEuronextClosingPrice euronext = new CsvStringEuronextClosingPrice();
        XmlErrorHandler eh = new XmlErrorHandler(log, "");
		int lineCount = 1, processedCount = 0, skippedCount = 0;
		//String inputType = "other";
        try {
            //csv = reader.readLine();
            //lineCount++;
            //if (csv.startsWith("Stocks")) {
            //	inputType = CsvStringEuronextClosingPrice.STOCK;
            //} else if (csv.startsWith("Trackers")) {
            //	inputType = CsvStringEuronextClosingPrice.ETF;
            //} else if (csv.startsWith("Funds")) {
            //	inputType = CsvStringEuronextClosingPrice.FUND;
            //} else if (csv.startsWith("Indices")) {
            //	inputType = CsvStringEuronextClosingPrice.INDEX;
            //}
            boolean begingParsing = false;
            while (true) {
                csv = reader.readLine();
                lineCount++;
                if (null == csv || begingParsing) {
                	break;
                }
                if (csv.startsWith("Instrument's name;")) {
                	begingParsing = true;
                }
            }
            proceed = true;
        } catch(IOException e) {
        	eh.error(buffer, "failed to read header line ", lineCount, ": ", e.getMessage());
        	skippedCount++;
        	proceed = false;
        }
        DocumentBuilder documentBuilder = XmlFactory.getDocumentBuilder(log, "", eh);
        Transformer transformer = XmlFactory.getTransformer(log, "");
        XPath xpath = XmlFactory.getXPath(log, "");
		Document document = null;
		try {
			File repositoryFile = new File(approvedRepositoryFilePath);
			if (repositoryFile.exists()) {
				document = documentBuilder.parse(approvedRepositoryFilePath);				
			} else {
				eh.error(buffer, "approved repository file not found: ", approvedRepositoryFilePath);
			}
		} catch (SAXException e) {
			eh.error(buffer, "approved repository XML not well formed, aborting: ", e.getMessage());
		} catch (IOException e) {
			eh.error(buffer, e.getMessage());
		}			
		if (null == document) {
			return;
		}
		int jdnNow = JulianDayNumber.toJdn();
        while (null != csv) {
        	if (proceed) {
        		try {
        			euronext.parse(csv, eh);
            		securityFilePath = SecurityInfoRepository.retrieve(euronext, approvedRepositoryFilePath, discoveredRepositoryFilePath,
            			log, documentBuilder, document, xpath, transformer, sharedDocument, buffer);
            		documentBuilder.setErrorHandler(eh);
            		if (null == securityFilePath) {
                    	if (null == discoveredRepositoryFilePath) {
                    		if (verbose) {
                        		eh.message(buffer, "skipping line ", lineCount, ": security not found in approved repository");
                    		}
                    	} else {
                    		eh.error(buffer, "skipping line ", lineCount, ": failed to retrieve the security file path");
                    	}
                    	skippedCount++;
            		} else {
                    	buffer.setLength(0);
                    	buffer.append(securityFilePath);
                    	buffer.append(": ");
            			eh.setProlog(buffer.toString());
            			EodQuoteRepository.update(euronext, securityFilePath, log, documentBuilder, xpath, transformer, buffer, verbose, jdnNow, daysBack);
                		documentBuilder.setErrorHandler(eh);
            			processedCount++;
            		}
        		} catch (IllegalArgumentException e) {
        			eh.error(buffer, "skipping line ", lineCount, ": ", e.getMessage());
                	skippedCount++;
        		}
        	}
        	lineCount++;
            eh.setProlog("");
            try {
                csv = reader.readLine();
                proceed = true;
            	
            } catch(IOException e) {
        		eh.error(buffer, "skipping line ", lineCount, ": ", e.getMessage());
            	skippedCount++;
            	proceed = false;
            }
        }
        eh.setProlog("");
        if (verbose) {
            eh.message(buffer, "total ", lineCount, " lines, processed ", processedCount, ", skipped ", skippedCount);
        }
	}

	/**
	 * Checks if the {@code name} matches the valid EuroNext Price Search file names: {@code MMDDenx_xxXX.csv}.
	 *
	 * @param name the file name to match.
	 */
	public boolean matchFileName(String name) {
		return name.matches("(?i).*enx_st.*\\.csv")
			|| name.matches("(?i).*enx_is.*\\.csv")
			|| name.matches("(?i).*enx_fu.*\\.csv")
			|| name.matches("(?i).*enx_tr.*\\.csv")
			|| name.matches("(?i).*enx_eop.*\\.zip");
	}

	private static List<String> doDownload(EodProviderProperties properties, Logger log, StringBuffer buffer) {
		if (null == buffer) {
        	buffer = new StringBuffer(512);
		}
		List<String> list = new ArrayList<String>();
		Calendar calendar = new GregorianCalendar();
		int jdn = JulianDayNumber.toJdn(calendar.get(Calendar.YEAR), calendar.get(Calendar.MONTH) + 1, calendar.get(Calendar.DAY_OF_MONTH));
		int lastJdn = properties.eodEuronextClosingPriceDownloadLastJdn(); 
		int[] afterTime = properties.eodEuronextClosingPriceDownloadAfterTime();
		int[] beforeTime = properties.eodEuronextClosingPriceDownloadBeforeTime();
		int hour = calendar.get(Calendar.HOUR_OF_DAY);
		int i = calendar.get(Calendar.MINUTE);		
		if (!JulianDayNumber.isWeekend(jdn) &&
			(hour > beforeTime[0] || (hour == beforeTime[0] && i > beforeTime[1])) &&
			(hour < afterTime[0] || (hour == afterTime[0] && i < afterTime[1]))) {
			buffer.setLength(0);
			if (10 > hour) {
				buffer.append('0');				
			}
			buffer.append(hour);
			buffer.append(':');
			if (10 > i) {
				buffer.append('0');
			}
			buffer.append(i);
			buffer.append(" nothing to do from ");
			if (10 > beforeTime[0]) {
				buffer.append('0');				
			}
			buffer.append(beforeTime[0]);
			buffer.append(':');
			if (10 > beforeTime[1]) {
				buffer.append('0');				
			}
			buffer.append(beforeTime[1]);
			buffer.append(" till ");
			if (10 > beforeTime[0]) {
				buffer.append('0');				
			}
			buffer.append(beforeTime[0]);
			buffer.append(':');
			if (10 > beforeTime[1]) {
				buffer.append('0');				
			}
			buffer.append(beforeTime[1]);
			log.warning(buffer);
			return list;
		}
		if (hour < beforeTime[0] || (hour == beforeTime[0] && i < beforeTime[1])) {
			jdn--;
		}
		if (jdn <= lastJdn) {
			buffer.setLength(0);
			buffer.append("nothing to do: last processed jdn ");
			buffer.append(lastJdn);
			buffer.append(", current jdn ");
			buffer.append(lastJdn);
			log.warning(buffer);
			return list;
		}
		boolean verbose = properties.eodEuronextClosingPriceLogVerbose();
		int retries = properties.eodEuronextClosingPriceDownloadRetries();
		int timeout = properties.eodEuronextClosingPriceDownloadTimeout();
		buffer.setLength(0);
		buffer.append(properties.eodEuronextClosingPriceDownloadTargetDir());
		buffer.append(File.separatorChar);
		buffer.append(calendar.get(Calendar.YEAR));
		buffer.append(File.separatorChar);
		final String path = buffer.toString();
		int[] ymd = JulianDayNumber.toYmd(jdn);
		if (JulianDayNumber.isSunday(jdn)) {
			buffer.setLength(0);
			buffer.append(path);
			buffer.append("sundays");			
			buffer.append(File.separatorChar);
			list = download(buffer.toString(), list, true, ymd[0], ymd[1], ymd[2],
				retries, timeout, log, buffer, verbose);
		} else if (JulianDayNumber.isSaturday(jdn)) {
			buffer.setLength(0);
			buffer.append(path);
			buffer.append("saturdays");			
			buffer.append(File.separatorChar);
			list = download(buffer.toString(), list, true, ymd[0], ymd[1], ymd[2],
				retries, timeout, log, buffer, verbose);
		} else if (Euronext.isHoliday(jdn)) {
			buffer.setLength(0);
			buffer.append(path);
			buffer.append("holidays");			
			buffer.append(File.separatorChar);
			list = download(buffer.toString(), list, true, ymd[0], ymd[1], ymd[2],
				retries, timeout, log, buffer, verbose);
		} else {
			buffer.setLength(0);
			buffer.append(path);
			buffer.append("workdays");			
			buffer.append(File.separatorChar);
			list = download(buffer.toString(), list, true, ymd[0], ymd[1], ymd[2],
				retries, timeout, log, buffer, verbose);
		}
		properties.eodEuronextClosingPriceDownloadLastJdn(jdn);
		return list;
	}

	/**
	 * Batch file (day.bat) to download the EuroNext price search end-of day file set manually.
	 * The arguments are the month and the day ({@code day.bat 01 25}).
	 * <p/><table>
	 * <tr><td>{@code rem euronext all stocks}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st01.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_EURLS&resultsTitle=5257-market_EURLS&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st02.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=subLocal_100&resultsTitle=5257-subLocal_100&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st03.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=subLocal_200&resultsTitle=5257-subLocal_200&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st04.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=subLocal_300&resultsTitle=5257-subLocal_300&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st05.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_ALTX&resultsTitle=5257-market_ALTX&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st06.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_400&resultsTitle=5257-market_400&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st07.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_500&resultsTitle=5257-market_500&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st08.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_AMSTL&resultsTitle=5257-market_AMSTL&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st09.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=eligibility_SRD,CFP&resultsTitle=5257-eligibility_SRD,CFP&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st10.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_BRUMC&resultsTitle=5257-market_BRUMC&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st11.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_BRUTF&resultsTitle=5257-market_BRUTF&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st12.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_BRUCI&resultsTitle=5257-market_BRUCI&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st13.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_RADMR&resultsTitle=5257-market_RADMR&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st14.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_MC&resultsTitle=5257-market_MC&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_st15.csv "http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_LISUS&resultsTitle=5257-market_LISUS&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code rem euronext all etf //&skipInstrumentSubType=410}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_tr01.csv "http://www.euronext.com/search/download/pricesearchdownloadpopup.jcsv?pricesearchresults=actif&lan=EN&resultsTitle=ETF/Trackers&cha=1821&instrumentType=4&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code rem euronext all funds //&mep=8583&fundType=710}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_fu01.csv "http://www.euronext.com/search/download/pricesearchdownloadpopup.jcsv?pricesearchresults=actif&equitiesChoice=3&lan=EN&resultsTitle=null&mep=5257&cha=1847&instrumentType=7&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code rem indices}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is01.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&mep=5257&selectedMep=5257&display=GLOBAL&lan=EN&cha=1864&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is02.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&mep=5257&selectedMep=5257&display=NATIONALINDICES&lan=EN&cha=1864&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is11.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2908&selectedMepId=2908&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is12.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2909&selectedMepId=2909&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is21.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2912&selectedMepId=2914&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is22.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2914&selectedMepId=2914&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is31.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2910&selectedMepId=2910&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is32.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2911&selectedMepId=2911&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * <tr><td>{@code wget.exe --tries=1 -nd -aenx%1%2.log -O%1%2enx_is41.csv "http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=3271&selectedMepId=3271&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"}</td></tr>
	 * </table>
	 */
	private static List<String> download(String path, List<String> bucket, boolean addToBucket,
		int year, int month, int day, int retries, int timeout, Logger log, StringBuffer buffer, boolean verbose) {
		final String [] names = {
			"enx_st01.csv", "enx_st02.csv", "enx_st03.csv", "enx_st04.csv", "enx_st05.csv",
			"enx_st06.csv", "enx_st07.csv", "enx_st08.csv", "enx_st09.csv", "enx_st10.csv",
			"enx_st11.csv", "enx_st12.csv", "enx_st13.csv", "enx_st14.csv", "enx_st15.csv",
			"enx_tr01.csv", "enx_fu01.csv", "enx_is01.csv", "enx_is02.csv", "enx_is11.csv",
			"enx_is12.csv", "enx_is21.csv", "enx_is22.csv", "enx_is31.csv", "enx_is32.csv",
			"enx_is41.csv"
		};
		final int [] sizes = {
			600, 600, 600, 600, 600,
			500, 500, 600, 600, 600,
			600, 600, 600, 600, 600,
			600, 600, 600, 600, 600,
			600, 600, 600, 600, 600,
			600
		};
		final String [] urls = {
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_EURLS&resultsTitle=5257-market_EURLS&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=subLocal_100&resultsTitle=5257-subLocal_100&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=subLocal_200&resultsTitle=5257-subLocal_200&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=subLocal_300&resultsTitle=5257-subLocal_300&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_ALTX&resultsTitle=5257-market_ALTX&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_400&resultsTitle=5257-market_400&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_500&resultsTitle=5257-market_500&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_AMSTL&resultsTitle=5257-market_AMSTL&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=eligibility_SRD,CFP&resultsTitle=5257-eligibility_SRD,CFP&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_BRUMC&resultsTitle=5257-market_BRUMC&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_BRUTF&resultsTitle=5257-market_BRUTF&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_BRUCI&resultsTitle=5257-market_BRUCI&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_RADMR&resultsTitle=5257-market_RADMR&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_MC&resultsTitle=5257-market_MC&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapridownloadpopup.jcsv?pricesearchresults=actif&mep=5257&lan=EN&belongsToList=market_LISUS&resultsTitle=5257-market_LISUS&cha=1800&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/pricesearchdownloadpopup.jcsv?pricesearchresults=actif&lan=EN&resultsTitle=ETF/Trackers&cha=1821&instrumentType=4&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/pricesearchdownloadpopup.jcsv?pricesearchresults=actif&equitiesChoice=3&lan=EN&resultsTitle=null&mep=5257&cha=1847&instrumentType=7&formatValue=txt&formatDateValue=dd/mm/yy&formatDecimalValue=&format=txt&formatDecimal=&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&mep=5257&selectedMep=5257&display=GLOBAL&lan=EN&cha=1864&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&mep=5257&selectedMep=5257&display=NATIONALINDICES&lan=EN&cha=1864&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2908&selectedMepId=2908&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2909&selectedMepId=2909&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2912&selectedMepId=2914&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2914&selectedMepId=2914&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2910&selectedMepId=2910&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=2911&selectedMepId=2911&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy",
			"http://www.euronext.com/search/download/trapiindicedownloadpopup.jcsv?pricesearchresults=actif&cha=3271&selectedMepId=3271&display=SECTORIALINDICES&lan=EN&resultsTitle=null&format=txt&formatDecimal=.&formatDate=dd/MM/yy"			
		};
		if (null == bucket) {
			bucket = new ArrayList<String>();			
		}
		byte[] bbuf = new byte[32768];
		buffer.setLength(0);
		buffer.append(year);
		if (10 > month) {
			buffer.append('0');
		}
		buffer.append(month);
		if (10 > day) {
			buffer.append('0');
		}
		buffer.append(day);
		final String stamp = buffer.toString();
		buffer.setLength(0);
		buffer.append(path);
		buffer.append(stamp);
		buffer.append(File.separatorChar);
		path = buffer.toString();
		String filename, pathname;
		int i, length = names.length;
		for (i = 0; i < length; i++) {
			buffer.setLength(0);
			buffer.append(stamp);
			buffer.append(names[i]);
			filename = buffer.toString();
			buffer.setLength(0);
			buffer.append(path);
			buffer.append(filename);
			pathname = buffer.toString();
			if (Downloader.download(urls[i], pathname, retries, timeout, sizes[i], log, buffer, bbuf, verbose)) {
				if (addToBucket) {
					if (bucket.contains(pathname)) {
						buffer.setLength(0);
						buffer.append("file ");
						buffer.append(pathname);
						buffer.append(" already in the bucket, skipping");
						log.error(buffer);
					} else {
						bucket.add(pathname);
					}
				}
			}
		}
		return bucket;
	}

	private static final String threadName = "EOD Euronext closing price";
	private static final ThreadGroup threadGroup = new ThreadGroup(threadName);

	private static class DownloadRunnable implements Runnable {
		private final EodProviderProperties properties;
		private final String approvedRepositoryFilePath;
		private final String discoveredRepositoryFilePath;
		private final int threadCount;
		private final LoggerHtml log;
		
		public DownloadRunnable(EodProviderProperties properties, Logger parent) {
			this.properties = properties;
			approvedRepositoryFilePath = properties.eodEuronextClosingPriceImportApprovedSecurities();
			final String path = properties.eodEuronextClosingPriceImportDiscoveredSecurities();
			if ("".equals(path)) {
				discoveredRepositoryFilePath = null;
			} else {
				discoveredRepositoryFilePath = path;
			}
			final int count = properties.eodEuronextClosingPriceImportThreadPoolSize();
			threadCount = (1 > count) ? 1 : count;
			log = new LoggerHtml(properties.eodEuronextClosingPriceLogFilePrefix(), parent);
		}

	    public void run() {
    		StringBuffer buffer = new StringBuffer(1024);
    		List<String> bucket = doDownload(properties, log, buffer);
    		//List<String> bucket = new ArrayList<String>();
    		//bucket.add("20080208enx_all.zip");
			int bucketSize = bucket.size();
			if (0 == bucketSize) {
				log.close();
    			return;				
			}
			buffer.setLength(0);
    		buffer.append(Thread.currentThread().getName());
    		buffer.append(": ");
    		String prolog = buffer.toString();
    		XmlErrorHandler eh = new XmlErrorHandler(log, prolog); 
    		DocumentBuilder documentBuilder = XmlFactory.getDocumentBuilder(log, prolog, eh);
    		Transformer transformer = XmlFactory.getTransformer(log, prolog);
    		if (null == documentBuilder || null == transformer) {
				log.close();
    			return;
    		}
    		boolean verbose = properties.eodEuronextClosingPriceLogVerbose();
    		int daysBack = properties.eodEuronextClosingPriceImportDaysBack();
    		SharedDocument sharedDocument = null;
    		EodEuronextClosingPrice instance = new EodEuronextClosingPrice();
    		if (null == discoveredRepositoryFilePath) {
    			if (properties.eodEuronextClosingPriceDownloadImport()) {
        			try {
        				instance.doImport(bucket, approvedRepositoryFilePath, discoveredRepositoryFilePath,
        					threadCount, log, sharedDocument, buffer, threadGroup, verbose, daysBack);				
        			} catch (Exception e) {
        				eh.error(buffer, e.getMessage());
        			}
    			}
    			if (properties.eodEuronextClosingPriceDownloadZip()) {
    				Zipper.compress(bucket, "enx_eop", true, buffer, log, verbose);
    			}
				log.close();
    			return;
    		}
			buffer.setLength(0);
    		buffer.append(discoveredRepositoryFilePath);
    		buffer.append(": ");
    		prolog = buffer.toString();
    		eh.setProlog(prolog);
    		LockedRandomAccessFile lraf = new LockedRandomAccessFile(log, prolog, buffer);
			File file = new File(discoveredRepositoryFilePath);
			Document document = null;
			if (ParentDir.ensureExists(file)) {
				document = lraf.openDocument(file, documentBuilder);
			} else {
				document = documentBuilder.newDocument();
				Element securities = document.createElement(XmlElements.SECURITIES);
			    Comment comment = document.createComment("TODO: review the attributes of these securities");
			    securities.appendChild(comment);
				document.appendChild(securities);
				if (verbose) {
					eh.message(buffer, "created file");
				}
			}
			sharedDocument = new SharedDocument(document);
			try {
    			if (properties.eodEuronextClosingPriceDownloadImport()) {
    				instance.doImport(bucket, approvedRepositoryFilePath, discoveredRepositoryFilePath,
    					threadCount, log, sharedDocument, buffer, threadGroup, verbose, daysBack);				
    			}
			} catch (Exception e) {
				eh.error(buffer, e.getMessage());
			}
			// All the worker threads are finished now.
			if (null != document) {
				lraf.saveDocument(document, transformer);
			}
			if (properties.eodEuronextClosingPriceDownloadZip()) {
				Zipper.compress(bucket, "enx_eop", true, buffer, log, verbose);
			}
			log.close();
	    }
	}

	public static void doDownload(EodProviderProperties properties, Logger parentLogger) {
		DownloadRunnable runnable = new DownloadRunnable(properties, parentLogger);
		runnable.run();
	}

	public static Thread createDownloadThread(EodProviderProperties properties, Logger parentLogger) {
		return new Thread(threadGroup, new DownloadRunnable(properties, parentLogger), threadName);
	}

}
