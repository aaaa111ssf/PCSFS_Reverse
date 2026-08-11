using SFS.Translations;
using SFS.UI;
using SFS.Variables;
using SFS.World;
using UnityEngine;

namespace SFS.Parts.Modules;

public class DisableOnNoFlow : MonoBehaviour
{
	public FlowModule flowModule;

	public Bool_Reference on;

	public bool logOutOfResource;

	private bool previousFrameWasOn;

	private void Update()
	{
		if (GameManager.main == null || !on.Value)
		{
			previousFrameWasOn = on.Value;
			return;
		}
		FlowModule.Flow[] sources = flowModule.sources;
		foreach (FlowModule.Flow flow in sources)
		{
			FlowModule.FlowState value = flow.state.Value;
			if (value != FlowModule.FlowState.IsFlowing && value != FlowModule.FlowState.CanFlow)
			{
				if (logOutOfResource)
				{
					MsgDrawer.main.Log(previousFrameWasOn ? Loc.main.Msg_No_Resource_Left.InjectField(flow.resourceType.displayName, "resource") : Loc.main.Msg_No_Resource_Source.InjectField(flow.resourceType.displayName, "resource"));
				}
				on.Value = false;
				break;
			}
		}
		previousFrameWasOn = on.Value;
	}
}
