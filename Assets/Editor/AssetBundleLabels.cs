using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AssetBundleLabels 
{
	[MenuItem("Custom/BundleName")]
	private static void SetAssetBundleName()
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
			if(files[i].Name.Contains(".meta"))
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

	[MenuItem("Custom/MakeX1")]
	private static void MakeX1()
	{
		if(AssetLabelSettings.Instance == null)
			return;

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
				if(Directory.Exists(directoryNameVariant) == false)
				{
					Directory.CreateDirectory(directoryNameVariant);
				}	

				Debug.Log(directoryNameVariant);
				//	clone the default directory into the newly created variant directory
			}
		}
	}
}