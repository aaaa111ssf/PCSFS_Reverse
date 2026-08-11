using System;
using Beebyte.Obfuscator;

namespace SFS.WorldBase;

[Serializable]
[Skip]
public class SolarSystemSettings
{
	public bool includeDefaultPlanets;

	public bool includeDefaultHeightmaps = true;

	public bool includeDefaultTextures = true;

	public bool hideStarsInAtmosphere = true;

	public string authorName = "n/a";

	public string version = "n/a";

	public string description = "n/a";
}
