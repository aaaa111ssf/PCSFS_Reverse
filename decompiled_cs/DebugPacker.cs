using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using SFS.UI;
using SFS.WorldBase;
using UnityEngine;

public class DebugPacker
{
	public static void SetupLogsExport(Button button)
	{
		button.onLongClick += (Action)delegate
		{
			using MemoryStream memoryStream = new MemoryStream();
			GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true);
			try
			{
				foreach (IFile file in FileLocations.LogsFolder.GetFiles())
				{
					if (file.GetNameWithoutExtension() != "counter")
					{
						WriteFileEntry(file.Name, file.ReadBytes());
					}
				}
				zipStream.Flush();
				zipStream.Close();
				GUIUtility.systemCopyBuffer = Convert.ToBase64String(memoryStream.ToArray());
			}
			finally
			{
				if (zipStream != null)
				{
					((IDisposable)zipStream).Dispose();
				}
			}
			void WriteFileEntry(string fileName, byte[] fileData)
			{
				Write(BitConverter.GetBytes(fileName.Length));
				Write(Encoding.UTF8.GetBytes(fileName));
				Write(BitConverter.GetBytes(fileData.Length));
				Write(fileData);
			}
		};
		void Write(byte[] data)
		{
			P_1.zipStream.Write(data, 0, data.Length);
		}
	}

	public static void SetupLogsExport_World(Button button, WorldReference world)
	{
		button.onLongClick += (Action)delegate
		{
			using MemoryStream memoryStream = new MemoryStream();
			GZipStream zipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true);
			try
			{
				Write(BitConverter.GetBytes(world.worldName.Length));
				Write(Encoding.UTF8.GetBytes(world.worldName));
				foreach (IFile file in world.path.GetFiles(recursive: true))
				{
					WriteFileEntry(file, file.ReadBytes());
				}
				zipStream.Flush();
				zipStream.Close();
				GUIUtility.systemCopyBuffer = Convert.ToBase64String(memoryStream.ToArray());
			}
			finally
			{
				if (zipStream != null)
				{
					((IDisposable)zipStream).Dispose();
				}
			}
			void Write(byte[] data)
			{
				zipStream.Write(data, 0, data.Length);
			}
		};
		void WriteFileEntry(IFile path, byte[] fileData)
		{
			string text = path.Path.Replace(world.path?.ToString() + "/", "");
			DebugPacker._003CSetupLogsExport_World_003Eg__Write_007C1_2(BitConverter.GetBytes(text.Length));
			DebugPacker._003CSetupLogsExport_World_003Eg__Write_007C1_2(Encoding.UTF8.GetBytes(text));
			DebugPacker._003CSetupLogsExport_World_003Eg__Write_007C1_2(BitConverter.GetBytes(fileData.Length));
			DebugPacker._003CSetupLogsExport_World_003Eg__Write_007C1_2(fileData);
		}
	}
}
