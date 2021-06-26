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
        static void Main(string[] args)
        {
            try
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
                ConsoleRenderer.DisplayList("\nFound available links: ", foundLinks);

                var sitemapProvider = new SitemapProvider(crawler.Uri);
                List<string> sitemapList = sitemapProvider.GetSitemapFromSite();

                bool sitemap = false;
                List<string> allLinks;
                if (sitemapList.Count != 0)
                {
                    ConsoleRenderer.DisplayList("\nAll links from sitemap.xml:", sitemapList);
                    sitemap = true;
                    allLinks = foundLinks.Union(sitemapList).Distinct().ToList();
                }
                else
                {
                    allLinks = foundLinks;
                }


                if (sitemap)
                {
                    List<string> displayedList = sitemapList.Except(foundLinks, new UrlComparer()).ToList();
                    ConsoleRenderer.TableWidth = 90;
                    ConsoleRenderer.AlignFunc = ConsoleRenderer.AlignLeft;
                    ConsoleRenderer.DisplayListInTable("\nUrls FOUNDED IN SITEMAP.XML but not founded after crawling a web site", new List<string> { "Url" }, new List<string>[] { displayedList });

                    displayedList = foundLinks.Except(sitemapList, new UrlComparer()).ToList();
                    ConsoleRenderer.DisplayListInTable("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml", new List<string> { "Url" }, new List<string>[] { displayedList });
                }

                Console.WriteLine("Press any key to exit!");
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
