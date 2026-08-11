using System;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World;

public class FrontClouds : MonoBehaviour
{
	public static FrontClouds Create(Planet planet, Transform parent, Transform cloudsPrefab, Material material)
	{
		Transform obj = UnityEngine.Object.Instantiate(cloudsPrefab.transform, parent, worldPositionStays: true);
		obj.name = planet.codeName + " Front Clouds";
		obj.localPosition = Vector3.forward * planet.data.frontClouds.positionZ;
		obj.transform.localScale = Vector3.one;
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		component.material = material;
		component.sortingLayerName = "Default";
		component.sortingOrder = 200;
		obj.GetComponent<MeshFilter>().sharedMesh = CreateMesh(planet);
		return obj.gameObject.AddComponent<FrontClouds>();
	}

	private static Mesh CreateMesh(Planet planet)
	{
		float num = (float)(planet.Radius + (double)planet.data.frontClouds.height);
		Vector3[] array = new Vector3[502];
		Vector2[] array2 = new Vector2[502];
		for (int i = 0; i <= 501; i++)
		{
			float f = (float)i * -MathF.PI * 2f / 501f;
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			array[i] = vector * num;
			array2[i] = vector * 0.5f + Vector2.one * 0.5f;
		}
		int[] array3 = new int[1500];
		for (int j = 0; j < 500; j++)
		{
			int num2 = j * 3;
			array3[num2] = 0;
			array3[num2 + 1] = j + 1;
			array3[num2 + 2] = j + 2;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateBounds();
		mesh.name = planet.name + " Front Clouds Mesh";
		return mesh;
	}

	public void SetLayer(string layer)
	{
		base.gameObject.layer = LayerMask.NameToLayer(layer);
	}
}
