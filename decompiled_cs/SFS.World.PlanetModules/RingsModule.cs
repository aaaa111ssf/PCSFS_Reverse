using System;
using UnityEngine;

namespace SFS.World.PlanetModules;

[Serializable]
public class RingsModule
{
	public string ringsTexture;

	public double startRadius;

	public double endRadius;

	public float positionZ;

	public Color mapColor;
}
