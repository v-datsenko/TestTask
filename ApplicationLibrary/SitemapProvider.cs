using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;

namespace ApplicationLibrary
{
    public class SitemapProvider
    {
        private Uri _uri;

        private List<string> _urlList;
        private Queue<string> _sitemapsQueue;

        public SitemapProvider(Uri uri)
        {
            _uri = uri;
            _urlList = new List<string>();
            _sitemapsQueue = new Queue<string>();
        }

        public List<string> GetSitemapFromSite()
        {
            try
            {
                string sitemapURL = "https://" + _uri.Host + "/sitemap.xml";

                WebClient wc = new WebClient();

                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");

                string sitemapString = wc.DownloadString(sitemapURL);

                XmlDocument urldoc = new XmlDocument();

                urldoc.LoadXml(sitemapString);
                var rootElement = urldoc.DocumentElement;
                if (rootElement.Name == "sitemapindex")
                {
                    XmlNodeList xmlSitemapListSitemap = urldoc.GetElementsByTagName("sitemap");
                    foreach (XmlNode sitemap in xmlSitemapListSitemap)
                        _sitemapsQueue.Enqueue(sitemap["loc"].InnerText);
                }
                else if (rootElement.Name == "urlset")
                {
                    XmlNodeList xmlSitemapList = urldoc.GetElementsByTagName("url");

                    foreach (XmlNode node in xmlSitemapList)
                    {
                        if (node["loc"] != null)
                        {
                            _urlList.Add(node["loc"].InnerText);
                        }
                    }
                }

                while (_sitemapsQueue.Count != 0)
                {
                    sitemapString = wc.DownloadString(_sitemapsQueue.Dequeue());

                    urldoc = new XmlDocument();

                    urldoc.LoadXml(sitemapString);
                    rootElement = urldoc.DocumentElement;
                    if (rootElement.Name == "sitemapindex")
                    {
                        XmlNodeList xmlSitemapListSitemap = urldoc.GetElementsByTagName("sitemap");
                        foreach (XmlNode sitemap in xmlSitemapListSitemap)
                            _sitemapsQueue.Enqueue(sitemap["loc"].InnerText);
                    }
                    else if (rootElement.Name == "urlset")
                    {
                        XmlNodeList xmlSitemapList = urldoc.GetElementsByTagName("url");

                        foreach (XmlNode node in xmlSitemapList)
                        {
                            if (node["loc"] != null)
                            {
                                _urlList.Add(node["loc"].InnerText);
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("\nSitemap could not be retrieved!");
            }
            return _urlList;
        }
    }
}
