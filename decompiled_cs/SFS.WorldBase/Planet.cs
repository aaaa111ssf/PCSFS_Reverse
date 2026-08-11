using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Translations;
using SFS.World;
using SFS.World.Maps;
using SFS.World.PlanetModules;
using UnityEngine;

namespace SFS.WorldBase;

public class Planet : MonoBehaviour
{
	public string codeName;

	public Transform mapHolder;

	public MapPlanet mapPlanet;

	public Landmark[] landmarks;

	public Trajectory trajectory;

	public Orbit orbit;

	public Planet parentBody;

	public Planet[] satellites;

	public double mass;

	public double SOI;

	public double maxTerrainHeight;

	public int commonDenominator = 1;

	public int surfaceWavesRepeat;

	public Texture2D planetTexture;

	public Texture2D waterTexture;

	public Material terrainMaterial;

	public Material waterMaterial;

	public Material atmosphereMaterial;

	public Material frontCloudsMaterial;

	public Material ringsMaterial;

	public int orbitalDepth;

	public int satelliteIndex;

	public PlanetData data;

	public double Radius => data.basics.radius;

	public double SurfaceArea => Radius * (Math.PI * 2.0);

	public double AtmosphereHeightPhysics
	{
		get
		{
			if (!HasAtmospherePhysics)
			{
				return double.NegativeInfinity;
			}
			return data.atmospherePhysics.height;
		}
	}

	public double TimewarpRadius_Ascend => Radius + data.basics.timewarpHeight.Round(Math.Pow(10.0, Math.Floor(Math.Log10(data.basics.timewarpHeight / 2.0))) / 2.0);

	public double TimewarpRadius_Descend => Radius + Math.Max(maxTerrainHeight, Math.Max(data.basics.timewarpHeight, AtmosphereHeightPhysics));

	public double OrbitRadius => Radius + Math.Max(AtmosphereHeightPhysics, maxTerrainHeight);

	public bool HasParent => parentBody != null;

	public bool HasAtmospherePhysics => data.hasAtmospherePhysics;

	public bool HasAtmosphereVisuals => data.hasAtmosphereVisuals;

	public bool HasFrontClouds => data.hasFrontClouds;

	public bool HasRings => data.hasRings;

	public bool DontDistortTextureCutout
	{
		get
		{
			if (!data.hasWater)
			{
				if (data.hasTerrain)
				{
					return data.terrain.TERRAIN_TEXTURE_DATA.planetTextureDontDistort;
				}
				return false;
			}
			return true;
		}
	}

	public double RewardMultiplier => 1.0;

	public Field DisplayName { get; private set; }

	private void OnStart()
	{
		Loc.OnChange += new Action(UpdateName);
	}

	private void OnDestroy()
	{
		Loc.OnChange -= new Action(UpdateName);
		if (terrainMaterial != null)
		{
			UnityEngine.Object.Destroy(terrainMaterial);
		}
		if (waterMaterial != null)
		{
			UnityEngine.Object.Destroy(waterMaterial);
		}
		if (atmosphereMaterial != null)
		{
			UnityEngine.Object.Destroy(atmosphereMaterial);
		}
		if (frontCloudsMaterial != null)
		{
			UnityEngine.Object.Destroy(frontCloudsMaterial);
		}
		if (ringsMaterial != null)
		{
			UnityEngine.Object.Destroy(ringsMaterial);
		}
	}

	private void UpdateName()
	{
		SFS_Translation main = Loc.main;
		DisplayName = codeName switch
		{
			"Sun" => main.Sun, 
			"Mercury" => main.Mercury, 
			"Venus" => main.Venus, 
			"Earth" => main.Earth, 
			"Moon" => main.Moon, 
			"Near_Earth_Asteroid" => main.Near_Earth_Asteroid, 
			"Mars" => main.Mars, 
			"Phobos" => main.Phobos, 
			"Deimos" => main.Deimos, 
			"Ceres" => main.Ceres, 
			"Jupiter" => main.Jupiter, 
			"Io" => main.Io, 
			"Europa" => main.Europa, 
			"Ganymede" => main.Ganymede, 
			"Callisto" => main.Callisto, 
			"Thebe" => main.Thebe, 
			"Saturn" => main.Saturn, 
			"Pan" => main.Pan, 
			"Enceladus" => main.Enceladus, 
			"Tethys" => main.Tethys, 
			"Dione" => main.Dione, 
			"Rhea" => main.Rhea, 
			"Titan" => main.Titan, 
			"Iapetus" => main.Iapetus, 
			"Mimas" => main.Mimas, 
			"Uranus" => main.Uranus, 
			"Miranda" => main.Miranda, 
			"Ariel" => main.Ariel, 
			"Umbriel" => main.Umbriel, 
			"Titania" => main.Titania, 
			"Oberon" => main.Oberon, 
			"Puck" => main.Puck, 
			"Neptune" => main.Neptune, 
			"Proteus" => main.Proteus, 
			"Triton" => main.Triton, 
			"Naiad" => main.Naiad, 
			"Pluto" => main.Pluto, 
			"Charon" => main.Charon, 
			"Nix" => main.Nix, 
			"Hydra" => main.Hydra, 
			_ => Field.Text(Application.isEditor ? ("*" + codeName) : codeName), 
		};
	}

	public double GetGravity(double radius)
	{
		return mass / (radius * radius);
	}

	public Double2 GetGravity(Double2 position)
	{
		return -position.normalized * (mass / position.sqrMagnitude);
	}

	public double GetEscapeVelocity(double radius)
	{
		return Math.Sqrt(2.0 * mass / radius);
	}

	public Location GetLocation(double time)
	{
		if (!(parentBody != null))
		{
			return new Location(time, this, Double2.zero, Double2.zero);
		}
		return orbit.GetLocation(time);
	}

	public Double2 GetSolarSystemPosition()
	{
		return GetSolarSystemPosition(WorldTime.main.worldTime);
	}

	public Double2 GetSolarSystemPosition(double time)
	{
		if (!data.hasOrbit)
		{
			return Double2.zero;
		}
		return GetLocation(time).position + parentBody.GetSolarSystemPosition(time);
	}

	public int GetVerticeCount(double size_Angular, double verticeSize)
	{
		return Math.Max((int)(SurfaceArea * size_Angular / verticeSize), 10);
	}

	public int GetMaxLOD()
	{
		int num = 0;
		double num2 = Radius * (Math.PI * 2.0) / 8.0;
		num2 /= 2.0;
		num++;
		while (num2 > 120.0)
		{
			num2 /= 2.0;
			num++;
		}
		return num;
	}

	public bool IsInsideTerrain(Double2 position, double threshold, bool clampToWater)
	{
		if (position.Mag_MoreThan(Radius + maxTerrainHeight))
		{
			return false;
		}
		double a = Radius + GetTerrainHeightAtAngle(position.AngleRadians, clampToWater) - threshold;
		return position.Mag_LessThan(a);
	}

	public Double2 GetTerrainNormal(Double2 globalPosition)
	{
		double angleRadians = globalPosition.AngleRadians;
		double num = 0.1 / SurfaceArea;
		double angleRadians2 = angleRadians + num;
		double angleRadians3 = angleRadians - num;
		Double2 @double = Double2.CosSin(angleRadians2, Radius + GetTerrainHeightAtAngle(angleRadians2, clampToWater: false));
		return (Double2.CosSin(angleRadians3, Radius + GetTerrainHeightAtAngle(angleRadians3, clampToWater: false)) - @double).normalized;
	}

	public double GetTerrainHeightAtAngle(double angleRadians, bool clampToWater)
	{
		return GetTerrainHeightAtAngles(new double[1] { angleRadians }, clampToWater)[0];
	}

	public float[] GetTerrainNormals(params double[] angles_Radians)
	{
		double num = 0.5 / SurfaceArea;
		float[] array = new float[angles_Radians.Length];
		double[] array2 = new double[angles_Radians.Length * 2];
		for (int i = 0; i < angles_Radians.Length; i++)
		{
			double num2 = angles_Radians[i];
			double num3 = num2 + num;
			double num4 = num2 - num;
			array2[i * 2] = num3;
			array2[i * 2 + 1] = num4;
		}
		double[] terrainHeightAtAngles = GetTerrainHeightAtAngles(array2, clampToWater: false);
		for (int j = 0; j < angles_Radians.Length; j++)
		{
			double num5 = angles_Radians[j];
			double angleRadians = num5 + num;
			double angleRadians2 = num5 - num;
			Double2 @double = Double2.CosSin(angleRadians, Radius + terrainHeightAtAngles[j * 2]);
			Double2 double2 = Double2.CosSin(angleRadians2, Radius + terrainHeightAtAngles[j * 2 + 1]);
			array[j] = (float)(double2 - @double).AngleDegrees;
		}
		return array;
	}

	public double[] GetTerrainHeightAtAngles(double[] angleRadians, bool clampToWater)
	{
		double[] array = new double[angleRadians.Length];
		for (int i = 0; i < angleRadians.Length; i++)
		{
			array[i] = Kepler.PositiveAngle(angleRadians[i]);
		}
		if (data.hasTerrain)
		{
			double[] terrainSamples = TerrainSampler.GetTerrainSamples(this, array, 0.0, Math.PI * 2.0);
			if (data.hasWater && clampToWater)
			{
				for (int j = 0; j < terrainSamples.Length; j++)
				{
					if (terrainSamples[j] < 0.0)
					{
						terrainSamples[j] = 0.0;
					}
				}
			}
			return terrainSamples;
		}
		return new double[array.Length];
	}

	public bool IsInsideAtmosphere(Double2 position)
	{
		if (!data.hasAtmospherePhysics)
		{
			return false;
		}
		double a = data.basics.radius + data.atmospherePhysics.height;
		return position.Mag_LessThan(a);
	}

	public double GetAtmosphericDensity(double height)
	{
		if (!data.hasAtmospherePhysics)
		{
			return 0.0;
		}
		if (height > AtmosphereHeightPhysics)
		{
			return 0.0;
		}
		return (Math.Exp(height / AtmosphereHeightPhysics * (0.0 - data.atmospherePhysics.curve)) - Math.Exp(0.0 - data.atmospherePhysics.curve)) * data.atmospherePhysics.density;
	}

	public bool IsOutsideSOI(Double2 position)
	{
		return position.Mag_MoreThan(SOI);
	}

	public bool IsInsideSOI(Double2 positionToParent)
	{
		return (positionToParent - orbit.GetLocation(WorldTime.main.worldTime).position).Mag_LessThan(SOI);
	}

	public static double GetTimewarpRadius_AscendDescend(Location location)
	{
		if (!(Double2.Dot(location.position, location.velocity) > 0.0))
		{
			return location.planet.TimewarpRadius_Descend;
		}
		return location.planet.TimewarpRadius_Ascend;
	}

	public void SetupData(string codeName, PlanetData data, Shader terrainShader, Shader waterShader, Shader atmosphereShader, Shader frontCloudsShader, Shader ringsShader, I_MsgLogger log)
	{
		this.codeName = codeName;
		this.data = data;
		OnStart();
		mass = Kepler.GetMass(data.basics.gravity, Radius);
		if (data.hasTerrain)
		{
			TerrainModule.TerrainTexture tERRAIN_TEXTURE_DATA = data.terrain.TERRAIN_TEXTURE_DATA;
			int num = (int)GetRepeat(new Vector2(100f, 100f), useCommonDenominator: false).x;
			int num2 = (int)GetRepeat(tERRAIN_TEXTURE_DATA.surfaceTextureSize_A, useCommonDenominator: false).x;
			int num3 = (int)GetRepeat(tERRAIN_TEXTURE_DATA.surfaceTextureSize_B, useCommonDenominator: false).x;
			int num4 = (int)GetRepeat(tERRAIN_TEXTURE_DATA.terrainTextureSize_C, useCommonDenominator: false).x;
			int num5 = (data.hasWater ? ((int)GetRepeat(data.water.wavesSize, useCommonDenominator: false).x) : int.MaxValue);
			int num6 = new int[5] { num, num2, num3, num4, num5 }.Select(Mathf.Abs).Min();
			commonDenominator = Mathf.CeilToInt((float)num6 / 5f);
			terrainMaterial = CreateTerrainMaterial(tERRAIN_TEXTURE_DATA, terrainShader, log);
			terrainMaterial.SetColor("_Fog", Color.clear);
			if (data.hasWater)
			{
				waterMaterial = CreateWaterMaterial(data.water, waterShader, log);
			}
			data.terrain.SetupSamplers(codeName, log);
			maxTerrainHeight = data.terrain.GetMaxTerrainHeight(this) + 200.0;
		}
		if (data.landmarks != null)
		{
			landmarks = data.landmarks.Select(delegate(LandmarkData landmarkData)
			{
				Landmark landmark = base.gameObject.AddComponent<Landmark>();
				landmark.Initialize(landmarkData, this);
				return landmark;
			}).ToArray();
		}
		else
		{
			landmarks = new Landmark[0];
		}
		if (data.hasAtmosphereVisuals)
		{
			atmosphereMaterial = CreateAtmosphereMaterial(data.atmosphereVisuals, atmosphereShader, log);
		}
		if (data.hasFrontClouds)
		{
			frontCloudsMaterial = CreateFrontCloudsMaterial(data.frontClouds, frontCloudsShader, log);
		}
		if (data.hasRings)
		{
			ringsMaterial = CreateRingsMaterial(data.rings, ringsShader, log);
		}
	}

	private Material CreateAtmosphereMaterial(Atmosphere_Visuals input, Shader shader, I_MsgLogger log)
	{
		Material material = new Material(shader);
		Texture2D texture = Base.planetLoader.GetTexture(input.GRADIENT.texture, log);
		texture.wrapMode = TextureWrapMode.Clamp;
		material.SetTexture("_GradientTex", texture);
		material.SetFloat("_GradientMultiplier", (float)((Radius + input.GRADIENT.height) / input.GRADIENT.height) - 1f);
		Texture2D texture2 = Base.planetLoader.GetTexture(input.CLOUDS.texture, log);
		material.SetTexture("_CouldTex", texture2);
		material.SetFloat("_CloudStartY", (float)((Radius + (double)input.CLOUDS.startHeight + input.GRADIENT.height) / input.GRADIENT.height) - 1f);
		material.SetFloat("_CloudSizeY", (float)(Radius + input.GRADIENT.height) / input.CLOUDS.height);
		material.SetFloat("_CloudSizeX", Mathf.CeilToInt((float)((Radius + (double)input.CLOUDS.startHeight) * (Math.PI * 2.0)) / input.CLOUDS.width));
		material.SetFloat("_Alpha", input.CLOUDS.alpha);
		return material;
	}

	private Material CreateWaterMaterial(WaterModule input, Shader shader, I_MsgLogger log)
	{
		Material material = new Material(shader);
		waterTexture = Base.planetLoader.GetTexture(input.oceanMaskTexture, log);
		if (!waterTexture.isReadable)
		{
			waterTexture = waterTexture.GetReadableCopy();
		}
		material.SetTexture("_OceanTex", waterTexture);
		material.SetTexture("_NoiseTex", Base.planetLoader.noiseTexture);
		material.SetColor("_Shallow", input.shallow);
		material.SetColor("_Deep", input.deep);
		float waterGradientWidthMultiplier = input.waterGradientWidthMultiplier;
		material.SetFloat("_Must", 1f / (input.maskGradient_Water.must * waterGradientWidthMultiplier));
		material.SetFloat("_Cannot", 1f / (input.maskGradient_Water.cannot * waterGradientWidthMultiplier));
		material.SetFloat("_Global", 100000f / ((0f - input.maskGradient_Water.global) * waterGradientWidthMultiplier));
		Vector4 value = new Vector4(waterTexture.width, waterTexture.height, 0f, 0f);
		material.SetVector("_TexSizePixels", value);
		Vector2 vector = new Vector2(15f, 2f);
		material.SetVector("waveRepeat1", GetRepeat(vector * 2f, useCommonDenominator: true));
		material.SetVector("waveRepeat2", GetRepeat(vector * 3f, useCommonDenominator: true));
		material.SetVector("waveRepeat3", GetRepeat(vector * 4f, useCommonDenominator: true));
		surfaceWavesRepeat = (int)GetRepeat(new Vector2(data.water.wavesSize.x, 1f), useCommonDenominator: true).x;
		material.SetFloat("surfaceWavesRepeat", surfaceWavesRepeat);
		material.SetFloat("surfaceWavesHeight", data.water.wavesSize.y);
		material.SetFloat("cutout", data.terrain.TERRAIN_TEXTURE_DATA.planetTextureCutout);
		Vector2 repeat = GetRepeat(input.shoreNoiseSize, useCommonDenominator: false);
		material.SetVector("_RepeatShoreNoise", repeat);
		terrainMaterial.SetTexture("_OceanTex", waterTexture);
		terrainMaterial.SetTexture("_NoiseTex", Base.planetLoader.noiseTexture);
		terrainMaterial.SetColor("_Sand", input.sand);
		terrainMaterial.SetColor("_Floor", input.floor);
		float sandGradientWidthMultiplier = input.sandGradientWidthMultiplier;
		terrainMaterial.SetFloat("_Must", 1f / (input.maskGradient_Terrain.must * sandGradientWidthMultiplier));
		terrainMaterial.SetFloat("_Cannot", 1f / (input.maskGradient_Terrain.cannot * sandGradientWidthMultiplier));
		terrainMaterial.SetFloat("_Global", 100000f / ((0f - input.maskGradient_Terrain.global) * sandGradientWidthMultiplier));
		terrainMaterial.SetFloat("_FloorWidthM", 1f / input.floorGradientWidthMultiplier * sandGradientWidthMultiplier);
		terrainMaterial.SetFloat("_Radius", (float)Radius);
		terrainMaterial.SetVector("_TexSizePixels", value);
		terrainMaterial.SetVector("_RepeatShoreNoise", repeat);
		terrainMaterial.SetVector("_RepeatSand", GetRepeat(input.sandNoiseSize, useCommonDenominator: false));
		return material;
	}

	private Material CreateTerrainMaterial(TerrainModule.TerrainTexture input, Shader shader, I_MsgLogger log)
	{
		Material material = new Material(shader);
		planetTexture = Base.planetLoader.GetTexture(input.planetTexture, log);
		material.SetTexture("_PlanetTexture", planetTexture);
		material.SetTexture("_TextureA", Base.planetLoader.GetTexture(input.surfaceTexture_A, log));
		material.SetTexture("_TextureB", Base.planetLoader.GetTexture(input.surfaceTexture_B, log));
		material.SetTexture("_TextureC", Base.planetLoader.GetTexture(input.terrainTexture_C, log));
		material.SetVector("_RepeatA", GetRepeat(input.surfaceTextureSize_A, useCommonDenominator: true));
		material.SetVector("_RepeatB", GetRepeat(input.surfaceTextureSize_B, useCommonDenominator: true));
		material.SetVector("_RepeatC", GetRepeat(input.terrainTextureSize_C, useCommonDenominator: true));
		material.SetFloat("_SurfaceSize", (float)Radius / input.surfaceLayerSize);
		material.SetFloat("_Min", input.minFade);
		material.SetFloat("_Max", input.maxFade);
		material.SetFloat("_ShadowIntensity", input.shadowIntensity);
		material.SetFloat("_ShadowSize", (float)Radius / input.shadowHeight);
		material.SetColor("_Fog", Color.clear);
		return material;
	}

	private Material CreateFrontCloudsMaterial(FrontCloudsModule input, Shader shader, I_MsgLogger log)
	{
		Material material = new Material(shader);
		material.SetTexture("_CloudsTex", Base.planetLoader.GetTexture(input.cloudsTexture, log));
		material.SetFloat("_TextureCutout", input.cloudTextureCutout);
		material.SetFloat("_FadeZoneM", 1f / Mathf.Clamp(input.fadeZoneHeight / ((float)Radius + input.height), 0.0001f, 1f));
		material.SetFloat("_SharpenAlpha", input.sharpenAlpha ? 1 : 0);
		material.renderQueue = 3010;
		return material;
	}

	private Material CreateRingsMaterial(RingsModule input, Shader shader, I_MsgLogger log)
	{
		Material material = new Material(shader);
		Texture2D texture = Base.planetLoader.GetTexture(input.ringsTexture, log);
		material.SetTexture("_RingsTex", texture);
		return material;
	}

	private Vector2 GetRepeat(Vector2 size, bool useCommonDenominator)
	{
		int num = ((!useCommonDenominator) ? 1 : commonDenominator);
		size.x *= 4.712389f;
		double num2 = SurfaceArea / (double)size.x / (double)num;
		double num3 = Radius / (double)size.y;
		int x = Mathf.Clamp((int)num2, int.MinValue, int.MaxValue);
		int y = Mathf.Clamp((int)num3, int.MinValue, int.MaxValue);
		return new Vector2Int(x, y);
	}

	public void SetupDepthAndSatelliteIndex()
	{
		orbitalDepth = GetOrbitalDepth();
		satelliteIndex = (data.hasOrbit ? new List<Planet>(parentBody.satellites).IndexOf(this) : (-1));
	}

	private int GetOrbitalDepth()
	{
		if (!(parentBody != null))
		{
			return 0;
		}
		return parentBody.GetOrbitalDepth() + 1;
	}

	public void SetupInteractions(Dictionary<string, Planet> planets)
	{
		bool flag = data.hasOrbit && planets.ContainsKey(data.orbit.parent);
		parentBody = (flag ? planets[data.orbit.parent] : null);
		orbit = (flag ? new Orbit(data.orbit.semiMajorAxis, data.orbit.eccentricity, data.orbit.argumentOfPeriapsis * 0.01745329238474369, -data.orbit.direction, parentBody, PathType.Eternal, null) : null);
		trajectory = (flag ? new Trajectory(orbit) : Trajectory.Empty);
		SOI = (flag ? Kepler.GetSphereOfInfluence(data.orbit.semiMajorAxis, mass, parentBody.mass, data.orbit.multiplierSOI) : double.PositiveInfinity);
		satellites = GetSatellites(planets);
	}

	private Planet[] GetSatellites(Dictionary<string, Planet> planets)
	{
		List<Planet> list = planets.Values.Where((Planet planet) => planet.data.hasOrbit && planet.data.orbit.parent == codeName).ToList();
		list.Sort((Planet a, Planet b) => (a.data.orbit.semiMajorAxis > b.data.orbit.semiMajorAxis) ? 1 : (-1));
		return list.ToArray();
	}

	public Color GetTerrainColor(Double2 position)
	{
		Vector2 vector = (Vector2)((!DontDistortTextureCutout) ? position.normalized : (position / Radius)) * (data.terrain.TERRAIN_TEXTURE_DATA.planetTextureCutout * 0.5f) + new Vector2(0.5f, 0.5f);
		return planetTexture.GetPixelBilinear(vector.x, vector.y);
	}

	public float GetWaterColor(Vector2 normalPosition)
	{
		Vector2 vector = normalPosition * (data.terrain.TERRAIN_TEXTURE_DATA.planetTextureCutout * 0.5f) + new Vector2(0.5f, 0.5f);
		return 1f - waterTexture.GetPixelBilinear(vector.x, vector.y).r - 0.5f;
	}
}
