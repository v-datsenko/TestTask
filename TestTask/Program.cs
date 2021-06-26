using ApplicationLibrary.ConsoleOutput;
using ApplicationLibrary.Crawling;
using ApplicationLibrary.FileOutput;
using ApplicationLibrary.Network;
using ApplicationLibrary.Sitemap;
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
                //Получение url от пользователя
                Console.WriteLine("Enter site url!");
                string url = Console.ReadLine();

                //Создание токена для отмены поиска url на сайте
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                //Создание класса Crawler и запуск поиска url на сайте
                var crawler = new Crawler(url, token);
                Task scan = crawler.StartCrawlerAsync();

                //Создание потока для считывания нажатия клавиши пользователем для отмены поиска url
                Thread consoleRead = new Thread(() =>
                {
                    ConsoleKeyInfo key = new ConsoleKeyInfo();
                    while (key.KeyChar != 'c')
                    {
                        key = Console.ReadKey();
                    }
                    cancelTokenSource.Cancel();
                });

                //Запуск потока для считывания нажатия клавиши пользователем
                consoleRead.Start();
                //Ожидание завершения процесса поиска url на сайте
                scan.Wait();

                //Получение списка найденных url на сайте и вывод его на консоль
                List<string> foundLinks = crawler.FinalUriList.Select(u => u.ToString()).ToList();
                ConsoleRenderer.DisplayList("\nFound available links: ", foundLinks);

                //Создание объекта SitemapProvider для получения sitemap.xml сайта
                var sitemapProvider = new SitemapProvider(crawler.Uri);
                List<string> sitemapList = sitemapProvider.GetSitemapFromSite();

                //Соединение всех url
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

                //Настройка свойств класса ConsoleRenderer для вывода таблиц с данными на консоль
                ConsoleRenderer.TableWidth = Console.WindowWidth-4;
                ConsoleRenderer.AlignFunc = ConsoleRenderer.AlignLeft;

                List<string> listOnlySitemap = new List<string>();
                List<string> listOnlyFound;
                
                //Если есть sitemap то выполняется разность списков
                if (sitemap)
                {
                    listOnlySitemap = sitemapList.Except(foundLinks, new UrlComparer()).ToList();
                    ConsoleRenderer.DisplayListInTable("\nUrls FOUNDED IN SITEMAP.XML but not founded after crawling a web site", new List<string> { "Url" }, new List<string>[] { listOnlySitemap });
                }
                //Вывод таблицы url, которые не содержаться в sitemap
                listOnlyFound = foundLinks.Except(sitemapList, new UrlComparer()).ToList();
                ConsoleRenderer.DisplayListInTable("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml", new List<string> { "Url" }, new List<string>[] { listOnlyFound });

                //Получение и вывод таблицы с временем ответа от сервера
                Console.WriteLine("\nServer response time is measured...");
                UrlTestClient testClient = new UrlTestClient(allLinks);
                Dictionary<string, long> resultTest = testClient.GetResult();
                List<string> timingList = resultTest.Values.Select(t => $"{t}ms").ToList();
                ConsoleRenderer.DisplayListInTable("\nTiming", new List<string> { "Url", "Timing (ms)" }, new List<string>[] { allLinks, timingList });

                //Вывод кол-во url найденных двумя способами
                Console.WriteLine($"\nUrls(html documents) found after crawling a website: {foundLinks.Count}");
                Console.WriteLine($"Urls found in sitemap: {sitemapList.Count}");

                //Запись данных в файл
                try
                {
                    Console.WriteLine("\nData is written to file...");

                    FileWriter fw = new FileWriter("C:\\Test", "SiteInfo.txt");
                    FileWriter.AlignFunc = ConsoleRenderer.AlignLeft;

                    fw.WriteLine($"Site data '{url}'");
                    fw.WriteList("\nFound available links: ", foundLinks);

                    if (sitemap)
                    {
                        fw.WriteList("\nAll links from sitemap.xml:", sitemapList);
                        
                        FileWriter.TableWidth = listOnlySitemap.Select((str, index) => (str, index)).Max(tuple => tuple.str.Length + tuple.index.ToString().Length) + 6;
                        fw.WriteListInTable("\nUrls FOUNDED IN SITEMAP.XML but not founded after crawling a web site", new List<string> { "Url" }, new List<string>[] { listOnlySitemap });
                    }
                    FileWriter.TableWidth = listOnlyFound.Select((str, index) => (str, index)).Max(tuple => tuple.str.Length + tuple.index.ToString().Length) + 6;
                    fw.WriteListInTable("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml", new List<string> { "Url" }, new List<string>[] { listOnlyFound });
                    
                    FileWriter.TableWidth = 140;
                    fw.WriteListInTable("\nTiming", new List<string> { "Url", "Timing (ms)" }, new List<string>[] { allLinks, timingList });
                    
                    fw.WriteLine($"\nUrls(html documents) found after crawling a website: {foundLinks.Count}");
                    fw.WriteLine($"Urls found in sitemap: {sitemapList.Count}");
                    
                    fw.CloseFile();
                    Console.WriteLine("Data has been successfully written to file!");
                }
                catch(Exception ex)
                {
                    Console.WriteLine("File write failed!");
                    Console.WriteLine("Error: " + ex.Message);
                }

                Console.WriteLine("\nPress any key to exit!");
                Console.ReadKey();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
