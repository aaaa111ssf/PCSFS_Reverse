using System;
using System.Collections.Generic;

public static class FastLinq
{
	public static bool AnyFast<T>(this IList<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		for (int i = 0; i < source.Count; i++)
		{
			T arg = source[i];
			if (predicate(arg))
			{
				return true;
			}
		}
		return false;
	}

	public static void RemoveWhere<T>(this IList<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		T val = source.FirstFast(predicate);
		if (val != null)
		{
			source.Remove(val);
		}
	}

	public static float SumFast<T>(this IList<T> source, Func<T, float> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		float num = 0f;
		for (int i = 0; i < source.Count; i++)
		{
			num += predicate(source[i]);
		}
		return num;
	}

	public static int SumFast<T>(this IList<T> source, Func<T, int> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		int num = 0;
		for (int i = 0; i < source.Count; i++)
		{
			num += predicate(source[i]);
		}
		return num;
	}

	public static bool AllFast<T>(this IList<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		for (int i = 0; i < source.Count; i++)
		{
			if (!predicate(source[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool AllFast<T>(this Span<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		for (int i = 0; i < source.Length; i++)
		{
			if (!predicate(source[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int CountFast<T>(this IList<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		int num = 0;
		for (int i = 0; i < source.Count; i++)
		{
			T arg = source[i];
			if (predicate(arg))
			{
				num = checked(num + 1);
			}
		}
		return num;
	}

	public static int IndexOf<T>(this IList<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		for (int i = 0; i < source.Count; i++)
		{
			T arg = source[i];
			if (predicate(arg))
			{
				return i;
			}
		}
		return -1;
	}

	public static T FirstFast<T>(this IList<T> source, Func<T, bool> predicate, T @default = default(T))
	{
		if (source == null)
		{
			throw new Exception("Source is null");
		}
		if (predicate == null)
		{
			throw new Exception("Predicate is null");
		}
		for (int i = 0; i < source.Count; i++)
		{
			T val = source[i];
			if (predicate(val))
			{
				return val;
			}
		}
		return @default;
	}
}
