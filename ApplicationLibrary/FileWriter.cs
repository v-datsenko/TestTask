using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ApplicationLibrary
{
    public class FileWriter
    {
        public static int TableWidth { get; set; } = 73;
        public static Func<string, int, string> AlignFunc = AlignCentre;

        private string _path;
        private FileStream fstream;

        public FileWriter(string path,string nameFile)
        {
            _path = path;
            FileInfo fileInf = new FileInfo($"{path}\\{nameFile}");
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }
            fstream = new FileStream($"{path}\\{nameFile}", FileMode.OpenOrCreate);
        }
        public void WriteList(string header, IEnumerable<string> list)
        {
            StringBuilder sb = new StringBuilder($"{header}\n");
            int i = 1;
            foreach (var str in list)
            {
                sb.Append($"{i}) {str}\n");
                i++;
            }
            byte[] array = System.Text.Encoding.Default.GetBytes(sb.ToString());
            fstream.Write(array, 0, array.Length);
        }

        public void WriteListInTable(string title, List<string> headers, params List<string>[] listOfList)
        {
            byte[] bytes = System.Text.Encoding.Default.GetBytes($"{title}\n");
            fstream.Write(bytes, 0, bytes.Length);
            WriteTableLine();
            WriteRow(0, headers.ToArray());
            WriteTableLine();

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
                WriteRow(i + 1, array[i].ToArray());
            }
            WriteTableLine();
        }
        public void WriteLine(string str)
        {
            byte[] array = System.Text.Encoding.Default.GetBytes(str + "\n");
            fstream.Write(array, 0, array.Length);
        }

        public void WriteTableLine()
        {
            byte[] array = System.Text.Encoding.Default.GetBytes(" " + new string('-', TableWidth - 1) + "\n");
            fstream.Write(array, 0, array.Length);
        }

        public void WriteRow(int numberLine, params string[] columns)
        {
            int width = (TableWidth - columns.Length) / columns.Length;
            StringBuilder sbrow = new StringBuilder("|");

            foreach (var (column, index) in columns.Select((value, i) => (value, i)))
            {
                sbrow.Append(numberLine != 0 && index == 0 ? 
                    AlignFunc($"{numberLine}) {column}", width) + "|" 
                    : AlignFunc(column, width) + "|");
                if (index == columns.Length - 1)
                    sbrow.Append("\n");
            }
            byte[] array = System.Text.Encoding.Default.GetBytes(sbrow.ToString());
            fstream.Write(array, 0, array.Length);
        }
        public void CloseFile()
        {
            fstream.Close();
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
