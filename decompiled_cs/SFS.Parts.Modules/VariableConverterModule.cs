using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SFS.Variables;
using UnityEngine;

namespace SFS.Parts.Modules;

public class VariableConverterModule : MonoBehaviour
{
	[Serializable]
	public struct BoolToDouble
	{
		public Bool_Reference input;

		public Double_Reference output;

		public void Convert()
		{
			output.Value = (input.Value ? 1 : 0);
		}
	}

	[Serializable]
	public struct DoubleToBool
	{
		public Double_Reference input;

		public Bool_Reference output;

		public Bool_Reference useEpsilon;

		public void Convert()
		{
			output.Value = Math.Abs(input.Value) > (useEpsilon.Value ? 1E-06 : 0.0);
		}
	}

	[Serializable]
	public struct DoubleToString
	{
		public Double_Reference input;

		public String_Reference output;

		public void Convert()
		{
			output.Value = input.Value.ToString(CultureInfo.InvariantCulture);
		}
	}

	[Serializable]
	public struct StringToDouble
	{
		public String_Reference input;

		public Double_Reference output;

		public void Convert()
		{
			if (double.TryParse(input.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				output.Value = result;
			}
		}
	}

	[Serializable]
	public struct BoolToString
	{
		public Bool_Reference input;

		public String_Reference output;

		public bool overrideOutputs;

		public String_Reference trueString;

		public String_Reference falseString;

		public void Convert()
		{
			if (overrideOutputs)
			{
				output.Value = (input.Value ? trueString.Value : falseString.Value);
			}
			else
			{
				output.Value = (input.Value ? "true" : "false");
			}
		}
	}

	[Serializable]
	public struct StringToBool
	{
		public String_Reference input;

		public Bool_Reference output;

		public bool overrideParser;

		public String_Reference[] trueStrings;

		public void Convert()
		{
			StringToBool t = this;
			bool result;
			if (overrideParser)
			{
				output.Value = trueStrings.Any((String_Reference s) => s.Value == t.input.Value);
			}
			else if (bool.TryParse(input.Value, out result))
			{
				output.Value = result;
			}
		}
	}

	public List<BoolToDouble> boolToDouble = new List<BoolToDouble>();

	public List<DoubleToBool> doubleToBool = new List<DoubleToBool>();

	[Space]
	public List<DoubleToString> doubleToString = new List<DoubleToString>();

	public List<StringToDouble> stringToDouble = new List<StringToDouble>();

	[Space]
	public List<BoolToString> boolToString = new List<BoolToString>();

	public List<StringToBool> stringToBool = new List<StringToBool>();

	private void Awake()
	{
		SubscribeToInputChanges();
	}

	private void SubscribeToInputChanges()
	{
		foreach (BoolToDouble item in boolToDouble)
		{
			item.input.OnChange -= new Action(item.Convert);
			item.input.OnChange += new Action(item.Convert);
		}
		foreach (DoubleToBool item2 in doubleToBool)
		{
			item2.input.OnChange -= new Action(item2.Convert);
			item2.input.OnChange += new Action(item2.Convert);
		}
		foreach (DoubleToString item3 in doubleToString)
		{
			item3.input.OnChange -= new Action(item3.Convert);
			item3.input.OnChange += new Action(item3.Convert);
		}
		foreach (StringToDouble item4 in stringToDouble)
		{
			item4.input.OnChange -= new Action(item4.Convert);
			item4.input.OnChange += new Action(item4.Convert);
		}
		foreach (StringToBool item5 in stringToBool)
		{
			item5.input.OnChange -= new Action(item5.Convert);
			item5.input.OnChange += new Action(item5.Convert);
		}
		foreach (BoolToString item6 in boolToString)
		{
			item6.input.OnChange -= new Action(item6.Convert);
			item6.input.OnChange += new Action(item6.Convert);
		}
	}
}
