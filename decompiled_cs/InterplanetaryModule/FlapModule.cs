using SFS.Parts.Modules;
using SFS.Variables;
using SFS.World;
using UnityEngine;

namespace InterplanetaryModule;

public class FlapModule : MonoBehaviour, Rocket.INJ_TurnAxisTorque, Rocket.INJ_Location
{
	public MoveModule moveModule;

	public Float_Reference duration;

	public bool unscaledTime;

	public Bool_Reference activated;

	private Location location;

	private float turnAxis;

	public TorqueModule torqueModule;

	public float coefficient;

	Location Rocket.INJ_Location.Location
	{
		set
		{
			location = value;
		}
	}

	float Rocket.INJ_TurnAxisTorque.TurnAxis
	{
		set
		{
			turnAxis = value;
		}
	}

	private void Update()
	{
		float num = (float)location.planet.GetAtmosphericDensity(location.Height);
		float num2 = (float)location.velocity.magnitude;
		torqueModule.torque.Value = (activated.Value ? (coefficient * num * num2 * num2) : 0f);
		float num3 = (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		float maxDelta = ((duration.Value != 0f) ? (num3 / duration.Value) : 10000f);
		float target = (activated.Value ? turnAxis : 0f);
		moveModule.time.Value = Mathf.MoveTowards(moveModule.time.Value, target, maxDelta);
	}
}
