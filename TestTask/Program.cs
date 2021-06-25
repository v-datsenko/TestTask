using ApplicationLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestTask
{
    class Program
    {
        static int tableWidth = 73;
        static void Main(string[] args)
        {
            Console.WriteLine("Enter site url!");
            string url = Console.ReadLine();
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            var crawler = new Crawler(url, token);

            Task scan = crawler.StartCrawlerAsync();
            Thread consoleRead = new Thread(() =>
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                while (key.KeyChar != 'c')
                {
                    key = Console.ReadKey();
                }
                cancelTokenSource.Cancel();
            });
            consoleRead.Start();
            scan.Wait();

            List<string> foundLinks = crawler.FinalUriList.Select(u => u.ToString()).ToList();
            DisplayList("\nFound available links: ", foundLinks);

            var sitemapProvider = new SitemapProvider(crawler.Uri);
            List<string> sitemapList = sitemapProvider.GetSitemapFromSite();

            if (sitemapList.Count != 0)
            {
                DisplayList("\nAll links from sitemap.xml:", sitemapList);
            }
            else
            {
                Console.WriteLine("Sitemap could not be retrieved!");
            }

            List<string> allLinks = foundLinks.Union(sitemapList).Distinct().ToList();

            DisplayListInTable(new List<string> { "Url"}, new List<string>[] { allLinks });

            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
        }

        public static void DisplayList(string header,IEnumerable<string> list)
        {
            Console.WriteLine(header);
            int i = 1;
            foreach (var str in list)
            {
                Console.WriteLine(i + str);
                i++;
            }
        }

        public static void DisplayListInTable(List<string> headers, params List<string>[] listOfList)
        {
            PrintLine();
            PrintRow(0,headers.ToArray());
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
                PrintRow(i + 1,array[i].ToArray());
            }
            PrintLine();
        }

        static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        static void PrintRow(int numberLine,params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                
                row += numberLine == 0 ? AlignCentre(column, width) + "|" : AlignCentre($"{numberLine}) {column}", width) + "|";
            }

            Console.WriteLine(row);
        }

        static string AlignCentre(string text, int width)
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
    }
}
