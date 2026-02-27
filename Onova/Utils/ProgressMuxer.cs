using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Onova.Utils;

internal class ProgressMuxer
{
    private readonly Lock _lock = new();
    private readonly IProgress<double> _output;
    private readonly Dictionary<int, double> _splitTotals;

    private int _splitCount;

    public ProgressMuxer(IProgress<double> output)
    {
        _output = output;
        _splitTotals = new Dictionary<int, double>();
    }

    public IProgress<double> CreateInput(double weight = 1)
    {
        var index = _splitCount++;
        return new Progress<double>(p =>
        {
            using (_lock.EnterScope())
            {
                _splitTotals[index] = weight * p;
                _output.Report(_splitTotals.Values.Sum());
            }
        });
    }
}
