using System;
using SFS.Variables;
using SFS.World;
using SFS.World.Drag;
using UnityEngine;

namespace SFS.Parts.Modules;

public class ExternalVariableModule : MonoBehaviour, Rocket.INJ_Rocket
{
	public Double_Reference physicsTimeScale = new Double_Reference();

	public Double_Reference railsTimewarp = new Double_Reference();

	public Double_Reference effectiveTimewarp = new Double_Reference();

	public Bool_Reference physicsEnabled = new Bool_Reference();

	[Space]
	public Double_Reference rocketAltitude = new Double_Reference();

	public Double_Reference currentAtmosphereDensity = new Double_Reference();

	public Double_Reference rocketPositionAngleRadians = new Double_Reference();

	[Space]
	public Double_Reference surfaceVelocityX = new Double_Reference();

	public Double_Reference surfaceVelocityY = new Double_Reference();

	public Double_Reference velocityMagnitude = new Double_Reference();

	[Space]
	public Double_Reference rcsX = new Double_Reference();

	public Double_Reference rcsY = new Double_Reference();

	public Double_Reference turnInput = new Double_Reference();

	public Double_Reference wheelsInput = new Double_Reference();

	public Bool_Reference throttleOn = new Bool_Reference();

	public Double_Reference throttlePercent = new Double_Reference();

	public Double_Reference throttleOutput = new Double_Reference();

	[Space]
	public Double_Reference partTemperature = new Double_Reference();

	public Double_Reference orientationX = new Double_Reference();

	public Double_Reference orientationY = new Double_Reference();

	public Double_Reference partRotation = new Double_Reference();

	public Double_Reference partRotation_Normalized = new Double_Reference();

	public Double_Reference globalRotation = new Double_Reference();

	private OrientationModule ormod;

	private HeatModuleBase hmb;

	private Rocket rkt;

	public Rocket Rocket
	{
		set
		{
			rkt = value;
		}
	}

	private void Awake()
	{
		ormod = GetComponentInParent<OrientationModule>();
		hmb = GetComponentInParent<HeatModuleBase>();
	}

	private float NormalizeAngle(float angle)
	{
		if (!(angle >= 0f))
		{
			return (360f + angle % 360f) % 360f;
		}
		return angle % 360f;
	}

	private void Update()
	{
		if (WorldTime.main != null)
		{
			physicsTimeScale.Value = Time.timeScale;
			physicsEnabled.Value = WorldTime.main.realtimePhysics.Value;
			railsTimewarp.Value = (physicsEnabled.Value ? 1.0 : WorldTime.main.timewarpSpeed);
			effectiveTimewarp.Value = (physicsEnabled.Value ? ((double)Time.timeScale) : WorldTime.main.timewarpSpeed);
		}
		if (ormod != null)
		{
			orientationX.Value = ormod.orientation.Value.x;
			orientationY.Value = ormod.orientation.Value.y;
			partRotation.Value = ormod.orientation.Value.z;
			partRotation_Normalized.Value = NormalizeAngle((float)partRotation.Value);
			globalRotation.Value = base.transform.eulerAngles.z;
		}
		if (rkt != null)
		{
			rocketAltitude.Value = rkt.physics.location.Height;
			currentAtmosphereDensity.Value = rkt.physics.location.planet.Value.GetAtmosphericDensity(rocketAltitude.Value);
			rocketPositionAngleRadians.Value = rkt.physics.location.position.Value.AngleRadians;
			velocityMagnitude.Value = rkt.physics.location.velocity.Value.magnitude;
			surfaceVelocityX.Value = rkt.physics.location.Value.velocity.Rotate(0.0 - (rkt.physics.location.position.Value.AngleRadians - Math.PI / 2.0)).x;
			surfaceVelocityY.Value = rkt.physics.location.Value.VerticalVelocity;
			turnInput.Value = rkt.output_TurnAxisTorque.Value;
			wheelsInput.Value = rkt.output_TurnAxisWheels.Value;
			rcsX.Value = rkt.output_DirectionalAxis.Value.x;
			rcsY.Value = rkt.output_DirectionalAxis.Value.y;
			throttleOn.Value = rkt.throttle.throttleOn.Value;
			throttlePercent.Value = rkt.throttle.throttlePercent.Value;
			throttleOutput.Value = rkt.throttle.output_Throttle.Value;
		}
		if (hmb != null)
		{
			partTemperature.Value = hmb.Temperature;
		}
	}
}
