using System;
using System.Collections.Generic;
using System.Linq;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World.Terrain;

[Serializable]
public class Chunk
{
	public class TerrainPoints
	{
		public Vector3[] points;

		public Vector2[] otherData;

		public TerrainPoints(int pointCount)
		{
			points = new Vector3[pointCount];
			otherData = new Vector2[pointCount];
		}
	}

	public Transform terrainTransform;

	public Transform waterTransform;

	public Mesh terrainMesh;

	public Mesh waterMesh;

	public Chunk(Transform chunkPrefab, double from, double size, int pointCount, Planet planet, (Material, Material) material, bool useTerrainUV, Transform parent, bool offsetTerrain)
	{
		float num = (float)planet.Radius;
		terrainTransform = UnityEngine.Object.Instantiate(chunkPrefab, parent).transform;
		Double2 offsetAmount;
		double[] angles;
		double[] height;
		TerrainPoints terrainPoints = planet.data.terrain.GetTerrainPoints(from, size, pointCount, offsetTerrain, forCollider: false, planet, out offsetAmount, out angles, out height);
		int[] indices = GetIndices(terrainPoints.points.Length);
		terrainMesh = CreateMesh(terrainTransform, terrainPoints.points, indices, "Default", 10, material.Item1);
		terrainTransform.localPosition = offsetAmount;
		double num2 = size / (double)(pointCount - 1);
		int num3 = (int)((from + num2 * (double)(terrainPoints.points.Length - 1) * 0.5) * (double)planet.commonDenominator * 6.0);
		Vector2[] array = new Vector2[terrainPoints.points.Length];
		for (int i = 0; i < terrainPoints.points.Length - 1; i++)
		{
			float x = (float)((from + num2 * (double)i) * (double)planet.commonDenominator * 6.0 - (double)num3);
			array[i + 1] = new Vector2(x, 0f);
		}
		array[0] = Vector2.up;
		Vector2[] array2 = null;
		if (planet.data.hasWater)
		{
			array2 = new Vector2[terrainPoints.points.Length];
			for (int j = 0; j < terrainPoints.points.Length - 1; j++)
			{
				double num4 = (from + num2 * (double)j) * 6.0;
				array2[j + 1] = new Vector2((float)num4, 1f - (float)((double)num / ((double)num - height[j])));
			}
			array2[0] = Vector2.up;
		}
		float planetTextureCutout = planet.data.terrain.TERRAIN_TEXTURE_DATA.planetTextureCutout;
		Vector2 vector = offsetAmount;
		Matrix2x2 matrix2x = Matrix2x2.Angle((0f - planet.data.terrain.TERRAIN_TEXTURE_DATA.planetTextureRotation) * (MathF.PI / 180f));
		Vector2[] array3 = new Vector2[terrainPoints.points.Length];
		for (int k = 0; k < terrainPoints.points.Length; k++)
		{
			Vector2 vector2 = (Vector2)terrainPoints.points[k] + vector;
			array3[k] = ((!planet.DontDistortTextureCutout) ? vector2.normalized : (vector2 / num)) * matrix2x * (planetTextureCutout * 0.5f) + new Vector2(0.5f, 0.5f);
		}
		if (useTerrainUV)
		{
			terrainMesh.uv = array;
			terrainMesh.uv2 = array3;
			terrainMesh.uv3 = terrainPoints.otherData;
			if (planet.data.hasWater)
			{
				Vector2[] array4 = new Vector2[pointCount + 1];
				for (int l = 0; l < pointCount; l++)
				{
					array4[l + 1] = new Vector2((float)height[l], 0f);
				}
				terrainMesh.uv4 = array4;
				terrainMesh.uv5 = array2;
			}
		}
		if (!planet.data.hasWater)
		{
			return;
		}
		waterTransform = new GameObject("Water").transform;
		waterTransform.parent = terrainTransform;
		waterTransform.localScale = Vector3.one;
		waterTransform.localPosition = Vector3.zero;
		waterTransform.gameObject.AddComponent<MeshFilter>();
		waterTransform.gameObject.AddComponent<MeshRenderer>();
		Vector3[] array5 = new Vector3[pointCount + 1];
		for (int m = 0; m < pointCount; m++)
		{
			array5[m + 1] = Double2.CosSin(angles[m], num) - offsetAmount;
		}
		array5[0] = -offsetAmount;
		waterMesh = CreateMesh(waterTransform, array5, indices, "Default", 10, material.Item2);
		if (useTerrainUV)
		{
			waterMesh.uv = array;
			for (int n = 1; n < terrainPoints.points.Length; n++)
			{
				array3[n] = ((Vector2)array5[n] + vector).normalized * matrix2x * (planetTextureCutout * 0.5f) + new Vector2(0.5f, 0.5f);
			}
			waterMesh.uv2 = array3;
			Vector2[] array6 = new Vector2[pointCount + 1];
			for (int num5 = 0; num5 < pointCount; num5++)
			{
				float num6 = (float)((double)num / ((double)num + height[num5]));
				array6[num5 + 1] = new Vector2((float)height[num5], (1f - num6) * num);
			}
			array6[0] = Vector2.up * num;
			waterMesh.uv3 = array6;
			for (int num7 = 0; num7 < terrainPoints.points.Length - 1; num7++)
			{
				array2[num7 + 1].y = 0f;
			}
			waterMesh.uv4 = array2;
		}
		else
		{
			Vector2[] array7 = new Vector2[terrainPoints.points.Length];
			array7[0] = Vector2.up;
			waterMesh.uv = array7;
		}
	}

	private static Mesh CreateMesh(Transform transform, Vector3[] points, int[] indices, string sortingLayer, int sortingOrder, Material material)
	{
		Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = points.ToArray();
		mesh.triangles = indices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		Renderer component = transform.GetComponent<Renderer>();
		component.sortingLayerName = sortingLayer;
		component.sortingOrder = sortingOrder;
		component.material = material;
		return mesh;
	}

	private int[] GetIndices(int length)
	{
		List<int> list = new List<int>
		{
			Capacity = (length - 2) * 3
		};
		for (int i = 0; i < length - 2; i++)
		{
			list.Add(0);
			list.Add(i + 2);
			list.Add(i + 1);
		}
		return list.ToArray();
	}

	public static double GetAngleBetweenPoints(double size, int pointCount)
	{
		return size * Math.PI * 2.0 / (double)(pointCount - 1);
	}
}
