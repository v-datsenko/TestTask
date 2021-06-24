using ApplicationLibrary;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestTask
{
    class Program
    {
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
            foreach(var link in crawler.FinalUriList)
            {
                Console.WriteLine(link);
            }
            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
        }
    }
}
