using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SFS.Input;
using SFS.Translations;
using SFS.UI;
using SFS.World.Maps;
using SFS.WorldBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SFS.World;

public class TeleportMenu : BasicMenu
{
	private enum Mode
	{
		Surface,
		Orbit
	}

	public static TeleportMenu main;

	public SFS.UI.Button planetSelectButton;

	public TMP_Dropdown planetSelectDropdown;

	private Planet selectedPlanet;

	[Space]
	public GameObject planetSelector;

	[Space]
	public ScrollElement planetScrollElement;

	public GameObject spacerPrefab;

	public SFS.UI.Button planetButtonPrefab;

	private List<GameObject> createdElements = new List<GameObject>();

	[Space]
	public GameObject orbitTypeSelector;

	public SFS.UI.Button progradeMode;

	public SFS.UI.Button retrogradeMode;

	private bool prograde;

	public SFS.UI.Button surfaceMode;

	public SFS.UI.Button orbitMode;

	private Mode mode;

	public GameObject longitudeHolder;

	public TextBoxAdapter longitudeInput;

	public TextAdapter heightText;

	public TextBoxAdapter heightInput;

	public SFS.UI.Button teleportButton;

	public SFS.UI.Button closeButton;

	private bool enteredInput;

	private float longitude;

	private float height;

	private void Awake()
	{
		main = this;
	}

	private void Start()
	{
		teleportButton.onClick += new Action(ConfirmTeleport);
		closeButton.onClick += (Action)delegate
		{
			if (ScreenManager.main.CurrentScreen != this)
			{
				ScreenManager.main.CloseCurrent();
			}
			Close();
		};
		progradeMode.onClick += (Action)delegate
		{
			SetOrbitType(prograde: true);
		};
		retrogradeMode.onClick += (Action)delegate
		{
			SetOrbitType(prograde: false);
		};
		surfaceMode.onClick += new Action(SetSurfaceMode);
		orbitMode.onClick += new Action(SetOrbitMode);
		surfaceMode.keepGlowOnInit = true;
		orbitMode.keepGlowOnInit = true;
		UpdateUI();
		TMP_InputField component = heightInput.GetComponent<TMP_InputField>();
		component.characterValidation = TMP_InputField.CharacterValidation.Decimal;
		component.onDeselect.AddListener(UpdateHeight);
		TMP_InputField component2 = longitudeInput.GetComponent<TMP_InputField>();
		component2.characterValidation = TMP_InputField.CharacterValidation.Decimal;
		component2.onDeselect.AddListener(UpdateLongitude);
		planetSelectDropdown.onValueChanged.AddListener(delegate(int n)
		{
			selectedPlanet = Base.planetLoader.planets.Values.ToArray()[n];
		});
		void UpdateHeight(string txt)
		{
			if (!float.TryParse(txt.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				heightInput.Text = "0";
			}
			GetMinMaxHeight(out var min, out var max);
			result = Mathf.Clamp(result, min, max);
			heightInput.Text = result.ToString(CultureInfo.InvariantCulture);
			height = result;
		}
		void UpdateLongitude(string txt)
		{
			if (!float.TryParse(txt.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
			{
				longitudeInput.Text = "0";
			}
			result = Mathf.Clamp(result, -360f, 360f);
			longitudeInput.Text = result.ToString(CultureInfo.InvariantCulture);
			longitude = result;
		}
	}

	public void OpenFromCheats()
	{
		selectedPlanet = ((PlayerController.main.player.Value != null) ? ((Planet)PlayerController.main.player.Value.location.planet) : WorldView.main.ViewLocation.planet);
		SetSurfaceMode();
		Open();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		planetSelectDropdown.options.Clear();
		foreach (Planet value in Base.planetLoader.planets.Values)
		{
			planetSelectDropdown.options.Add(new TMP_Dropdown.OptionData(value.DisplayName));
		}
		planetSelectDropdown.SetValueWithoutNotify(Array.IndexOf(Base.planetLoader.planets.Values.ToArray(), selectedPlanet));
		UpdateUI();
		UpdatePlanetSelector();
	}

	private void OnEnable()
	{
		UpdateUI();
	}

	private void OpenPlanetSelectMenu_Mobile()
	{
		foreach (GameObject createdElement in createdElements)
		{
			UnityEngine.Object.Destroy(createdElement);
		}
		createdElements.Clear();
		List<Planet> flatPlanetTree = GetFlatPlanetTree();
		Planet planet = null;
		Planet planet2 = null;
		foreach (Planet planet3 in flatPlanetTree)
		{
			Planet planet4 = planet3.orbit?.Planet;
			if (planet4 != planet)
			{
				if (planet4 != planet2)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(spacerPrefab, planetScrollElement.transform);
					gameObject.SetActive(value: true);
					createdElements.Add(gameObject);
				}
				planet = planet4;
			}
			SFS.UI.Button button = UnityEngine.Object.Instantiate(planetButtonPrefab, planetScrollElement.transform);
			button.gameObject.SetActive(value: true);
			button.GetComponentInChildren<Text>().text = planet3.DisplayName;
			button.onClick += (Action)delegate
			{
				selectedPlanet = planet3;
				UpdateUI();
				planetSelector.SetActive(value: false);
			};
			planetScrollElement.RegisterScrolling(button);
			createdElements.Add(button.gameObject);
			planet2 = planet3;
		}
		planetSelector.SetActive(value: true);
	}

	private void SetOrbitType(bool prograde)
	{
		this.prograde = prograde;
		UpdateUI();
	}

	private void SetSurfaceMode()
	{
		mode = Mode.Surface;
		longitude = 0f;
		height = 0f;
		TextBoxAdapter textBoxAdapter = longitudeInput;
		float num = (longitude = Mathf.Clamp(longitude, 0f, 360f));
		textBoxAdapter.Text = num.ToString(CultureInfo.CurrentCulture);
		heightInput.Text = "0";
		prograde = true;
		orbitTypeSelector.SetActive(value: false);
		longitudeHolder.SetActive(value: true);
		UpdateUI();
	}

	private void SetOrbitMode()
	{
		mode = Mode.Orbit;
		longitude = 0f;
		height = (float)((selectedPlanet.TimewarpRadius_Descend - selectedPlanet.Radius) / 1000.0);
		heightInput.Text = height.ToString(CultureInfo.CurrentCulture);
		prograde = true;
		orbitTypeSelector.SetActive(value: true);
		longitudeHolder.SetActive(value: true);
		UpdateUI();
	}

	private void UpdatePlanetSelector()
	{
	}

	private void UpdateUI()
	{
		Field subs = ((mode == Mode.Surface) ? Loc.main.Meter_Unit : Loc.main.Km_Unit);
		heightText.Text = Loc.main.Teleport_Height.InjectField(subs, "unit").GetText();
		surfaceMode.FindByName<Transform>("Select").gameObject.SetActive(mode == Mode.Surface);
		orbitMode.FindByName<Transform>("Select").gameObject.SetActive(mode == Mode.Orbit);
		progradeMode.FindByName<Transform>("Select").gameObject.SetActive(prograde);
		retrogradeMode.FindByName<Transform>("Select").gameObject.SetActive(!prograde);
		teleportButton.SetEnabled(CanTeleport());
	}

	private bool CanTeleport()
	{
		return selectedPlanet != null;
	}

	private void ConfirmTeleport()
	{
		if (!CanTeleport())
		{
			return;
		}
		Player value = PlayerController.main.player.Value;
		Rocket rocket = value as Rocket;
		if ((object)rocket == null)
		{
			return;
		}
		GetMinMaxHeight(out var min, out var max);
		height = Mathf.Clamp(height, min, max);
		longitude = Mathf.Clamp((longitude + 360f) % 360f, 0f, 360f);
		if (mode == Mode.Surface)
		{
			rocket.partHolder.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			rocket.rb2d.rotation = 0f;
			Bounds bounds = default(Bounds);
			Collider2D[] modules = rocket.partHolder.GetModules<Collider2D>();
			foreach (Collider2D collider2D in modules)
			{
				if (collider2D.gameObject.activeSelf && !collider2D.isTrigger)
				{
					if (bounds == default(Bounds))
					{
						bounds = collider2D.bounds;
					}
					else
					{
						bounds.Encapsulate(collider2D.bounds);
					}
				}
			}
			float num = bounds.max.y - rocket.partHolder.transform.position.y;
			float num2 = bounds.size.y - num + rocket.rb2d.centerOfMass.y;
			double num3 = (0f - longitude + 90f) * (MathF.PI / 180f);
			double terrainHeightAtAngle = selectedPlanet.GetTerrainHeightAtAngle(num3, clampToWater: true);
			double num4 = selectedPlanet.Radius + terrainHeightAtAngle + (double)num2 + (double)height;
			Location location = new Location(position: new Double2(Math.Cos(num3) * num4, Math.Sin(num3) * num4), time: WorldTime.main.worldTime, planet: selectedPlanet, velocity: Double2.zero);
			TeleportToLocation(location, rotate: true);
		}
		else
		{
			double num5 = selectedPlanet.Radius + (double)(height * 1000f);
			double num6 = Math.Sqrt(selectedPlanet.mass / num5) + 0.0001;
			Double2 @double = new Double2(num5, 0.0);
			Double2 double2 = new Double2(0.0, 0.0 - num6);
			float num7 = (0f - longitude + 90f) * (MathF.PI / 180f);
			@double = @double.Rotate(num7);
			double2 = double2.Rotate(num7);
			if (!prograde)
			{
				double2 *= -1.0;
			}
			Location location2 = new Location(WorldTime.main.worldTime, selectedPlanet, @double, double2);
			TeleportToLocation(location2, rotate: false);
		}
		ScreenManager.main.CloseStack();
		void TeleportToLocation(Location location3, bool rotate)
		{
			PlayerController.main.player.Value = null;
			rocket.physics.PhysicsMode = false;
			rocket.physics.SetLocationAndState(location3, physicsMode: false);
			rocket.physics.PhysicsMode = true;
			PlayerController.main.player.Value = rocket;
			Map.view.SetViewSmooth(new MapView.View(location3.planet.mapPlanet, location3.position, (double)Map.view.view.distance * 0.800000011920929));
			rocket.physics.SetLocationAndState(location3, physicsMode: true);
			if (rotate)
			{
				rocket.partHolder.transform.rotation = Quaternion.Euler(0f, 0f, longitude);
				rocket.rb2d.angularVelocity = 0f;
				rocket.rb2d.rotation = longitude;
			}
		}
	}

	private void GetMinMaxHeight(out float min, out float max)
	{
		if (mode == Mode.Surface)
		{
			min = 0f;
			max = 5000f;
		}
		else
		{
			min = (float)((selectedPlanet.TimewarpRadius_Descend - selectedPlanet.Radius) / 1000.0) + 0.5f;
			max = (float)((selectedPlanet.SOI - selectedPlanet.Radius) / 1000.0) - 1f;
		}
	}

	private List<Planet> GetFlatPlanetTree()
	{
		List<Planet> flatPlanetTree = new List<Planet>();
		AddRecursively(Base.planetLoader.planets.Values.FirstOrDefault((Planet p) => !p.data.hasOrbit));
		return flatPlanetTree;
		void AddRecursively(Planet planet)
		{
			flatPlanetTree.Add(planet);
			Planet[] satellites = planet.satellites;
			for (int i = 0; i < satellites.Length; i++)
			{
				AddRecursively(satellites[i]);
			}
		}
	}
}
