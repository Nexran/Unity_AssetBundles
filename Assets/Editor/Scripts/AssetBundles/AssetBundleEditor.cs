using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Asset bundle editor, contains all logic needed to create asset bundles with variants / labels.
/// </summary>
public class AssetBundleEditor 
{		
	/// <summary>
	///	calculate the start index to get the length of the project path
	///	this is used to get where in the local project Asset folder the path is
	/// </summary>
	/// <value>The lenght of project path.</value>
	private static int LenghtOfProjectPath
	{
		get { return Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length; }
	}

	/// <summary>
	/// Creates all asset variants.
	/// </summary>
	[MenuItem("Custom/Asset Bundles/1) Create All Asset Variants", false, 1)]
	private static void CreateAllAssetVariants()
	{
		if(AssetBundleSettings.Instance == null)
			return;

		List<string> variantDirectories = AssetBundleSettings.Instance.GetAllVariantDirectories();

		//	loop through all variants directories to see if assets need to be created or
		//	if directories need to be created
		for(int i = 0; i < variantDirectories.Count; ++i)
		{
			//	exit out quickly if the directory is the Default Variant directory we will be
			//	cloning all assets from that directory to create the other variant directories
			if(!variantDirectories[i].Contains(AssetBundleSettings.Instance.DefaultVariantFolder))
				continue;

			string directoryNameMinusVariant = variantDirectories[i].Substring(0, 
				variantDirectories[i].Length - AssetBundleSettings.Instance.DefaultVariantFolder.Length);

			//	create all variant directories based on the variants defined in the labels
			//	also cycle through and ensure all assets are copied into each directory
			for(int j = 0; j < AssetBundleSettings.Instance.Variants.Length; ++j)
			{
				if(!AssetBundleSettings.Instance.Variants[j].IsValid)
					continue;
				
				string directoryNameVariant = directoryNameMinusVariant + AssetBundleSettings.Instance.Variants[j].Name;

				//	create the variant directory if it doesn't exist
				if(Directory.Exists(directoryNameVariant) == false)
				{
					Directory.CreateDirectory(directoryNameVariant);

					//	refresh the database after every directory gets created to make sure its there when we 
					//	attempt to validate the move
					AssetDatabase.Refresh();
				}	

				//	now that directory is created copy assets from old directory to new directory
				CopyAssets(variantDirectories[i], directoryNameVariant, AssetBundleSettings.Instance.Variants[j]);
			}
		}

		//	refresh the database now that all imports and moves are all done to make
		//	sure the project view is updated
		AssetDatabase.Refresh();
		Debug.Log("Asset Variants Created!\n");
	}

	/// <summary>
	/// Sets the asset bundle labels.
	/// </summary>
	[MenuItem("Custom/Asset Bundles/2) Set Asset Bundle Labels", false, 2)]
	private static void SetAssetBundleLabels()
	{
		if(AssetBundleSettings.Instance == null)
			return;

		//	get all Files that are under the main Asset TO Bundle directory 
		DirectoryInfo directory = new DirectoryInfo(AssetBundleSettings.Instance.AssetsToBundleDirectory);
		FileInfo [] files = directory.GetFiles("*", SearchOption.AllDirectories);

		List<string> assetPaths = new List<string>();
		for(int i = 0; i < files.Length; ++i)
		{
			if(IgnoreFile(files[i].Name))
				continue;

			assetPaths.Add(files[i].FullName.Substring(LenghtOfProjectPath));
		}

		//	loop through every asset and make sure that the bundle name and variant are set
		for(int i = 0; i < assetPaths.Count; ++i)
		{
			string assetBundleName = string.Empty;
			string assetBundleVariant = string.Empty;

			GetAssetBundleNameFromPath(assetPaths[i], out assetBundleName, out assetBundleVariant);

			AssetImporter assetImporter = AssetImporter.GetAtPath(assetPaths[i]);
			assetImporter.assetBundleName = assetBundleName;
			assetImporter.assetBundleVariant = assetBundleVariant;
			assetImporter.SaveAndReimport();
		}
		Debug.Log("Asset Bundle Labels Set!\n");
	}

	/// <summary>
	/// Builds all asset bundles.
	/// </summary>
	[MenuItem("Custom/Asset Bundles/3) Build Asset Bundles", false, 3)]
	private static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = string.Empty;
		if(AssetBundleSettings.Instance != null)
		{
			assetBundleDirectory = AssetBundleSettings.Instance.AssetBundleDirectory;
		}

		if(!string.IsNullOrEmpty(assetBundleDirectory))
		{
			//	we create the directory if we doesn't exist
			if(Directory.Exists(assetBundleDirectory) == false)
			{
				Directory.CreateDirectory(assetBundleDirectory);
			}	

			//	build build!!
			BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
		}
		Debug.Log("Asset Bundles Built!\n");
	}

	/// <summary>
	/// Deletes all asset variants.
	/// </summary>
	[MenuItem("Custom/Asset Bundles/4) Delete All Asset Variants", false, 4)]
	private static void DeleteAllAssetVariants()
	{
		if(AssetBundleSettings.Instance == null)
			return;

		List<string> variantDirectories = AssetBundleSettings.Instance.GetAllVariantDirectories();

		for(int i = 0; i < variantDirectories.Count; ++i)
		{
			if(variantDirectories[i].Contains(AssetBundleSettings.Instance.DefaultVariantFolder))
				continue;

			string directoryNameMinusVariant = variantDirectories[i].Substring(0, 
				variantDirectories[i].Length - AssetBundleSettings.Instance.DefaultVariantFolder.Length);
			
			//	create all variant directories
			for(int j = 0; j < AssetBundleSettings.Instance.Variants.Length; ++j)
			{
				if(!AssetBundleSettings.Instance.Variants[j].IsValid)
					continue;
				
				string directoryNameVariant = directoryNameMinusVariant + AssetBundleSettings.Instance.Variants[j].Name;
			
				if(Directory.Exists(directoryNameVariant) == true)
				{
					Directory.Delete(directoryNameVariant, true);
				}	
			}
		}

		AssetDatabase.Refresh();
		Debug.Log("Asset Variants Deleted!\n");
	}
		
	/// <summary>
	/// Runs all asset bundle logic.
	/// </summary>
	[MenuItem("Custom/Asset Bundles/RUN ALL", false, 5)]
	private static void RunAllAssetBundleLogic()
	{
		CreateAllAssetVariants();
		SetAssetBundleLabels();
		BuildAllAssetBundles();
		DeleteAllAssetVariants();
	}

	/// <summary>
	/// Gets the asset bundle name from path.
	/// </summary>
	/// <param name="assetPath">Asset path to detect name from.</param>
	/// <param name="assetBundleName">Asset bundle name.</param>
	/// <param name="assetBundleVariant">Asset bundle variant.</param>
	private static void GetAssetBundleNameFromPath(string assetPath, out string assetBundleName, out string assetBundleVariant)
	{	
		assetBundleName = string.Empty;
		assetBundleVariant = string.Empty;

		//	remove out the first part of the asset path
		//	before: Assets/AssetsToBundle/Globals/Prefabs/Characters/this.jpg
		//	after: Globals/Prefabs/Characters/this.jpg
		string path = assetPath.Substring(AssetBundleSettings.Instance.AssetsToBundleDirectory.Length + 1);

		//	split out the remaining string Globals/Prefabs/Characters/this.jpg to an array so we can truncate out the object this.jpg
		string [] splitPath = path.Split(Path.AltDirectorySeparatorChar);

		string newPath = string.Empty;
		//	we make sure the for loop will never add in the last part of the array the object this.jpg
		for(int j = 0; j < splitPath.Length - 1; ++j)
		{
			if(AssetBundleSettings.Instance.IsVariantFolderName(splitPath[j]))
			{
				assetBundleVariant = splitPath[j].ToLower();
				break;
			}

			//	add back the slashes, we skip adding the slash on the first pass
			if(j != 0) newPath += Path.AltDirectorySeparatorChar;

			//	make sure all paths are lower case
			newPath += splitPath[j].ToLower();
		}
		//	final: globals/prefabs/characters
		assetBundleName = newPath;
	}

	/// <summary>
	/// Copies the assets from one directory to another directory.
	/// </summary>
	/// <param name="oldDirectory">Old directory.</param>
	/// <param name="newDirectory">New directory.</param>
	private static void CopyAssets(string oldDirectory, string newDirectory, Variant variant)
	{
		//	clone the default directory into the newly created variant directory
		DirectoryInfo dir = new DirectoryInfo(oldDirectory);
		FileInfo [] files = dir.GetFiles("*", SearchOption.AllDirectories);

		List<string> oldPaths = new List<string>();
		List<string> newPaths = new List<string>();

		//	create the old and new paths for all the files 
		for(int i = 0; i < files.Length; ++i)
		{
			if(IgnoreFile(files[i].Name))
				continue;

			oldPaths.Add(files[i].FullName.Substring(LenghtOfProjectPath));
			newPaths.Add(newDirectory.Substring(LenghtOfProjectPath) + Path.AltDirectorySeparatorChar + files[i].Name);
		}

		//	loop through every path to see if we need to copy any assets from the old location
		//	to the new variant location
		for(int i = 0; i < oldPaths.Count; ++i)
		{
			//	before we move the file we check to see if its already there
			//	ValidateMoveAsset will return a string with an error message on failure and an empty string on success
			string isValidMove = AssetDatabase.ValidateMoveAsset(oldPaths[i], newPaths[i]);

			//	on successful copy of the asset lets reimport and customize out some settings
			if(string.IsNullOrEmpty(isValidMove) && AssetDatabase.CopyAsset(oldPaths[i], newPaths[i]))
			{
				TextureImporter importer = AssetImporter.GetAtPath(newPaths[i]) as TextureImporter;
				importer.mipmapEnabled = variant.MipMaps;

				//	attempt to load the old texture so we can adjust the size of the new texture
				Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(oldPaths[i]);
				if(texture != null)
				{
					int longestSide = texture.width > texture.height ? texture.width : texture.height;
					int newSize = longestSide;

					if(variant.TextureCompression == Variant.TextureCompressionType.HALF)
					{
						newSize = longestSide / 2;
					}

					//	32 is the lowest max size unity support
					if(newSize < 32) { newSize = 32; }

					importer.maxTextureSize = newSize;
				}

				AssetDatabase.ImportAsset(newPaths[i], ImportAssetOptions.ForceUpdate);
			}
		}
	}

	/// <summary>
	/// Ignores the file, if the file is a META or DS_STORE type.
	/// </summary>
	/// <returns><c>true</c>, if file was ignored, <c>false</c> otherwise.</returns>
	/// <param name="name">file name .</param>
	private static bool IgnoreFile(string name)
	{
		bool ignoreFile = false;
		if(name.Contains(".meta") || name.Contains(".DS_Store"))
		{
			ignoreFile = true;
		}
		return ignoreFile;
	}
}