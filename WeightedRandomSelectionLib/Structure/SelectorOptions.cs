using System;
using WeightedRandomSelectionLib.Algorithm;

namespace WeightedRandomSelectionLib.Structure
{
	[Flags]
	public enum SelectorOptions : short
	{
        /// <summary>
        ///     No special options will be used
        /// </summary>
        None = 0,

        /// <summary>
        ///     Allow the same item to appear multiple times in a <see cref="SelectorEngine{T}.SelectMulti(int)" />
        /// </summary>
        AllowDuplicates = 1,

        /// <summary>
        ///     Items with zero weight will not be considered when selecting items
        /// </summary>
        IgnoreZeroWeight = 2
	}
}