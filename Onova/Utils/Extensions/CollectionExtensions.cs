using System.Collections.Generic;
using System.Linq;

namespace Onova.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(HashSet<T> hashSet)
    {
        public int AddRange(IEnumerable<T> sequence) => sequence.Count(hashSet.Add);
    }
}
