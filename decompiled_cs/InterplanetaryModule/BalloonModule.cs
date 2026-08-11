using System;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.Translations;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;
using UnityEngine.Events;

namespace InterplanetaryModule;

public class BalloonModule : MonoBehaviour, Rocket.INJ_Location, Rocket.INJ_Physics, I_PartMenu
{
	public Float_Reference radius;

	public double dragConstant = 1.0;

	public double areaToVolume = 20.0;

	public double buoyantMultiplier = 0.4;

	public double maxDeployVelocity;

	public Transform balloon;

	public OrientationModule orientation;

	[Space]
	public Float_Reference state;

	public Float_Reference targetState;

	public Float_Reference maxSpeed;

	private Double2 oldPosition;

	[Space]
	public UnityEvent onDeploy;

	public Location Location { private get; set; }

	public Rigidbody2D Rb2d { get; set; }

	void I_PartMenu.Draw(StatsMenu drawer, PartDrawSettings settings)
	{
	}

	public void DeployBalloon(UsePartData data)
	{
		bool flag = false;
		double num = (Location.planet.HasAtmospherePhysics ? Location.planet.data.atmospherePhysics.parachuteMultiplier : 1.0);
		if (targetState.Value == 0f && state.Value == 0f)
		{
			if (!Location.planet.HasAtmospherePhysics || Location.Height > Location.planet.AtmosphereHeightPhysics * 0.9)
			{
				MsgDrawer.main.Log("Cannot inflate balloon in vacuum");
			}
			else if (Location.velocity.magnitude > maxDeployVelocity * num)
			{
				MsgDrawer.main.Log(Loc.main.Msg_Cannot_Deploy_Parachute_While_Faster.Inject((maxDeployVelocity * num).ToVelocityString(decimals: false), "velocity"));
			}
			else
			{
				MsgDrawer.main.Log("Balloon inflated");
				targetState.Value = 2f;
				onDeploy.Invoke();
				flag = true;
			}
		}
		else if (targetState.Value == 2f && state.Value == 2f)
		{
			MsgDrawer.main.Log("Balloon deflated");
			targetState.Value = 0f;
			state.Value = 0f;
			flag = true;
		}
		else if (targetState.Value == 3f)
		{
			flag = true;
		}
		if (!flag)
		{
			data.successfullyUsedPart = false;
		}
	}

	private void Start()
	{
		if (GameManager.main == null)
		{
			base.enabled = false;
		}
		else
		{
			targetState.OnChange += new Action(UpdateEnabled);
		}
	}

	private void UpdateEnabled()
	{
		base.enabled = targetState.Value == 1f || targetState.Value == 2f;
	}

	private void FixedUpdate()
	{
		if (state.Value != 0f)
		{
			double num = radius.Value * (state.Value / 2f);
			double height = Location.Height;
			double atmosphericDensity = Location.planet.GetAtmosphericDensity(height);
			double gravity = Location.planet.GetGravity(Location.planet.Radius + height);
			double num2 = areaToVolume * Math.PI * num * num;
			double num3 = atmosphericDensity * num2 * gravity * buoyantMultiplier;
			if ((double)maxSpeed.Value > 0.1 && Location.velocity.Mag_MoreThan(0.1) && Location.VerticalVelocity > (double)maxSpeed.Value)
			{
				num3 *= (double)(maxSpeed.Value * maxSpeed.Value) / Location.velocity.sqrMagnitude;
			}
			Vector2 vector = Location.position.normalized;
			float num4 = Mathf.Atan2(vector.y, vector.x);
			float num5 = Rb2d.rotation * (MathF.PI / 180f);
			float num6 = orientation.orientation.Value.z + ((orientation.orientation.Value.y < 0f) ? 180f : 0f);
			num6 *= MathF.PI / 180f;
			Vector2 vector2 = new Vector2((float)Math.Cos(num4 - num5 - num6), (float)Math.Sin(num4 - num5 - num6));
			Vector2 force = base.transform.TransformVector(vector2 * (float)num3);
			Vector2 relativePoint = Rb2d.GetRelativePoint(Transform_Utility.LocalToLocalPoint(base.transform, Rb2d, new Vector2(0f, 3f)));
			Rb2d.AddForceAtPosition(force, relativePoint, ForceMode2D.Force);
		}
	}

	private void LateUpdate()
	{
		if (!(GameManager.main == null) && !(Location.planet == null))
		{
			Double2 @double = oldPosition;
			if (@double.x == 0.0 && @double.y == 0.0)
			{
				oldPosition = WorldView.ToGlobalPosition(base.transform.position) - Location.velocity;
			}
			AngleToOldPosition();
		}
	}

	private void AngleToOldPosition()
	{
		Vector2 vector = Location.position.normalized;
		float num = Mathf.Atan2(vector.y, vector.x);
		float num2 = Rb2d.rotation * (MathF.PI / 180f);
		float num3 = orientation.orientation.Value.z + ((orientation.orientation.Value.y < 0f) ? 180f : 0f);
		num3 *= MathF.PI / 180f;
		float num4 = num - num2 - num3;
		num4 *= 57.29578f;
		num4 = 90f + num4;
		balloon.localEulerAngles = new Vector3(0f, 0f, num4 + Mathf.Sin(Time.time) * 3f * balloon.parent.lossyScale.x * balloon.parent.lossyScale.y);
	}
}
