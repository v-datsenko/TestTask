using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationLibrary
{
    public static class ConsoleRenderer
    {
        public static int TableWidth { get; set; } = 73;
        public static Func<string, int, string> AlignFunc = AlignCentre;
        public static void DisplayList(string header, IEnumerable<string> list)
        {
            Console.WriteLine(header);
            int i = 1;
            foreach (var str in list)
            {
                Console.WriteLine($"{i}) {str}");
                i++;
            }
        }

        public static void DisplayListInTable(string title,List<string> headers, params List<string>[] listOfList)
        {
            Console.WriteLine(title);
            PrintLine();
            PrintRow(0, headers.ToArray());
            PrintLine();

            var array = new string[listOfList[0].Count][];
            for (int i = 0; i < listOfList[0].Count; i++)
            {
                array[i] = new string[headers.Count()];
            }
            for (int i = 0; i < listOfList[0].Count; i++)
            {
                for (int j = 0; j < headers.Count(); j++)
                {
                    array[i][j] = listOfList[j][i];
                }
            }

            for (int i = 0; i < listOfList[0].Count; i++)
            {
                PrintRow(i + 1, array[i].ToArray());
            }
            PrintLine();
        }

        public static void PrintLine()
        {
            Console.WriteLine(" " + new string('-', TableWidth-1));
        }

        public static void PrintRow(int numberLine, params string[] columns)
        {
            int width = (TableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (var (column, index) in columns.Select((value,i) => (value,i)))
            {
                row += numberLine != 0 && index != 1 ? AlignFunc($"{numberLine}) {column}", width) + "|" : AlignFunc(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        public static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }
        public static string AlignLeft(string text, int width)
        {
            text = " " + text;
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width);
            }
        }
    }
}
