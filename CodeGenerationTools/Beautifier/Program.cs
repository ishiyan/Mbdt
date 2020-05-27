using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Beautifier
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Arguments: input file");
                return;
            }
            string[] input = File.ReadAllLines(args[0]);
            for (int i = 0; i < input.Length; ++i)
                input[i] = input[i].Replace(',', '.');
            const int rowLength = 10;
            int length = input.Length, length1 = length - 1;
            int rowCount = length / rowLength;
            int lastLength = length % rowLength;
            var output = new List<string>(rowCount + 1);
            var columnLength = new int[rowLength];
            int line = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                for (int j = 0; j < rowLength; ++j, ++line)
                {
                    int len = input[line].Length;
                    if (input[line][0] != '-')
                        ++len;
                    if (columnLength[j] < len)
                        columnLength[j] = len;
                }
            }
            var sb = new StringBuilder(1024);
            line = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                sb.Clear();
                for (int j = 0; j < rowLength; ++j, ++line)
                {
                    string l = input[line];
                    if (line != length1)
                    {
                        if (l[0] != '-')
                            l = " " + l;
                        l += ",";
                        if (j != rowLength - 1)
                        {
                            l += " ";
                            for (int k = l.Length - 1; k <= columnLength[j]; ++k)
                                l += " ";
                        }
                        sb.Append(l);
                    }
                }
                output.Add("           " + sb);
            }
            if (lastLength > 0)
            {
                sb.Clear();
                for (int j = 0; line < length; ++line, ++j)
                {
                    string l = input[line];
                    if (line != length1)
                    {
                        if (l[0] != '-')
                            l = " " + l;
                        l += ",";
                        if (j != rowLength - 1)
                        {
                            l += " ";
                            for (int k = l.Length - 1; k <= columnLength[j]; ++k)
                                l += " ";
                        }
                        sb.Append(l);
                    }
                    sb.Append(l);
                    if (line != length1)
                        sb.Append(", ");
                }
                output.Add("           " + sb);
            }
            foreach (var s in output)
                Console.WriteLine(s);
        }
    }
}
