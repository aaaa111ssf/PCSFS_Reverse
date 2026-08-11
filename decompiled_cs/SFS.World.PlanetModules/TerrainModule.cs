using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utility;
using Newtonsoft.Json;
using SFS.World.Terrain;
using SFS.WorldBase;
using UnityEngine;

namespace SFS.World.PlanetModules;

[Serializable]
public class TerrainModule
{
	[Serializable]
	public class TerrainTexture
	{
		public string planetTexture = "None";

		public float planetTextureCutout = -1f;

		public float planetTextureRotation;

		public bool planetTextureDontDistort;

		public string surfaceTexture_A = "None";

		public Vector2 surfaceTextureSize_A = new Vector2
		{
			x = -1f,
			y = -1f
		};

		public string surfaceTexture_B = "None";

		public Vector2 surfaceTextureSize_B = new Vector2
		{
			x = -1f,
			y = -1f
		};

		public string terrainTexture_C = "None";

		public Vector2 terrainTextureSize_C = new Vector2
		{
			x = -1f,
			y = -1f
		};

		[Space]
		public float surfaceLayerSize = -1f;

		[Space]
		public float minFade = -1f;

		public float maxFade = -1f;

		[Space]
		public float shadowIntensity = -1f;

		public float shadowHeight = -1f;
	}

	[Serializable]
	public class FlatZone
	{
		public double height;

		public double angle;

		public double width;

		public double transition;
	}

	[Serializable]
	public class RockData
	{
		public string rockType;

		public float rockDensity;

		public float minSize;

		public float maxSize;

		public float powerCurve;

		public float maxAngle;
	}

	public TerrainTexture TERRAIN_TEXTURE_DATA;

	public string[] terrainFormula;

	public Dictionary<Difficulty.DifficultyType, string[]> terrainFormulaDifficulties = new Dictionary<Difficulty.DifficultyType, string[]>();

	public string[] textureFormula;

	public float verticeSize;

	public bool collider = true;

	public FlatZone[] flatZones = new FlatZone[0];

	public Dictionary<Difficulty.DifficultyType, FlatZone[]> flatZonesDifficulties = new Dictionary<Difficulty.DifficultyType, FlatZone[]>();

	public RockData rocks;

	[JsonIgnore]
	public TerrainSampler.Executor terrainSampler;

	[JsonIgnore]
	public TerrainSampler.Executor textureSampler;

	public void SetupSamplers(string codeName, I_MsgLogger log)
	{
		string[] formula = terrainFormulaDifficulties?.At(Base.worldBase.settings.difficulty.difficulty) ?? terrainFormulaDifficulties?.At(Difficulty.DifficultyType.Normal) ?? terrainFormula ?? new string[0];
		try
		{
			terrainSampler = TerrainSampler.Compiler.Compile(formula, log);
		}
		catch
		{
			log.Log("ERROR: terrain formula: " + codeName);
		}
		try
		{
			textureSampler = TerrainSampler.Compiler.Compile(textureFormula, log);
		}
		catch
		{
			log.Log("ERROR: texture formula: " + codeName);
		}
	}

	public Chunk.TerrainPoints GetTerrainPoints(double from_Angle_01, double size_Angle_01, int pointCount, bool offset, bool forCollider, Planet planet, out Double2 offsetAmount, out double[] angles, out double[] height)
	{
		from_Angle_01 = (from_Angle_01 + 1.0) % 1.0;
		double num = from_Angle_01 * (Math.PI * 2.0);
		double num2 = (from_Angle_01 + size_Angle_01) * (Math.PI * 2.0);
		double angleBetweenPoints = Chunk.GetAngleBetweenPoints(size_Angle_01, pointCount);
		float num3 = (float)(planet.Radius * angleBetweenPoints) * 8f;
		offsetAmount = (offset ? WorldView.GetOffset(Double2.CosSin(num, planet.Radius), 1000.0) : Double2.zero);
		angles = new double[pointCount + 1];
		for (int i = 0; i < angles.Length; i++)
		{
			angles[i] = num + angleBetweenPoints * (double)i;
		}
		height = TerrainSampler.GetTerrainSamples(planet, angles, num, num2);
		double[] textureSamples = TerrainSampler.GetTextureSamples(planet.data, angles);
		Chunk.TerrainPoints terrainPoints = new Chunk.TerrainPoints(pointCount + ((!forCollider) ? 1 : 2));
		for (int j = 0; j < pointCount; j++)
		{
			double radius = planet.Radius + height[j];
			terrainPoints.points[j + 1] = (Double2.CosSin(angles[j], radius) - offsetAmount).ToVector3;
			terrainPoints.otherData[j + 1] = new Vector2((float)(height[j] - height[j + 1]) / num3, (float)textureSamples[j]);
		}
		if (forCollider)
		{
			terrainPoints.points[0] = (Double2.CosSin(num, Math.Max(1.0, planet.Radius + height[0] - 1000.0)) - offsetAmount).ToVector3;
			terrainPoints.points[^1] = (Double2.CosSin(num2, Math.Max(1.0, planet.Radius + height[^2] - 1000.0)) - offsetAmount).ToVector3;
		}
		else
		{
			terrainPoints.points[0] = -offsetAmount.ToVector3;
		}
		return terrainPoints;
	}

	public double GetMaxTerrainHeight(Planet planet)
	{
		double[] array = new double[1001];
		double num = Math.PI / 500.0;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = num * (double)i;
		}
		return TerrainSampler.GetTerrainSamples(planet, array, 0.0, Math.PI * 2.0).Max();
	}

	public double GetVerticeSize(int LOD, int LOD_Max)
	{
		return Math.Pow(2.55, LOD_Max - LOD) * (double)verticeSize;
	}

	public double GetLoadDistance(int LOD, int LOD_Max)
	{
		return Math.Pow(2.05, LOD_Max - LOD) * 250.0;
	}

	public double GetChunkSize_Angular(int baseChunkCount, int LOD)
	{
		return 1.0 / (double)((int)Math.Pow(2.0, LOD) * baseChunkCount);
	}
}
