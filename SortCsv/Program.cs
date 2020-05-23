using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Runtime.Remoting.Messaging;

namespace SortCsv
{
    static class Program
    {
        internal class Duo
        {
            public DateTime DateTime { get; set; }
            public string Line { get; set; }
        }
        static void Main(string[] args)
        {
            if (args.Length < 1)
                Console.WriteLine("Argument: file.csv");
            else
            {
                var list = new List<Duo>();
                var lines = File.ReadAllLines(args[0]);
                foreach (var line in lines)
                {
                    var splitted = line.Split(';');
                    var duo = new Duo
                    {
                        DateTime = DateTime.Parse(splitted[0]),
                        Line = line
                    };
                    list.Add(duo);
                }
                list.Sort((a,b) => DateTime.Compare(a.DateTime, b.DateTime));
                File.WriteAllLines(args[0]+".sorted", list.ConvertAll(x => x.Line));
            }
        }
    }
}
