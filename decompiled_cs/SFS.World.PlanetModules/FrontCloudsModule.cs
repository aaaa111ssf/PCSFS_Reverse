using System;

namespace SFS.World.PlanetModules;

[Serializable]
public class FrontCloudsModule
{
	public string cloudsTexture;

	public float cloudTextureCutout = 1f;

	public float fadeZoneHeight = 10000f;

	public float height = 10000f;

	public float positionZ = -5000f;

	public bool sharpenAlpha;
}
