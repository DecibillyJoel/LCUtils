using System;
using System.Collections.Generic;
using System.Linq;
using LogLevel = BepInEx.Logging.LogLevel;

namespace LCUtils;

public static class Probability
{
    public static int GetRandomWeightedIndex(List<double> weights, Random? random = null)
    {
        if (weights.Count == 0) {
            Plugin.Log(LogLevel.Warning, "[libs.Probability.GetRandomWeightedIndex] Cannot get random weighted index from empty list!");
			return -1;
		}

        weights = weights.Select(weight => Math.Clamp(weight, 0.0, int.MaxValue)).ToList();
        random ??= new();

        double totalWeight = weights.Sum();
        if (totalWeight <= 0.0) return random.Next(0, weights.Count);
        
        double randomValue = random.NextDouble() * totalWeight;
        double accumulatedValue = 0.0;

        return weights.FindIndex((weight) => (accumulatedValue += weight) > randomValue);
    }

    public static int GetRandomWeightedIndex(List<int> weights, Random? random = null)
    {
        return GetRandomWeightedIndex(weights.Select(weight => (double)weight).ToList(), random);
    }

    public static (double key, T value)? GetRandomPairFromWeightedKeys<T>(Dictionary<double, T> weightedDict, Random? random = null)
    {
        if (weightedDict.Count == 0) {
            Plugin.Log(LogLevel.Warning, "[libs.Probability.GetRandomPairFromWeightedKeys] Cannot get random pair from empty dict!");
			return null;
		}

        List<(double weight, (double key, T value) pair)> weightedPairs = weightedDict.Select(pair => (Math.Clamp(pair.Key, 0.0, int.MaxValue), (pair.Key, pair.Value))).ToList();
        random ??= new();

        double totalWeight = weightedPairs.Sum(weightedPair => weightedPair.weight);
        if (totalWeight <= 0.0) return weightedPairs.ElementAt(random.Next(0, weightedPairs.Count)).pair;
        
        double randomValue = random.NextDouble() * totalWeight;
        double accumulatedValue = 0.0;

        return weightedPairs.Find(weightedPair => (accumulatedValue += weightedPair.weight) > randomValue).pair;
    }

    public static (T key, double value)? GetRandomPairFromWeightedValues<T>(Dictionary<T, double> weightedDict, Random? random = null)
    {
        if (weightedDict.Count == 0) {
            Plugin.Log(LogLevel.Warning, "[libs.Probability.GetRandomPairFromWeightedKeys] Cannot get random pair from empty dict!");
			return null;
		}

        List<(double weight, (T key, double value) pair)> weightedPairs = weightedDict.Select(pair => (Math.Clamp(pair.Value, 0.0, int.MaxValue), (pair.Key, pair.Value))).ToList();
        random ??= new();

        double totalWeight = weightedPairs.Sum(weightedPair => weightedPair.weight);
        if (totalWeight <= 0.0) return weightedPairs.ElementAt(random.Next(0, weightedPairs.Count)).pair;
        
        double randomValue = random.NextDouble() * totalWeight;
        double accumulatedValue = 0.0;

        return weightedPairs.Find(weightedPair => (accumulatedValue += weightedPair.weight) > randomValue).pair;
    }
}