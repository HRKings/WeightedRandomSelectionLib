﻿using System;
using System.Collections.Generic;
using System.Linq;
using WeightedRandomSelectionLib.Structure;

namespace WeightedRandomSelectionLib.Utils
{
	public static class WeightedHelper<T>
	{
		public static (int[], int) CalculateCumulativeWeights(List<WeightedItem<T>> weights, int integerFactor)
		{
			var currentWeight = 0;
			var index = 0;
			var result = new int[weights.Count];
			var totalCumulativeWeight = 0;

			foreach (int roundedWeight in weights.Select(weight => (int) (weight.Weight * integerFactor)))
			{
				currentWeight += roundedWeight;
				totalCumulativeWeight += roundedWeight;

				result[index] = currentWeight;

				index++;
			}

			return (result, totalCumulativeWeight);
		}
		
				
		public static WeightedItem<T> BinarySearch(List<WeightedItem<T>> items, int[] cumulativeWeights, int rollResult)
		{
			if (items.Count == 1)
				return items[0];

			int index = Array.BinarySearch(cumulativeWeights, rollResult);

			index = index < 0 ? ~index : index;

			return items[index];
		}
	}
}