using System;
using System.Collections.Generic;
using System.Linq;

namespace Onova.Updater.Utils.Extensions;

internal static class DisposableExtensions
{
    extension(IEnumerable<IDisposable> disposables)
    {
        public void DisposeAll()
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
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            if (exceptions?.Any() == true)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
