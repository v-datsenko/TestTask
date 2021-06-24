using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLibrary
{
    public interface IUriFilter
    {
        List<Uri> Filter(IEnumerable<Uri> input);
    }
}
