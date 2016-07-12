using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class AssetLabelSettings : ScriptableObject
{
	public const string AssetLabelSettingsName = "Asset Label Settings";
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
	private string _defaultVariant;

	[SerializeField]
	private string [] _variants;

	private static AssetLabelSettings _instance;

	/// <summary>
	/// Gets the asset bundle directory.
	/// </summary>
	/// <value>The asset bundle directory.</value>
	public string AssetBundleDirectory
	{
		get { return ProjectPath + _bundleDirectory; }
		private set { _bundleDirectory = value; }
	}

	public string AssetsToBundleDirectory
	{
		get { return _toBundleDirectory; }
		private set { _toBundleDirectory = value; }
	}

	public string DefaultVariant
	{
		get { return _defaultVariant; }
		private set { _defaultVariant = value; }
	}

	public string [] Variants
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

	private Object AssetLabelSettingsObject
	{
		get { return Resources.Load(AssetLabelSettingsName); }
	}

	/// <summary>
	/// Gets the instance of AssetLabelSettings
	/// </summary>
	/// <value>The instance!</value>
	public static AssetLabelSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				//	attempt to load the Resource
				//	Resources.Load will auto poll any folder named Resources in the project hierarchy
				//	this is why the EditorResourcesPath is set to Resources 
				_instance = Resources.Load(AssetLabelSettingsName) as AssetLabelSettings;
				if(_instance == null)
				{
					// If not found, autocreate the asset object.
					_instance = ScriptableObject.CreateInstance<AssetLabelSettings>();

					//	if directory doesn't exist create it
					string properPath = Path.Combine(Application.dataPath, EditorResourcesPath);
					if(!Directory.Exists(properPath)) {	Directory.CreateDirectory(properPath); }

					//	create asset at the directory path
					string fullPath = Path.Combine(Assets + EditorResourcesPath, AssetLabelSettingsName + AssetExtension);
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

	public bool IsVariantFolderName(string folderName)
	{
		bool variant = false;

		//	check the entire variant array to see if the name is in the array
		for(int i = 0; i < _variants.Length; ++i)
		{
			if(folderName == _variants[i])
			{
				variant = true;
				break;
			}
		}

		//	check to see if its the default variant
		if(folderName == _defaultVariant)
		{
			variant = true;
		}

		return variant;
	}
		
	[MenuItem("Custom/Asset Bundles/View Asset Bundle Settings", false, 1)]
	private static void ViewAssetBundleSettings()
	{
		Object resource = Resources.Load(AssetLabelSettingsName);
		if(resource == null) { resource = Instance.AssetLabelSettingsObject; }
		Selection.activeObject = resource;
	}
}