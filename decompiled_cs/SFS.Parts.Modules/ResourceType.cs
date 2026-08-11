using SFS.Translations;
using UnityEngine;

namespace SFS.Parts.Modules;

[CreateAssetMenu]
public class ResourceType : ScriptableObject
{
	public TranslationVariable displayName;

	public TranslationVariable resourceUnit;

	public double resourceMass = 1.0;

	public double transferRate = 1.0;

	public float density = 1f;
}
