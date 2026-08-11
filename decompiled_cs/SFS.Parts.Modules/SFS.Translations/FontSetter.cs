using System;
using UnityEngine;
using UnityEngine.UI;

namespace SFS.Translations;

public class FontSetter : MonoBehaviour
{
	private Text textComponent;

	private TextMesh textMeshComponent;

	private MeshRenderer textMeshRendererComponent;

	private TextMesh textMeshUguiComponent;

	private bool isInitialized;

	private void Start()
	{
		Loc.OnChange += new Action(UpdateFont);
	}

	private void OnDestroy()
	{
		Loc.OnChange -= new Action(UpdateFont);
	}

	private void UpdateFont()
	{
		if (!(Base.language == null))
		{
			SetFont(Base.language.currentFont);
		}
	}

	private void SetFont(Font font)
	{
		if (!isInitialized)
		{
			Initialize();
		}
		if (textComponent != null)
		{
			textComponent.font = font;
			textComponent.SetLayoutDirty();
			textComponent.rectTransform.ForceUpdateRectTransforms();
		}
		else if (textMeshComponent != null)
		{
			textMeshComponent.font = font;
			if (textMeshRendererComponent != null)
			{
				textMeshRendererComponent.material = font.material;
			}
		}
		else if (textMeshUguiComponent != null)
		{
			textMeshUguiComponent.font = font;
		}
	}

	private void Initialize()
	{
		textComponent = GetComponentInChildren<Text>();
		if (textComponent == null)
		{
			textMeshComponent = GetComponentInChildren<TextMesh>();
			if (textMeshComponent != null)
			{
				textMeshRendererComponent = textMeshComponent.GetComponent<MeshRenderer>();
			}
			else
			{
				textMeshUguiComponent = GetComponentInChildren<TextMesh>();
			}
		}
		isInitialized = true;
	}
}
