using UnityEditor;
using System.IO;
using UnityEngine;

public class CreateAssetBundles
{
	[MenuItem ("Custom/Build AssetBundles")]
	private static void BuildAllAssetBundles()
	{
		//	we create the directory if we doesn't exist
		if(Directory.Exists(Application.dataPath + "/~/AssetBundles") == false)
		{
			Directory.CreateDirectory(Application.dataPath + "/~/AssetBundles");
		}

		//	build build!!
		BuildPipeline.BuildAssetBundles("Assets/~/AssetBundles");
	}
}