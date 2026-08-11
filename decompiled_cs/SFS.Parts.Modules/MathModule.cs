using System;
using System.Collections.Generic;
using SFS.Variables;
using UnityEngine;

namespace SFS.Parts.Modules;

public class MathModule : MonoBehaviour
{
	[Serializable]
	public struct NumericCalculator
	{
		public Composed_Double input;

		public Double_Reference output;

		public void Register()
		{
			NumericCalculator inst = this;
			_ = input.Value;
			input.OnChange -= new Action<double, double>(OnChange);
			input.OnChange += new Action<double, double>(OnChange);
			void OnChange(double _, double value)
			{
				inst.output.Value = value;
			}
		}
	}

	[Serializable]
	public struct BooleanComparisonCalculator
	{
		public enum Condition
		{
			GreaterThan,
			LessThan,
			GreaterThanOrEqual,
			LessThanOrEqual,
			Equal
		}

		public Double_Reference a;

		public Double_Reference b;

		[Space]
		public Condition comparisonCondition;

		public Bool_Reference invert;

		[Tooltip("Adds a tolerance of 0.00001 when comparing values. Less/GreaterThan actually become more strict as the difference has to be at least the epsilon.")]
		public bool laxMode;

		[Space]
		public Bool_Reference output;

		private const double epsilon = 1E-05;

		public void Register()
		{
			a.OnChange += new Action(OnChange);
			b.OnChange += new Action(OnChange);
			invert.OnChange += new Action(OnChange);
		}

		private void OnChange()
		{
			bool flag = false;
			if (comparisonCondition == Condition.Equal)
			{
				flag = (laxMode ? (Math.Abs(a.Value - b.Value) <= 1E-05) : (a.Value == b.Value));
			}
			else if (comparisonCondition == Condition.GreaterThan)
			{
				flag = (laxMode ? (a.Value - b.Value >= 1E-05) : (a.Value > b.Value));
			}
			else if (comparisonCondition == Condition.LessThan)
			{
				flag = (laxMode ? (b.Value - a.Value >= 1E-05) : (a.Value < b.Value));
			}
			else if (comparisonCondition == Condition.GreaterThanOrEqual)
			{
				flag = (laxMode ? (a.Value - b.Value >= -1E-05) : (a.Value >= b.Value));
			}
			else if (comparisonCondition == Condition.LessThanOrEqual)
			{
				flag = (laxMode ? (b.Value - a.Value >= -1E-05) : (a.Value <= b.Value));
			}
			if (invert.Value)
			{
				flag = !flag;
			}
			output.Value = flag;
		}
	}

	[Serializable]
	public struct BooleanOperatorCalculator
	{
		public enum Operator
		{
			AND,
			NAND,
			OR,
			NOR,
			XOR,
			XNOR
		}

		public Bool_Reference a;

		public Operator op;

		public Bool_Reference b;

		[Space]
		public Bool_Reference output;

		public void Register()
		{
			a.OnChange += new Action(OnChange);
			b.OnChange += new Action(OnChange);
		}

		private void OnChange()
		{
			bool value = false;
			if (op == Operator.OR)
			{
				value = a.Value || b.Value;
			}
			else if (op == Operator.XOR)
			{
				value = a.Value ^ b.Value;
			}
			else if (op == Operator.XNOR)
			{
				value = a.Value == b.Value;
			}
			else if (op == Operator.AND)
			{
				value = a.Value && b.Value;
			}
			else if (op == Operator.NAND)
			{
				value = !a.Value || !b.Value;
			}
			else if (op == Operator.NOR)
			{
				value = !a.Value && !b.Value;
			}
			output.Value = value;
		}
	}

	public List<NumericCalculator> numericCalculators = new List<NumericCalculator>();

	public List<BooleanComparisonCalculator> booleanComparisonCalculators = new List<BooleanComparisonCalculator>();

	public List<BooleanOperatorCalculator> booleanOperatorCalculators = new List<BooleanOperatorCalculator>();

	private void Awake()
	{
		RegisterAll();
	}

	private void RegisterAll()
	{
		foreach (NumericCalculator numericCalculator in numericCalculators)
		{
			numericCalculator.Register();
		}
		foreach (BooleanComparisonCalculator booleanComparisonCalculator in booleanComparisonCalculators)
		{
			booleanComparisonCalculator.Register();
		}
		foreach (BooleanOperatorCalculator booleanOperatorCalculator in booleanOperatorCalculators)
		{
			booleanOperatorCalculator.Register();
		}
	}
}
