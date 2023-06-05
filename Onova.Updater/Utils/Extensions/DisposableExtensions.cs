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
            // TODO: No aggregate exception on .NET 3.5
            // throw new AggregateException(exceptions);
            throw new Exception("An error occurred while disposing of one or more objects.", exceptions.First());
        }
    }
}