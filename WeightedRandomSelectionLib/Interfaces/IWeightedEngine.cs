using System.Collections.Generic;

namespace WeightedRandomSelectionLib.Interfaces
{
	public interface IWeightedEngine<T>
	{
		public T Select();
		public IEnumerable<T> Select(int amount);
	}
}