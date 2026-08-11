using SFS.Translations;
using UnityEngine;

namespace SFS.World.Maps;

public class MapRocket : MapPlayer
{
	public Rocket rocket;

	public override Player Player => rocket;

	public override Trajectory Trajectory => rocket.physics.GetTrajectory();

	public override string EncounterText => Loc.main.Rendezvous;

	public override Vector3 Select_MenuPosition
	{
		get
		{
			if (Map.manager.mapMode.Value)
			{
				return mapIcon.mapIcon.transform.position;
			}
			return rocket.rb2d.worldCenterOfMass;
		}
	}

	public override bool Select_CanFocus => true;

	public override bool Select_CanRename => true;

	public override string Select_DisplayName
	{
		get
		{
			if (!(rocket.rocketName == ""))
			{
				return rocket.rocketName;
			}
			return Loc.main.Default_Rocket_Name;
		}
		set
		{
			rocket.rocketName = ((value == Loc.main.Default_Rocket_Name) ? "" : value);
		}
	}

	public override bool Select_CanNavigate => !Player.isPlayer.Value;

	public override bool Select_CanEndMission => true;

	public override string Select_EndMissionText => CanRecover(rocket, checkAchievements: false) ? Loc.main.Recover_Rocket : Loc.main.Destroy_Rocket;

	public override Sprite Select_EndMissionIcon
	{
		get
		{
			if (!CanRecover(rocket, checkAchievements: false))
			{
				return ResourcesLoader.main.buttonIcons.destroy;
			}
			return ResourcesLoader.main.buttonIcons.recover;
		}
	}

	public override double Navigation_Tolerance => 2500.0;

	public static bool CanRecover(Rocket rocket, bool checkAchievements)
	{
		if (rocket.location.planet.Value != Base.planetLoader.spaceCenter.Planet)
		{
			return false;
		}
		if (!rocket.location.velocity.Value.Mag_LessThan(rocket.floating ? 1.0 : 0.01))
		{
			return false;
		}
		if (rocket.location.planet.Value.data.hasWater && rocket.location.Height < -20.0)
		{
			return false;
		}
		if (checkAchievements)
		{
			return rocket.stats.HasFlown();
		}
		return true;
	}
}
