using System.Linq;
using SFS.Builds;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace SFS.Tutorials;

public class Tutorial_Build : Tutorial_Base
{
	public GameObject capsulePopup;

	public GameObject parachutePopup;

	public GameObject fuelTankPopup;

	public GameObject enginePopup;

	public GameObject separatorPopup;

	public GameObject descriptionPopup;

	public GameObject infiniteArea;

	[Space]
	public PickCategory basicCategory;

	private void Start()
	{
		capsulePopup.SetActive(value: false);
		parachutePopup.SetActive(value: false);
		fuelTankPopup.SetActive(value: false);
		enginePopup.SetActive(value: false);
		separatorPopup.SetActive(value: false);
		descriptionPopup.SetActive(value: false);
		infiniteArea.SetActive(value: false);
		if (FileLocations.HasNotification("Tut_Basic_Build"))
		{
			return;
		}
		if (!FileLocations.HasNotification("First_Time_Playing"))
		{
			FileLocations.WriteNotification("Tut_Basic_Build");
		}
		else
		{
			if (AnalyticsSessionInfo.sessionCount > 3 && !Application.isEditor)
			{
				return;
			}
			PartHolder hold = BuildManager.main.holdGrid.holdGrid.partsHolder;
			PartHolder active = BuildManager.main.buildGrid.activeGrid.partsHolder;
			Add_ShowPopup(capsulePopup, () => hold.HasModule<CrewModule>());
			Add_Check(delegate
			{
				bool active2 = BuildManager.main.pickGrid.current == basicCategory;
				capsulePopup.SetActive(active2);
				return active.HasModule<CrewModule>();
			});
			Add_Action(delegate
			{
				capsulePopup.SetActive(value: false);
			});
			Add_Action(delegate
			{
				enginePopup.SetActive(value: true);
				fuelTankPopup.SetActive(value: true);
			});
			bool heldFuelTank;
			bool heldEngine = (heldFuelTank = false);
			Add_Check(delegate
			{
				bool flag = BuildManager.main.pickGrid.current == basicCategory;
				enginePopup.SetActive(flag && !heldEngine);
				fuelTankPopup.SetActive(flag && !heldFuelTank);
				if (hold.HasModule<EngineModule>())
				{
					heldEngine = true;
					enginePopup.SetActive(value: false);
				}
				if (hold.HasModule<ResourceModule>())
				{
					heldFuelTank = true;
					fuelTankPopup.SetActive(value: false);
				}
				return !enginePopup.activeSelf && !fuelTankPopup.activeSelf;
			});
			Add_Check(() => active.HasModule<EngineModule>() && active.HasModule<ResourceModule>());
			Add_Action(delegate
			{
				enginePopup.SetActive(value: false);
				fuelTankPopup.SetActive(value: false);
			});
			Add_ShowPopup(separatorPopup, () => hold.HasModule<DetachModule>());
			Add_Check(delegate
			{
				bool active2 = BuildManager.main.pickGrid.current == basicCategory;
				separatorPopup.SetActive(active2);
				return active.HasModule<DetachModule>();
			});
			Add_Action(delegate
			{
				separatorPopup.SetActive(value: false);
			});
			Add_ShowPopup(parachutePopup, () => hold.HasModule<ParachuteModule>());
			Add_Check(delegate
			{
				bool active2 = BuildManager.main.pickGrid.current == basicCategory;
				parachutePopup.SetActive(active2);
				return active.HasModule<ParachuteModule>();
			});
			Add_Action(delegate
			{
				parachutePopup.SetActive(value: false);
			});
			Add_Action(delegate
			{
				FileLocations.WriteNotification("Tut_Basic_Build");
			});
		}
	}

	private void Update()
	{
		Camera camera;
		if (!(BuildManager.main.pickGrid.current != basicCategory))
		{
			camera = BuildManager.main.buildCamera.cameraManager.camera;
			PickGridUI pickGrid = BuildManager.main.pickGrid;
			SetY(capsulePopup.transform, pickGrid.icons.First(((Part, RawImage) v) => v.Item1.name == "Capsule").Item2.transform);
			SetY(enginePopup.transform, pickGrid.icons.First(((Part, RawImage) v) => v.Item1.name == "Engine Hawk").Item2.transform);
			SetY(fuelTankPopup.transform, pickGrid.icons.Where(((Part, RawImage) v) => v.Item1.name == "Fuel Tank").ToArray()[1].Item2.transform);
			SetY(separatorPopup.transform, pickGrid.icons.First(((Part, RawImage) v) => v.Item1.name == "Separator").Item2.transform);
			SetY(parachutePopup.transform, pickGrid.icons.First(((Part, RawImage) v) => v.Item1.name == "Parachute").Item2.transform);
		}
		void SetY(Transform popup, Transform partIcon)
		{
			Vector3 position = camera.WorldToViewportPoint(partIcon.position);
			position.z = 0f - camera.transform.position.z;
			Vector3 vector = camera.ViewportToScreenPoint(position);
			Vector3 position2 = popup.position;
			position2.y = vector.y;
			popup.position = position2;
		}
	}
}
