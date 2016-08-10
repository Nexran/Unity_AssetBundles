using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Asset viewer directory, stores a custom directory along with sub directories, files, and special logic to view in the Asset Viewer Window.
/// </summary>
public class AssetViewerDirectory
{
	private static char _smallRightArrowUnicode = '\u25B8';
		
	public DirectoryInfo Directory { get; private set; }
	public DirectoryInfo ParentDirectory { get; private set; }

	public List<AssetViewerInfo> AssetInfo { get; private set; }
	public List<Rect> FileRects { get; set; }

	public List<string> DependencyDirectories { get; private set; }
	public AssetViewerManifest AssetViewerManifest { get; private set; }

	public bool ContainsSubDirectories { get; private set; }
	public bool IsSelected { get; set; }
	public bool IsExpanded { get; set; }
	public Rect SelectionRect { get; set; }
	public bool IsSearched { get; set; }
	public bool ShowAssetBundleDependencies { get; set; }

	public string ExpandedDirectoryName { get; private set; } 
	public string ExpandedParentName { get; private set; }
	public string ProjectPathDisplayName { get; private set; }

	public int IndentLevel { get; private set; } 
	public string ProjectPathFolderLocation { get { return Directory.FullName.RemoveProjectPath(); } }

	/// <summary>
	/// Initializes a new instance of the <see cref="AssetViewerDirectory"/> class.
	/// </summary>
	/// <param name="directory">Directory to set as base</param>
	/// <param name="baseIndentCount">Base indent count.</param>
	public AssetViewerDirectory(string directoryPath)
	{ 
		IsSearched = false;
		IsSelected = false;
		IsExpanded = false;
		ContainsSubDirectories = false;
		ShowAssetBundleDependencies = false;

		if(!string.IsNullOrEmpty(directoryPath))
		{
			Directory = new DirectoryInfo(directoryPath);
			if(!Directory.Exists) { throw new DirectoryNotFoundException(); }
		}
		else
		{
			throw new DirectoryNotFoundException();
		}

		ParentDirectory = Directory.Parent;

		//	set all path names, remove all ~ 
		ExpandedDirectoryName = Directory.FullName.Replace("~", string.Empty);
		ExpandedParentName = Directory.Parent.FullName.Replace("~", string.Empty);

		//	for project path turn all / to -> 
		ProjectPathDisplayName = ExpandedDirectoryName.RemoveProjectPath().Replace(Path.DirectorySeparatorChar, _smallRightArrowUnicode);

		IndentLevel = Directory.FullName.Split(Path.DirectorySeparatorChar).Length;
	}
		
	internal void Init()
	{
		InitAssetInfo();
		InitAssetManifest();
		InitDependencyDirectories();
	}

	internal void InitAssetInfo()
	{
		AssetInfo = new List<AssetViewerInfo>();

		DirectoryInfo [] subDirectories = Directory.GetDirectories("*", SearchOption.TopDirectoryOnly);
		for(int i = 0; i < subDirectories.Length; ++i)
		{
			if(subDirectories[i].FullName.Contains("~") && !Directory.Parent.FullName.Contains("~"))
			{
				IsExpanded = true;
			}

			ContainsSubDirectories = true;
			AssetInfo.Add(new AssetViewerInfo(subDirectories[i], subDirectories[i].FullName.Replace("~", string.Empty)));
		}

		//	ignore all files with .meta or .DS_Store
		FileInfo [] files = Directory.GetFiles("*", SearchOption.TopDirectoryOnly).
			Where(o => !o.Name.EndsWith(".meta") && !o.Name.EndsWith(".DS_Store")).ToArray();

		for(int i = 0; i < files.Length; ++i)
		{
			AssetInfo.Add(new AssetViewerInfo(files[i], files[i].FullName.Replace("~", string.Empty)));
		}

		//	set up all the asset info to detect missing components
		for(int i = 0; i < AssetInfo.Count; ++i)
		{			
			AssetInfo[i].MissingComponents = new List<string>();

			Object obj = AssetDatabase.LoadAssetAtPath(AssetInfo[i].FileSystemInfo.FullName.RemoveProjectPath(), typeof(Object));
			GameObject gObj = obj as GameObject;

			if(gObj != null)
			{
				Component [] components = gObj.GetComponents(typeof(Component));
				for(int a = 0; a < components.Length; ++a)
				{
					SerializedObject s = new SerializedObject(components[a]);
					SerializedProperty o = s.GetIterator();
					while(o.NextVisible(true))
					{
						if(o.propertyType == SerializedPropertyType.ObjectReference)
						{
							if(o.objectReferenceValue == null && o.objectReferenceInstanceIDValue != 0)
							{
								AssetInfo[i].MissingComponents.Add("MISSING " + o.displayName);
							}
						}
					}
				}
			}

			string [] dependencies = AssetDatabase.GetDependencies(AssetInfo[i].FileSystemInfo.FullName.RemoveProjectPath(), false);

			for(int j = 0; j < dependencies.Length; ++j)
			{
				FileInfo file = new FileInfo(dependencies[j]);
				AssetInfo[i].Dependencies.Add(new AssetViewerInfo(file, file.FullName.Replace("~", string.Empty)));
			}
		}
	}

	internal void InitAssetManifest()
	{
		//	set up some offsets to find manifest
		int assetToBundleOffset = AssetBundleSettings.Instance.AssetsToBundleDirectory.Length + StringExtensions.LengthOfProjectPath +  1;
		int assetBundleOffset = AssetBundleSettings.Instance.AssetBundleDirectory.Length + 1;
		string manifestDir = AssetBundleSettings.Instance.AssetBundleDirectory + "/" + 
			ExpandedDirectoryName.Substring(assetToBundleOffset).ToLower() + 
			".manifest";

		//	if the manifest exists parse it!
		if(System.IO.File.Exists(manifestDir))
		{
			AssetViewerManifest = new AssetViewerManifest(manifestDir, 
				AssetBundleSettings.Instance.AssetsToBundleDirectory, 
				StringExtensions.LengthOfProjectPath, 
				assetBundleOffset);
		}
		else
		{
			throw new FileNotFoundException();
		}
	}

	internal void InitDependencyDirectories()
	{
		//	set up all dependency directories
		DependencyDirectories = new List<string>();
		int countToAdd = Application.dataPath.Split(Path.DirectorySeparatorChar).Length;
		string toAdd = string.Empty;
		string [] splitPath = ExpandedDirectoryName.Split(Path.DirectorySeparatorChar);
		//	we make sure the for loop will never add in the last part of the array the object this.jpg
		for(int j = 0; j < splitPath.Length - 1; ++j)
		{
			//	add back the slashes, we skip adding the slash on the first pass
			if(j != 0) toAdd += Path.DirectorySeparatorChar;

			toAdd += splitPath[j];

			if(j >= countToAdd)
			{
				DependencyDirectories.Add(toAdd.RemoveProjectPath());
			}
		}
	}

	public void Clone(AssetViewerDirectory directory)
	{
		this.IsSelected = directory.IsSelected;
		this.IsExpanded = directory.IsExpanded;
		this.IsSearched = directory.IsSearched;
		this.ShowAssetBundleDependencies = directory.ShowAssetBundleDependencies;

		if(this.AssetInfo.Count == directory.AssetInfo.Count)
		{
			for(int i = 0; i < this.AssetInfo.Count; ++i)
			{
				this.AssetInfo[i].IsSelected = directory.AssetInfo[i].IsSelected;
				this.AssetInfo[i].IsSearched = directory.AssetInfo[i].IsSearched;
			}
		}
	}

	public AssetViewerInfo.ClickType CheckMousePress(Vector2 mousePosition, out string folderName)
	{
		AssetViewerInfo.ClickType press = AssetViewerInfo.ClickType.NONE;
		folderName = string.Empty;

		if(IsSelected == false && IsSearched == false)
			return press;

		for(int j = 0; j < AssetInfo.Count; ++j)
		{				
			press = AssetInfo[j].Click(mousePosition, out folderName);

			if(press == AssetViewerInfo.ClickType.EXPAND)
			{
				IsExpanded = true;
			}

			if(press != AssetViewerInfo.ClickType.NONE)
				break;
		}
			
		return press;
	}
}