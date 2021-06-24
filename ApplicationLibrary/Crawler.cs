using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ApplicationLibrary
{
    public class Crawler
    {
        private readonly Uri _uri;
        private IEnumerable<IUriFilter> _filters;

        private List<Uri> _finalUriList;
        private Queue<Uri> _queueUri;

        private CorrectAbsoluteLinkConverter _correctAbsoluteLinkConverter;

        public Crawler(string url) 
            : this (url, Enumerable.Empty<IUriFilter>().ToArray())
        {
        }

        public Crawler(string url, params IUriFilter[] filters)
        {
            _correctAbsoluteLinkConverter = new CorrectAbsoluteLinkConverter(url);
            _uri = _correctAbsoluteLinkConverter.MainUri;
            _filters = filters.Length == 0 ? 
                new IUriFilter[] { new ExcludeRootUriFilter(_uri), new ExternalUriFilter(_uri), new AlreadyVisitedUriFilter() } 
                : filters;
            _finalUriList = new List<Uri>();
            _queueUri = new Queue<Uri>();
        }
        public async void StartCrawlerAsync()
        {

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            
            var html = await httpClient.GetStringAsync(_uri);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            //var links = htmlDocument.DocumentNode.Descendants("a").Where(a => a != null).Select(node => new Uri(node.GetAttributeValue("href", ""))).ToList();
            var links = htmlDocument.DocumentNode.Descendants("a").Where(a => a.GetAttributeValue("href", null) != null).Select(node => node.GetAttributeValue("href", "")).ToList();

            var uriList = _correctAbsoluteLinkConverter.CheckAndConvertList(links);
            uriList = Filter(uriList, _filters.ToArray());

            foreach (var link in uriList)
            {
                Console.WriteLine(link);
            }
            Console.WriteLine("It's all!");
        }
        private bool IsValidURL(string URL)
        {
            /*string Pattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";
            Regex Rgx = new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return Rgx.IsMatch(URL);*/
            Uri uriResult;
            bool result = Uri.TryCreate(URL, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
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
