using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity Editor script Folder modification sets up custom icons based on folder locks acts as the view for folder locks. 
/// </summary>
[InitializeOnLoad]
public class FolderModification
{
	private static string _lockIconName = "LockIcon.png";
	private static Texture _customFolderIcon;

	/// <summary>
	/// Initializes the <see cref="FolderModification"/> class.
	/// </summary>
	static FolderModification()
	{
		EditorApplication.projectWindowItemOnGUI += ReplaceFolderIcon;

		//	this searches in the default folder Editor Default Resources for the texture
		_customFolderIcon = EditorGUIUtility.Load(_lockIconName) as Texture;
	}

	/// <summary>
	/// Called based on projectWindowItemOnGUI replaces folder icons based on folder locks.
	/// </summary>
	/// <param name="guid">GUID.</param>
	/// <param name="selectionRect">Selection rect.</param>
	private static void ReplaceFolderIcon(string guid, Rect selectionRect)
	{
		//	convert the GUID to a path
		//	folders in Unity are generally recoginized based on GUIDs
		string path = AssetDatabase.GUIDToAssetPath(guid);

		//	no texture or invalid folder bail!
		if(string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path) || _customFolderIcon == null) 
			return;

		//	check to see if the folder is locked
		if(FolderLockSettings.Instance != null && FolderLockSettings.Instance.IsLockedFolder(path))
		{
			//	create a modified Rect that has the same width as height to make the icon square
			GUI.DrawTexture(new Rect(selectionRect.x, selectionRect.y, selectionRect.height, selectionRect.height), _customFolderIcon);
		}
	}

	/// <summary>
	/// Adds a lock on the selected folders. 
	/// </summary>
	[MenuItem("Assets/Lock/Add")]
	private static void AddLock()
	{
		//	grab all paths
		string [] assetPaths = SelectionExtensions.GetAllSelectedFolderAssetPaths();

		//	cycle and lock all selected folders
		if(assetPaths != null && FolderLockSettings.Instance != null)
		{
			for(int i = 0; i < assetPaths.Length; ++i)
			{
				FolderLockSettings.Instance.LockFolder(assetPaths[i]);
			}
		}
	}

	/// <summary>
	/// Removes locks on selected folders. 
	/// </summary>
	[MenuItem("Assets/Lock/Remove")]
	private static void RemoveLock()
	{
		//	grab all paths
		string [] assetPaths = SelectionExtensions.GetAllSelectedFolderAssetPaths();

		//	cycle and unlock all selected folders
		if(assetPaths != null && FolderLockSettings.Instance != null)
		{
			for(int i = 0; i < assetPaths.Length; ++i)
			{
				FolderLockSettings.Instance.UnlockFolder(assetPaths[i]);
			}
		}
	}

	/// <summary>
	/// Clears all locks on every folder.
	/// </summary>
	[MenuItem("Assets/Lock/Remove All")]
	public static void ClearLocks()
	{
		if(FolderLockSettings.Instance != null)
		{
			FolderLockSettings.Instance.ClearLocks();
		}
	}
}