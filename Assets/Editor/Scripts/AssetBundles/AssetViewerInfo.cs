using UnityEngine;
using System.IO;

/// <summary>
/// Asset viewer info, is used to show either a sub directory or a file with additional data.
/// </summary>
public class AssetViewerInfo
{
	public FileSystemInfo FileSystemInfo { get; private set; }
	public string FileSystemName { get; private set; }

	public Rect SelectionRect { get; set; }
	public Rect DrawRect { get; set; }
	public bool IsSelected { get; set; }
	public bool IsSearched { get; set; }

	public AssetViewerInfo(FileSystemInfo info, string fileSystemName)
	{
		FileSystemInfo = info;
		FileSystemName = fileSystemName;
	}
}