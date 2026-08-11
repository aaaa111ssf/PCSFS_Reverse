using System;
using System.Collections.Generic;
using UnityEngine;

public class GLDrawer : MonoBehaviour
{
	public static GLDrawer main;

	public Material material;

	private Dictionary<float, Material> sortedMaterials = new Dictionary<float, Material>();

	private Dictionary<float, List<LineCommand>> lineBatches = new Dictionary<float, List<LineCommand>>();

	private Dictionary<float, List<CircleCommand>> circleBatches = new Dictionary<float, List<CircleCommand>>();

	public List<I_GLDrawer> drawers = new List<I_GLDrawer>();

	private void Awake()
	{
		main = this;
	}

	public static void Register(I_GLDrawer drawer)
	{
		main.drawers.Add(drawer);
	}

	public static void Unregister(I_GLDrawer drawer)
	{
		main.drawers.Remove(drawer);
	}

	private void OnPostRender()
	{
		if (!main || drawers.Count == 0)
		{
			return;
		}
		foreach (I_GLDrawer drawer in drawers)
		{
			drawer.Draw();
		}
		FlushLines();
		FlushCircles();
	}

	public static void DrawOutline(Vector2[] vertices, float width, Color color, float sortingOrder)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			DrawCircle(vertices[i], width * 0.5f, 12, color, sortingOrder);
			DrawLine(vertices[i], vertices[(i + 1) % vertices.Length], color, width, sortingOrder);
		}
	}

	public static void DrawLine(Vector3 start, Vector3 end, Color color, float width, float sortingOrder = 1f)
	{
		if (!main.lineBatches.TryGetValue(sortingOrder, out var value))
		{
			value = new List<LineCommand>();
			main.lineBatches.Add(sortingOrder, value);
		}
		value.Add(new LineCommand
		{
			start = start,
			end = end,
			color = color,
			width = width
		});
	}

	public static void DrawCircle(Vector2 position, float radius, int resolution, Color color, float sortingOrder = 1f)
	{
		if (!main.circleBatches.TryGetValue(sortingOrder, out var value))
		{
			value = new List<CircleCommand>();
			main.circleBatches.Add(sortingOrder, value);
		}
		value.Add(new CircleCommand
		{
			position = position,
			radius = radius,
			resolution = resolution,
			color = color
		});
	}

	public static void DrawCircles(List<Vector2> positions, float radius, int resolution, Color color, float sortingOrder = 1f)
	{
		for (int i = 0; i < positions.Count; i++)
		{
			DrawCircle(positions[i], radius, resolution, color, sortingOrder);
		}
	}

	private void FlushLines()
	{
		foreach (KeyValuePair<float, List<LineCommand>> lineBatch in lineBatches)
		{
			float key = lineBatch.Key;
			List<LineCommand> value = lineBatch.Value;
			if (value.Count == 0)
			{
				continue;
			}
			Material material = GetMaterial(key);
			if (!material)
			{
				value.Clear();
				continue;
			}
			material.SetPass(0);
			GL.Begin(7);
			for (int i = 0; i < value.Count; i++)
			{
				LineCommand lineCommand = value[i];
				Vector3 start = lineCommand.start;
				Vector3 end = lineCommand.end;
				Vector3 vector = Vector2.Perpendicular((start - end).normalized) * lineCommand.width * 0.5f;
				GL.Color(lineCommand.color);
				GL.Vertex(start - vector);
				GL.Vertex(end - vector);
				GL.Vertex(end + vector);
				GL.Vertex(start + vector);
			}
			GL.End();
			value.Clear();
		}
	}

	private void FlushCircles()
	{
		foreach (KeyValuePair<float, List<CircleCommand>> circleBatch in circleBatches)
		{
			float key = circleBatch.Key;
			List<CircleCommand> value = circleBatch.Value;
			if (value.Count == 0)
			{
				continue;
			}
			Material material = GetMaterial(key);
			if (!material)
			{
				value.Clear();
				continue;
			}
			material.SetPass(0);
			GL.Begin(7);
			for (int i = 0; i < value.Count; i++)
			{
				CircleCommand circleCommand = value[i];
				float num = MathF.PI * 2f / (float)circleCommand.resolution;
				for (int j = 0; j < circleCommand.resolution; j++)
				{
					int num2 = (j + 1) % circleCommand.resolution;
					Vector2 position = circleCommand.position;
					Vector2 vector = circleCommand.position + new Vector2(Mathf.Cos(num * (float)num2), Mathf.Sin(num * (float)num2)) * circleCommand.radius;
					Vector2 vector2 = circleCommand.position + new Vector2(Mathf.Cos(num * (float)j), Mathf.Sin(num * (float)j)) * circleCommand.radius;
					GL.Color(circleCommand.color);
					GL.Vertex(position);
					GL.Vertex(vector);
					GL.Vertex(vector2);
					GL.Vertex(position);
				}
			}
			GL.End();
			value.Clear();
		}
	}

	private Material GetMaterial(float sortingOrder)
	{
		if (!main)
		{
			return null;
		}
		if (!sortedMaterials.TryGetValue(sortingOrder, out var value))
		{
			if (!material)
			{
				return null;
			}
			value = new Material(material);
			value.SetFloat("_Depth", sortingOrder);
			sortedMaterials.Add(sortingOrder, value);
		}
		return value;
	}
}
