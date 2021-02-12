using System;
using System.Collections.Generic;
using System.Linq;
using WeightedRandomSelectionLib.Interfaces;
using WeightedRandomSelectionLib.Structure;
using WeightedRandomSelectionLib.Utils;

namespace WeightedRandomSelectionLib
{
    /// <summary>
    ///     Selects items based on their percentage chances
    /// </summary>
    /// <typeparam name="T">The type of the items to select</typeparam>
    public class WeightedRandomSelector<T> : IWeightedBuilder<T>, IWeightedEngine<T>
	{
        /// <summary>
        ///     All weights are multiplied by this factor to get a integer number
        ///     <para>All numbers after this decimal place will be ignored</para>
        ///     <para>If this number is set too hight, might occur performance loss</para>
        /// </summary>
        public readonly int IntegerFactor;

		internal readonly List<WeightedItem<T>> Items;
		public readonly SelectorOptions Options;
		
		private readonly Random _rng;

        /// <summary>
        ///     If this variable is set to true, before making any selections, the cumulative weight of the items will be
        ///     recalculated
        /// </summary>
        private bool _forceRecalculation;

		internal int[] CumulativeWeights;
		internal List<int> CumulativeWeightsList;
		internal int TotalCumulativeWeight;

        /// <summary>
        ///     Creates a instance of the selector with the desired options
        /// </summary>
        /// <param name="options"><see cref="SelectorOptions" /> flags for the selector</param>
        /// <param name="decimalPlaces">The maximum allowed decimal places in the items weights</param>
        public WeightedRandomSelector(
			SelectorOptions options = SelectorOptions.AllowDuplicates | SelectorOptions.IgnoreZeroWeight,
			int decimalPlaces = 2)
		{
			Items = new List<WeightedItem<T>>();
			Options = options;

			CumulativeWeights = null;
			TotalCumulativeWeight = 0;
			CumulativeWeightsList = null;

			IntegerFactor = (int) Math.Pow(10, decimalPlaces);
			
			_rng = new Random();

			_forceRecalculation = false;
		}

        /// <summary>
        ///     Creates a instance of the selector with an already made list of <see cref="WeightedItem{T}" /> and the desired
        ///     options
        ///     <seealso cref="SelectorOptions" />
        /// </summary>
        /// <param name="items">A list of <see cref="WeightedItem{T}" /></param>
        /// <param name="options"><see cref="SelectorOptions" /> flags for the selector</param>
        /// <param name="decimalPlaces">The maximum allowed decimal places in the items weights</param>
        public WeightedRandomSelector(List<WeightedItem<T>> items,
			SelectorOptions options = SelectorOptions.AllowDuplicates | SelectorOptions.IgnoreZeroWeight,
			int decimalPlaces = 2)
		{
			Items = items;
			Options = options;

			CumulativeWeights = null;
			TotalCumulativeWeight = 0;
			CumulativeWeightsList = null;

			IntegerFactor = (int) Math.Pow(10, decimalPlaces);
			
			_rng = new Random();

			_forceRecalculation = true;
		}
        
        /// <summary>
        ///     Adds a new <see cref="WeightedItem{T}" /> to the selector
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(WeightedItem<T> item)
		{
			if (item.Weight <= 0)
			{
				if (Options.HasFlag(SelectorOptions.IgnoreZeroWeight))
					return;
				throw new InvalidOperationException("Scores must be >= 0");
			}

			_forceRecalculation = true;
			Items.Add(item);
		}

        /// <summary>
        ///     Adds multiple <see cref="WeightedItem{T}" /> to the selector
        /// </summary>
        /// <param name="items">A Enumerable collection on <see cref="WeightedItem{T}" /></param>
        public void Add(IEnumerable<WeightedItem<T>> items)
		{
			foreach (var i in items) Add(i);
		}

        /// <summary>
        ///     Adds a new <see cref="WeightedItem{T}" /> with the desired value and weight
        /// </summary>
        /// <param name="value">The value of the item</param>
        /// <param name="weight">The weight of the item (up to two decimal places)</param>
        public void Add(T value, double weight)
		{
			Add(new WeightedItem<T>(value, weight));
		}

        /// <summary>
        ///     Removes the specified <see cref="WeightedItem{T}" /> from the selector
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void Remove(WeightedItem<T> item)
		{
			_forceRecalculation = true;
			Items.Remove(item);
		}

        /// <summary>
        ///     Clears the items list
        /// </summary>
        public void Clear()
		{
			_forceRecalculation = true;
			Items.Clear();
		}
        
        /// <summary>
        ///     Calculates the cumulative weights if it's needed
        /// </summary>
        public void Build()
		{
			if (!_forceRecalculation)
				return;

			_forceRecalculation = false;
			(CumulativeWeights, TotalCumulativeWeight) = WeightedHelper<T>.CalculateCumulativeWeights(Items, IntegerFactor);

			// If the selector allow duplicates then it don't have to create a copy of the weights in list form
			if (!Options.HasFlag(SelectorOptions.AllowDuplicates))
				CumulativeWeightsList = CumulativeWeights.ToList();
		}
        
        /// <summary>
        ///     A method to generate a number for the selector
        /// </summary>
        /// <param name="items">A list containing all the <see cref="WeightedItem{T}" /></param>
        /// <returns>A number between 1 and the sum of all the weights of items in the list</returns>
        private int RollRNG(IEnumerable<WeightedItem<T>> items)
		{
			// If the selector allow duplicates, there is no need to recalculate the total cumulative weight every time
			int maxWeight = Options.HasFlag(SelectorOptions.AllowDuplicates)
				? TotalCumulativeWeight + 1
				: (int) items.Sum(i => i.Weight * IntegerFactor) + 1;

			return _rng.Next(1, maxWeight);
		}
        
        /// <summary>
        ///     Selects a single item based in the weights
        /// </summary>
        /// <returns>The value of the item</returns>
        public T Select()
		{
			Build();
			
			var items = Items;

			if (items.Count == 0)
				throw new InvalidOperationException("There was no items to select from");

			return WeightedHelper<T>.SelectItem(items, CumulativeWeights, RollRNG(items)).Value;
		}

        /// <summary>
        ///     Selects multiple items based in the weights
        /// </summary>
        /// <param name="count">The number of items to select</param>
        /// <returns>A list containing the values of the items selected</returns>
        public IEnumerable<T> Select(int count)
		{
			if (count <= 0)
				throw new InvalidOperationException("Count must be > 0.");

			int itemsCount = Items.Count;

			if (itemsCount == 0)
				throw new InvalidOperationException("There were no items to select from.");

			if (!Options.HasFlag(SelectorOptions.AllowDuplicates) && itemsCount < count)
				throw new InvalidOperationException("There aren't enough items in the collection to take " + count);
			
			Build();

			var resultList = new List<T>();

			// A implementation of the select multiple code that uses O(log(n)) even when removing items from the list
			if (!Options.HasFlag(SelectorOptions.AllowDuplicates))
			{
				// A shallow copy of the lists containing the items and the cumulative weights, since we will be removing items of the list we want to conserve the original ones
				var items = new List<WeightedItem<T>>(Items);
				var weights = new List<int>(CumulativeWeightsList);

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
				for (var i = 0; i < count; i++) resultList.Add(WeightedHelper<T>.SelectItem(Items, CumulativeWeights, RollRNG(Items)).Value);
			}

			return resultList;
		}
	}
}