using System;
using System.Collections.Generic;
using System.Linq;

namespace LCUtils;

/// <summary>
/// Provides utility methods for weighted random selection and probability calculations.
/// </summary>
public static class Probability
{
    /// <summary>
    /// Returns a random index from a list based on weighted probabilities.
    /// </summary>
    /// <param name="weights">List of weights where each weight represents the probability of selecting the corresponding index. Negative values and NaN are clamped to 0.</param>
    /// <param name="random">Optional Random instance to use. If null, a new Random instance will be created.</param>
    /// <returns>
    /// The selected index based on weighted probability, or -1 if the weights list is empty.
    /// If all weights are 0 or negative, returns a uniformly random index.
    /// </returns>
    public static int GetRandomWeightedIndex(List<double> weights, Random? random = null)
    {
        if (weights.Count == 0) {
            Plugin.LogWarning( "[libs.Probability.GetRandomWeightedIndex] Cannot get random weighted index from empty list!");
			return -1;
		}

        weights = weights.Select(weight => Math.Clamp(weight, 0.0, int.MaxValue).IfNaNThen(0)).ToList();
        random ??= new();

        double totalWeight = weights.Sum();
        if (totalWeight <= 0.0) return random.Next(0, weights.Count);
        
        double randomValue = random.NextDouble() * totalWeight;
        double accumulatedValue = 0.0;

        return weights.FindIndex((weight) => (accumulatedValue += weight) > randomValue);
    }

    /// <summary>
    /// Returns a random index from a list based on weighted probabilities.
    /// </summary>
    /// <param name="weights">List of integer weights where each weight represents the probability of selecting the corresponding index. Negative values are clamped to 0.</param>
    /// <param name="random">Optional Random instance to use. If null, a new Random instance will be created.</param>
    /// <returns>
    /// The selected index based on weighted probability, or -1 if the weights list is empty.
    /// If all weights are 0 or negative, returns a uniformly random index.
    /// </returns>
    public static int GetRandomWeightedIndex(List<int> weights, Random? random = null)
    {
        return GetRandomWeightedIndex(weights.Select(weight => (double)weight).ToList(), random);
    }

    /// <summary>
    /// Returns a random key-value pair from a dictionary where the keys represent weights.
    /// </summary>
    /// <typeparam name="T">The type of the dictionary values.</typeparam>
    /// <param name="weightedDict">Dictionary where keys are weights and values are the items to select from. Negative keys and NaN are clamped to 0.</param>
    /// <param name="random">Optional Random instance to use. If null, a new Random instance will be created.</param>
    /// <returns>
    /// A tuple containing the selected key and value, or null if the dictionary is empty.
    /// If all weights are 0 or negative, returns a uniformly random pair.
    /// </returns>
    public static (double key, T value)? GetRandomPairFromWeightedKeys<T>(Dictionary<double, T> weightedDict, Random? random = null)
    {
        if (weightedDict.Count == 0) {
            Plugin.LogWarning("[libs.Probability.GetRandomPairFromWeightedKeys] Cannot get random pair from empty dict!");
			return null;
		}

        List<(double weight, (double key, T value) pair)> weightedPairs = weightedDict.Select(pair => (Math.Clamp(pair.Key, 0.0, int.MaxValue).IfNaNThen(0), (pair.Key, pair.Value))).ToList();
        random ??= new();

        double totalWeight = weightedPairs.Sum(weightedPair => weightedPair.weight);
        if (totalWeight <= 0.0) return weightedPairs.ElementAt(random.Next(0, weightedPairs.Count)).pair;
        
        double randomValue = random.NextDouble() * totalWeight;
        double accumulatedValue = 0.0;

        return weightedPairs.Find(weightedPair => (accumulatedValue += weightedPair.weight) > randomValue).pair;
    }

    /// <summary>
    /// Returns a random key-value pair from a dictionary where the values represent weights.
    /// </summary>
    /// <typeparam name="T">The type of the dictionary keys.</typeparam>
    /// <param name="weightedDict">Dictionary where keys are the items to select from and values are weights. Negative values and NaN are clamped to 0.</param>
    /// <param name="random">Optional Random instance to use. If null, a new Random instance will be created.</param>
    /// <returns>
    /// A tuple containing the selected key and value, or null if the dictionary is empty.
    /// If all weights are 0 or negative, returns a uniformly random pair.
    /// </returns>
    public static (T key, double value)? GetRandomPairFromWeightedValues<T>(Dictionary<T, double> weightedDict, Random? random = null)
    {
        if (weightedDict.Count == 0) {
            Plugin.LogWarning("[libs.Probability.GetRandomPairFromWeightedKeys] Cannot get random pair from empty dict!");
			return null;
		}

        List<(double weight, (T key, double value) pair)> weightedPairs = weightedDict.Select(pair => (Math.Clamp(pair.Value, 0.0, int.MaxValue).IfNaNThen(0), (pair.Key, pair.Value))).ToList();
        random ??= new();

        double totalWeight = weightedPairs.Sum(weightedPair => weightedPair.weight);
        if (totalWeight <= 0.0) return weightedPairs.ElementAt(random.Next(0, weightedPairs.Count)).pair;
        
        double randomValue = random.NextDouble() * totalWeight;
        double accumulatedValue = 0.0;

        return weightedPairs.Find(weightedPair => (accumulatedValue += weightedPair.weight) > randomValue).pair;
    }
}
