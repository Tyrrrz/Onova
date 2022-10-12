using System.Collections.Generic;
using System.Linq;

namespace Onova.Utils.Extensions;

internal static class CollectionExtensions
{
    public static int AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> sequence) =>
        sequence.Count(hashSet.Add);
}