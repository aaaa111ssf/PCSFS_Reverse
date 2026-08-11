using System;
using SFS.World.PlanetModules;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World;

public class Rings : MonoBehaviour
{
	public static Rings Create(Planet planet, Transform parent, Transform ringsPrefab, Material material)
	{
		Transform obj = UnityEngine.Object.Instantiate(ringsPrefab.transform, parent, worldPositionStays: true);
		obj.name = planet.codeName + " Rings";
		obj.localPosition = Vector3.forward * planet.data.rings.positionZ;
		obj.transform.localScale = Vector3.one;
		obj.GetComponent<MeshRenderer>().material = material;
		obj.GetComponent<MeshFilter>().sharedMesh = CreateMesh(planet);
		return obj.gameObject.AddComponent<Rings>();
	}

	private static Mesh CreateMesh(Planet planet)
	{
		RingsModule rings = planet.data.rings;
		float num = (float)rings.startRadius;
		float num2 = (float)rings.endRadius + (float)(rings.endRadius - rings.startRadius) * 0.01f;
		Vector3[] array = new Vector3[2002];
		Vector2[] array2 = new Vector2[2002];
		for (int i = 0; i <= 1000; i++)
		{
			float f = (float)i * -MathF.PI * 2f / 1000f;
			Vector2 vector = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
			array[i * 2] = vector * num;
			array[i * 2 + 1] = vector * num2;
			array2[i * 2] = vector;
			array2[i * 2 + 1] = vector * 2f;
		}
		int[] array3 = new int[6000];
		for (int j = 0; j < 1000; j++)
		{
			int num3 = j * 6;
			array3[num3] = j * 2;
			array3[num3 + 1] = j * 2 + 1;
			array3[num3 + 2] = (j + 1) * 2;
			array3[num3 + 3] = (j + 1) * 2;
			array3[num3 + 4] = j * 2 + 1;
			array3[num3 + 5] = (j + 1) * 2 + 1;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = array3;
		mesh.RecalculateBounds();
		mesh.name = planet.name + " Ring Mesh";
		return mesh;
	}

	public void SetLayer(string layer)
	{
		base.gameObject.layer = LayerMask.NameToLayer(layer);
	}
}
