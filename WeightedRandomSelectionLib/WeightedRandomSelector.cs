using System;
using System.Collections.Generic;
using System.Linq;

using WeightedRandomSelectorLib.Algorithm;
using WeightedRandomSelectorLib.Structure;

namespace WeightedRandomSelectorLib
{
    /// <summary>
    /// Selects items based on their percentage chances
    /// </summary>
    /// <typeparam name="T">The type of the items to select</typeparam>
    public class WeightedRandomSelector<T>
    {
        internal readonly List<WeightedItem<T>> Items;
        public readonly SelectorOptions Options;

        internal int[] CumulativeWeights;
        internal int TotalCumulativeWeight;
        internal List<int> CumulativeWeightsList;

        /// <summary>
        /// All weights are multiplied by this factor to get a integer number
        /// <para>All numbers after this decimal place will be ignored</para>
        /// <para>If this number is set too hight, might occur performance loss</para>
        /// </summary>
        public readonly int IntegerFactor;

        /// <summary>
        /// If this variable is set to true, before making any selections, the cumulative weight of the items will be recalculated
        /// </summary>
        private bool _forceRecalculation;

        private readonly SelectorEngine<T> selector;

        /// <summary>
        /// Creates a instance of the selector with the desired options
        /// </summary>
        /// <param name="options"><see cref="SelectorOptions"/> flags for the selector</param>
        /// <param name="decimalPlaces">The maximum allowed decimal places in the items weights</param>
        public WeightedRandomSelector(SelectorOptions options = SelectorOptions.AllowDuplicates | SelectorOptions.IgnoreZeroWeight, int decimalPlaces = 2)
        {
            this.Items = new List<WeightedItem<T>>();
            this.Options = options;

            this.CumulativeWeights = null;
            this.TotalCumulativeWeight = 0;
            this.CumulativeWeightsList = null;

            this.IntegerFactor = (int)Math.Pow(10, decimalPlaces);

            this.selector = new SelectorEngine<T>(this);

            _forceRecalculation = false;
        }

        /// <summary>
        /// Creates a instance of the selector with an already made list of <see cref="WeightedItem{T}"/> and the desired options
        /// <seealso cref="SelectorOptions"/>
        /// </summary>
        /// <param name="items">A list of <see cref="WeightedItem{T}"/></param>
        /// <param name="options"><see cref="SelectorOptions"/> flags for the selector</param>
        /// <param name="decimalPlaces">The maximum allowed decimal places in the items weights</param>
        public WeightedRandomSelector(List<WeightedItem<T>> items, SelectorOptions options = SelectorOptions.AllowDuplicates | SelectorOptions.IgnoreZeroWeight, int decimalPlaces = 2)
        {
            this.Items = items;
            this.Options = options;

            this.CumulativeWeights = null;
            this.TotalCumulativeWeight = 0;
            this.CumulativeWeightsList = null;

            this.IntegerFactor = (int)Math.Pow(10, decimalPlaces);

            this.selector = new SelectorEngine<T>(this);

            _forceRecalculation = true;
        }

        /// <summary>
        /// Adds a new <see cref="WeightedItem{T}"/> to the selector
        /// </summary>
        /// <param name="item">The item to add</param>
        public void Add(WeightedItem<T> item)
        {
            if (item.Weight <= 0)
            {
                if (Options.HasFlag(SelectorOptions.IgnoreZeroWeight))
                    return;
                else
                    throw new InvalidOperationException("Scores must be >= 0");
            }

            _forceRecalculation = true;
            Items.Add(item);
        }

        /// <summary>
        /// Adds multiple <see cref="WeightedItem{T}"/> to the selector
        /// </summary>
        /// <param name="items">A Enumerable collection on <see cref="WeightedItem{T}"/></param>
        public void Add(IEnumerable<WeightedItem<T>> items)
        {
            foreach (WeightedItem<T> i in items)
            {
                this.Add(i);
            }
        }

        /// <summary>
        /// Adds a new <see cref="WeightedItem{T}"/> with the desired value and weight
        /// </summary>
        /// <param name="value">The value of the item</param>
        /// <param name="weight">The weight of the item (up to two decimal places)</param>
        public void Add(T value, double weight) => this.Add(new WeightedItem<T>(value, weight));

        /// <summary>
        /// Removes the specified <see cref="WeightedItem{T}"/> from the selector
        /// </summary>
        /// <param name="item">The item to remove</param>
        public void Remove(WeightedItem<T> item)
        {
            _forceRecalculation = true;
            Items.Remove(item);
        }

        /// <summary>
        /// Selects a single item based in the weights
        /// </summary>
        /// <returns>The value of the item</returns>
        public T Select()
        {
            CalculateCumulativeWeights();

            return selector.SelectSingle();
        }

        /// <summary>
        /// Selects multiple items based in the weights
        /// </summary>
        /// <param name="count">The number of items to select</param>
        /// <returns>A list containing the values of the items selected</returns>
        public List<T> SelectMultiple(int count)
        {
            CalculateCumulativeWeights();

            return selector.SelectMulti(count);
        }

        /// <summary>
        /// Calculates the cumulative weights of every <see cref="WeightedItem{T}"/> in the selector
        /// <para>This is used for an effective use of the BinarySearch</para>
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public int[] GetCumulativeWeights(List<WeightedItem<T>> items)
        {
            int currentWeight = 0;
            int index = 0;
            int[] result = new int[items.Count];

            foreach (WeightedItem<T> item in items)
            {
                int roundedWeight = (int)(item.Weight * IntegerFactor);
                currentWeight += roundedWeight;
                this.TotalCumulativeWeight += roundedWeight;

                result[index] = currentWeight;

                index++;
            }

            return result;
        }

        /// <summary>
        /// Calculates the cumulative weights if it's needed
        /// </summary>
        public void CalculateCumulativeWeights()
        {
            if (!_forceRecalculation)
                return;

            _forceRecalculation = false;
            CumulativeWeights = GetCumulativeWeights(Items);

            // If the selector allow duplicates then it don't have to create a copy of the weights in list form
            if (!Options.HasFlag(SelectorOptions.AllowDuplicates))
                CumulativeWeightsList = CumulativeWeights.ToList();
        }
    }
}