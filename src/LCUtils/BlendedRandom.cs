using System;

namespace LCUtils;

public class BlendedRandom : Random
{
    private readonly Random _random1;
    private readonly Random _random2;
    private readonly double _blendFactor;
    private readonly double _blendFactorInv;

    public BlendedRandom(Random? random1, Random? random2, double blendFactor)
    {
        if (blendFactor < 0.0 || blendFactor > 1.0)
            throw new ArgumentOutOfRangeException(nameof(blendFactor), "blendFactor must be between 0.0 and 1.0.");

        _random1 = random1 ?? new();
        _random2 = random2 ?? new();
        _blendFactor = blendFactor;
        _blendFactorInv = 1.0 - blendFactor;
    }

    public override double NextDouble()
    {
        return (_random1.NextDouble() * _blendFactor + _random2.NextDouble() * _blendFactorInv) % 1.0;
    }

    public override int Next()
    {
        return Next(0, int.MaxValue);
    }

    public override int Next(int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than 0.");

        return Next(0, maxValue);
    }

    public override int Next(int minValue, int maxValue)
    {
        if (maxValue < minValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

        return (int)(NextDouble() * (maxValue - minValue) + minValue);
    }
}