# Weighted Random Selection Lib

Weighted Random Selection Lib is a library implementation of a weighted random selection algorithm built on top of .NET Core 3.1 to reach almost any platform

The implementation uses efficient algorithms to deliver fast selections (like Binary Search)

## Example
Bellow is an example of how the library can be used

The code will select 10 of the available choices based on their weights and display one by line on the console

```c#
using WeightedRandomSelectionLib;

class Program {
	static void Main (string[] args) 
	{
		WeightedRandomSelector<string> selector = new WeightedRandomSelector<string>();

		selector.Add("Choice 1", 0.8);
		selector.Add("Choice 2", 15.0);
		selector.Add("Choice 3", 62.21);
		selector.Add("Choice 4", 32.5);
		selector.Add("Choice 5", 70.0);


		foreach (string i in selector.SelectMultiple (10))
		{
			Console.WriteLine(i);
		}
	}
}
```

Obs.: You can also add a WeightedItem collection for more control (or to add more than one item at time), you can also pass this collection in the constructor if you want to.