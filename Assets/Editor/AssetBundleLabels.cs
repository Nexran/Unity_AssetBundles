using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class AssetBundleLabels 
{
	[MenuItem("Custom/BundleName")]
	private static void SetAssetBundleName()
	{
		string [] assetPaths = SelectionExtensions.GetAllSelectedAssetPaths();

		for(int j = 0; j < assetPaths.Length; ++j)
		{
			AssetImporter assetImporter = AssetImporter.GetAtPath(assetPaths[j]);
			assetImporter.assetBundleName = "myLevel";
			assetImporter.assetBundleVariant = "x2";
			assetImporter.SaveAndReimport();

			Debug.Log(assetPaths[j]);
		}

	//	for(int i = 0; i < selectedObjects.Length; ++i)	
	//	{
	//		EditorUtility.SetDirty(selectedObjects[i]);
	//	}
	}
}