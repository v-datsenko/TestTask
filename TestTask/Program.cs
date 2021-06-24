using ApplicationLibrary;
using System;

namespace TestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter site url!");
            string url = Console.ReadLine();
            var crawler = new Crawler(url);
            crawler.StartCrawlerAsync();
            
            Console.ReadKey();
        }
    }
}
