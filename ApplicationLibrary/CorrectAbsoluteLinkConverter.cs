using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApplicationLibrary
{
    public class CorrectAbsoluteLinkConverter
    {
        public Uri MainUri { get; set; }
        public CorrectAbsoluteLinkConverter(string link)
        {
            Uri uri;
            if(!CheckAndCreateCorrectUri(link,out uri))
            {
                if (link.StartsWith("//"))
                {
                    link = "https:" + link;
                    uri = new Uri(link);
                }
                else
                {
                    throw new ArgumentException("Incorrect url!");
                }
            }
            MainUri = uri;
        }
        public List<Uri> CheckAndConvertList(IEnumerable<string> input)
        {
            var result = input.Select(link =>
            {
                Uri uri;
                if (CheckAndCreateCorrectUri(link, out uri))
                {
                    return uri;
                }
                else
                {
                    return CreateCorrectUri(link);
                }
            })
                .ToList();
            return result;
        }
        private bool CheckAndCreateCorrectUri(string link, out Uri uri)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(link, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            uri = uriResult;
            return result;
        }
        private Uri CreateCorrectUri(string url)
        {
            if (url.StartsWith("//"))
                url = (MainUri.Scheme == Uri.UriSchemeHttp ? "http:" : "https:") + url;
            else if (url.StartsWith("/"))
                url = (MainUri.Scheme == Uri.UriSchemeHttp ? "http://" : "https://") + MainUri.Host + url;
            else
                url = (MainUri.Scheme == Uri.UriSchemeHttp ? "http://" : "https://") + MainUri.Host + "/" + url;
            return new Uri(url);
        }
    }
}
