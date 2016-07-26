using UnityEngine;
using System.IO;

/// <summary>
/// Asset viewer info, is used to show either a sub directory or a file with additional data.
/// </summary>
public class AssetViewerInfo
{
	public FileSystemInfo FileSystemInfo { get; private set; }
	public string FileSystemName { get; private set; }
	public string FolderName { get; private set; }
	public bool CanExplore { get; private set; }

	public Rect SelectionRect { get; set; }
	public Rect DrawRect { get; set; }
	public bool IsSelected { get; set; }
	public bool IsSearched { get; set; }

	public AssetViewerInfo(FileSystemInfo info, string fileSystemName)
	{
		FileSystemInfo = info;
		FileSystemName = fileSystemName;
		CanExplore = false;

		if(string.IsNullOrEmpty(info.Extension))
		{
			FolderName = FileSystemName;
			CanExplore = true;
		}
		else
		{
			if(info.FullName.Contains("~"))
				CanExplore = true;
			
			string [] splitPath = FileSystemName.Split(Path.DirectorySeparatorChar);
			//	we make sure the for loop will never add in the last part of the array the object this.jpg
			for(int j = 0; j < splitPath.Length - 1; ++j)
			{
				//	add back the slashes, we skip adding the slash on the first pass
				if(j != 0) FolderName += Path.DirectorySeparatorChar;

				FolderName += splitPath[j];
			}
		}
	}
}