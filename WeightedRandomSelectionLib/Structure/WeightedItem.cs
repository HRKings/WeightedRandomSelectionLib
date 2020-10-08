﻿namespace WeightedRandomSelectorLib.Structure
{
    public struct WeightedItem<T>
    {
        /// <summary>
        /// The weight of the item in decimal form
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// The actual item which will be selected, here named Value
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Creates a new item
        /// </summary>
        /// <param name="value">The desired value of the item</param>
        /// <param name="weight">The weight of the item</param>
        public WeightedItem(T value, double weight)
        {
            this.Weight = weight;
            this.Value = value;
        }
    }
}