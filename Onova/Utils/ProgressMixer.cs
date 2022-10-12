using System;
using System.Collections.Generic;
using System.Linq;

namespace Onova.Utils;

internal class ProgressMixer
{
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
            lock (_splitTotals)
            {
                _splitTotals[index] = multiplier * p;
                _output.Report(_splitTotals.Values.Sum());
            }
        });
    }
}