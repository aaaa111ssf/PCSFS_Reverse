using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using ModLoader;
using ModLoader.UI;
using Newtonsoft.Json;
using SFS.Builds;
using SFS.Parts.Modules;
using UnityEngine;

namespace SFS.Parts;

public static class CustomAssetsPacksLoader
{
	[Serializable]
	public class AssetBundlePack
	{
		public byte[] MacBuild;

		public byte[] WindowsBuild;

		public byte[] AndroidBuild;

		public byte[] IOS_Build;

		public byte[] CodeAssembly;

		public byte[] Data => WindowsBuild;
	}

	public static async UniTask LoadAssetPacks()
	{
		await UniTask.Yield();
		Debug.Log("Loading asset packs from: " + FileLocations.CustomAssetsFolder.Path);
		if (FileLocations.CustomAssetsFolderOld.Exists())
		{
			FileLocations.CustomAssetsFolderOld.Move(FileLocations.CustomAssetsFolder);
		}
		foreach (IFile path in FileLocations.CustomAssetsFolder.GetFolder("Parts").Create().GetFiles())
		{
			Debug.Log("Try load asset pack: " + path.Location);
			try
			{
				ModsListElement.ModData data = new ModsListElement.ModData
				{
					name = path.Name,
					author = "n/a",
					description = "n/a",
					icon = null,
					type = ModsListElement.ModType.AssetsPack,
					version = "n/a",
					saveName = path.Name
				};
				AssetBundlePack bundlePack = await UniTask.RunOnThreadPool(delegate
				{
					try
					{
						return JsonConvert.DeserializeObject<AssetBundlePack>(path.ReadText());
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
					return (AssetBundlePack)null;
				});
				CustomAssetsLoader.onUnload.Push(delegate
				{
					ModsMenu.RemoveMod(data);
				});
				if (bundlePack == null)
				{
					data.description = "ERROR: Cannot deserialize pack file";
					ModsMenu.AddMod(data);
					continue;
				}
				if (bundlePack.Data == null)
				{
					data.description = "ERROR: Pack doesn't support current platform. Ask the pack creator to export with the latest modding toolkit version";
					ModsMenu.AddMod(data);
					continue;
				}
				if (bundlePack.CodeAssembly == null)
				{
					goto IL_042e;
				}
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
				if (success)
				{
					goto IL_042e;
				}
				data.description = "ERROR: Failed to load custom scripts for pack";
				ModsMenu.AddMod(data);
				goto end_IL_012a;
				IL_042e:
				AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(bundlePack.Data);
				UnityEngine.Object[] array = assetBundle.LoadAllAssets();
				CustomAssetsLoader.onUnload.Push(delegate
				{
					assetBundle.Unload(unloadAllLoadedObjects: true);
				});
				if (array.Any((UnityEngine.Object x) => x.GetType() == typeof(PackData)))
				{
					PackData packData = array.OfType<PackData>().First();
					data = new ModsListElement.ModData
					{
						name = packData.DisplayName,
						author = packData.Author,
						description = packData.Description,
						icon = (packData.ShowIcon ? packData.Icon : null),
						type = ModsListElement.ModType.AssetsPack,
						version = packData.Version,
						saveName = path.Name
					};
				}
				else
				{
					data.description = "Failed to load info and description";
				}
				if (!DevSettings.FullVersion)
				{
					data.description = "Full Bundle ownership is required to use mods";
					ModsMenu.AddMod(data);
					continue;
				}
				ModsMenu.AddMod(data);
				Dictionary<string, bool> assetPacksActive = ModsSettings.main.settings.assetPacksActive;
				assetPacksActive.TryAdd(path.Name, value: true);
				if (assetPacksActive[path.Name])
				{
					LoadPackAssets(array);
				}
				end_IL_012a:;
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				CustomAssetsLoader.Report.AppendLine("Failed to load asset pack: " + path.Name);
				CustomAssetsLoader.Report.AppendLine(ex.Message);
			}
		}
		GC.Collect();
	}

	private static void LoadPackAssets(UnityEngine.Object[] allAssets)
	{
		List<string> addedColorTextures = new List<string>();
		List<string> addedShapeTextures = new List<string>();
		List<string> addedParts = new List<string>();
		List<string> addedVariants = new List<string>();
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
			if (!item5.HasComponent<Part>(out var component))
			{
				continue;
			}
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
			if (Base.partsLoader.parts.TryAdd(component.name, component))
			{
				addedParts.Add(component.name);
			}
			for (int l = 0; l < component.variants.Length; l++)
			{
				for (int m = 0; m < component.variants[l].variants.Length; m++)
				{
					VariantRef variantRef = new VariantRef(component, l, m);
					Base.partsLoader.partVariants[variantRef.GetNameID()] = variantRef;
					addedVariants.Add(variantRef.GetNameID());
				}
			}
			CustomAssetsLoader.onUnload.Push(delegate
			{
				foreach (string item6 in addedColorTextures)
				{
					Base.partsLoader.colorTextures.Remove(item6);
				}
				foreach (string item7 in addedShapeTextures)
				{
					Base.partsLoader.shapeTextures.Remove(item7);
				}
				foreach (string item8 in addedParts)
				{
					Base.partsLoader.parts.Remove(item8);
				}
				foreach (string item9 in addedVariants)
				{
					Base.partsLoader.partVariants.Remove(item9);
				}
			});
		}
	}
}
