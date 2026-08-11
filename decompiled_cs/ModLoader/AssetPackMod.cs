using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using ModLoader.UI;
using Newtonsoft.Json;
using SFS;
using SFS.Builds;
using SFS.Parts;
using SFS.Parts.Modules;
using UnityEngine;

namespace ModLoader;

public class AssetPackMod
{
	private IFile packFile;

	private ModsListElement.ModData data;

	private AssetBundle assetBundle;

	private UnityEngine.Object[] allAssets;

	private List<string> addedColorTextures = new List<string>();

	private List<string> addedShapeTextures = new List<string>();

	private List<string> addedVariants = new List<string>();

	public async UniTask Load()
	{
		_ = 2;
		try
		{
			data = new ModsListElement.ModData
			{
				name = packFile.Name,
				author = "n/a",
				description = "n/a",
				icon = null,
				type = ModsListElement.ModType.AssetsPack,
				version = "n/a",
				saveName = packFile.Name
			};
			CustomAssetsPacksLoader.AssetBundlePack bundlePack = await UniTask.RunOnThreadPool(delegate
			{
				try
				{
					return JsonConvert.DeserializeObject<CustomAssetsPacksLoader.AssetBundlePack>(packFile.ReadText());
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				return (CustomAssetsPacksLoader.AssetBundlePack)null;
			});
			if (bundlePack == null)
			{
				data.description = "ERROR: Cannot deserialize pack file";
				ModsMenu.AddMod(data);
				return;
			}
			if (bundlePack.Data == null)
			{
				data.description = "ERROR: Pack doesn't support current platform. Ask the pack creator to export with the latest modding toolkit version";
				ModsMenu.AddMod(data);
				return;
			}
			if (bundlePack.CodeAssembly != null)
			{
				bool success = true;
				await UniTask.RunOnThreadPool(delegate
				{
					try
					{
						Assembly.Load(bundlePack.CodeAssembly);
					}
					catch (Exception exception)
					{
						success = false;
						Debug.LogException(exception);
					}
				});
				if (!success)
				{
					data.description = "ERROR: Failed to load custom scripts for pack";
					ModsMenu.AddMod(data);
					return;
				}
			}
			allAssets = (await AssetBundle.LoadFromMemoryAsync(bundlePack.Data)).LoadAllAssets();
			if (allAssets.Any((UnityEngine.Object x) => x.GetType() == typeof(PackData)))
			{
				PackData packData = allAssets.OfType<PackData>().First();
				data = new ModsListElement.ModData
				{
					name = packData.DisplayName,
					author = packData.Author,
					description = packData.Description,
					icon = (packData.ShowIcon ? packData.Icon : null),
					type = ModsListElement.ModType.AssetsPack,
					version = packData.Version,
					saveName = packFile.Name
				};
			}
			else
			{
				data.description = "Failed to load info and description";
			}
			if (!DevSettings.FullVersion)
			{
				data.description = "Full bundle is required to load mods";
				ModsMenu.AddMod(data);
				return;
			}
			ModsMenu.AddMod(data);
			Dictionary<string, bool> assetPacksActive = ModsSettings.main.settings.assetPacksActive;
			assetPacksActive.TryAdd(packFile.Name, value: true);
			if (assetPacksActive[packFile.Name])
			{
				LoadPackAssets();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
			CustomAssetsLoader.Report.AppendLine("Failed to load asset pack: " + packFile.Name);
			CustomAssetsLoader.Report.AppendLine(ex.Message);
		}
		void LoadPackAssets()
		{
			Dictionary<string, ResourceType> files_Dictionary = ResourcesLoader.GetFiles_Dictionary<ResourceType>("Fuels");
			foreach (ResourceType item in allAssets.OfType<ResourceType>())
			{
				files_Dictionary.TryAdd(item.name, item);
			}
			Dictionary<string, PickCategory> files_Dictionary2 = ResourcesLoader.GetFiles_Dictionary<PickCategory>("Pick Categories");
			foreach (PickCategory item2 in allAssets.OfType<PickCategory>())
			{
				files_Dictionary2.TryAdd(item2.name, item2);
			}
			foreach (ColorTexture item3 in allAssets.OfType<ColorTexture>())
			{
				if (Base.partsLoader.colorTextures.TryGetValue(item3.name, out var value))
				{
					item3.multiple = value.multiple;
					item3.colorTex = value.colorTex;
					item3.segments = value.segments;
				}
				else
				{
					Base.partsLoader.colorTextures.Add(item3.name, item3);
					addedColorTextures.Add(item3.name);
				}
			}
			foreach (ShapeTexture item4 in allAssets.OfType<ShapeTexture>())
			{
				if (Base.partsLoader.shapeTextures.TryGetValue(item4.name, out var value2))
				{
					item4.multiple = value2.multiple;
					item4.shapeTex = value2.shapeTex;
					item4.segments = value2.segments;
					item4.shadowTex = value2.shadowTex;
				}
				else
				{
					Base.partsLoader.shapeTextures.Add(item4.name, item4);
					addedShapeTextures.Add(item4.name);
				}
			}
			foreach (GameObject item5 in allAssets.OfType<GameObject>())
			{
				if (item5.HasComponent<Part>(out var component))
				{
					Variants[] variants = component.variants;
					foreach (Variants variants2 in variants)
					{
						Variants.Variant[] variants3 = variants2.variants;
						for (int j = 0; j < variants3.Length; j++)
						{
							Variants.PickTag[] tags = variants3[j].tags;
							foreach (Variants.PickTag pickTag in tags)
							{
								pickTag.tag = files_Dictionary2.GetValueOrDefault(pickTag.tag.name, pickTag.tag);
							}
						}
						variants2.tags = Array.Empty<Variants.PickTag>();
					}
					FlowModule[] componentsInChildren = item5.GetComponentsInChildren<FlowModule>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						FlowModule.Flow[] sources = componentsInChildren[i].sources;
						foreach (FlowModule.Flow flow in sources)
						{
							flow.resourceType = files_Dictionary.GetValueOrDefault(flow.resourceType.name, flow.resourceType);
						}
					}
					ResourceModule[] componentsInChildren2 = item5.GetComponentsInChildren<ResourceModule>();
					foreach (ResourceModule resourceModule in componentsInChildren2)
					{
						resourceModule.resourceType = files_Dictionary.GetValueOrDefault(resourceModule.resourceType.name, resourceModule.resourceType);
					}
					Base.partsLoader.parts.TryAdd(component.name, component);
					for (int l = 0; l < component.variants.Length; l++)
					{
						for (int m = 0; m < component.variants[l].variants.Length; m++)
						{
							VariantRef variantRef = new VariantRef(component, l, m);
							Base.partsLoader.partVariants[variantRef.GetNameID()] = variantRef;
							addedVariants.Add(variantRef.GetNameID());
						}
					}
				}
			}
		}
	}

	public void Unload()
	{
		foreach (string addedColorTexture in addedColorTextures)
		{
			Base.partsLoader.colorTextures.Remove(addedColorTexture);
		}
		foreach (string addedShapeTexture in addedShapeTextures)
		{
			Base.partsLoader.shapeTextures.Remove(addedShapeTexture);
		}
		foreach (string addedVariant in addedVariants)
		{
			Base.partsLoader.partVariants.Remove(addedVariant);
		}
		ModsMenu.RemoveMod(data);
		UnityEngine.Object[] array = allAssets;
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
		addedColorTextures.Clear();
		addedShapeTextures.Clear();
		addedVariants.Clear();
		allAssets = null;
	}
}
