using System;
using System.Collections.Generic;

namespace ApplicationLibrary.Network
{
    public class UrlComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            
            return GetUrl(x) == GetUrl(y);
        }

        public int GetHashCode(string str)
        {
            return GetUrl(str).GetHashCode();
        }

        public string GetUrl(string str)
        {
            var index = str.IndexOf("://");
            if (index == -1)
                return str;
            return str.Substring(index + 3);
        }
    }
}
