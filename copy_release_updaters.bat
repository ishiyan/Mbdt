@echo off

rem please no spaces in dst
set dst=last_updaters

:: ----------------------------------------

if exist "%dst%" rmdir /S /Q "%dst%")
mkdir "%dst%"

:: ----------------------------------------

copy /Y "Mbh5_64\Mbh5.dll"         "%dst%"
copy /Y "Mbh5_64\vcruntime140.dll" "%dst%"

:: ----------------------------------------

:: call copy_single.bat AscAceUpdate                            %dst%
:: call copy_single.bat BeursBox                                %dst%
call copy_single.bat CmeFutureUpdate                         %dst%
:: call copy_single.bat ConvertInstrumentFilesToH5              %dst%
:: call copy_single.bat ConvertSecurityFiles                    %dst%
call copy_single.bat DukascopyFxUpdate                       %dst%
:: call copy_single.bat DukascopyUpdate                         %dst%
call copy_single.bat EcbDailyUpdate                          %dst%
:: call copy_single.bat EoniaConvert                            %dst%
:: call copy_single.bat EoniaswapConvert                        %dst%
:: call copy_single.bat EoniaswapUpdate                         %dst%
call copy_single.bat EoniaUpdate                             %dst%
:: call copy_single.bat EurepoConvert                           %dst%
call copy_single.bat EurepoUpdate                            %dst%
:: call copy_single.bat EuriborConvert                          %dst%
call copy_single.bat EuriborUpdate                           %dst%
:: call copy_single.bat EuronextAudit                           %dst%
:: call copy_single.bat EuronextBigConverter                    %dst%
call copy_single.bat EuronextCollectIllegalTimestamps        %dst%
call copy_single.bat EuronextDiscover                        %dst%
call copy_single.bat EuronextEnrich                          %dst%
call copy_single.bat EuronextHistoryRepair                   %dst%
call copy_single.bat EuronextHistoryUpdate                   %dst%
call copy_single.bat EuronextHistoryUpdateCollectAdjustments %dst%
:: call copy_single.bat EuronextInstrumentIndexConverter        %dst%
:: call copy_single.bat EuronextIntradayJoin                    %dst%
:: call copy_single.bat EuronextIntradaySplit                   %dst%
call copy_single.bat EuronextIntradayUpdate                  %dst%
:: call copy_single.bat EuronextJsonExport                      %dst%
:: call copy_single.bat FibbsDownload                           %dst%
:: call copy_single.bat Fix15sec                                %dst%
call copy_single.bat GaincapitalFxUpdate                     %dst%
:: call copy_single.bat GeliumConvert                           %dst%
:: call copy_single.bat ImportCsv                               %dst%
:: call copy_single.bat InstrumentFileAuditor                   %dst%
:: call copy_single.bat InstrumentFileXml2h5                    %dst%
:: call copy_single.bat InstrumentFileXmlStatistics             %dst%
:: call copy_single.bat InstrumentIndexAuditor                  %dst%
:: call copy_single.bat Knap                                    %dst%
:: call copy_single.bat Knip                                    %dst%
:: call copy_single.bat KnipCsv                                 %dst%
:: call copy_single.bat LiborConvert                            %dst%
call copy_single.bat LiborFred2Update                        %dst%
call copy_single.bat LiborUpdate                             %dst%
:: call copy_single.bat LiffeUpdate                             %dst%
:: call copy_single.bat NedkoersDownload                        %dst%
call copy_single.bat NgdcAastarUpdate                        %dst%
call copy_single.bat NgdcApstarUpdate                        %dst%
call copy_single.bat NgdcDstUpdate                           %dst%
call copy_single.bat NgdcKpApUpdate                          %dst%
:: call copy_single.bat NyseUpdate                              %dst%
:: call copy_single.bat ObvionConvert                           %dst%
call copy_single.bat RenameLog                               %dst%
:: call copy_single.bat SidcSsnConvert                          %dst%
call copy_single.bat SidcSsnUpdate                           %dst%
:: call copy_single.bat SohoConvert                             %dst%
call copy_single.bat SohoUpdate                              %dst%
call copy_single.bat SorceTsiUpdate                          %dst%
:: call copy_single.bat SwpcAceConvert                          %dst%
call copy_single.bat SwpcAceUpdate                           %dst%
:: call copy_single.bat SwpcDxdConvert                          %dst%
call copy_single.bat SwpcDxdUpdate                           %dst%
:: call copy_single.bat SwpcGoesConvert                         %dst%
call copy_single.bat SwpcGoesUpdate                          %dst%

:: ----------------------------------------
