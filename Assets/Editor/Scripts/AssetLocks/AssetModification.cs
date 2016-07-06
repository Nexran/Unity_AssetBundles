using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Asset modification inherits from AssetModificationProcessor and is used to disable folder / file saving, moving, and deleting.
/// </summary>
public class AssetModification : UnityEditor.AssetModificationProcessor
{
	/// <summary>
	/// Unity method called whenever an asset save is attempted. 
	/// </summary>
	/// <param name="paths">Paths to save.</param>
	private static string[] OnWillSaveAssets(string[] paths) 
	{
		List<string> assetsToSave = new List<string>();

		//	Unity will attempt to save all assets that are returned 
		//	we cycle through and ensure no assets are currently locked.
		for(int i = 0; i < paths.Length; ++i)
		{
			if(FolderLocks.Instance != null && !FolderLocks.Instance.IsLockedFolder(paths[i]))
			{
				assetsToSave.Add(paths[i]);
			}
			else
			{
				Debug.LogError("Error prefab not saved due to Read-Only folder at " + paths[i]);
			}
		}
		return assetsToSave.ToArray();
	}

	/// <summary>
	/// Unity method called whenever an asset deletion is attempted. 
	/// </summary>
	/// <param name="path">Path.</param>
	/// <param name="option">RemoveAssetOptions.</param>
	private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions option) 
	{
		AssetDeleteResult assetDeleteResult = AssetDeleteResult.DidNotDelete;
		if(FolderLocks.Instance != null && FolderLocks.Instance.IsLockedFolder(path))
		{
			assetDeleteResult = AssetDeleteResult.FailedDelete;
		}
		return assetDeleteResult;
	}

	/// <summary>
	/// Unity method called whenever an asset move is attempted.
	/// </summary>
	/// <param name="oldPath">Old path.</param>
	/// <param name="newPath">New path.</param>
	private static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath) 
	{
		AssetMoveResult assetMoveResult = AssetMoveResult.DidNotMove;
		if(FolderLocks.Instance != null && FolderLocks.Instance.IsLockedFolder(oldPath) && FolderLocks.Instance.IsLockedFolder(newPath))
		{
			assetMoveResult = AssetMoveResult.FailedMove;
		}
		return assetMoveResult;
	}

	/// <summary>
	/// Unity method determines if as asset is open for edit based on the assetPath and folder lock.
	/// Called whenever as asset is selected.
	/// </summary>
	/// <returns><c>true</c> if is open for edit based on the assetPath; otherwise, <c>false</c>.</returns>
	/// <param name="assetPath">Asset path to check.</param>
	/// <param name="msg">Message THIS IS USELESS.</param>
	private static bool IsOpenForEdit(string assetPath, out string msg)
	{
		//	set this to something so it compiles, this is not used
		msg = string.Empty;

		bool edit = true;
		if(FolderLocks.Instance != null && FolderLocks.Instance.IsLockedFolder(assetPath))
		{
			edit = false;
		}
		return edit;
	}
}