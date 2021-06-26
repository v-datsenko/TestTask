using System;
using System.Collections.Generic;

namespace ApplicationLibrary.Crawling
{
    public interface IUriFilter
    {
        List<Uri> Filter(IEnumerable<Uri> input);
    }
}
