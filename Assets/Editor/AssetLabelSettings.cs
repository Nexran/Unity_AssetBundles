using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu()]
public class AssetLabelSettings : ScriptableObject
{
	private const string AssetLabelSettingsName = "Asset Label Settings";
	private const string EditorResourcesPath = "Editor/Resources";
	private const string AssetExtension = ".asset";
	private const string Assets = "Assets/";

	[SerializeField]
	[ContextMenuItem("Open Folder Selection", "SetAssetBundleDirectory")]
	[Tooltip("Please right click to Open Folder selection")]
	[Header("Please right click to Open Folder selection")]
	private string _assetBundleDirectory;

	private static AssetLabelSettings _instance;

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
		
	private void SetAssetBundleDirectory()
	{
		_assetBundleDirectory = EditorUtility.OpenFolderPanel("Set Asset Bundle Directory", Application.dataPath, string.Empty);
	}
}