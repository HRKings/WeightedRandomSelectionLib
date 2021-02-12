using System;
using System.Collections.Generic;
using System.Linq;
using WeightedRandomSelectionLib.Structure;
using WeightedRandomSelectionLib.Utils;

namespace WeightedRandomSelectionLib.Algorithm
{
    /// <summary>
    ///     Controls how the items are selected
    /// </summary>
    /// <typeparam name="T">The type of the value returned</typeparam>
    internal class SelectorEngine<T>
	{
		private readonly Random _rng;

        /// <summary>
        ///     The selector containing the WeightedItem list
        /// </summary>
        public readonly WeightedRandomSelector<T> _selector;

        /// <summary>
        ///     Initializes the selector with the desired selector
        /// </summary>
        /// <param name="selector">The selector to be used</param>
        internal SelectorEngine(WeightedRandomSelector<T> selector)
		{
			_selector = selector;
			_rng = new Random();
		}

        /// <summary>
        ///     A method to generate a number for the selector
        /// </summary>
        /// <param name="items">A list containing all the <see cref="WeightedItem{T}" /></param>
        /// <returns>A number between 1 and the sum of all the weights of items in the list</returns>
        protected int RollRNG(List<WeightedItem<T>> items)
		{
			// If the selector allow duplicates, there is no need to recalculate the total cumulative weight every time
			int maxWeight = _selector.Options.HasFlag(SelectorOptions.AllowDuplicates)
				? _selector.TotalCumulativeWeight + 1
				: (int) items.Sum(i => i.Weight * _selector.IntegerFactor) + 1;

			return _rng.Next(1, maxWeight);
		}
        
        /// <summary>
        ///     Selects a single item from the <see cref="WeightedRandomSelector{T}" />
        /// </summary>
        /// <returns>The value of the selected item</returns>
        internal T SelectSingle()
		{
			var items = _selector.Items;

			if (items.Count == 0)
				throw new InvalidOperationException("There was no items to select from");

			return WeightedHelper<T>.SelectItem(items, _selector.CumulativeWeights, RollRNG(items)).Value;
		}

        /// <summary>
        ///     Selects multiple items from the <see cref="WeightedRandomSelector{T}" />
        /// </summary>
        /// <param name="count">The number of items to select</param>
        /// <returns>A list with all the items selected</returns>
        internal List<T> SelectMulti(int count)
		{
			Validate(count);

			var resultList = new List<T>();

			// A implementation of the select multiple code that uses O(log(n)) even when removing items from the list
			if (!_selector.Options.HasFlag(SelectorOptions.AllowDuplicates))
			{
				// A shallow copy of the lists containing the items and the cumulative weights, since we will be removing items of the list we want to conserve the original ones
				var items = new List<WeightedItem<T>>(_selector.Items);
				var weights = new List<int>(_selector.CumulativeWeightsList);

				for (var i = 0; i < count; i++)
				{
					int index = WeightedHelper<T>.SelectItemIndex(items, weights, RollRNG(items));
					resultList.Add(items[index].Value);

					if (items.Count > 0)
						items.RemoveAt(index);

					if (weights.Count > 0)
						weights.RemoveAt(index);
				}
			}
			else
			{
				for (var i = 0; i < count; i++) resultList.Add(WeightedHelper<T>.SelectItem(_selector.Items, _selector.CumulativeWeights, RollRNG(_selector.Items)).Value);
			}

			return resultList;
		}

        /// <summary>
        ///     Validates the number input for a <see cref="SelectMulti(int)" />
        /// </summary>
        /// <param name="count">The number of items to select</param>
        private void Validate(int count)
		{
			if (count <= 0)
				throw new InvalidOperationException("Count must be > 0.");

			int itemsCount = _selector.Items.Count;

			if (itemsCount == 0)
				throw new InvalidOperationException("There were no items to select from.");

			if (!_selector.Options.HasFlag(SelectorOptions.AllowDuplicates) && itemsCount < count)
				throw new InvalidOperationException("There aren't enough items in the collection to take " + count);
		}
	}
}