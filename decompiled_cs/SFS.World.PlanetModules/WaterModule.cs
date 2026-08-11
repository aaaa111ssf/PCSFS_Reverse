using System;
using UnityEngine;

namespace SFS.World.PlanetModules;

[Serializable]
public class WaterModule
{
	[Serializable]
	public class WaterMask
	{
		public float must;

		public float cannot;

		public float global;
	}

	public string oceanMaskTexture;

	public bool lowerTerrain;

	public float oceanDepth;

	public Color sand;

	public Color floor;

	public Color shallow;

	public Color deep;

	public WaterMask maskGradient_Water = new WaterMask();

	public float waterGradientWidthMultiplier = 1f;

	public WaterMask maskGradient_Terrain = new WaterMask();

	public float sandGradientWidthMultiplier = 1f;

	public float floorGradientWidthMultiplier = 1f;

	public Vector2 shoreNoiseSize;

	public Vector2 sandNoiseSize;

	public Vector2 wavesSize;

	public float opacity_Surface = 0.75f;

	public float opacity_Far = 1f;

	public float opacity_FullDarkness = 0.95f;

	public float surfaceVisibilityDistance = 1200f;

	public float fullDarknessDepth = 500f;

	public float fullDarknessVisibilityDistance = 300f;

	public Color mapColor;
}
