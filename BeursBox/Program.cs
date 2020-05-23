using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Globalization;

namespace BeursBox
{
    class Program
    {
        private class NewsItem
        {
            internal DateTime DateTime;
            internal string Headline;
            internal string Content;
            internal bool IsGood
            {
                get
                {
                    if (2 != (DateTime.Year / 1000))
                        return false;
                    if (string.IsNullOrEmpty(Headline) || string.IsNullOrEmpty(Content))
                        return false;
                    return true;
                }
            }
            internal string Dump
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("[{0}-{1}-{2}:", DateTime.Year, DateTime.Month, DateTime.Day);
                    sb.AppendFormat(" ({0})", Headline);
                    sb.AppendFormat(" {0}]", Content);
                    return sb.ToString();
                }
            }
        }

        private class NewsList : List<NewsItem>
        {
            internal string Symbol;
            internal NewsList() : base(128) {}

            internal void Fetch(string symbol, string url)
            {
                Symbol = symbol;
                base.Clear();
                const string errorFormat = "unexpected line [{0}] failed to find [{1}], aborting";
                Debug.WriteLine("Downloading URL " + url);
                WebRequest webRequest = HttpWebRequest.Create(url);
                webRequest.Proxy = WebRequest.DefaultWebProxy;
                // DefaultCredentials represents the system credentials for the current
                // security context in which the application is running. For a client-side
                // application, these are usually the Windows credentials
                // (user name, password, and domain) of the user running the application.
                webRequest.Proxy.Credentials = CredentialCache.DefaultCredentials;
                webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                webRequest.Timeout = 240000;
                using (var streamReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    const string pattern1 = "<p><font color=\"#000000\">"; // <p><font color="#000000">21 apr 2009</font></p>
                    const int pattern1Len = 25;
                    const string pattern2 = "</font></p>";
                    const string pattern3 = "<";
                    int i; bool ending;
                    string s, line = streamReader.ReadLine();
                    NewsItem newsItem;
                    while (null != line)
                    {
                        i = line.IndexOf("Speculatie tips (gratis)", StringComparison.Ordinal);
                        if (-1 < i)
                            break;
                        i = line.IndexOf(pattern1, StringComparison.Ordinal);
                        if (-1 < i)
                        {
                            Debug.WriteLine(">" + line); // <p><font color="#000000">21 apr 2009</font></p>
                            s = line.Substring(i + pattern1Len);
                            i = s.IndexOf(pattern2, StringComparison.Ordinal);
                            ending = false;
                            if (0 > i)
                            {
                                ending = true;
                                i = s.IndexOf(pattern3, StringComparison.Ordinal);
                                if (0 > i)
                                {
                                    Trace.TraceError(errorFormat, line, pattern2);
                                    i = s.Length - 1;
                                }
                            }
                            if (ending)
                            {
                                line = streamReader.ReadLine();
                                int j = line.IndexOf("Speculatie tips (gratis)", StringComparison.Ordinal);
                                if (-1 < j)
                                    return;
                            }
                            s = s.Substring(0, i);
                            i = s.IndexOf("(", StringComparison.Ordinal);
                            if (-1 < i)
                                s = s.Substring(0, i);
                            s = s.Replace("sept", "sep");
                            s = s.Replace("juli", "jul");
                            s = s.Replace("juni", "jun");
                            s = s.Trim();
                            Debug.WriteLine(">>>" + s);
                            //if ("21 nov 2008" == s)
                            //    ending = false;
                            newsItem = new NewsItem();
                            base.Insert(0, newsItem);
                            newsItem.Headline = s;
                            newsItem.DateTime = DateTime.ParseExact(s, "d MMM yyyy", CultureInfo.CreateSpecificCulture("nl-NL"));
                            newsItem.Content = "";
                            line = streamReader.ReadLine();
                            i = line.IndexOf(pattern1, StringComparison.Ordinal);
                            if (-1 < i)
                            {
                                s = line.Substring(i + pattern1Len);
                                i = s.IndexOf(pattern2, StringComparison.Ordinal);
                                if (-1 < i)
                                    line = streamReader.ReadLine();
                                else
                                {
                                    while (0 > i)
                                    {
                                        newsItem.Content += s;
                                        line = streamReader.ReadLine();
                                        s = line;
                                        i = s.IndexOf(pattern2, StringComparison.Ordinal);
                                    }
                                }
                                newsItem.Content += s.Substring(0, i);
                            }
                        }
                        else
                            line = streamReader.ReadLine();
                    }
                }
            }
        }

        private class NewsListCollection
        {
            internal List<NewsList> list = new List<NewsList>(64);
            internal NewsListCollection()
            {
            }
        }

        static void Main(string[] args)
        {
            NewsList list1 = new NewsList();
            list1.Fetch("AEX", "http://www.beursbox.nl/aex-beurs-analyse.html");
            Debug.WriteLine("count = " + list1.Count);
            NewsList list2 = new NewsList();
            list2.Fetch("ASML", "http://www.beursbox.nl/asml-beurs-analyse.html");
            Debug.WriteLine("count = " + list2.Count);
        }
    }
}
