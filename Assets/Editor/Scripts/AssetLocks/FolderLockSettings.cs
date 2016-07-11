using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Folder Locks Settings is used to determine which folders are locked and should not be modified acts as the data for folder locks.
/// </summary>
[CreateAssetMenu()]
public class FolderLockSettings : ScriptableObject
{
	private const string FolderLockSettingsName = "Folder Lock Settings";
	private const string EditorResourcesPath = "Editor/Resources";
	private const string AssetExtension = ".asset";
	private const string Assets = "Assets/";

	[SerializeField]
	[DelayedAttribute()]
	private List<string> _lockedFolders;

	private static FolderLockSettings _instance;

	/// <summary>
	/// Gets all folders that have locks on them.
	/// </summary>
	/// <value>The locked folders.</value>
	public List<string> LockedFolders 
	{ 
		get { return _lockedFolders; } 
		private set { _lockedFolders = value; }
	}

	/// <summary>
	/// Gets the instance of Folder Locks. 
	/// </summary>
	/// <value>The instance!</value>
	public static FolderLockSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				//	attempt to load the Resource
				//	Resources.Load will auto poll any folder named Resources in the project hierarchy
				//	this is why the EditorResourcesPath is set to Resources 
				_instance = Resources.Load(FolderLockSettingsName) as FolderLockSettings;
				if(_instance == null)
				{
					// If not found, autocreate the asset object.
					_instance = ScriptableObject.CreateInstance<FolderLockSettings>();

					//	if directory doesn't exist create it
					string properPath = Path.Combine(Application.dataPath, EditorResourcesPath);
					if(!Directory.Exists(properPath)) {	Directory.CreateDirectory(properPath); }

					//	create asset at the directory path
					string fullPath = Path.Combine(Assets + EditorResourcesPath, FolderLockSettingsName + AssetExtension);
					AssetDatabase.CreateAsset(_instance, fullPath);
				}
			}
			return _instance;
		}
	}

	/// <summary>
	/// Determines whether the passed in asset is locked based on if its path is contained in any of the locked folders paths.
	/// </summary>
	/// <returns><c>true</c> if this instance is asset locked based on specified assetPath; otherwise, <c>false</c>.</returns>
	/// <param name="assetPath">Asset path to check for every folder</param>
	public bool IsLockedFolder(string assetPath)
	{
		bool isLocked = false;
		//	validation and make sure you never lock the file for the locks!
		//	cycle through all locked folders and see if they contain any of the passed in asset path
		if(_lockedFolders != null && !assetPath.Contains(FolderLockSettingsName))
		{
			for(int i = 0; i < _lockedFolders.Count; ++i)
			{
				if(assetPath.Contains(_lockedFolders[i]))
				{
					isLocked = true;
					break;
				}
			}
		}
		return isLocked;
	}

	/// <summary>
	/// Locks the folder at the passed in path.
	/// </summary>
	/// <returns><c>true</c>, if folder was locked, <c>false</c> otherwise.</returns>
	/// <param name="path">Path to lock.</param>
	public bool LockFolder(string path)
	{
		bool locked = false;

		//	make sure its valid and not already locked
		if(_lockedFolders != null && _lockedFolders.Contains(path) == false)
		{
			_lockedFolders.Add(path);
			locked = true;
		}
		return locked;
	}

	/// <summary>
	/// Unlocks the folder as the passed in path.
	/// </summary>
	/// <returns><c>true</c>, if folder was unlocked, <c>false</c> otherwise.</returns>
	/// <param name="path">Path to unlock.</param>
	public bool UnlockFolder(string path)
	{
		bool unlocked = false;
		//	make sure its valid and already locked.
		if(_lockedFolders != null && _lockedFolders.Contains(path))
		{
			_lockedFolders.Remove(path);
			unlocked = true;
		}
		return unlocked;
	}

	/// <summary>
	/// Clears all locks.
	/// </summary>
	public void ClearLocks()
	{
		if(_lockedFolders != null)
		{
			_lockedFolders.Clear();
		}
	}
}