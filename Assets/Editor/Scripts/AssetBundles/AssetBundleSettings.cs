using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Asset bundle settings, contains all logic to determine where and what will be bundled into asset bundles.
/// </summary>
[CreateAssetMenu()]
public class AssetBundleSettings : ScriptableObject
{
	private const string AssetBundleSettingsName = "Asset Bundle Settings";
	private const string EditorResourcesPath = "Editor/Resources";
	private const string AssetExtension = ".asset";
	private const string Assets = "Assets/";

	[Header("Please right click to Open Folder selection")]
	[Header("Note directory must be local to project")]

	[SerializeField]
	[ContextMenuItem("Open Folder Selection", "SetAssetBundleDirectory")]
	[Tooltip("Please right click to Open Folder selection\nNote directory must be local to project")]
	private string _bundleDirectory;

	[SerializeField]
	[ContextMenuItem("Open Folder Selection", "SetAssetsToBundleDirectory")]
	[Tooltip("Please right click to Open Folder selection\nNote directory must be local to project")]
	private string _toBundleDirectory;

	[SerializeField] 
	private string _defaultVariantFolder;

	[SerializeField]
	public Variant [] _variants;

	private static AssetBundleSettings _instance;

	/// <summary>
	/// Gets the asset bundle directory.
	/// </summary>
	/// <value>The asset bundle directory.</value>
	public string AssetBundleDirectory
	{
		get { return ProjectPath + _bundleDirectory; }
		private set { _bundleDirectory = value; }
	}

	/// <summary>
	/// Gets the assets TO bundle directory.
	/// </summary>
	/// <value>The assets TO bundle directory.</value>
	public string AssetsToBundleDirectory
	{
		get { return _toBundleDirectory; }
		private set { _toBundleDirectory = value; }
	}

	/// <summary>
	/// Gets the default variant folder name
	/// </summary>
	/// <value>The default variant.</value>
	public string DefaultVariantFolder
	{
		get { return _defaultVariantFolder; }
		private set { _defaultVariantFolder = value; }
	}

	/// <summary>
	/// Gets an array of all variants, these are used to set unique variant data.
	/// </summary>
	/// <value>The variants.</value>
	public Variant [] Variants
	{
		get { return _variants; }
		private set { _variants = value; }
	}

	/// <summary>
	/// Gets the project path with the last 6 characters /Assets removed
	/// </summary>
	/// <value>The project path with the last 6 characters /Assets removed /Users/me/PROJECT_NAME/</value>
	private string ProjectPath 
	{
		get { return Application.dataPath.Remove(Application.dataPath.Length - 6, 6); }
	}

	/// <summary>
	/// Gets the asset label settings object Resource.
	/// </summary>
	/// <value>The asset label settings object.</value>
	private Object AssetLabelSettingsObject
	{
		get { return Resources.Load(AssetBundleSettingsName); }
	}

	/// <summary>
	/// Gets the instance of AssetLabelSettings
	/// </summary>
	/// <value>The instance!</value>
	public static AssetBundleSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				//	attempt to load the Resource
				//	Resources.Load will auto poll any folder named Resources in the project hierarchy
				//	this is why the EditorResourcesPath is set to Resources 
				_instance = Resources.Load(AssetBundleSettingsName) as AssetBundleSettings;
				if(_instance == null)
				{
					// If not found, autocreate the asset object.
					_instance = ScriptableObject.CreateInstance<AssetBundleSettings>();

					//	if directory doesn't exist create it
					string properPath = Path.Combine(Application.dataPath, EditorResourcesPath);
					if(!Directory.Exists(properPath)) {	Directory.CreateDirectory(properPath); }

					//	create asset at the directory path
					string fullPath = Path.Combine(Assets + EditorResourcesPath, AssetBundleSettingsName + AssetExtension);
					AssetDatabase.CreateAsset(_instance, fullPath);
				}
			}
			return _instance;
		}
	}
		
	/// <summary>
	/// Sets the asset bundle directory.
	/// </summary>
	private void SetAssetBundleDirectory()
	{
		string selectedFolder = EditorUtility.OpenFolderPanel("Set Asset Bundle Directory", Application.dataPath, string.Empty);
		int startIndex = ProjectPath.Length;
		if(startIndex < selectedFolder.Length)
		{
			_bundleDirectory = selectedFolder.Substring(startIndex);
			Debug.Log("Asset Bundle Directory set as " + selectedFolder + "\n");
		}
		else
		{
			_bundleDirectory = string.Empty;
			Debug.LogError("Please select a folder local to the project.\n");
		}
	}

	/// <summary>
	/// Sets the assets to bundle directory.
	/// </summary>
	private void SetAssetsToBundleDirectory()
	{
		string selectedFolder = EditorUtility.OpenFolderPanel("Set Assets To Bundle Directory", Application.dataPath, string.Empty);
		int startIndex = ProjectPath.Length;
		if(startIndex < selectedFolder.Length)
		{
			_toBundleDirectory = selectedFolder.Substring(startIndex);
			Debug.Log("Assets To Bundle Directory set as " + selectedFolder + "\n");
		}
		else
		{
			_toBundleDirectory = string.Empty;
			Debug.LogError("Please select a folder local to the project.\n");
		}
	}

	/// <summary>
	/// Shows the Asset Bundle Settings in the inspector.
	/// </summary>
	[MenuItem("Custom/Asset Bundles/View Asset Bundle Settings", false, 1)]
	private static void ViewAssetBundleSettings()
	{
		Object resource = Resources.Load(AssetBundleSettingsName);
		if(resource == null) { resource = Instance.AssetLabelSettingsObject; }
		Selection.activeObject = resource;
	}

	/// <summary>
	/// Determines whether the passed in folder name is a variant folder or not.
	/// </summary>
	/// <returns><c>true</c> if this instance is variant folder name the specified folderName; otherwise, <c>false</c>.</returns>
	/// <param name="folderName">Folder name.</param>
	public bool IsVariantFolderName(string folderName)
	{
		bool variant = false;

		//	check to see if its the default variant
		if(folderName == _defaultVariantFolder)
		{
			variant = true;
		}
		else
		{
			//	check the entire variant array to see if the name is in the array
			for(int i = 0; i < _variants.Length; ++i)
			{
				if(folderName == _variants[i].Name)
				{
					variant = true;
					break;
				}
			}
		}

		return variant;
	}

	/// <summary>
	/// Gets all variant directories, based on the To Bundle Directory
	/// </summary>
	/// <returns>all variant directories names</returns>
	public List<string> GetAllVariantDirectories()
	{
		DirectoryInfo directory = new DirectoryInfo(_toBundleDirectory);
		DirectoryInfo [] subDirectories = directory.GetDirectories("*", SearchOption.AllDirectories);

		List<string> variantDirectories = new List<string>();
		for(int i = 0; i < subDirectories.Length; ++i)
		{
			if(IsVariantFolderName(subDirectories[i].Name))
			{
				variantDirectories.Add(subDirectories[i].FullName);
			}
		}

		return variantDirectories;
	}
}