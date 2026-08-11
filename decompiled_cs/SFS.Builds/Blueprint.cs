using System;
using SFS.Parsers.Json;
using SFS.Parts;
using SFS.Translations;
using SFS.World;
using UnityEngine;

namespace SFS.Builds;

[Serializable]
public class Blueprint
{
	public float center = float.NaN;

	public PartSave[] parts;

	public StageSave[] stages = Array.Empty<StageSave>();

	public float rotation;

	public Vector2 offset;

	public bool interiorView = true;

	public Blueprint()
	{
	}

	public Blueprint(PartSave[] parts, StageSave[] stages, float center, float rotation, bool interiorView)
	{
		this.parts = parts;
		this.stages = stages;
		this.center = center;
		this.rotation = rotation;
		this.interiorView = interiorView;
	}

	public static void Save(IFolder path, Blueprint blueprint, string version)
	{
		JsonWrapper.SaveAsJson(path.GetFile("Version.txt"), version, pretty: false);
		JsonWrapper.SaveAsJson(path.GetFile("Blueprint.txt"), blueprint, pretty: true);
	}

	public static bool TryLoad(IFolder path, I_MsgLogger errorLogger, out Blueprint blueprint)
	{
		if (path.Exists() && JsonWrapper.TryLoadJson<Blueprint>(path.GetFile("Blueprint.txt"), out blueprint))
		{
			return true;
		}
		errorLogger.Log(Loc.main.Load_Failed.InjectField(Loc.main.Blueprint, "filetype", formatCapitalization: true).Inject(path.Path, "filepath"));
		blueprint = null;
		return false;
	}
}
