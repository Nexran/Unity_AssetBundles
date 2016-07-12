using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AssetBundleLabels 
{
	[MenuItem("Custom/Asset Bundles/2) Set Asset Bundle Labels", false, 2)]
	private static void SetAssetBundleLabels()
	{
		if(AssetLabelSettings.Instance == null)
			return;

		//	get all Files that are under the main Asset TO Bundle directory 
		DirectoryInfo directory = new DirectoryInfo(AssetLabelSettings.Instance.AssetsToBundleDirectory);
		FileInfo [] files = directory.GetFiles("*", SearchOption.AllDirectories);

		//	calculate the start index to get the length of the project path
		//	this is used to get where in the local project Asset folder the path is
		int lengthOfProjectPath = Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length;			

		List<string> assetPaths = new List<string>();
		for(int i = 0; i < files.Length; ++i)
		{
			//	ignore all meta files
			if(files[i].Name.Contains(".meta") || files[i].Name.Contains(".DS_Store"))
				continue;
			
			assetPaths.Add(files[i].FullName.Substring(lengthOfProjectPath));
		}

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

	private static void GetAssetBundleNameFromPath(string assetPath, out string assetBundleName, out string assetBundleVariant)
	{	
		assetBundleName = string.Empty;
		assetBundleVariant = string.Empty;

		//	remove out the first part of the asset path
		//	before: Assets/AssetsToBundle/Globals/Prefabs/Characters/this.jpg
		//	after: Globals/Prefabs/Characters/this.jpg
		string path = assetPath.Substring(AssetLabelSettings.Instance.AssetsToBundleDirectory.Length + 1);

		//	split out the remaining string Globals/Prefabs/Characters/this.jpg to an array so we can truncate out the object this.jpg
		string [] splitPath = path.Split(Path.AltDirectorySeparatorChar);

		string newPath = string.Empty;
		//	we make sure the for loop will never add in the last part of the array the object this.jpg
		for(int j = 0; j < splitPath.Length - 1; ++j)
		{
			if(AssetLabelSettings.Instance.IsVariantFolderName(splitPath[j]))
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

	[MenuItem("Custom/Asset Bundles/1) Create All Asset Variants", false, 1)]
	private static void CreateAllAssetVariants()
	{
		if(AssetLabelSettings.Instance == null)
			return;

		//	calculate the start index to get the length of the project path
		//	this is used to get where in the local project Asset folder the path is
		int lengthOfProjectPath = Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length;		

		DirectoryInfo directory = new DirectoryInfo(AssetLabelSettings.Instance.AssetsToBundleDirectory);
		DirectoryInfo [] subDirectories = directory.GetDirectories("*", SearchOption.AllDirectories);

		List<string> variantDirectories = new List<string>();
		for(int i = 0; i < subDirectories.Length; ++i)
		{
			if(AssetLabelSettings.Instance.IsVariantFolderName(subDirectories[i].Name))
			{
				variantDirectories.Add(subDirectories[i].FullName);
			}
		}

		//	loop through all added variatn directories to see which ones need asets copied in
		//	or which variant directories need to be created
		for(int i = 0; i < variantDirectories.Count; ++i)
		{
			//	exit out quickly if the directory is the Default Variant directory we will be
			//	cloning all assets from that directory to create the other variant directories
			if(!variantDirectories[i].Contains(AssetLabelSettings.Instance.DefaultVariant))
				continue;

			string directoryNameMinusVariant = variantDirectories[i].Substring(0, 
				variantDirectories[i].Length - AssetLabelSettings.Instance.DefaultVariant.Length);

			//	create all variant directories
			for(int j = 0; j < AssetLabelSettings.Instance.Variants.Length; ++j)
			{
				string directoryNameVariant = directoryNameMinusVariant + AssetLabelSettings.Instance.Variants[j];

				if(Directory.Exists(directoryNameVariant) == false)
				{
					Directory.CreateDirectory(directoryNameVariant);

					//	refresh the database after every directory gets created to make sure its there when we 
					//	attempt to validate the move
					AssetDatabase.Refresh();
				}	
					
				//	clone the default directory into the newly created variant directory
				DirectoryInfo dir = new DirectoryInfo(variantDirectories[i]);
				FileInfo [] files = dir.GetFiles("*", SearchOption.AllDirectories);

				List<string> oldPaths = new List<string>();
				List<string> newPaths = new List<string>();

				//	create the old and new paths for all the files 
				for(int w = 0; w < files.Length; ++w)
				{
					//	ignore all meta files
					if(files[w].Name.Contains(".meta") || files[w].Name.Contains(".DS_Store"))
						continue;

					oldPaths.Add(files[w].FullName.Substring(lengthOfProjectPath));
					newPaths.Add(directoryNameVariant.Substring(lengthOfProjectPath) + Path.AltDirectorySeparatorChar + files[w].Name);
				}

				//	loop through every path to see if we need to copy any assets from the old location
				//	to the new variant location
				for(int a = 0; a < oldPaths.Count; ++a)
				{
					//	before we move the file we check to see if its already there
					//	ValidateMoveAsset will return a string with an error message on failure and an empty string on success
					string isValidMove = AssetDatabase.ValidateMoveAsset(oldPaths[a], newPaths[a]);

					//	on successful copy of the asset lets reimport and customize out some settings
					if(string.IsNullOrEmpty(isValidMove) && AssetDatabase.CopyAsset(oldPaths[a], newPaths[a]))
					{
						TextureImporter importer = AssetImporter.GetAtPath(newPaths[a]) as TextureImporter;
						importer.mipmapEnabled = false;

						//	attempt to load the old texture so we can adjust the size of the new texture
						Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(oldPaths[a]);
						if(texture != null)
						{
							int longestSide = texture.width > texture.height ? texture.width : texture.height;
							int newSize = longestSide / 2;

							//	32 is the lowest max size unity support
							if(newSize < 32) { newSize = 32; }

							importer.maxTextureSize = newSize;
						}

						AssetDatabase.ImportAsset(newPaths[a], ImportAssetOptions.ForceUpdate);
					}
				}
			}
		}
		//	refresh the database now that all imports and moves are all done to make
		//	sure the project view is updated
		AssetDatabase.Refresh();
		Debug.Log("Asset Variants Created!\n");
	}

	[MenuItem("Custom/Asset Bundles/4) Delete All Asset Variants", false, 4)]
	private static void DeleteAllAssetVariants()
	{
		if(AssetLabelSettings.Instance == null)
			return;

		//	calculate the start index to get the length of the project path
		//	this is used to get where in the local project Asset folder the path is
		int lengthOfProjectPath = Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length;	

		DirectoryInfo directory = new DirectoryInfo(AssetLabelSettings.Instance.AssetsToBundleDirectory);
		DirectoryInfo [] subDirectories = directory.GetDirectories("*", SearchOption.AllDirectories);

		List<string> variantDirectories = new List<string>();
		for(int i = 0; i < subDirectories.Length; ++i)
		{
			if(AssetLabelSettings.Instance.IsVariantFolderName(subDirectories[i].Name))
			{
				variantDirectories.Add(subDirectories[i].FullName);
			}
		}

		for(int i = 0; i < variantDirectories.Count; ++i)
		{
			if(variantDirectories[i].Contains(AssetLabelSettings.Instance.DefaultVariant))
				continue;

			string directoryNameMinusVariant = variantDirectories[i].Substring(0, 
				variantDirectories[i].Length - AssetLabelSettings.Instance.DefaultVariant.Length);
			
			//	create all variant directories
			for(int j = 0; j < AssetLabelSettings.Instance.Variants.Length; ++j)
			{
				string directoryNameVariant = directoryNameMinusVariant + AssetLabelSettings.Instance.Variants[j];
			
				if(Directory.Exists(directoryNameVariant) == true)
				{
					FileInfo file = new FileInfo(directoryNameVariant + ".meta");
					if(file != null && file.Exists)
					{
						file.Delete();
					}
					Directory.Delete(directoryNameVariant, true);
				}	
			}
		}

		AssetDatabase.Refresh();
		Debug.Log("Asset Variants Deleted!\n");
	}

	[MenuItem("Custom/Asset Bundles/3) Build Asset Bundles", false, 3)]
	private static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = string.Empty;
		if(AssetLabelSettings.Instance != null)
		{
			assetBundleDirectory = AssetLabelSettings.Instance.AssetBundleDirectory;
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

	[MenuItem("Custom/Asset Bundles/RUN ALL", false, 5)]
	private static void RunAllAssetBundleLogic()
	{
		CreateAllAssetVariants();
		SetAssetBundleLabels();
		BuildAllAssetBundles();
		DeleteAllAssetVariants();
	}
}