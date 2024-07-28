using System.Collections.Generic;
using System.Linq;

namespace Address.API
{
    public static class Extensions
    {
        public static List<string> Stringify(this List<string> container)
        {
            return container.Select(item => string.Format($"\'{item}\'")).ToList();
        }
    }
}
