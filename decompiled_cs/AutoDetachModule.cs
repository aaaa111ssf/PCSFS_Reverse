using System;
using System.Collections.Generic;
using SFS.Parts;
using SFS.Parts.Modules;
using SFS.World;
using UnityEngine;

public class AutoDetachModule : MonoBehaviour, Rocket.INJ_Rocket
{
	public Part Part;

	public string[] partToDetach = Array.Empty<string>();

	public DetachModule detachModule;

	private short _currentStage;

	private List<Part> _visited;

	private const short _max_search_level = 3;

	public Rocket Rocket { private get; set; }

	private void Awake()
	{
		_currentStage = 0;
		_visited = new List<Part>();
	}

	private Part SearchPart(string partName, Part startPoint, int level)
	{
		_visited.Add(startPoint);
		if (level >= 3)
		{
			return null;
		}
		foreach (PartJoint connectedJoint in Rocket.jointsGroup.GetConnectedJoints(startPoint))
		{
			Part otherPart = connectedJoint.GetOtherPart(startPoint);
			if (!AlreadyVisited(otherPart))
			{
				if (otherPart.name == partName)
				{
					return otherPart;
				}
				otherPart = SearchPart(partName, otherPart, level + 1);
				if (otherPart != null)
				{
					return otherPart;
				}
			}
		}
		return null;
	}

	private bool AlreadyVisited(Part part)
	{
		return _visited.Exists((Part e) => e == part);
	}

	public void OnDetach(UsePartData data)
	{
		if (data.sharedData.fromStaging)
		{
			if (_currentStage + 1 == partToDetach.Length || partToDetach.Length == 0)
			{
				detachModule.Detach(data);
			}
			_currentStage++;
		}
		else
		{
			if (partToDetach == null)
			{
				return;
			}
			Part part = SearchPart(partToDetach[_currentStage], Part, 0);
			_visited = new List<Part>();
			_currentStage++;
			DetachModule[] modules = part.GetModules<DetachModule>();
			if (modules == null || modules.Length == 0)
			{
				Debug.Log("Other part don't have DetachModule");
				return;
			}
			DetachModule[] array = modules;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Detach(data);
			}
		}
	}
}
