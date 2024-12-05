using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace eShop.Observability;

public class MeterCounter
{
    private readonly Counter<long> _counter;

    public MeterCounter(string meterName)
    {
        Meter meter = new(meterName);
        _counter = meter.CreateCounter<long>(meterName);
    }

    public void Increment(ReadOnlySpan<KeyValuePair<string, object?>> tags, long count = 1) => 
        _counter.Add(count, tags);
}