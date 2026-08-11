using SFS.Variables;
using UnityEngine;

namespace SFS.Parts.Modules;

public class MultiStepToggleSequenceModule : MonoBehaviour
{
	public Float_Reference state;

	public UsePartUnityEvent[] steps;

	private bool CanActivate => steps.IsValidIndex(Index);

	private int Index => Mathf.RoundToInt(state.Value);

	public void Activate(UsePartData data)
	{
		if (CanActivate)
		{
			steps[Index].Invoke(data);
			state.Value = (state.Value + 1f) % (float)steps.Length;
		}
	}
}
