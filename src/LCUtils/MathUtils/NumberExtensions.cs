namespace LCUtils;

/// <summary>
/// Provides extension methods for numeric types to handle special floating-point values.
/// </summary>
public static class NumberExtensions
{
    /// <summary>
    /// Returns a default value if the number is NaN (Not a Number), otherwise returns the original number.
    /// </summary>
    /// <param name="num">The double value to check.</param>
    /// <param name="defaultIfNaN">The value to return if the number is NaN. Defaults to 0.</param>
    /// <returns>The original number if it's not NaN, otherwise the default value.</returns>
    public static double IfNaNThen(this double num, double defaultIfNaN = 0)
    {
        return double.IsNaN(num) ? defaultIfNaN : num;
    }

    /// <summary>
    /// Returns a default value if the number is positive infinity, otherwise returns the original number.
    /// </summary>
    /// <param name="num">The double value to check.</param>
    /// <param name="defaultIfInf">The value to return if the number is positive infinity. Defaults to 0.</param>
    /// <returns>The original number if it's not positive infinity, otherwise the default value.</returns>
    public static double IfInfThen(this double num, double defaultIfInf = 0)
    {
        return double.IsPositiveInfinity(num) ? defaultIfInf : num;
    }

    /// <summary>
    /// Returns a default value if the number is negative infinity, otherwise returns the original number.
    /// </summary>
    /// <param name="num">The double value to check.</param>
    /// <param name="defaultIfNegInf">The value to return if the number is negative infinity. Defaults to 0.</param>
    /// <returns>The original number if it's not negative infinity, otherwise the default value.</returns>
    public static double IfNegInfThen(this double num, double defaultIfNegInf = 0)
    {
        return double.IsNegativeInfinity(num) ? defaultIfNegInf : num;
    }
}