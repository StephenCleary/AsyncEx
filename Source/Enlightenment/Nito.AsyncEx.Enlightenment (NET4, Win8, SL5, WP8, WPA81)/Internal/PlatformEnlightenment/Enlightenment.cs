using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.AsyncEx.Internal.PlatformEnlightenment
{
    internal static class Enlightenment
    {
        public static Exception Exception()
        {
            return new NotImplementedException("Error: No implementation found. Install the Nito.AsyncEx NuGet package into your application project.");
        }
    }
}
