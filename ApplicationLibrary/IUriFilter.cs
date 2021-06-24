using System;
using System.Collections.Generic;

namespace ApplicationLibrary
{
    public interface IUriFilter
    {
        List<Uri> Filter(IEnumerable<Uri> input);
    }
}
