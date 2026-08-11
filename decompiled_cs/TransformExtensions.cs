using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
	public static IEnumerable<Transform> GetChildren(this Transform transform)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			yield return transform.GetChild(i);
		}
	}

	public static IEnumerable<Transform> GetChildrenReverse(this Transform transform)
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			yield return transform.GetChild(i);
		}
	}
}
