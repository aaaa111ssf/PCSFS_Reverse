using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ModLoader;
using ModLoader.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SFS.Parsers.Json;
using UnityEngine;

namespace SFS.Parts;

public static class TextureLoader
{
	private class T2DConverter : JsonConverter<Texture2D>
	{
		public override Texture2D ReadJson(JsonReader reader, Type objectType, Texture2D existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.Value == null || string.IsNullOrWhiteSpace((string)reader.Value))
			{
				return null;
			}
			Texture2D texture2D = TextureUtility.FromFile(currentPack.GetFolder("Textures").GetFile((string)reader.Value));
			texture2D.wrapMode = TextureWrapMode.Clamp;
			return texture2D;
		}

		public override void WriteJson(JsonWriter writer, Texture2D value, JsonSerializer serializer)
		{
			string text = value.name + ".png";
			writer.WriteValue(text);
			IFile file = currentPack.GetFolder("Textures").Create().GetFile(text);
			if (!file.Exists())
			{
				value.SaveToFile(file);
			}
		}
	}

	private class ShadowTextureConverter : JsonConverter<ShadowTexture>
	{
		public override void WriteJson(JsonWriter writer, ShadowTexture value, JsonSerializer serializer)
		{
			writer.WriteValue(value.name);
		}

		public override ShadowTexture ReadJson(JsonReader reader, Type objectType, ShadowTexture existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return shadowTexturesDictionary.GetValueOrDefault((string)reader.Value);
		}
	}

	private class SpriteConverter : JsonConverter<Sprite>
	{
		public override void WriteJson(JsonWriter writer, Sprite value, JsonSerializer serializer)
		{
			writer.WriteValue((object?)null);
		}

		public override Sprite ReadJson(JsonReader reader, Type objectType, Sprite existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return null;
		}
	}

	private static IFolder currentPack;

	private static Dictionary<string, ShadowTexture> shadowTexturesDictionary;

	public static async UniTask LoadTexturePacks()
	{
		JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
		{
			MaxDepth = 10,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			Converters = new List<JsonConverter>
			{
				new T2DConverter(),
				new ShadowTextureConverter(),
				new SpriteConverter()
			},
			Formatting = Formatting.Indented
		});
		JsonSerializer serializerForShadowTexture = JsonSerializer.CreateDefault(new JsonSerializerSettings
		{
			MaxDepth = 10,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			Converters = new List<JsonConverter>
			{
				new T2DConverter(),
				new SpriteConverter()
			},
			Formatting = Formatting.Indented
		});
		shadowTexturesDictionary = ResourcesLoader.GetFiles_Dictionary<ShadowTexture>("Part Textures/Shadow Textures");
		IFolder folder = FileLocations.CustomAssetsFolder.GetFolder("Texture Packs").Create();
		if (!Application.isEditor)
		{
			CreateExampleTexturePack(folder, serializer, serializerForShadowTexture);
		}
		foreach (IFolder folder2 in folder.GetFolders())
		{
			if (folder2.Name != "Example")
			{
				await LoadTexturePack(folder2, serializer, serializerForShadowTexture);
			}
		}
	}

	private static void CreateExampleTexturePack(IFolder basePath, JsonSerializer serializer, JsonSerializer serializerForShadowTexture)
	{
		IFolder folder = basePath.GetFolder("Example");
		if (folder.Exists())
		{
			return;
		}
		folder.Create();
		IFolder folder2 = folder.GetFolder("Color Textures").Create();
		IFolder folder3 = folder.GetFolder("Shape Textures").Create();
		IFolder folder4 = folder.GetFolder("Shadow Textures").Create();
		currentPack = folder;
		foreach (KeyValuePair<string, ColorTexture> colorTexture in Base.partsLoader.colorTextures)
		{
			folder2.GetFile(colorTexture.Key + ".txt").WriteText(serializer.Serialize(colorTexture.Value));
		}
		foreach (KeyValuePair<string, ShapeTexture> shapeTexture in Base.partsLoader.shapeTextures)
		{
			folder3.GetFile(shapeTexture.Key + ".txt").WriteText(serializer.Serialize(shapeTexture.Value));
		}
		foreach (KeyValuePair<string, ShadowTexture> item in shadowTexturesDictionary)
		{
			folder4.GetFile(item.Key + ".txt").WriteText(serializerForShadowTexture.Serialize(item.Value));
		}
		folder.GetFile("pack_info.txt").WriteText(serializerForShadowTexture.Serialize(PackData.ExampleTexturePack()));
	}

	private static async UniTask LoadTexturePack(IFolder folder, JsonSerializer serializer, JsonSerializer serializerForShadowTexture)
	{
		currentPack = folder;
		ModsListElement.ModData data = new ModsListElement.ModData
		{
			name = folder.Name,
			author = "n/a",
			description = "n/a",
			icon = null,
			type = ModsListElement.ModType.TexturesPack,
			version = "n/a",
			saveName = folder.Name
		};
		CustomAssetsLoader.onUnload.Push(delegate
		{
			ModsMenu.RemoveMod(data);
		});
		IFile file = folder.GetFile("pack_info.txt");
		if (file.Exists())
		{
			try
			{
				PackData packData = ScriptableObject.CreateInstance<PackData>();
				JObject.Parse(await file.ReadTextAsync()).Populate(packData, serializer);
				data = new ModsListElement.ModData
				{
					author = packData.Author,
					description = packData.Description,
					icon = packData.Icon,
					name = packData.DisplayName,
					type = ModsListElement.ModType.TexturesPack,
					version = packData.Version,
					saveName = folder.Name
				};
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				data.description = "Failed to load info and description";
			}
		}
		else
		{
			data.description = "No info and description";
		}
		if (!DevSettings.FullVersion)
		{
			data.description = "Full bundle is required to load mods";
			ModsMenu.AddMod(data);
			return;
		}
		ModsMenu.AddMod(data);
		Dictionary<string, bool> texturePacksActive = ModsSettings.main.settings.texturePacksActive;
		texturePacksActive.TryAdd(folder.Name, value: true);
		if (!texturePacksActive[folder.Name])
		{
			return;
		}
		IFolder folder2 = folder.GetFolder("Color Textures").Create();
		IFolder shadowTextures = folder.GetFolder("Shadow Textures").Create();
		IFolder shapeTextures = folder.GetFolder("Shape Textures").Create();
		List<(string name, ColorTexture old)> addedColorTextures = new List<(string, ColorTexture)>();
		List<(string name, ShadowTexture old)> addedShadowTextures = new List<(string, ShadowTexture)>();
		List<(string name, ShapeTexture old)> addedShapeTextures = new List<(string, ShapeTexture)>();
		if (folder2.Exists())
		{
			foreach (IFile colorTextureFile in folder2.GetFiles())
			{
				try
				{
					ColorTexture colorTexture = ScriptableObject.CreateInstance<ColorTexture>();
					JObject.Parse(await colorTextureFile.ReadTextAsync()).Populate(colorTexture, serializer);
					Base.partsLoader.colorTextures.TryGetValue(colorTexture.name, out var value);
					Base.partsLoader.colorTextures[colorTexture.name] = colorTexture;
					addedColorTextures.Add((colorTexture.name, value));
				}
				catch (Exception ex)
				{
					CustomAssetsLoader.Report.AppendLine("Failed to load " + colorTextureFile.GetNameWithoutExtension() + " texture in " + folder.Name + " pack:");
					CustomAssetsLoader.Report.AppendLine(ex.Message);
				}
			}
		}
		if (shadowTextures.Exists())
		{
			foreach (IFile colorTextureFile in shadowTextures.GetFiles())
			{
				try
				{
					ShadowTexture shadowTexture = ScriptableObject.CreateInstance<ShadowTexture>();
					JObject.Parse(await colorTextureFile.ReadTextAsync()).Populate(shadowTexture, serializerForShadowTexture);
					shadowTexturesDictionary.TryGetValue(shadowTexture.name, out var value2);
					shadowTexturesDictionary[shadowTexture.name] = shadowTexture;
					addedShadowTextures.Add((shadowTexture.name, value2));
				}
				catch (Exception ex2)
				{
					CustomAssetsLoader.Report.AppendLine("Failed to load " + colorTextureFile.GetNameWithoutExtension() + " texture in " + folder.Name + " pack:");
					CustomAssetsLoader.Report.AppendLine(ex2.Message);
				}
			}
		}
		if (shapeTextures.Exists())
		{
			foreach (IFile colorTextureFile in shapeTextures.GetFiles())
			{
				try
				{
					ShapeTexture shapeTexture = ScriptableObject.CreateInstance<ShapeTexture>();
					JObject.Parse(await colorTextureFile.ReadTextAsync()).Populate(shapeTexture, serializer);
					Base.partsLoader.shapeTextures.TryGetValue(shapeTexture.name, out var value3);
					Base.partsLoader.shapeTextures[shapeTexture.name] = shapeTexture;
					addedShapeTextures.Add((shapeTexture.name, value3));
				}
				catch (Exception ex3)
				{
					CustomAssetsLoader.Report.AppendLine("Failed to load " + colorTextureFile.GetNameWithoutExtension() + " texture in " + folder.Name + " pack:");
					CustomAssetsLoader.Report.AppendLine(ex3.Message);
				}
			}
		}
		CustomAssetsLoader.onUnload.Push(delegate
		{
			foreach (var (key, colorTexture2) in addedColorTextures)
			{
				if ((bool)colorTexture2)
				{
					Base.partsLoader.colorTextures[key] = colorTexture2;
				}
				else
				{
					Base.partsLoader.colorTextures.Remove(key);
				}
			}
			foreach (var (key2, shapeTexture2) in addedShapeTextures)
			{
				if ((bool)shapeTexture2)
				{
					Base.partsLoader.shapeTextures[key2] = shapeTexture2;
				}
				else
				{
					Base.partsLoader.shapeTextures.Remove(key2);
				}
			}
			foreach (var (key3, shadowTexture2) in addedShadowTextures)
			{
				if ((bool)shadowTexture2)
				{
					shadowTexturesDictionary[key3] = shadowTexture2;
				}
				else
				{
					shadowTexturesDictionary.Remove(key3);
				}
			}
		});
	}
}
