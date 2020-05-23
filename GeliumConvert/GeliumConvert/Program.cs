using GeliumConvert.Properties;
using Mbh5;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
namespace GeliumConvert
{
	internal class Program
	{
		private static int addHours = Settings.Default.AddHours;
		private static Dictionary<string, OhlcvPriceOnlyData> dataDictionary = new Dictionary<string, OhlcvPriceOnlyData>();
		private static Dictionary<string, Instrument> instrumentDictionary = new Dictionary<string, Instrument>();
		private static Dictionary<string, SortedList<DateTime, OhlcvPriceOnly>> listDictionary = new Dictionary<string, SortedList<DateTime, OhlcvPriceOnly>>();
		private static void TraverseTree(string root, Action<string> action)
		{
			if (Directory.Exists(root))
			{
				string[] array = Directory.GetFiles(root);
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string obj = array2[i];
					action(obj);
				}
				array = Directory.GetDirectories(root);
				string[] array3 = array;
				for (int j = 0; j < array3.Length; j++)
				{
					string root2 = array3[j];
					Program.TraverseTree(root2, action);
				}
				return;
			}
			if (File.Exists(root))
			{
				action(root);
			}
		}
		private static DateTime StringToDateTime(string input)
		{
			int year = int.Parse(input.Substring(0, 4), CultureInfo.InvariantCulture);
			int month = int.Parse(input.Substring(5, 2), CultureInfo.InvariantCulture);
			int day = int.Parse(input.Substring(8, 2), CultureInfo.InvariantCulture);
			int hour = int.Parse(input.Substring(11, 2), CultureInfo.InvariantCulture);
			int minute = int.Parse(input.Substring(14, 2), CultureInfo.InvariantCulture);
			DateTime result = new DateTime(year, month, day, hour, minute, 0);
			if (Program.addHours != 0)
			{
				result = result.AddHours((double)Program.addHours);
			}
			return result;
		}
		private static void Collect(string sourceFileName)
		{
			Trace.TraceInformation(string.Format("Collecting [{0}]", sourceFileName));
			DateTime dateTime = new DateTime(0L);
			FileInfo fileInfo = new FileInfo(sourceFileName);
			string[] array = fileInfo.Name.Split(new char[]
			{
				'_'
			});
			string text = array[0];
			SortedList<DateTime, OhlcvPriceOnly> sortedList;
			if (Program.listDictionary.ContainsKey(text))
			{
				sortedList = Program.listDictionary[text];
			}
			else
			{
				sortedList = new SortedList<DateTime, OhlcvPriceOnly>(1024);
				Program.listDictionary.Add(text, sortedList);
			}
			OhlcvPriceOnly ohlcvPriceOnly = default(OhlcvPriceOnly);
			OhlcvPriceOnly ohlcvPriceOnly2 = default(OhlcvPriceOnly);
			int num = 0;
			using (StreamReader streamReader = new StreamReader(sourceFileName))
			{
				while ((text = streamReader.ReadLine()) != null)
				{
					dateTime = Program.StringToDateTime(text);
					array = text.Split(new char[]
					{
						','
					});
					ohlcvPriceOnly.open = double.Parse(array[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
					ohlcvPriceOnly.high = double.Parse(array[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
					ohlcvPriceOnly.low = double.Parse(array[4].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
					ohlcvPriceOnly.close = double.Parse(array[5].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat);
					ohlcvPriceOnly.dateTimeTicks = dateTime.Ticks;
					if (sortedList.ContainsKey(dateTime))
					{
						ohlcvPriceOnly2 = sortedList[dateTime];
						if (ohlcvPriceOnly.open != ohlcvPriceOnly2.open || ohlcvPriceOnly.high != ohlcvPriceOnly2.high || ohlcvPriceOnly.low != ohlcvPriceOnly2.low || ohlcvPriceOnly.close != ohlcvPriceOnly2.close)
						{
							if (Settings.Default.UpdateExisting)
							{
								Trace.TraceInformation("Ohlc: the date/time [{0}] already exists (existing [{1}] new [{2}]), updating", new object[]
								{
									dateTime, 
									ohlcvPriceOnly2, 
									ohlcvPriceOnly
								});
								sortedList[dateTime] = ohlcvPriceOnly;
							}
							else
							{
								Trace.TraceError("Ohlc: the date/time [{0}] already exists (existing [{1}] new [{2}]), skipping", new object[]
								{
									dateTime, 
									ohlcvPriceOnly2, 
									ohlcvPriceOnly
								});
							}
						}
					}
					else
					{
						sortedList.Add(dateTime, ohlcvPriceOnly);
					}
					num++;
				}
			}
		}
		private static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("Argument: dir_or_file_name");
				return;
			}
			Repository repository = null;
			List<OhlcvPriceOnly> list = new List<OhlcvPriceOnly>(1024);
			Instrument instrument = null;
			OhlcvPriceOnlyData ohlcvPriceOnlyData = null;
			Repository.InterceptErrorStack();
			Data.set_DefaultMaximumReadBufferBytes((long)Settings.Default.Hdf5MaxReadBufferBytes);
			Trace.TraceInformation("=======================================================================================");
			Trace.TraceInformation("Started [{0}]: {1}", new object[]
			{
				args[0], 
				DateTime.Now
			});
			try
			{
				Trace.TraceInformation("Traversing...");
				Program.TraverseTree(args[0], delegate(string s)
				{
					Program.Collect(s);
				}
				);
				Trace.TraceInformation("Merging...");
				foreach (string current in Program.listDictionary.Keys)
				{
					string text = Settings.Default.RepositoryDir + current + ".h5";
					Trace.TraceInformation("Merging to [{0}]", new object[]
					{
						text
					});
					FileInfo fileInfo = new FileInfo(text);
					if (!Directory.Exists(fileInfo.DirectoryName))
					{
						Directory.CreateDirectory(fileInfo.DirectoryName);
					}
					repository = Repository.OpenReadWrite(text, true, Settings.Default.Hdf5CorkTheCache);
					SortedList<DateTime, OhlcvPriceOnly> sortedList = Program.listDictionary[current];
					if (sortedList.Count > 0)
					{
						instrument = repository.Open(Settings.Default.InstrumentPath + current, true);
						ohlcvPriceOnlyData = instrument.OpenOhlcvPriceOnly(0, 513, true);
						list.Clear();
						foreach (DateTime current2 in sortedList.Keys)
						{
							list.Add(sortedList[current2]);
						}
						ohlcvPriceOnlyData.Add(list, 1, true);
						ohlcvPriceOnlyData.Flush();
						ohlcvPriceOnlyData.Close();
						instrument.Close();
					}
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError("Exception: [{0}]", new object[]
				{
					ex.Message
				});
			}
			finally
			{
				foreach (KeyValuePair<string, OhlcvPriceOnlyData> current3 in Program.dataDictionary)
				{
					OhlcvPriceOnlyData value = current3.Value;
					value.Flush();
					value.Close();
				}
				foreach (KeyValuePair<string, Instrument> current4 in Program.instrumentDictionary)
				{
					current4.Value.Close();
				}
				if (repository != null)
				{
					repository.Close();
				}
			}
			Trace.TraceInformation("Finished: {0}", new object[]
			{
				DateTime.Now
			});
		}
	}
}
