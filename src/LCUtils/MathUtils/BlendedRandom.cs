using System;

namespace LCUtils;

/// <summary>
/// A random number generator that blends the output of two other random number generators.
/// </summary>
/// <remarks>
/// The BlendedRandom class combines two Random instances using a weighted average approach.
/// The blend factor determines how much each generator contributes to the final result.
/// </remarks>
public class BlendedRandom : Random
{
    private readonly Random _random1;
    private readonly Random _random2;
    private readonly double _blendFactor;
    private readonly double _blendFactorInv;

    /// <summary>
    /// Initializes a new instance of the BlendedRandom class with two random number generators and a blend factor.
    /// </summary>
    /// <param name="random1">The first random number generator. If null, a new Random instance will be created.</param>
    /// <param name="random2">The second random number generator. If null, a new Random instance will be created.</param>
    /// <param name="blendFactor">A value between 0.0 and 1.0 that determines the weight of the first generator's contribution.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when blendFactor is not between 0.0 and 1.0.</exception>
    /// <remarks>
    /// A blendFactor of 0.0 means only the second generator is used, while 1.0 means only the first generator is used.
    /// Values between create a weighted blend of both generators.
    /// </remarks>
    public BlendedRandom(Random? random1, Random? random2, double blendFactor)
    {
        if (blendFactor < 0.0 || blendFactor > 1.0)
            throw new ArgumentOutOfRangeException(nameof(blendFactor), "blendFactor must be between 0.0 and 1.0.");

        _random1 = random1 ?? new();
        _random2 = random2 ?? new();
        _blendFactor = blendFactor;
        _blendFactorInv = 1.0 - blendFactor;
    }

    /// <summary>
    /// Returns a random floating-point number between 0.0 and 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    /// <remarks>
    /// The returned value is a weighted blend of the NextDouble() results from both underlying random generators.
    /// </remarks>
    public override double NextDouble()
    {
        return (_random1.NextDouble() * _blendFactor + _random2.NextDouble() * _blendFactorInv) % 1.0;
    }

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0 and less than int.MaxValue.</returns>
    public override int Next()
    {
        return Next(0, int.MaxValue);
    }

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. maxValue must be greater than 0.</param>
    /// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than maxValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxValue is less than or equal to 0.</exception>
    public override int Next(int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than 0.");

        return Next(0, maxValue);
    }

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.</param>
    /// <returns>A 32-bit signed integer greater than or equal to minValue and less than maxValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxValue is less than minValue.</exception>
    public override int Next(int minValue, int maxValue)
    {
        if (maxValue < minValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

        return (int)(NextDouble() * (maxValue - minValue) + minValue);
    }
}