using System;
using UnityEngine;

namespace SFS.Parts.Modules;

public class MovingMassModule : MonoBehaviour, I_InitializePartModule
{
	public Part part;

	public MoveModule moveModule;

	public Transform massPosition;

	public int Priority => 0;

	public void Initialize()
	{
		moveModule.time.OnChange += new Action(Set);
	}

	private void Set()
	{
		part.centerOfMass.Value = part.transform.InverseTransformPoint(massPosition.position);
	}
}
