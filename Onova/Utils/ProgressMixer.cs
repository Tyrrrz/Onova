using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Onova.Utils;

internal class ProgressMixer
{
    private readonly Lock _lock = new();
    private readonly IProgress<double> _output;
    private readonly Dictionary<int, double> _splitTotals;

    private int _splitCount;

    public ProgressMixer(IProgress<double> output)
    {
        _output = output;
        _splitTotals = new Dictionary<int, double>();
    }

    public IProgress<double> Split(double multiplier)
    {
        var index = _splitCount++;
        return new Progress<double>(p =>
        {
            using (_lock.EnterScope())
            {
                _splitTotals[index] = multiplier * p;
                _output.Report(_splitTotals.Values.Sum());
            }
        });
    }
}
