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
		///     <para>If this number is set too high, might occur performance loss</para>
		/// </summary>
		private readonly int _integerFactor;

		private readonly List<WeightedItem<T>> _items;
		private readonly SelectorOptions _options;

		private readonly Random _rng;

		private int[] _cumulativeWeights;
		private List<int> _cumulativeWeightsList;

		/// <summary>
		///     If this variable is set to true, before making any selections, the cumulative weight of the items will be
		///     recalculated
		/// </summary>
		private bool _forceRecalculation;

		private int _totalCumulativeWeight;

		/// <summary>
		///     Creates a instance of the selector with the desired options
		/// </summary>
		/// <param name="options"><see cref="SelectorOptions" /> flags for the selector</param>
		/// <param name="decimalPlaces">The maximum allowed decimal places in the items weights. All numbers after this decimal place will be ignored</param>
		public WeightedRandomSelector(
			SelectorOptions options = SelectorOptions.AllowDuplicates | SelectorOptions.IgnoreZeroWeight,
			int decimalPlaces = 2)
		{
			_items = new List<WeightedItem<T>>();
			_options = options;

			_cumulativeWeights = null;
			_totalCumulativeWeight = 0;
			_cumulativeWeightsList = null;

			_integerFactor = (int) Math.Pow(10, decimalPlaces);

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
		/// <param name="decimalPlaces">The maximum allowed decimal places in the items weights. All numbers after this decimal place will be ignored</param>
		public WeightedRandomSelector(List<WeightedItem<T>> items,
			SelectorOptions options = SelectorOptions.AllowDuplicates | SelectorOptions.IgnoreZeroWeight,
			int decimalPlaces = 2) : this(options, decimalPlaces)
		{
			_items = items;
		}

		/// <summary>
		///     Adds a new <see cref="WeightedItem{T}" /> to the selector
		/// </summary>
		/// <param name="item">The item to add</param>
		public void Add(WeightedItem<T> item)
		{
			if (item.Weight <= 0)
			{
				if (_options.HasFlag(SelectorOptions.IgnoreZeroWeight))
					return;
				throw new InvalidOperationException("Scores must be >= 0");
			}

			_forceRecalculation = true;
			_items.Add(item);
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
			_items.Remove(item);
		}

		/// <summary>
		///     Clears the items list
		/// </summary>
		public void Clear()
		{
			_forceRecalculation = true;
			_items.Clear();
		}

		/// <summary>
		///     Calculates the cumulative weights if it's needed
		/// </summary>
		public void Build()
		{
			if (!_forceRecalculation)
				return;

			_forceRecalculation = false;
			(_cumulativeWeights, _totalCumulativeWeight) =
				WeightedHelper<T>.CalculateCumulativeWeights(_items, _integerFactor);

			// If the selector don't allow duplicates then it have to create a copy of the weights in list form
			if (!_options.HasFlag(SelectorOptions.AllowDuplicates))
				_cumulativeWeightsList = _cumulativeWeights.ToList();
		}

		/// <summary>
		///     Selects a single item based in the weights
		/// </summary>
		/// <returns>The value of the item</returns>
		public T Select()
		{
			if (_items.Count == 0)
				throw new InvalidOperationException("There was no items to select from");

			Build();

			return WeightedHelper<T>.SelectItem(_items, _cumulativeWeights, GenerateRandomWeight(_items)).Value;
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

			if (_items.Count == 0)
				throw new InvalidOperationException("There were no items to select from.");

			if (!_options.HasFlag(SelectorOptions.AllowDuplicates) && _items.Count < count)
				throw new InvalidOperationException("There aren't enough items in the collection to take " + count);

			Build();

			if (_options.HasFlag(SelectorOptions.AllowDuplicates))
				for (var i = 0; i < count; i++)
					yield return WeightedHelper<T>
						.SelectItem(_items, _cumulativeWeights, GenerateRandomWeight(_items)).Value;

			// The code bellow can perform under O(log(n)) even when removing items from the list
			// A shallow copy of the lists containing the items and the cumulative weights, since we will be removing items of the list we want to conserve the original ones
			var items = new List<WeightedItem<T>>(_items);
			var weights = new List<int>(_cumulativeWeightsList);

			for (var i = 0; i < count; i++)
			{
				int index = WeightedHelper<T>.SelectItemIndex(items, weights, GenerateRandomWeight(items));

				if (items.Count > 0)
					items.RemoveAt(index);

				if (weights.Count > 0)
					weights.RemoveAt(index);

				yield return items[index].Value;
			}
		}

		/// <summary>
		///     A method to generate a number for the selector
		/// </summary>
		/// <param name="items">A list containing all the <see cref="WeightedItem{T}" /></param>
		/// <returns>A number between 1 and the sum of all the weights of items in the list</returns>
		private int GenerateRandomWeight(IEnumerable<WeightedItem<T>> items)
		{
			// If the selector allow duplicates, there is no need to recalculate the total cumulative weight every time
			int maxWeight = _options.HasFlag(SelectorOptions.AllowDuplicates)
				? _totalCumulativeWeight + 1
				: (int) items.Sum(i => i.Weight * _integerFactor) + 1;

			return _rng.Next(1, maxWeight);
		}
	}
}