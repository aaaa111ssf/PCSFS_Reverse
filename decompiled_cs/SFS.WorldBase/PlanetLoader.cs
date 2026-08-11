using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using ModLoader;
using ModLoader.UI;
using SFS.Parsers.Json;
using SFS.Parts;
using SFS.World;
using SFS.World.Legacy;
using SFS.World.PlanetModules;
using UnityEngine;

namespace SFS.WorldBase;

public class PlanetLoader : MonoBehaviour
{
	private const string Version_File = "Version.txt";

	private const string Planet_Data_Version_File = "Version.txt";

	private const string ImportSetting_File = "Import_Settings.txt";

	private const string SpaceCenter_File = "Space_Center_Data.txt";

	private const string Planets_Directory = "Planet Data";

	private const string Heightmap_Directory = "Heightmap Data";

	private const string Textures_Directory = "Texture Data";

	public Shader terrainShader;

	public Shader waterShader;

	public Shader atmosphereShader;

	public Shader frontCloudsShader;

	public Shader ringsShader;

	public Texture2D noiseTexture;

	public GameObject planetHolder;

	public SolarSystemSettings solarSystemSettings;

	public SpaceCenterData spaceCenter;

	public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

	public Dictionary<string, HeightMap> heightmaps = new Dictionary<string, HeightMap>();

	public Dictionary<string, Planet> planets = new Dictionary<string, Planet>();

	private void Start()
	{
		if (!Application.isEditor)
		{
			ExportExampleAsync(FileLocations.SolarSystemsFolder.GetFolder("Example"));
		}
	}

	private void OnValidate()
	{
	}

	private static void ExportExampleAsync(IFolder folder)
	{
		IFile planetDataVersionFile = folder.GetFile("Version.txt");
		if (folder.Exists() && planetDataVersionFile.Exists() && planetDataVersionFile.ReadText() == "Planet_Data_v2")
		{
			return;
		}
		string version = Application.version;
		Dictionary<string, HeightMap> heightMaps = new Dictionary<string, HeightMap>();
		Dictionary<string, PlanetData> planetData = new Dictionary<string, PlanetData>();
		LoadHeightmaps_Private(heightMaps);
		LoadPlanets_Private(planetData);
		new Thread((ThreadStart)delegate
		{
			folder.Delete();
			folder.Create();
			planetDataVersionFile.WriteText("Planet_Data_v2");
			folder.GetFile("Version.txt").WriteText(version);
			folder.GetFile("Import_Settings.txt").WriteText(JsonWrapper.ToJson(new SolarSystemSettings(), pretty: true));
			folder.GetFile("Space_Center_Data.txt").WriteText(JsonWrapper.ToJson(new SpaceCenterData(), pretty: true));
			IFolder folder2 = folder.GetFolder("Planet Data");
			folder2.Create();
			foreach (KeyValuePair<string, PlanetData> item in planetData)
			{
				folder2.GetFile(item.Key + ".txt").WriteText(JsonWrapper.ToJson(item.Value, pretty: true));
			}
			IFolder folder3 = folder.GetFolder("Heightmap Data");
			folder3.Create();
			foreach (KeyValuePair<string, HeightMap> item2 in heightMaps)
			{
				folder3.GetFile(item2.Key + ".txt").WriteText(JsonUtility.ToJson(item2.Value, prettyPrint: true));
			}
			folder.GetFolder("Texture Data").Create();
		}).Start();
	}

	public void LoadSolarSystem(WorldSettings settings, I_MsgLogger log, Action<bool> callback)
	{
		UnloadSolarSystem();
		SolarSystemReference solarSystem = settings.solarSystem;
		if (solarSystem.name.Length > 0)
		{
			IFolder solarSystemFolder = GetSolarSystemFolder(solarSystem.name);
			if (!solarSystemFolder.Exists())
			{
				log.Log("Solar system " + solarSystem.name + " does not exist");
				callback(obj: false);
				return;
			}
			IFile file = solarSystemFolder.GetFile("Import_Settings.txt");
			if (!file.Exists())
			{
				log.Log("Solar system " + solarSystem.name + " does not have Import_Settings.txt file");
				callback(obj: false);
				return;
			}
			if (!JsonWrapper.TryLoadJson<SolarSystemSettings>(file, out solarSystemSettings))
			{
				log.Log("Failed to load import settings file");
				callback(obj: false);
				return;
			}
			Dictionary<string, PlanetData> dictionary = new Dictionary<string, PlanetData>();
			if (solarSystemSettings.includeDefaultPlanets)
			{
				LoadPlanets_Private(dictionary);
			}
			if (solarSystemSettings.includeDefaultTextures)
			{
				LoadTextures_Private(textures);
			}
			if (solarSystemSettings.includeDefaultHeightmaps)
			{
				LoadHeightmaps_Private(heightmaps);
			}
			LoadPlanets_Public(solarSystemFolder, dictionary, log);
			LoadTextures_Public(solarSystem, solarSystemFolder, textures, log);
			LoadHeightmaps_Public(solarSystem, solarSystemFolder, heightmaps, log);
			dictionary.ForEach(delegate(KeyValuePair<string, PlanetData> planet)
			{
				settings.difficulty.ScalePlanetData(planet.Value);
			});
			planets = CreatePlanets(dictionary, log, out var success);
			LoadSpaceCenter_Public(solarSystem, solarSystemFolder, log);
			callback(success);
			return;
		}
		solarSystemSettings = new SolarSystemSettings();
		spaceCenter = new SpaceCenterData();
		Dictionary<string, PlanetData> dictionary2 = new Dictionary<string, PlanetData>();
		LoadPlanets_Private(dictionary2);
		LoadTextures_Private(textures);
		LoadHeightmaps_Private(heightmaps);
		dictionary2.ForEach(delegate(KeyValuePair<string, PlanetData> planet)
		{
			try
			{
				settings.difficulty.ScalePlanetData(planet.Value);
			}
			catch (Exception)
			{
				Debug.LogError("Failed to set scale for " + planet.Key);
				throw;
			}
		});
		planets = CreatePlanets(dictionary2, log, out var _);
		callback(obj: true);
	}

	private void UnloadSolarSystem()
	{
		foreach (Planet value in planets.Values)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		textures.Clear();
		heightmaps.Clear();
		planets.Clear();
	}

	private static void LoadTextures_Private(Dictionary<string, Texture2D> output)
	{
		Texture2D[] array = UnityEngine.Resources.LoadAll<Texture2D>("Planet_Textures");
		foreach (Texture2D texture2D in array)
		{
			output[texture2D.name] = texture2D;
		}
	}

	private static void LoadHeightmaps_Private(Dictionary<string, HeightMap> output)
	{
		TextAsset[] array = UnityEngine.Resources.LoadAll<TextAsset>("Planet_Heightmaps");
		foreach (TextAsset textAsset in array)
		{
			if (TryLoadHeightmap(textAsset.text, out var heightmap))
			{
				output[textAsset.name] = heightmap;
			}
		}
		Texture2D[] array2 = UnityEngine.Resources.LoadAll<Texture2D>("Planet_Heightmaps");
		foreach (Texture2D texture2D in array2)
		{
			output[texture2D.name] = new HeightMap(texture2D);
		}
	}

	private static void LoadPlanets_Private(Dictionary<string, PlanetData> output)
	{
		List<string> list = new List<string> { "Sun", "Mercury", "Venus", "Earth", "Moon", "Near_Earth_Asteroid", "Mars", "Phobos", "Deimos", "Ceres" };
		List<string> collection = new List<string>
		{
			"Jupiter", "Io", "Europa", "Ganymede", "Callisto", "Thebe", "Saturn", "Pan", "Enceladus", "Titan",
			"Iapetus", "Rhea", "Tethys", "Mimas", "Dione"
		};
		List<string> collection2 = new List<string>
		{
			"Uranus", "Miranda", "Ariel", "Titania", "Puck", "Oberon", "Umbriel", "Neptune", "Proteus", "Triton",
			"Naiad", "Pluto", "Charon", "Nix", "Hydra"
		};
		if (DevSettings.FullVersion)
		{
			list.AddRange(collection);
			list.AddRange(collection2);
		}
		TextAsset[] array = UnityEngine.Resources.LoadAll<TextAsset>("Planet_Data");
		foreach (TextAsset textAsset in array)
		{
			if (list.Contains(textAsset.name))
			{
				try
				{
					output[textAsset.name] = JsonWrapper.FromJson<PlanetData>(textAsset.text);
				}
				catch (Exception message)
				{
					Debug.LogError("Error when parsing planet data for: " + textAsset.name);
					Debug.LogError(message);
					throw;
				}
			}
		}
	}

	private void LoadSpaceCenter_Public(SolarSystemReference solarSystem, IFolder path, I_MsgLogger log)
	{
		IFile file = path.GetFile("Space_Center_Data.txt");
		if (!file.Exists() || !JsonWrapper.TryLoadJson<SpaceCenterData>(file, out spaceCenter))
		{
			IFile file2 = path.GetFile("Launch_Pad_Position.txt");
			if (file2.Exists() && JsonWrapper.TryLoadJson<LegacyLaunchPad>(file2, out var data))
			{
				spaceCenter = SpaceCenterData.FromLegacyLaunchLocation(data);
			}
			else
			{
				log.Log("Solar system " + solarSystem.name + " does not have Space_Center_Data.txt file");
			}
		}
	}

	private static void LoadTextures_Public(SolarSystemReference solarSystem, IFolder path, Dictionary<string, Texture2D> outputTextures, I_MsgLogger log)
	{
		IFolder folder = path.GetFolder("Texture Data");
		if (!folder.Exists())
		{
			log.Log("Solar system " + solarSystem.name + " does not have Texture Data folder");
			return;
		}
		foreach (IFile file in folder.GetFiles())
		{
			if (Application.isEditor && file.GetExtension().ToLowerInvariant() == "meta")
			{
				continue;
			}
			if (file.GetExtension().ToLowerInvariant() == "png" || file.GetExtension().ToLowerInvariant() == "jpg" || file.GetExtension().ToLowerInvariant() == "jpeg")
			{
				Texture2D texture2D = new Texture2D(0, 0);
				if (texture2D.LoadImage(file.ReadBytes()))
				{
					outputTextures[file.GetNameWithoutExtension()] = texture2D;
				}
				else
				{
					log.Log("ERROR: loading texture failed: " + file.GetNameWithoutExtension());
				}
			}
			else
			{
				log.Log("ERROR: texture format invalid: " + file.GetExtension());
			}
		}
	}

	private static void LoadHeightmaps_Public(SolarSystemReference solarSystem, IFolder path, Dictionary<string, HeightMap> outputHeightmaps, I_MsgLogger log)
	{
		IFolder folder = path.GetFolder("Heightmap Data");
		if (!folder.Exists())
		{
			log.Log("Solar system " + solarSystem.name + " does not have Heightmap Data folder");
			return;
		}
		foreach (IFile file in folder.GetFiles())
		{
			if (Application.isEditor && file.GetExtension().ToLowerInvariant() == "meta")
			{
				continue;
			}
			if (file.GetExtension().ToLowerInvariant() == "png" || file.GetExtension().ToLowerInvariant() == "jpg" || file.GetExtension().ToLowerInvariant() == "jpeg")
			{
				Texture2D texture2D = new Texture2D(0, 0);
				if (texture2D.LoadImage(file.ReadBytes()))
				{
					outputHeightmaps[file.Name] = new HeightMap(texture2D);
				}
				else
				{
					log.Log("ERROR: loading heightmap failed: " + file.GetNameWithoutExtension());
				}
				UnityEngine.Object.Destroy(texture2D);
			}
			else if (file.GetExtension().ToLowerInvariant() == "txt")
			{
				if (TryLoadHeightmap(file.ReadText(), out var heightmap))
				{
					outputHeightmaps[file.Name] = heightmap;
				}
				else
				{
					log.Log("ERROR: loading heightmap failed: " + file.GetNameWithoutExtension());
				}
			}
			else
			{
				log.Log("ERROR: heightmap format invalid: " + file.GetExtension());
			}
		}
	}

	private static void LoadPlanets_Public(IFolder path, Dictionary<string, PlanetData> outputPlanetData, I_MsgLogger log)
	{
		IFolder folder = path.GetFolder("Planet Data");
		if (!folder.Exists())
		{
			log.Log("Solar system does not have Planet Data folder");
			return;
		}
		IFile[] array = folder.GetFiles().ToArray();
		int num = 0;
		IFile[] array2 = array;
		foreach (IFile file in array2)
		{
			if (Application.isEditor && file.GetExtension().ToLowerInvariant() == "meta")
			{
				continue;
			}
			if (file.GetExtension().ToLowerInvariant() != "txt")
			{
				log.Log("ERROR: planet format invalid: " + file.GetExtension());
				continue;
			}
			bool converted;
			bool success;
			PlanetData value = LegacyConverter.CheckAndConvert_Planet(file.GetNameWithoutExtension(), file.ReadText(), log, out converted, out success);
			if (!success)
			{
				log.Log("ERROR: Loading planet failed: " + file.GetNameWithoutExtension());
				continue;
			}
			if (converted)
			{
				num++;
			}
			if (outputPlanetData.ContainsKey(file.GetNameWithoutExtension()))
			{
				log.Log("ERROR: Already has a planet named: " + file.GetNameWithoutExtension());
			}
			else
			{
				outputPlanetData[file.GetNameWithoutExtension()] = value;
			}
		}
		if (num > 0 && FileLocations.GetOneTimeNotification("Converted_Planets"))
		{
			log.Log("Found " + num + " legacy planet files and converted them automatically");
		}
	}

	private static bool TryLoadHeightmap(string json, out HeightMap heightmap)
	{
		return TryLoadHeightmapJson(json, out heightmap);
	}

	private static bool TryLoadHeightmapJson(string json, out HeightMap heightmap)
	{
		heightmap = null;
		try
		{
			heightmap = JsonUtility.FromJson<HeightMap>(json);
			return true;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return false;
		}
	}

	private static bool TryLoadHeightmapCustom(string json, out HeightMap heightmap)
	{
		List<float> list = new List<float>();
		int num = -1;
		for (int i = 0; i < json.Length; i++)
		{
			char c = json[i];
			if (char.IsNumber(c) || c == '.' || c == '-' || c == 'e')
			{
				if (num == -1)
				{
					num = i;
				}
			}
			else if (num != -1)
			{
				if (!float.TryParse(json.Substring(num, i - num), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					heightmap = null;
					return false;
				}
				list.Add(result);
				num = -1;
			}
		}
		heightmap = new HeightMap
		{
			points = list.ToArray()
		};
		return true;
	}

	private Dictionary<string, Planet> CreatePlanets(Dictionary<string, PlanetData> planets, I_MsgLogger log, out bool success)
	{
		Dictionary<string, Planet> dictionary = new Dictionary<string, Planet>();
		foreach (KeyValuePair<string, PlanetData> planet2 in planets)
		{
			try
			{
				Transform obj = new GameObject(planet2.Key).transform;
				obj.parent = base.transform;
				Planet planet = obj.gameObject.AddComponent<Planet>();
				planet.SetupData(planet2.Key, planet2.Value, terrainShader, waterShader, atmosphereShader, frontCloudsShader, ringsShader, log);
				dictionary.Add(planet2.Key, planet);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				log.Log("ERROR: creating planet from loaded data: " + planet2.Key);
				success = false;
				return dictionary;
			}
		}
		foreach (Planet value in dictionary.Values)
		{
			try
			{
				value.SetupInteractions(dictionary);
			}
			catch
			{
				log.Log("ERROR: finding parent/satellite: " + value.codeName);
				success = false;
				return dictionary;
			}
		}
		foreach (Planet value2 in dictionary.Values)
		{
			try
			{
				value2.SetupDepthAndSatelliteIndex();
			}
			catch
			{
				log.Log("ERROR: finding satellite index/depth of " + value2.codeName);
				success = false;
				return dictionary;
			}
		}
		success = true;
		return dictionary;
	}

	public Texture2D GetTexture(string name, I_MsgLogger log)
	{
		if (textures.ContainsKey(name))
		{
			return textures[name];
		}
		log.Log("ERROR: cant find texture: " + name);
		return new Texture2D(1, 1);
	}

	public HeightMap GetHeightMap(string name, I_MsgLogger log)
	{
		if (name != null && heightmaps.ContainsKey(name))
		{
			return heightmaps[name];
		}
		log.Log("ERROR: Cant find heightmap: " + name);
		return new HeightMap(new float[2]);
	}

	public static void GetSolarSystemsForModsMenu()
	{
		foreach (string allSolarSystem in GetAllSolarSystems())
		{
			IFile file = GetSolarSystemFolder(allSolarSystem).GetFile("Import_Settings.txt");
			ModsSettings.main.settings.solarSystemsActive.TryAdd(allSolarSystem, value: true);
			ModsListElement.ModData data = new ModsListElement.ModData
			{
				name = allSolarSystem,
				author = "",
				description = "",
				icon = null,
				type = ModsListElement.ModType.SolarSystem,
				version = "",
				saveName = allSolarSystem
			};
			if (file.Exists() && JsonWrapper.TryLoadJson<SolarSystemSettings>(file, out var data2))
			{
				data = new ModsListElement.ModData
				{
					name = allSolarSystem,
					author = (string.IsNullOrWhiteSpace(data2.authorName) ? "" : data2.authorName),
					description = (string.IsNullOrWhiteSpace(data2.description) ? "" : data2.description),
					icon = null,
					type = ModsListElement.ModType.SolarSystem,
					version = (string.IsNullOrWhiteSpace(data2.version) ? "" : data2.version),
					saveName = allSolarSystem
				};
			}
			else
			{
				data.description = "Failed to load the solar system's Import_Settings.txt";
			}
			if (!DevSettings.FullVersion)
			{
				data.description = "Planet pack purchase is required to load custom solar systems";
			}
			ModsMenu.AddMod(data);
			CustomAssetsLoader.onUnload.Push(delegate
			{
				ModsMenu.RemoveMod(data);
			});
		}
	}

	public static List<string> GetAllSolarSystems()
	{
		List<string> collected = new List<string>();
		CollectFromPath(FileLocations.SolarSystemsFolder);
		return collected;
		void CollectFromPath(IFolder pathToCollect)
		{
			if (pathToCollect.Exists())
			{
				foreach (IFolder folder in pathToCollect.GetFolders())
				{
					if (folder.Name != "Example" && !collected.Contains(folder.Name))
					{
						collected.Add(folder.Name);
					}
				}
			}
		}
	}

	public static IFolder GetSolarSystemFolder(string solarSystemName)
	{
		return FileLocations.SolarSystemsFolder.GetFolder(solarSystemName);
	}
}
