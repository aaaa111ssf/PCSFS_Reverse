using System;
using UnityEngine;

namespace SFS.World.PlanetModules;

[Serializable]
public class HeightMap
{
	public float[] points = new float[2] { 0f, 1f };

	public HeightMap()
	{
	}

	public HeightMap(float[] points)
	{
		this.points = points;
	}

	public HeightMap(Texture2D image)
	{
		points = new float[image.width];
		int width = image.width;
		int height = image.height;
		Color32[] colors = image.GetPixels32();
		for (int i = 0; i < points.Length; i++)
		{
			points[points.Length - i - 1] = GetHeightAtX(i);
		}
		float GetHeightAtX(int x)
		{
			for (int j = 0; j < height; j++)
			{
				float num = (float)(int)colors[j * width + x].a / 255f;
				if (num < 1f)
				{
					return ((float)j + num) / (float)height;
				}
			}
			return 1f;
		}
	}

	public float Evaluate(float a)
	{
		a *= (float)(points.Length - 1);
		int num = (int)a % (points.Length - 1);
		float num2 = a % 1f;
		return points[num] * (1f - num2) + points[num + 1] * num2;
	}

	public double EvaluateDoubleOut(double a)
	{
		a *= (double)(points.Length - 1);
		int num = (int)a % (points.Length - 1);
		float num2 = (float)(a % 1.0);
		return points[num] * (1f - num2) + points[num + 1] * num2;
	}

	public float EvaluateClamped(float a)
	{
		if (a >= 1f)
		{
			return points[^1];
		}
		if (a <= 0f)
		{
			return points[0];
		}
		int num = (int)(a * (float)(points.Length - 1));
		float num2 = a * (float)(points.Length - 1) % 1f;
		return points[num] * (1f - num2) + points[num + 1] * num2;
	}
}
