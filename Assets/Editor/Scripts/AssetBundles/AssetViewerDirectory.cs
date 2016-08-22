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
	private static float _folderXOffset = 17f;

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

		//	calculate out indent levels 
		int indentRemoveLength = AssetBundleSettings.Instance.AssetsToBundleDirectory.Length + StringExtensions.LengthOfProjectPath;
		string indentString = Directory.FullName.Remove(0, indentRemoveLength + 1);
		IndentLevel = indentString.Split(Path.DirectorySeparatorChar).Length - 1;
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

	internal void CloneFlags(AssetViewerDirectory directory)
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

	internal void Render(string expandedDirectoryName, Texture selectedTexture, Texture folderTexture, Texture fileTexture)
	{
		if(ExpandedParentName == expandedDirectoryName)
		{
			GUILayout.BeginHorizontal();

			//	if its selected also draw the selection texture
			//	draw this first so the label/ foldout / image are rendered over
			if(IsSelected && selectedTexture != null)
			{
				GUI.DrawTexture(SelectionRect, selectedTexture);
			}

			//	show a foldout if the directory has sub directories
			//	we add in extra spaces for the folder to be placed inbetween
			if(ContainsSubDirectories)
			{
				IsExpanded = EditorGUILayout.Foldout(IsExpanded, string.Format("      {0}", Directory.Name));
			}
			else
			{
				EditorGUILayout.LabelField(string.Format("         {0}", Directory.Name));
			}

			Rect lastRect = GUILayoutUtility.GetLastRect();

			//	show a file icon to indicate that there is a manifest with this folder
			if(AssetViewerManifest != null && fileTexture != null)
			{
				GUI.DrawTexture(new Rect(lastRect.width - 16, lastRect.y, 16, 16), fileTexture);
			}

			//	for X reason the rect Width will sometimes give us default values of 0, 0, 1, 1
			//	we check to make sure its a valid size, anything larger than default 
			if(lastRect.width > 1f)
			{
				SelectionRect = new Rect(0, lastRect.y, lastRect.width + lastRect.x, lastRect.height);
			}

			//	render the folder texture
			Rect folderRect = EditorGUI.IndentedRect(lastRect);
			float folderOffset = (folderRect.x - lastRect.x) + _folderXOffset;
			Texture tex = AssetDatabase.GetCachedIcon(Directory.FullName.RemoveProjectPath());
			if(tex != null) 
			{
				GUI.DrawTexture(new Rect(folderOffset, lastRect.y, 16, 16), tex);
			}
			else if(folderTexture != null)
			{
				GUI.DrawTexture(new Rect(folderOffset, lastRect.y, 16, 16), folderTexture);
			}

			GUILayout.EndHorizontal();
		}
	}

	internal AssetViewerInfo.ClickType CheckMousePress(EventType eventType, int clickCount, Vector2 mousePosition, out string folderName)
	{
		AssetViewerInfo.ClickType press = AssetViewerInfo.ClickType.NONE;
		folderName = string.Empty;

		if(IsSelected == false && IsSearched == false)
			return press;

		for(int j = 0; j < AssetInfo.Count; ++j)
		{				
			press = AssetInfo[j].Click(eventType, clickCount, mousePosition, out folderName);

			if(press == AssetViewerInfo.ClickType.EXPAND)
			{
				IsExpanded = true;
			}

			if(press != AssetViewerInfo.ClickType.NONE)
				break;
		}
			
		return press;
	}

	internal void OpenDirectory()
	{
		//	found one to open!
		if(Directory.FullName.Contains("~"))
		{
			string newPath = Directory.FullName.Replace("~", string.Empty);

			bool exists = System.IO.Directory.Exists(newPath);

			if(exists == false && Directory.Exists)
			{
				Directory.MoveTo(newPath);
			}
		}
	}

	internal bool CloseDirectory()
	{
		bool directoryClosed = false;

		//	try to destroy and meta files that exist
		//	meta files are used to show folders in Unity the tilde property ensures meta files are NOT created
		//	if we did not destroy all meta files here if you close a folder at the top of a directory all the meta files
		//	will not get destroyed which will cause errors to pop up in the editor
		FileInfo metaFile = new FileInfo(Directory.FullName + ".meta");
		if(metaFile != null && metaFile.Exists == true)
		{
			metaFile.Delete();
		}

		//	if it already ends with a tilde don't add another!
		if(Directory.FullName.EndsWith("~") == false)
		{
			string newPath = Directory.FullName + "~";
			bool exists = System.IO.Directory.Exists(newPath);

			if(exists == false)
			{
				Directory.MoveTo(newPath);
				directoryClosed = true;
			}
		}
		IsExpanded = false;

		return directoryClosed;
	}
}