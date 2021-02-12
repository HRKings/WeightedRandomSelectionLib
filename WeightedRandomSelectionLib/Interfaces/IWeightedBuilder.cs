using System.Collections.Generic;
using WeightedRandomSelectionLib.Structure;

namespace WeightedRandomSelectionLib.Interfaces
{
	public interface IWeightedBuilder<T>
	{
		public void Build();
		public void Add(WeightedItem<T> item);
		public void Add(T item, double weight);
		public void Add(IEnumerable<WeightedItem<T>> item);
		public void Remove(WeightedItem<T> item);
		public void Clear();
	}
}