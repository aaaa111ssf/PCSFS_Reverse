using System.Collections.Generic;
using SFS;
using UnityEngine;

public static class PolygonPartioner
{
	private class VertexWorkSet
	{
		public readonly VertexWorkSet dispatcher;

		public VertexChain input;

		public List<VertexChain> list = new List<VertexChain>();

		public int pending;

		public bool done;

		public VertexWorkSet(VertexWorkSet dispatcher, VertexChain input)
		{
			this.dispatcher = dispatcher;
			this.input = input;
		}
	}

	public static List<ConvexPolygon> Partion(Object debugObject, Vector2[] points)
	{
		VertexChain input = new VertexChain(points);
		List<VertexChain> list = ConvexPartition(debugObject, input);
		List<ConvexPolygon> list2 = new List<ConvexPolygon>();
		for (int i = 0; i < list.Count; i++)
		{
			VertexChain vertexChain = list[i];
			Vector2[] array = new Vector2[vertexChain.Count];
			for (int j = 0; j < vertexChain.Count; j++)
			{
				array[j] = vertexChain[j];
			}
			list2.Add(new ConvexPolygon(array));
		}
		return list2;
	}

	private static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
	{
		return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
	}

	private static Vector2 At(int position, VertexChain vertices)
	{
		int count = vertices.Count;
		return vertices[(position < 0) ? (count - -position % count) : (position % count)];
	}

	private static bool CanSee(int i, int j, VertexChain vertices)
	{
		Vector2 prev = At(i - 1, vertices);
		Vector2 vector = At(i, vertices);
		Vector2 next = At(i + 1, vertices);
		if (Reflex(prev, vector, next))
		{
			if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) && RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)))
			{
				return false;
			}
		}
		else if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) || LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)))
		{
			return false;
		}
		Vector2 prev2 = At(j - 1, vertices);
		Vector2 vector2 = At(j, vertices);
		Vector2 next2 = At(j + 1, vertices);
		if (Reflex(prev2, vector2, next2))
		{
			if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) && RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)))
			{
				return false;
			}
		}
		else if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) || LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)))
		{
			return false;
		}
		for (int k = 0; k < vertices.Count; k++)
		{
			Vector2 vector3 = default(Vector2);
			Vector2 point = default(Vector2);
			Vector2 point2 = At(i, vertices);
			Vector2 point3 = At(j, vertices);
			Vector2 point4 = At(k, vertices);
			Vector2 point5 = At(k + 1, vertices);
			if (!point2.Equals(point4) && !point2.Equals(point5) && !point3.Equals(point4) && !point3.Equals(point5))
			{
				LineIntersect(ref point2, ref point3, ref point4, ref point5, firstIsSegment: true, secondIsSegment: true, out point);
				if (point != vector3 && (!point.Equals(At(k, vertices)) || !point.Equals(At(k + 1, vertices))))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static VertexChain CollinearSimplify(VertexChain vertices, float collinearityTolerance = 0f)
	{
		if (vertices.Count <= 3)
		{
			return vertices;
		}
		VertexChain vertexChain = new VertexChain(vertices.Count);
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector2 a = vertices.PreviousVertex(i);
			Vector2 b = vertices[i];
			Vector2 c = vertices.NextVertex(i);
			if (!IsCollinear(ref a, ref b, ref c, collinearityTolerance))
			{
				vertexChain.Add(b);
			}
		}
		return vertexChain;
	}

	private static List<VertexChain> ConvexPartition(Object debugObject, VertexChain input)
	{
		input.ForceCounterClockWise();
		if (input.Count < 3)
		{
			return new List<VertexChain>();
		}
		VertexWorkSet vertexWorkSet = new VertexWorkSet(null, input);
		Queue<VertexWorkSet> queue = new Queue<VertexWorkSet>();
		queue.Enqueue(vertexWorkSet);
		string arg = ((!(debugObject is MonoBehaviour component)) ? debugObject.name : component.GetPath());
		int num = 0;
		while (queue.Count > 0)
		{
			num++;
			if (num > 10000)
			{
				Debug.LogError($"ConvexPartition stopped on too many iterations after: {num} iterations; for: {arg}", debugObject);
				return new List<VertexChain>();
			}
			VertexWorkSet vertexWorkSet2 = queue.Dequeue();
			VertexChain input2 = vertexWorkSet2.input;
			List<VertexChain> list = vertexWorkSet2.list;
			Vector2 vector;
			Vector2 vector2;
			int num2;
			int i;
			int num3;
			if (input2.Count >= 3)
			{
				vector = default(Vector2);
				vector2 = default(Vector2);
				num2 = 0;
				i = 0;
				num3 = 0;
				while (num3 < input2.Count)
				{
					Vector2 prev = At(num3 - 1, input2);
					Vector2 vector3 = At(num3, input2);
					Vector2 next = At(num3 + 1, input2);
					if (!Reflex(prev, vector3, next))
					{
						num3++;
						continue;
					}
					goto IL_00ef;
				}
				list.Add(input2);
				for (int j = 0; j < list.Count; j++)
				{
					list[j] = CollinearSimplify(list[j], 1E-05f);
				}
			}
			vertexWorkSet2.done = true;
			VertexWorkSet vertexWorkSet3 = vertexWorkSet2;
			while (vertexWorkSet3 != null && vertexWorkSet3.dispatcher != null)
			{
				vertexWorkSet3.dispatcher.list.AddRange(vertexWorkSet3.list);
				vertexWorkSet3.dispatcher.pending--;
				if (vertexWorkSet3.dispatcher.pending > 0)
				{
					break;
				}
				vertexWorkSet3.dispatcher.done = true;
				vertexWorkSet3 = vertexWorkSet3.dispatcher;
			}
			continue;
			IL_00ef:
			double num5;
			double num4 = (num5 = double.MaxValue);
			for (int k = 0; k < input2.Count; k++)
			{
				if (num3 == k || num3 == Index(k - 1, input2.Count) || num3 == Index(k + 1, input2.Count))
				{
					continue;
				}
				Vector2 a = At(num3 - 1, input2);
				Vector2 b = At(num3, input2);
				Vector2 c = At(k, input2);
				Vector2 c2 = At(k - 1, input2);
				bool flag = Left(a, b, c);
				bool flag2 = Right(a, b, c2);
				bool num6 = IsCollinear(ref a, ref b, ref c);
				bool flag3 = IsCollinear(ref a, ref b, ref c2);
				if (num6 || flag3)
				{
					double num7 = SquareDist(b, c);
					if (num7 < num4)
					{
						num4 = num7;
						vector = c;
						num2 = k - 1;
					}
					num7 = SquareDist(b, c2);
					if (num7 < num4)
					{
						num4 = num7;
						vector = c2;
						num2 = k;
					}
				}
				else if (flag && flag2)
				{
					Vector2 vector4 = LineIntersect(At(num3 - 1, input2), At(num3, input2), At(k, input2), At(k - 1, input2));
					if (Right(At(num3 + 1, input2), At(num3, input2), vector4))
					{
						double num7 = SquareDist(At(num3, input2), vector4);
						if (num7 < num4)
						{
							num4 = num7;
							vector = vector4;
							num2 = k;
						}
					}
				}
				Vector2 a2 = At(num3 + 1, input2);
				Vector2 c3 = At(k + 1, input2);
				bool flag4 = Left(a2, b, c3);
				bool flag5 = Right(a2, b, c);
				bool num8 = IsCollinear(ref a2, ref b, ref c3);
				bool flag6 = IsCollinear(ref a2, ref b, ref c);
				if (num8 || flag6)
				{
					double num7 = SquareDist(b, c3);
					if (num7 < num5)
					{
						num5 = num7;
						vector2 = c3;
						i = k + 1;
					}
					num7 = SquareDist(At(num3, input2), At(k, input2));
					if (num7 < num5)
					{
						num5 = num7;
						vector2 = c;
						i = k;
					}
				}
				else
				{
					if (!(flag4 && flag5))
					{
						continue;
					}
					Vector2 vector4 = LineIntersect(At(num3 + 1, input2), At(num3, input2), At(k, input2), At(k + 1, input2));
					if (Left(At(num3 - 1, input2), At(num3, input2), vector4))
					{
						double num7 = SquareDist(At(num3, input2), vector4);
						if (num7 < num5)
						{
							num5 = num7;
							i = k;
							vector2 = vector4;
						}
					}
				}
			}
			VertexChain vertexChain;
			VertexChain vertexChain2;
			if (num2 == (i + 1) % input2.Count)
			{
				Vector2 item = (vector + vector2) / 2f;
				vertexChain = Copy(num3, i, input2);
				vertexChain.Add(item);
				vertexChain2 = Copy(num2, num3, input2);
				vertexChain2.Add(item);
			}
			else
			{
				double num9 = 0.0;
				double num10 = num2;
				for (; i < num2; i += input2.Count)
				{
				}
				for (int l = num2; l <= i; l++)
				{
					if (CanSee(num3, l, input2))
					{
						double num11 = 1f / (SquareDist(At(num3, input2), At(l, input2)) + 1f);
						Vector2 prev2 = At(l - 1, input2);
						Vector2 vector5 = At(l, input2);
						Vector2 next2 = At(l + 1, input2);
						num11 = ((!Reflex(prev2, vector5, next2)) ? (num11 + 1.0) : ((!RightOn(At(l - 1, input2), At(l, input2), At(num3, input2)) || !LeftOn(At(l + 1, input2), At(l, input2), At(num3, input2))) ? (num11 + 2.0) : (num11 + 3.0)));
						if (num11 > num9)
						{
							num10 = l;
							num9 = num11;
						}
					}
				}
				vertexChain = Copy(num3, (int)num10, input2);
				vertexChain2 = Copy((int)num10, num3, input2);
			}
			if (vertexChain.Count < vertexChain2.Count)
			{
				queue.Enqueue(new VertexWorkSet(vertexWorkSet2, vertexChain));
				queue.Enqueue(new VertexWorkSet(vertexWorkSet2, vertexChain2));
				vertexWorkSet2.pending += 2;
			}
			else
			{
				queue.Enqueue(new VertexWorkSet(vertexWorkSet2, vertexChain2));
				queue.Enqueue(new VertexWorkSet(vertexWorkSet2, vertexChain));
				vertexWorkSet2.pending += 2;
			}
		}
		return vertexWorkSet.list;
	}

	private static VertexChain Copy(int i, int j, VertexChain vertices)
	{
		VertexChain vertexChain = new VertexChain();
		while (j < i)
		{
			j += vertices.Count;
		}
		while (i <= j)
		{
			vertexChain.Add(At(i, vertices));
			i++;
		}
		return vertexChain;
	}

	private static bool FloatEquals(float value1, float value2)
	{
		return Mathf.Abs(value1 - value2) <= Mathf.Epsilon;
	}

	private static bool FloatInRange(float value, float min, float max)
	{
		if (value >= min)
		{
			return value <= max;
		}
		return false;
	}

	private static int Index(int i, int size)
	{
		if (i >= 0)
		{
			return i % size;
		}
		return size - -i % size;
	}

	private static bool IsCollinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance = 0f)
	{
		return FloatInRange(Area(ref a, ref b, ref c), 0f - tolerance, tolerance);
	}

	private static bool Left(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) > 0f;
	}

	private static bool LeftOn(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) >= 0f;
	}

	private static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 point)
	{
		point = default(Vector2);
		float num = point4.y - point3.y;
		float num2 = point2.x - point1.x;
		float num3 = point4.x - point3.x;
		float num4 = point2.y - point1.y;
		float num5 = num * num2 - num3 * num4;
		if (!(num5 >= 0f - Mathf.Epsilon) || !(num5 <= Mathf.Epsilon))
		{
			float num6 = point1.y - point3.y;
			float num7 = point1.x - point3.x;
			float num8 = 1f / num5;
			float num9 = num3 * num6 - num * num7;
			num9 *= num8;
			if (!firstIsSegment || (num9 >= 0f && num9 <= 1f))
			{
				float num10 = num2 * num6 - num4 * num7;
				num10 *= num8;
				if ((!secondIsSegment || (num10 >= 0f && num10 <= 1f)) && (num9 != 0f || num10 != 0f))
				{
					point.x = point1.x + num9 * num2;
					point.y = point1.y + num9 * num4;
					return true;
				}
			}
		}
		return false;
	}

	private static Vector2 LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
	{
		Vector2 zero = Vector2.zero;
		float num = p2.y - p1.y;
		float num2 = p1.x - p2.x;
		float num3 = num * p1.x + num2 * p1.y;
		float num4 = q2.y - q1.y;
		float num5 = q1.x - q2.x;
		float num6 = num4 * q1.x + num5 * q1.y;
		float num7 = num * num5 - num4 * num2;
		if (!FloatEquals(num7, 0f))
		{
			zero.x = (num5 * num3 - num2 * num6) / num7;
			zero.y = (num * num6 - num4 * num3) / num7;
		}
		return zero;
	}

	private static bool Reflex(Vector2 prev, Vector2 on, Vector2 next)
	{
		if (IsCollinear(ref prev, ref on, ref next))
		{
			return false;
		}
		return Right(prev, on, next);
	}

	private static bool Right(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) < 0f;
	}

	private static bool RightOn(Vector2 a, Vector2 b, Vector2 c)
	{
		return Area(ref a, ref b, ref c) <= 0f;
	}

	private static float SquareDist(Vector2 a, Vector2 b)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		return num * num + num2 * num2;
	}
}
