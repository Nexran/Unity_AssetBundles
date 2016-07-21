using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class AssetViewerInfo
{
	public FileSystemInfo FileSystemInfo { get; set; }

	public AssetViewerInfo(FileSystemInfo info)
	{
		FileSystemInfo = info;
	}
}

/// <summary>
/// Asset viewer directory, stores a custom directory along with sub directories, files, and special logic to view in the Asset Viewer Window.
/// </summary>
public class AssetViewerDirectory
{
	/// <summary>
	///	calculate the start index to get the length of the project path
	///	this is used to get where in the local project Asset folder the path is
	/// </summary>
	/// <value>The lenght of project path.</value>
	private static int LenghtOfProjectPath
	{
		get { return Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length; }
	}
	private static char _smallRightArrowUnicode = '\u25B8';
		
	public DirectoryInfo Directory { get; private set; }
	public DirectoryInfo ParentDirectory { get; private set; }
	public DirectoryInfo [] SubDirectories { get; private set; }
	public FileInfo [] Files { get; private set; }

	public List<AssetViewerInfo> AssetInfo { get; set; }
	public List<Rect> FileRects { get; set; }

	public bool IsSelected { get; set; }
	public bool IsExpanded { get; set; }
	public Rect SelectionRect { get; set; }

	public string ExpandedDirectoryName { get; private set; } 
	public string ExpandedParentName { get; private set; }
	public string ProjectPathDisplayName { get; private set; }

	public int IndentLevel { get; private set; } 
	public string ProjectPathFolderLocation { get { return Directory.FullName.Substring(LenghtOfProjectPath); } }

	/// <summary>
	/// Initializes a new instance of the <see cref="AssetViewerDirectory"/> class.
	/// </summary>
	/// <param name="directory">Directory to set as base</param>
	/// <param name="baseIndentCount">Base indent count.</param>
	public AssetViewerDirectory(DirectoryInfo directory, int baseIndentCount)
	{
		Directory = directory;
		ParentDirectory = directory.Parent;
		SubDirectories = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);

		//	ignore all files with .meta or .DS_Store
		Files = directory.GetFiles("*", SearchOption.TopDirectoryOnly).
			Where(o => !o.Name.EndsWith(".meta") && !o.Name.EndsWith(".DS_Store")).ToArray();

		AssetInfo = new List<AssetViewerInfo>();
		for(int i = 0; i < SubDirectories.Length; ++i)
		{
			AssetInfo.Add(new AssetViewerInfo(SubDirectories[i]));
		}
		for(int i = 0; i < Files.Length; ++i)
		{
			AssetInfo.Add(new AssetViewerInfo(Files[i]));
		}

		IsSelected = false;
		IsExpanded = false;

		//	set all path names, remove all ~ 
		ExpandedDirectoryName = directory.FullName.Replace("~", string.Empty);
		ExpandedParentName = directory.Parent.FullName.Replace("~", string.Empty);

		//	for project path turn all / to -> 
		ProjectPathDisplayName = ExpandedDirectoryName.Substring(LenghtOfProjectPath).
			Replace(Path.DirectorySeparatorChar, _smallRightArrowUnicode);

		IndentLevel = Directory.FullName.Split(Path.DirectorySeparatorChar).Length - baseIndentCount;

		//	if they are no tilde subdirectories it means the directory should be expanded by default
		if(SubDirectories != null && !SubDirectories.Any(o => o.FullName.Contains("~")))
		{
			IsExpanded = true;
		}
	}

	/// <summary>
	/// Gets the project path for a file based on the passed in index. 
	/// </summary>
	/// <returns>The project path file location.</returns>
	/// <param name="index">Index.</param>
	public string GetProjectPathFileLocation(int index)
	{
		string projectPathFile = string.Empty;
		if(Files !=  null && Files.Length != 0)
		{
			projectPathFile = Files[index].FullName.Substring(LenghtOfProjectPath);
		}
		return projectPathFile;
	}
}