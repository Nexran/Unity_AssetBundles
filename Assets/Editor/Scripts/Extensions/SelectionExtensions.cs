using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Extends the Unity Selection sealed class.
/// </summary>
public static class SelectionExtensions 
{
	private static string ErrorMsg = "Please select an object.";

	/// <summary>
	/// Gets all selected object asset paths.
	/// </summary>
	/// <returns>all selected object asset paths, returns null if nothing selected</returns>
	public static string [] GetAllSelectedAssetPaths()
	{
		//	ensure a valid selection
		if(Selection.objects == null || Selection.objects.Length == 0)
		{
			Debug.Log(ErrorMsg);
			return null;
		}

		Object [] selectedObjects = Selection.objects;
		List<string> selectedPaths = new List<string>();

		if(selectedObjects != null && selectedObjects.Length > 0)
		{
			for(int i = 0; i < selectedObjects.Length; ++i)	
			{
				selectedPaths.Add(AssetDatabase.GetAssetPath(selectedObjects[i].GetInstanceID()));
			}
		}

		return selectedPaths.ToArray();
	}

	/// <summary>
	/// Gets all GUID folder paths.
	/// </summary>
	/// <returns>all folder asset paths, returns null if nothing selected</returns>
	public static string [] GetAllSelectedFolderAssetPaths()
	{
		//	ensure a valid selection
		if(Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
		{
			Debug.Log(ErrorMsg);
			return null;
		}

		string [] assetGUIDs = Selection.assetGUIDs;
		List<string> assetPaths = new List<string>();

		if(assetGUIDs != null && assetGUIDs.Length > 0)
		{
			for(int i = 0; i < assetGUIDs.Length; ++i)	
			{
				string path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[i]);

				//	folders don't have periods 
				if(path.Contains(".") == false)
				{
					assetPaths.Add(path);
				}
			}
		}
		return assetPaths.ToArray();
	}
}
