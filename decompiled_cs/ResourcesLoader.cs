using System;
using System.Collections.Generic;
using System.Linq;
using SFS.Career;
using UnityEngine;

public class ResourcesLoader : MonoBehaviour
{
	[Serializable]
	public class ButtonIcons
	{
		public Sprite newRocket;

		public Sprite save;

		public Sprite load;

		public Sprite exit;

		public Sprite resume;

		public Sprite exit_Resume;

		public Sprite settings;

		public Sprite moveRocket;

		public Sprite clear;

		public Sprite videoTutorials;

		public Sprite exampleRockets;

		public Sprite shareRocket;

		public Sprite cheats;

		public Sprite revert;

		public Sprite recover;

		public Sprite destroy;

		[HideInInspector]
		public Sprite collectRock;

		[HideInInspector]
		public Sprite removeFlag;
	}

	[Serializable]
	public class ChallengeIcons
	{
		public Sprite firstFlight;

		public Sprite icon_10Km;

		public Sprite icon_30Km;

		public Sprite icon_50Km;

		public Sprite icon_100Km;

		public Sprite icon_Downrange;

		public Sprite icon_Reach_Orbit;

		public Sprite icon_Orbit_High;

		public Sprite icon_Capture;

		public Sprite icon_Crash;

		public Sprite icon_UnmannedLanding;

		public Sprite icon_MannedLanding;

		public Sprite icon_Tour;
	}

	public const string PART_TEXTURES_PATH = "Part Textures";

	public const string RESOURCE_TYPE_PATH = "Fuels";

	public const string PICK_CATEGORIES_PATH = "Pick Categories";

	public const string SHADOW_TEXTURES_PATH = "Part Textures/Shadow Textures";

	public const string PART_PACK_DATA_PATH = "Career_PartPacks";

	public static ResourcesLoader main;

	public ButtonIcons buttonIcons;

	public ChallengeIcons challengeIcons;

	public Dictionary<string, TT_PartPackData> partPacks;

	private void Awake()
	{
		main = this;
		partPacks = GetFiles_Dictionary<TT_PartPackData>("Career_PartPacks");
	}

	public static Dictionary<string, T> GetFiles_Dictionary<T>(string path) where T : UnityEngine.Object
	{
		return GetFiles_Array<T>(path).ToDictionary((T x) => x.name, (T x) => x);
	}

	public static T[] GetFiles_Array<T>(string path) where T : UnityEngine.Object
	{
		return Resources.LoadAll<T>(path);
	}
}
