https://www.astro.oma.be/en/scientific-research/solar-physics-space-weather/

SIDC - Solar Influence Data Analysis Center
===========================================
Daily sunspot number: Ri, Rn, Rs

Data Credits: The Sunspot Number data can be freely downloaded. However, we request that proper credit to the WDC-SILSO is explicitely included in
any publication using our data (paper article or book, on-line Web content, etc.), e.g.: "Source: WDC-SILSO, Royal Observatory of Belgium, Brussels".

http://www.sidc.be/silso/datafiles
http://www.sidc.be/silso/newdataset

========================================================
Total sunspot number
========================================================
--------------------------------------------------------
Daily total sunspot number [1/1/1818 - now]
--------------------------------------------------------
Old files: dayssn.dat, dayssnV0.dat, dayssn_import.dat,ISSN_D_tot.csv
replaced by: 
http://www.sidc.be/silso/DATA/SN_d_tot_V2.0.txt
Contents:
Column 1-3: Gregorian calendar date
- Year
- Month
- Day
Column 4: Date in fraction of year
Column 5: Daily total sunspot number. A value of -1 indicates that no number is available for that day (missing value).
Column 6: Daily standard deviation of the input sunspot numbers from individual stations.
Column 7: Number of observations used to compute the daily value.
Column 8: Definitive/provisional indicator. A blank indicates that the value is definitive. A '*' symbol indicates that the value is still provisional and is subject to a possible revision (Usually the last 3 to 6 months)

Line format [character position]:
- [1-4] Year
- [6-7] Month
- [9-10] Day
- [12-19] Decimal date
- [22-24] Daily sunspot number
- [26-30] Standard deviation
- [33-35] Number of observations
- [37] Definitive/provisional indicator

Data description:
Daily total sunspot number derived by the formula: R= Ns + 10 * Ng, with Ns the number of spots and Ng the number of groups counted over the entire solar disk.

No daily data are provided before 1818 because daily observations become too sparse in earlier years. Therefore, R. Wolf only compiled monthly means and yearly means for all years before 1818.

In the TXT and CSV files, the missing values are marked by -1 (valid Sunspot Number are always positive).

New scale:
The conventional 0.6 Zürich scale factor is not used anymore and A. Wolfer (Wolf's successor) is now defining the scale of the entire series. This puts the Sunspot Number at the scale of raw modern counts, instead of reducing it to the level of early counts by R. Wolf.

Error values:
Those values correspond to the standard deviation of raw numbers provided by all stations. Before 1981, the errors are estimated with the help of an auto-regressive model based on the Poissonian distribution of actual Sunspot Numbers. From 1981 onwards, the error value is the actual standard deviation of the sample of raw observations used to compute the daily value.
The standard error of the daily Sunspot Number can be computed by:
sigma/sqrt(N) where sigma is the listed standard deviation and N the number of observations for the day.
Before 1981, the number of observations is set to 1, as the Sunspot Number was then essentially the raw Wolf number from the Zürich Observatory.

--------------------------------------------------------
Monthly mean total sunspot number [1/1749 - now]
--------------------------------------------------------
Old files: monthssn.dat, ISSN_M_tot.csv
replaced by: 
http://www.sidc.be/silso/DATA/SN_m_tot_V2.0.txt
Contents:
Column 1-2: Gregorian calendar date
- Year
- Month
Column 3: Date in fraction of year for the middle of the corresponding month
Column 4: Monthly mean total sunspot number.
Column 5: Monthly mean standard deviation of the input sunspot numbers from individual stations.
Column 6: Number of observations used to compute the monthly mean total sunspot number.
Column 7: Definitive/provisional marker. A blank indicates that the value is definitive. A '*' symbol indicates that the monthly value is still provisional and is subject to a possible revision (Usually the last 3 to 6 months)

Line format [character position]:
- [1-4] Year
- [6-7] Month
- [9-16] Decimal date
- [19-23] Monthly total sunspot number
- [25-29] Standard deviation
- [32-35] Number of observations
- [37] Definitive/provisional indicator

Data description:
Monthly mean total sunspot number obtained by taking a simple arithmetic mean of the daily total sunspot number over all days of each calendar month. Monthly means are available only since 1749 because the original observations compiled by Rudolph Wolf were too sparse before that year. (Only yearly means are available back to 1700)
A value of -1 indicates that no number is available (missing value).

Error values:
The monthly standard deviation of individual data is derived from the daily values by: sigma(m)=sqrt(SUM(N(d)*sigma(d)^2)/SUM(N(d)))
where sigma(d) is the standard deviation for a single day and N(d) is the
number of observations for that day.
The standard error on the monthly mean values can be computed by:
sigma/sqrt(N) where sigma is the listed standard deviation and N the total number of observations in the month.

NB: February 1824 does not contain any daily value. As it is the only month without data after 1749, the monthly mean value was interpolated by R. Wolf between the adjacent months.

--------------------------------------------------------
13-month smoothed monthly total sunspot number [1/1749 - now]
--------------------------------------------------------
Old files: monthssn.dat, ISSN_M_tot.csv
New file: 
http://www.sidc.be/silso/DATA/SN_ms_tot_V2.0.txt
Contents:
Column 1-2: Gregorian calendar date
- Year
- Month
Column 3: Date in fraction of year for the middle of the corresponding month
Column 4: Monthly smoothed total sunspot number.
Column 5: Monthly mean standard deviation of the input sunspot numbers.
Column 6: Number of observations used to compute the corresponding monthly mean total sunspot number.
Column 7: Definitive/provisional marker. A blank indicates that the value is definitive. A '*' symbol indicates that the monthly value is still provisional and is subject to a possible revision (Usually the last 3 to 6 months)

Line format [character position]:
- [1-4] Year
- [6-7] Month
- [9-16] Decimal date
- [19-23] Smoothed total sunspot number
- [25-29] Standard deviation
- [32-35] Number of observations
- [37] Definitive/provisional indicator

Data description:
The 13-month smoothed monthly sunspot number is derived by a "tapered-boxcar" running mean of monthly sunspot numbers over 13 months centered on the corresponding month (Smoothing function: equal weights = 1, except for first and last elements (-6 and +6 months) = 0.5, Normalization by 1/12 factor). There are no smoothed values for the first 6 months and last 6 months of the data series: columns 4, 5 and 6 are set to -1 (no data).

Choice of smoothing:
This 13-month smoothed series is provided only for backward compatibility with a large number of past publications and methods resting on this smoothed series. It has thus become a base standard (e.g. for the conventional definition of the times of minima and maxima of solar cycles).

However, a wide range of other smoothing functions can be used, often with better low-pass filtering and anti-aliasing properties. As the optimal filter choice depends on the application, we thus invite users to start from the monthly mean Sunspot Numbers and apply the smoothing function that is most appropriate for their analyses. The classical smoothed series included here should only be used when direct comparisons with past published analyses must be made.

Error values:
The standard deviations in this files are obtained from the weighted mean of the variances of the 13 months in the running mean value:
sigma(ms)=sqrt(SUM(weigth(M)*sigma(M)^2)/SUM(weight(M))
where sigma(M) is the standard deviation for a single month, weight(M) is 1 or 0.5 and M=13 in this case.

As successive monthly means are highly correlated, the standard error on the smoothed values can be estimated by the same formula as for a single month: sigma/sqrt(N) where sigma is the listed standard deviation and N the total number of observations in the month.
The number of observations given in column 6 is the number of observations of the corresponding (middle) month: same value SUM N(d) as in the monthly mean file.
This thus gives a smoothed mean of monthly standard deviations, i.e. with the samme low-pass filtering as the data value itself. Further autocorrelation analyses will be needed to derive a conversion of this standard deviation to a standard error of the 13-month smoothed number.

--------------------------------------------------------
Yearly mean total sunspot number [1700 - now]
--------------------------------------------------------
Old files: yearssn.dat, ISSN_Y_tot.csv
replaced by: 
http://www.sidc.be/silso/DATA/SN_y_tot_V2.0.txt
Contents:
Column 1: Gregorian calendar year (mid-year date)
Column 2: Yearly mean total sunspot number.
Column 3: Yearly mean standard deviation of the input sunspot numbers from individual stations.
Column 4: Number of observations used to compute the yearly mean total sunspot number.
Column 5: Definitive/provisional marker. A blank indicates that the value is definitive. A '*' symbol indicates that the yearly average still contains provisional daily values and is subject to a possible revision.

Line format [character position]:
- [1-6] Year (decimal)
- [9-13] Yearly mean total sunspot number
- [15-19] Standard deviation
- [22-26] Number of observations
- [28] Definitive/provisional indicator

Data description:
Yearly mean total sunspot number obtained by taking a simple arithmetic mean of the daily total sunspot number over all days of each year. (NB: in early years in particular before 1749, the means are computed on only a fraction of the days in each year because on many days, no observation is available).
A value of -1 indicates that no number is available (missing value).

Error values:
The yearly standard deviation of individual data is derived from the daily values by the same formula as the monthly means:
sigma(m)=sqrt(SUM(N(d)*sigma(d)^2)/SUM(N(d)))
where sigma(d) is the standard deviation for a single day and N(d) is the
number of observations for that day.

The standard error on the yearly mean values can be computed by:
sigma/sqrt(N) where sigma is the listed standard deviation and N the total number of observations in the year.
NB: this standard error gives a measure of the precision, i.e. the sensitivity of the yearly value to different samples of daily values with random errors. The uncertainty on the mean (absolute accuracy) is only determined on longer time scales, and is thus not given here for individual yearly values.

========================================================
Hemispheric sunspot numbers
========================================================
--------------------------------------------------------
Daily hemispheric sunspot number [1/1/1992 - now]:
--------------------------------------------------------
Old files: yearly files dssnYYYY.dat,ISSN_D.hem.txt, ISSN_D_hem.txt
replaced by a single file:
http://www.sidc.be/silso/DATA/SN_d_hem_V2.0.txt
Contents:
Columns 1-3: Gregorian calendar date
Column 1: Year
Column 2: Month
Column 3: Day
Column 4: Date in fraction of year
Column 5: Daily total sunspot number.
Column 6: Daily North sunspot number.
Column 7: Daily South sunspot number.
Column 8: Standard deviation of raw daily total sunspot data
Column 9: Standard deviation of raw daily North sunspot data
Column 10: Standard deviation of raw daily South sunspot data
Column 11: Number of observations in daily total sunspot number
Column 12: Number of observations in daily North sunspot number (not determined yet: -1)
Column 13: Number of observations in daily South sunspot number (not determined yet: -1)
Column 14: Definitive/provisional marker. A blank indicates that the value is definitive. A '*' symbol indicates that the value is still provisional and is subject to a possible revision (Usually the last 3 to 6 months)

Line format [character position]:
- [1-4] Year
- [6-7] Month
- [9-10] Day
- [12-19] Decimal date
- [22-24] Daily total sunspot number
- [26-28] Daily north sunspot number
- [30-32] Daily south sunspot number
- [35-39] Standard deviation (Total)
- [41-45] Standard deviation (North)
- [46-51] Standard deviation (South)
- [54-56] Number of observations (Total)
- [58-60] Number of observations (North)
- [62-64] Number of observations (South)
- [66] Definitive/provisional indicator

Data description:
Daily total and hemispheric sunspot numbers derived by the formula: R= Ns + 10 * Ng, with Ns the number of spots and Ng the number of groups counted either over the entire solar disk (total), the North hemisphere or South hemisphere (based on the sunspot group heliographic latitude).

The North and South numbers are always normalized to the total number, which is the global scaling reference. The production of the hemispheric numbers together with the international total Sunspot Number started only in 1992.

New scale:
The conventional 0.6 Zürich scale factor is not used anymore and A. Wolfer (Wolf's successor) is now defining the scale of the entire series. This puts the Sunspot Number at the scale of raw modern counts, instead of reducing it to the level of early counts by R. Wolf.

Error values:
Those values correspond to the standard deviation of raw numbers provided by all stations. The error value for the total number is the actual standard deviation of the sample of raw observations used to compute the daily value.
As the actual standard deviations of raw daily hemispheric counts were not archived in the past, we derive an estimate of these standard deviations with the help of the same auto-regressive model based on the Poissonian distribution of actual Sunspot Numbers.


--------------------------------------------------------
Monthly mean North-South sunspot numbers [1/1992 - now]:
--------------------------------------------------------
Old files: monssnns.dat
replaced by: 
http://www.sidc.be/silso/DATA/SN_m_hem_V2.0.txt
Contents:
Column 1-2: Gregorian calendar date
- Year
- Month
Column 3: Date in fraction of year for the middle of the corresponding month
Column 4: Monthly mean total sunspot number.
Column 5: North monthly mean sunspot number.
Column 6: South monthly mean sunspot number.
Column 7: Monthly mean standard deviation of total sunspot number data
Column 8: Monthly mean standard deviation of North sunspot number data
Column 9: Monthly mean standard deviation of South sunspot number data
Column 10: Number of observations used to compute the monthly mean total sunspot number.
Column 11: Number of observations for the monthly mean North sunspot number (not used yet)
Column 12: Number of observations for the monthly mean South sunspot number (not used yet)
Column 13: Definitive/provisional marker. A blank indicates that the monthly mean values are definitive. A '*' symbol indicates that the monthly values are still provisional and are subject to a possible revision (Usually the last 3 to 6 months)

Line format [character position]:
- [1-4] Year
- [6-7] Month
- [9-16] Decimal date
- [19-23] Monthly total sunspot number
- [25-29] Monthly north sunspot number
- [31-35] Monthly south sunspot number
- [38-42] Standard deviation (Total)
- [44-48] Standard deviation (North)
- [50-54] Standard deviation (South)
- [57-60] Number of observations (Total)
- [62-65] Number of observations (North)
- [67-70] Number of observations (South)
- [72] Definitive/provisional indicator

Data description:
Monthly smoothed mean total and North/South sunspot numbers. The 13-month smoothed monthly sunspot numbers are derived by a "tapered-boxcar" running mean of monthly hemispheric sunspot numbers over 13 months centered on the corresponding month (Smoothing function: equal weights = 1, except for first and last elements (-6 and +6 months) = 0.5, Normalization by 1/12 factor). This is the same smoothing as the standard smoothing applied to the total monthly sunspot numbers. There are no smoothed values for the first 6 months and last 6 months of the file: columns 4, 5 and 6 are set to -1 (no data).

Choice of smoothing:
This 13-month smoothed series is provided only for backward compatibility with a large number of past publications and methods resting on this smoothed series. It has thus become a base standard (e.g. for the conventional definition of the times of minima and maxima of solar cycles).

However, a wide range of other smoothing functions can be used, often with better low-pass filtering and anti-aliasing properties. As the optimal filter choice depends on the application, we thus invite users to start from the monthly mean Sunspot Numbers and apply the smoothing function that is most appropriate for their analyses. The classical smoothed series included here should only be used when direct comparisons with past published analyses must be made.

Error values:
For total Sunspot Numbers, the standard deviations in this files are obtained from the weighted mean of the variances of the 13 months in the running mean value:
sigma(ms)=sqrt(SUM(weigth(M)*sigma(M)^2)/SUM(weight(M))
where sigma(M) is the standard deviation for a single month, weight(M) is 1 or 0.5 and M is over 13 months in this case.

The standard deviations for the hemispheric values are estimated by the weighted mean (squared) of the error on the monthly mean total sunspot number.

As successive monthly means are highly correlated, the standard error on the smoothed values can be estimated by the same formula as for a single month: sigma/sqrt(N) where sigma is the listed standard deviation and N the total number of observations in the month.
The number of observations given in column 6 is the number of observations of the corresponding (middle) month: same value SUM N(d) as in the monthly mean file.
This thus gives a smoothed mean of monthly standard deviations, i.e. with the samme low-pass filtering as the data value itself. Further autocorrelation analyses will be needed to derive a conversion of this standard deviation to a standard error of the 13-month smoothed number.


--------------------------------------------------------
13-month smoothed monthly hemispheric sunspot number [1/1992 - now]
--------------------------------------------------------
Old files: monsnns.dat
New file: 
http://www.sidc.be/silso/DATA/SN_ms_hem_V2.0.txt
Contents:
Column 1-2: Gregorian calendar date
- Year
- Month
Column 3: Date in fraction of year for the middle of the corresponding month
Column 4: Monthly smoothed total sunspot number.
Column 5: Monthly smoothed North sunspot number.
Column 6: Monthly smoothed South sunspot number.
Column 7: Monthly mean standard deviation of the raw total sunspot number data.
Column 8: Monthly mean standard deviation of the raw North sunspot number data.
Column 9: Monthly mean standard deviation of the raw South sunspot number data.
Column 10: Number of observations used to compute the corresponding monthly mean total sunspot number.
Column 11: Number of observations for the monthly mean north sunspot number (not used yet)
Column 12: Number of observations for the monthly mean south sunspot number (not used yet)
Column 13: Definitive/provisional marker. A blank indicates that the monthly mean values are definitive. A '*' symbol indicates that the monthly values are still provisional and are subject to a possible revision (Usually the last 3 to 6 months)

Line format [character position]:
- [1-4] Year
- [6-7] Month
- [9-16] Decimal date
- [19-23] Monthly smoothed total sunspot number
- [25-29] Monthly smoothed North sunspot number
- [31-35] Monthly smoothed South sunspot number
- [38-42] Standard deviation (Total)
- [44-48] Standard deviation (North)
- [50-54] Standard deviation (South)
- [57-60] Number of observations (Total)
- [62-65] Number of observations (North)
- [67-70] Number of observations (South)
- [72] Definitive/provisional indicator

Data description:
Monthly smoothed mean total and North/South sunspot numbers. The 13-month smoothed monthly sunspot numbers are derived by a "tapered-boxcar" running mean of monthly hemispheric sunspot numbers over 13 months centered on the corresponding month (Smoothing function: equal weights = 1, except for first and last elements (-6 and +6 months) = 0.5, Normalization by 1/12 factor). This is the same smoothing as the standard smoothing applied to the total monthly sunspot numbers. There are no smoothed values for the first 6 months and last 6 months of the file: columns 4, 5 and 6 are set to -1 (no data).

Choice of smoothing:
This 13-month smoothed series is provided only for backward compatibility with a large number of past publications and methods resting on this smoothed series. It has thus become a base standard (e.g. for the conventional definition of the times of minima and maxima of solar cycles).

However, a wide range of other smoothing functions can be used, often with better low-pass filtering and anti-aliasing properties. As the optimal filter choice depends on the application, we thus invite users to start from the monthly mean Sunspot Numbers and apply the smoothing function that is most appropriate for their analyses. The classical smoothed series included here should only be used when direct comparisons with past published analyses must be made.

Error values:
For total Sunspot Numbers, the standard deviations in this files are obtained from the weighted mean of the variances of the 13 months in the running mean value:
sigma(ms)=sqrt(SUM(weigth(M)*sigma(M)^2)/SUM(weight(M))
where sigma(M) is the standard deviation for a single month, weight(M) is 1 or 0.5 and M is over 13 months in this case.

The standard deviations for the hemispheric values are estimated by the weighted mean (squared) of the error on the monthly mean total sunspot number.

As successive monthly means are highly correlated, the standard error on the smoothed values can be estimated by the same formula as for a single month: sigma/sqrt(N) where sigma is the listed standard deviation and N the total number of observations in the month.
The number of observations given in column 6 is the number of observations of the corresponding (middle) month: same value SUM N(d) as in the monthly mean file.
This thus gives a smoothed mean of monthly standard deviations, i.e. with the samme low-pass filtering as the data value itself. Further autocorrelation analyses will be needed to derive a conversion of this standard deviation to a standard error of the 13-month smoothed number.

========================================================
Daily Estimated Sunspot Number
========================================================
http://www.sidc.be/silso/DATA/EISN/EISN_current.txt
Looks like the last month is here, this is probably to download every day.
Contents:
Column 1: Gregorian Year 
Column 2: Gregorian Month
Column 3: Gregorian Day
Column 4: Decimal date
Column 5: Estimated Sunspot Number
Column 6: Estimated Standard Deviation
Column 7: Number of Stations calculated
Column 8: Number of Stations available

Line format [character position]:
 - [1-4]   Year
 - [6-8]   Month
 - [9-10]   Day
 - [12-19] Decimal date
 - [21-23] Estimated Sunspot Number
 - [25-29] Estimated Standard Deviation
 - [31-33] Number of Stations calculated
 - [35-37] Number of Stations available 

The estimated international sunspot number (EISN) is a daily value obtained by a simple average over available sunspot counts from prompt stations in the SILSO network.
The raw values from each station are scaled using their mean annual k personal coefficient over the last elapsed year. Therefore, compared to the monthly international sunspot number (produced on the first day of each month), the accuracy of the EISN is lower because the calculation rests on a smaller number of stations and the k scaling coefficient is only an approximation of the true k coefficient of the month.
The EISN is computed every few minutes and thus evolves continuously as new observations from our worldwide network are entered into our database. 
The EISN thus gives a dynamical preview of the final sunspot number but is an ephemeral product. It should not be archived for long-term use. At the end of each month, those values are dropped and replaced by the provisional sunspot numbers from the full calculation for the corresponding month. 
