using System;

namespace LCUtils;

public static class NumberExtensions
{
    public static double IfNaNThen(this double num, double defaultIfNaN = 0)
    {
        return double.IsNaN(num) ? defaultIfNaN : num;
    }

    public static double IfInfThen(this double num, double defaultIfInf = 0)
    {
        return double.IsPositiveInfinity(num) ? defaultIfInf : num;
    }

    public static double IfNegInfThen(this double num, double defaultIfNegInf = 0)
    {
        return double.IsNegativeInfinity(num) ? defaultIfNegInf : num;
    }
}