using UnityEditor;
using System.IO;
using UnityEngine;

public class CreateAssetBundles
{
	[MenuItem ("Custom/Build AssetBundles")]
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
	}
}