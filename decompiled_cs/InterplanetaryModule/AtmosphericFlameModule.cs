using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;

namespace InterplanetaryModule;

public class AtmosphericFlameModule : MonoBehaviour, Rocket.INJ_Location
{
	public float maxPressure = 0.005f;

	public MoveModule moveModule;

	private Location location;

	Location Rocket.INJ_Location.Location
	{
		set
		{
			try
			{
				location = value;
			}
			catch
			{
			}
		}
	}

	private void Update()
	{
		if (location != null)
		{
			float num = (float)location.planet.GetAtmosphericDensity(location.Height);
			moveModule.time.Value = Mathf.Clamp01(num / maxPressure);
		}
	}
}
