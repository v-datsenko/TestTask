using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;

namespace ApplicationLibrary
{
    public class UrlTestClient
    {
        private List<string> _urlList;

        public UrlTestClient(List<string> urlList)
        {
            _urlList = urlList;
        }

        public Dictionary<string, long> GetResult()
        {
            var result = new Dictionary<string, long>();
            foreach(var url in _urlList)
            {
                result.Add(url,TestUrl(url));
                Thread.Sleep(500);
            }
            return result;
        }

        private long TestUrl(string url)
        {
            long timeMilliseconds = 0;
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Credentials = CredentialCache.DefaultCredentials;

                var timer = new Stopwatch();
                timer.Start();
                WebResponse response = request.GetResponse();
                timer.Stop();
                timeMilliseconds = timer.ElapsedMilliseconds;
            }catch(Exception ex)
            {

            }
            return timeMilliseconds;
        }
    }
}
