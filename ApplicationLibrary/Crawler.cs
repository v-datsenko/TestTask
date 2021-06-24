using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationLibrary
{
    public class Crawler
    {
        private readonly Uri _uri;
        private IEnumerable<IUriFilter> _filters;
        public ObservableCollection<Uri> FinalUriList;

        public Uri Uri { get { return _uri; } }
        private Queue<Uri> _queueUri;
        private CorrectAbsoluteLinkConverter _correctAbsoluteLinkConverter;
        private Random _random;
        private CancellationToken _token;

        public Crawler(string url, CancellationToken token) 
            : this (url, token, Enumerable.Empty<IUriFilter>().ToArray())
        {
        }

        public Crawler(string url, CancellationToken token, params IUriFilter[] filters)
        {
            _correctAbsoluteLinkConverter = new CorrectAbsoluteLinkConverter(url);
            _uri = _correctAbsoluteLinkConverter.MainUri;
            _filters = filters.Length == 0 ? 
                new IUriFilter[] { new ExcludeRootUriFilter(_uri), new ExternalUriFilter(_uri), new AlreadyVisitedUriFilter() } 
                : filters;
            FinalUriList = new ObservableCollection<Uri>();
            FinalUriList.CollectionChanged += Uri_CollectionChanged;
            _queueUri = new Queue<Uri>();
            _random = new Random();
            _token = token;
        }
        public async Task StartCrawlerAsync()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");

            _queueUri.Enqueue(_uri);
            while (_queueUri.Count != 0) {
                if (_token.IsCancellationRequested)
                {
                    Console.Clear();
                    Console.WriteLine("Operation aborted!");
                    Console.WriteLine("All pages:");
                    return;
                }
                var currentUri = _queueUri.Dequeue();
                try
                {
                    var html = await httpClient.GetStringAsync(currentUri);

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    var links = htmlDocument.DocumentNode.Descendants("a").Where(a => a.GetAttributeValue("href", null) != null).Select(node => node.GetAttributeValue("href", "")).ToList();

                    var uriList = _correctAbsoluteLinkConverter.CheckAndConvertList(links);
                    uriList = Filter(uriList, _filters.ToArray());
                    foreach (var uri in uriList)
                    {
                        _queueUri.Enqueue(uri);
                    }
                    FinalUriList.Add(currentUri);
                }catch(HttpRequestException ex)
                {
                    
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                await Task.Delay(1000);
            }
            Console.WriteLine("It's all!");
        }

        private void Uri_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Console.Clear();
            Console.WriteLine($"Total pages scanned: {FinalUriList.Count}");
            Console.WriteLine($"Total links in queue for crawling: {_queueUri.Count}");
            Console.WriteLine($"Scanning" + _random.Next(0, 3) switch { 0 => ".", 1 => "..", 2 => "...", _ => "...." });
            Console.WriteLine("Press c key to stop scanning");
        }
        private static List<Uri> Filter(IEnumerable<Uri> uris, params IUriFilter[] filters)
        {
            var filtered = uris.ToList();
            foreach (var filter in filters.ToList())
            {
                filtered = filter.Filter(filtered);
            }
            return filtered;
        }
    }
}
