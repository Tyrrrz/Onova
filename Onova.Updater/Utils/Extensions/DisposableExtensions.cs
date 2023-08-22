using System;
using System.Collections.Generic;
using System.Linq;

namespace Onova.Updater.Utils.Extensions;

internal static class DisposableExtensions
{
    public static void DisposeAll(this IEnumerable<IDisposable> disposables)
    {
        var exceptions = default(List<Exception>);

        foreach (var i in disposables)
        {
            try
            {
                i.Dispose();
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(ex);
            }
        }

        if (exceptions?.Any() == true)
        {
            throw new AggregateException(exceptions);
        }
    }
}
