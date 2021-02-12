using System;
using System.Collections.Generic;
using System.Linq;
using WeightedRandomSelectionLib.Structure;

namespace WeightedRandomSelectionLib.Utils
{
	public static class WeightedHelper<T>
	{
		/// <summary>
		///     Calculates the cumulative weights of every <see cref="WeightedItem{T}" />
		/// </summary>
		/// <param name="items">A list containing all the weighted items</param>
		/// <param name="integerFactor">The factor to multiply the weights to turn them into integers</param>
		/// <returns>An array with the cumulative weights and a the total cumulative weight</returns>
		public static (int[], int) CalculateCumulativeWeights(List<WeightedItem<T>> items, int integerFactor)
		{
			var currentWeight = 0;
			var index = 0;
			var result = new int[items.Count];
			var totalCumulativeWeight = 0;

			foreach (int roundedWeight in items.Select(weight => (int) (weight.Weight * integerFactor)))
			{
				currentWeight += roundedWeight;
				totalCumulativeWeight += roundedWeight;

				result[index] = currentWeight;

				index++;
			}

			return (result, totalCumulativeWeight);
		}

		/// <summary>
		///     Searches for the provided weight and returns the of the weighted item with the closest weight
		/// </summary>
		/// <param name="items">An array containing the weighted items</param>
		/// <param name="cumulativeWeights">A list containing the cumulative weights</param>
		/// <param name="weightToSearch">A weight to search for</param>
		/// <returns>A weighted item in which the random number corresponds to its weight</returns>
		public static WeightedItem<T> SelectItem(List<WeightedItem<T>> items, int[] cumulativeWeights,
			int weightToSearch)
		{
			if (items.Count == 0)
				throw new InvalidOperationException("There was no WeightedItem to search");

			if (items.Count == 1 || cumulativeWeights.Length == 1)
				return items[0];

			if (items.Count == 1)
				return items[0];

			int index = Array.BinarySearch(cumulativeWeights, weightToSearch);

			index = index < 0 ? ~index : index;

			return items[index];
		}

		/// <summary>
		///     Searches for the provided weight and returns the index of the weighted item with the closest weight
		/// </summary>
		/// <param name="items">A list containing the weighted items</param>
		/// <param name="cumulativeWeights">A list containing the cumulative weights</param>
		/// <param name="weightToSearch">A weight to search for</param>
		/// <returns>The index of a weighted item in which the random number corresponds to its weight</returns>
		/// <exception cref="InvalidOperationException">Exception if there is no weighted item to search</exception>
		public static int SelectItemIndex(List<WeightedItem<T>> items, List<int> cumulativeWeights, int weightToSearch)
		{
			if (items.Count == 0)
				throw new InvalidOperationException("There was no WeightedItem to search");

			if (items.Count == 1 || cumulativeWeights.Count == 1)
				return 0;

			int index = cumulativeWeights.BinarySearch(weightToSearch);

			index = index < 0 ? ~index : index;

			return index;
		}
	}
}