using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// Asset viewer info, is used to show either a sub directory or a file with additional data.
/// </summary>
public class AssetViewerInfo
{
	public enum ClickType
	{
		NONE,
		CLICK,
		DOUBLE_CLICK,
		EXPAND
	}

	public FileSystemInfo FileSystemInfo { get; private set; }
	public string FileSystemName { get; private set; }
	public string FolderName { get; private set; }
	public bool CanExplore { get; private set; }

	public List<AssetViewerInfo> Dependencies { get; set; }
	public List<string> MissingComponents { get; set; }

	public Rect SelectionRect { get; set; }
	public Rect DrawRect { get; set; }
	public bool IsSelected { get; set; }
	public bool IsSearched { get; set; }
	public bool ShowDependencies { get; private set; }

	public AssetViewerInfo(FileSystemInfo info, string fileSystemName)
	{
		Dependencies = new List<AssetViewerInfo>();
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

	public AssetViewerInfo.ClickType Click(Vector2 mousePosition)
	{
		AssetViewerInfo.ClickType clickType = ClickType.NONE;

		if(SelectionRect.Contains(mousePosition))
		{
			//	if it has we set it as selected and update the Selection.activeobject so its visible in the inspector
			Object obj = AssetDatabase.LoadAssetAtPath(FileSystemInfo.FullName.RemoveProjectPath(), typeof(Object));
			if(obj != null) { Selection.activeObject = obj; }

			//	 a user double clicks on the folder 
			//	select the folder and expand the folder to mimic project folder hierarchy
			if(Event.current.type == EventType.MouseDown && Event.current.clickCount == 2)
			{
				//	if there is no file extension which means its a folder expand it! 
				if(CanExplore)
				{
					IsSelected = false;
					clickType = ClickType.EXPAND;
				}
				else
				{
					clickType = ClickType.DOUBLE_CLICK;

					IsSelected = true;
					if(obj != null) { AssetDatabase.OpenAsset(obj); }
				}
			}
			else
			{
				clickType = ClickType.CLICK;
				 
				IsSelected = true;
			}
		}
		return clickType;
	}

	public void Render(Texture selectedTexture, Texture tildeFolderTexture, Texture warningTexture, float currentViewWidth, bool fullName, GUIStyle guiStyle)
	{
		//	if its selected also draw the selection texture 
		if(IsSelected && selectedTexture != null)
		{
			GUI.DrawTexture(DrawRect, selectedTexture);
		}

		EditorGUILayout.BeginHorizontal();

		if((Dependencies != null && Dependencies.Count > 0) || (MissingComponents != null && MissingComponents.Count > 0))
		{
			ShowDependencies = GUILayout.Toggle(ShowDependencies, string.Empty, EditorStyles.foldout, GUILayout.Width(10));
		}
						
		string toShow = string.Empty;
		if(fullName)
		{
			toShow = string.Format("     {0}", FileSystemInfo.FullName.RemoveProjectPath());
		}
		else
		{
			toShow = string.Format("     {0}", FileSystemInfo.Name);
		}

		GUILayout.Label(toShow, guiStyle);
		Rect lastRect = GUILayoutUtility.GetLastRect();
		Texture tex = AssetDatabase.GetCachedIcon(FileSystemInfo.FullName.RemoveProjectPath());
		if(tex != null) 
		{
			GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, 16, 16), tex);
		}
		else if(tildeFolderTexture != null)
		{
			GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, 16, 16), tildeFolderTexture);
		}

		//	if there are missing components alert the user
		if(MissingComponents != null && MissingComponents.Count != 0 && warningTexture != null)
		{
			GUI.DrawTexture(new Rect(lastRect.width - 16, lastRect.y, 16, 16), warningTexture);
		}

		//	for X reason the rect Width will sometimes give us default values of 0, 0, 1, 1
		//	we check to make sure its a valid size, anything larger than default 
		if(lastRect.width > 1f)
		{
			SelectionRect = new Rect(lastRect.x + currentViewWidth, lastRect.y, lastRect.width, lastRect.height);
			DrawRect = new Rect(5, lastRect.y, lastRect.width, lastRect.height);
		}

		EditorGUILayout.EndHorizontal();

		if(ShowDependencies)
		{
			for(int a = 0; a < Dependencies.Count; ++a)
			{
				Dependencies[a].Render(selectedTexture, tildeFolderTexture, warningTexture, currentViewWidth, true, EditorStyles.miniLabel);
			}

			for(int a = 0; a < MissingComponents.Count; ++a)
			{
				GUILayout.Label(MissingComponents[a], EditorStyles.miniBoldLabel);
			}
		}
	}
}