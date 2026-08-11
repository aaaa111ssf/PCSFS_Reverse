using System;

namespace SFS.Variables;

[Serializable]
public class Float_Local : Obs<float>
{
	protected override bool IsEqual(float a, float b)
	{
		return a == b;
	}

	public Float_Local()
	{
	}

	public Float_Local(float initialValue)
	{
		base.Value = initialValue;
	}
}
