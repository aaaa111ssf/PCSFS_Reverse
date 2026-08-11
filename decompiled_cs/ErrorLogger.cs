using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ErrorLogger : MonoBehaviour
{
	public static ErrorLogger main;

	public List<string> lastLogs = new List<string>();

	private Queue<(string, string, LogType)> logs = new Queue<(string, string, LogType)>();

	private IFile logLocation;

	private StringBuilder logBuilder = new StringBuilder();

	private void Awake()
	{
		main = this;
		IFolder logsFolder = FileLocations.LogsFolder;
		IFile file = logsFolder.GetFile("counter.txt");
		long num = 0L;
		if (file.Exists())
		{
			try
			{
				num = long.Parse(file.ReadText()) % 5;
			}
			catch
			{
			}
		}
		file.WriteText((num + 1).ToString());
		foreach (IFile file2 in logsFolder.GetFiles())
		{
			if (file2.GetNameWithoutExtension().StartsWith(num + "_"))
			{
				file2.Delete();
			}
		}
		logLocation = logsFolder.GetFile(num + "_" + DateTime.Now.ToString("hh_mm__dd_MMMM_yyyy") + ".txt");
		Application.logMessageReceivedThreaded += LogMessage;
		UniTask.RunOnThreadPool((Action)LogLoop, true, base.destroyCancellationToken);
	}

	public string GetLogsDumpBase64Gzip()
	{
		using MemoryStream memoryStream = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true);
		using StreamWriter streamWriter = new StreamWriter(gZipStream, Encoding.UTF8, 2048, leaveOpen: true);
		foreach (string lastLog in lastLogs)
		{
			streamWriter.WriteLine(lastLog);
		}
		streamWriter.Flush();
		gZipStream.Flush();
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	private async void LogLoop()
	{
		while (!base.destroyCancellationToken.IsCancellationRequested)
		{
			int num = 0;
			logBuilder.Clear();
			while (logs.Count > 0 && num < 20)
			{
				string arg;
				string arg2;
				LogType logType;
				lock (logs)
				{
					(arg, arg2, logType) = logs.Dequeue();
				}
				string text = "[" + DateTime.Now.ToString("hh:mm:ss") + $" {logType}]: {arg}\n{arg2}\n";
				logBuilder.Append(text);
				lastLogs.Add(text);
				if (lastLogs.Count > 1000)
				{
					lastLogs.RemoveAt(0);
				}
				num++;
			}
			int retries = 5;
			if (num > 0)
			{
				while (retries-- > 0)
				{
					try
					{
						logLocation.AppendText(logBuilder.ToString());
					}
					catch
					{
						goto IL_0133;
					}
					break;
					IL_0133:
					await UniTask.Delay(1);
				}
			}
			logBuilder.Clear();
			await UniTask.Delay(10, ignoreTimeScale: false, PlayerLoopTiming.Update, base.destroyCancellationToken);
		}
	}

	private void LogMessage(string condition, string stackTrace, LogType type)
	{
		if (stackTrace.Contains("ErrorLogger"))
		{
			return;
		}
		lock (logs)
		{
			logs.Enqueue((condition, stackTrace, type));
		}
	}
}
