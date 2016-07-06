using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.Networking;
using System.IO;
using System.Collections.Generic;

public static class AssetBundleManager
{
	private static string AssetBundleLocation = "/~/AssetBundles/";
	private static string MainAssetBundleName = "AssetBundles";
	private static string AssetBundleManifestName = "AssetBundleManifest";

	private static AssetBundleManifest _assetBundleManifest;

	public static void Initialize()
	{
		string loadFromFile = Application.dataPath + AssetBundleLocation + MainAssetBundleName;
	
		AssetBundle assetBundle = AssetBundle.LoadFromFile(loadFromFile);
		if(assetBundle != null)
		{
			Debug.Log("Loaded AssetBundle at " + loadFromFile);
		}
		else
		{
			Debug.LogError("Failed to load AssetBundle at " + loadFromFile);
			return;
		}

		_assetBundleManifest = assetBundle.LoadAsset<AssetBundleManifest>(AssetBundleManifestName);
		if(_assetBundleManifest != null)
		{
			Debug.Log("Loaded AssetBundleManifest");
		}
		else
		{
			Debug.LogError("Failed to load AssetBundleManifest");
			return;
		}

		assetBundle.Unload(false);
	}

	/// <summary>
	/// Non-async Loads all asset bundles.
	/// </summary>
	/// <returns>The all asset bundles.</returns>
	public static List<AssetBundle> LoadAllAssetBundles()
	{
		if(_assetBundleManifest == null)
			return null;

		//	make sure we some valid asset bundles in the manifest
		string [] allAssetBundles = _assetBundleManifest.GetAllAssetBundles();
		if(allAssetBundles == null || allAssetBundles.Length == 0)
			return null;

		//	set up the queue of asset bundles we are going to request
		Queue<string> assetBundlesToLoad = new Queue<string>();
		for(int i = 0; i < allAssetBundles.Length; ++i)
		{
			assetBundlesToLoad.Enqueue(allAssetBundles[i]);
		}

		//	TODO make this a coroutine
		return LoadAssetBundles(assetBundlesToLoad);
	}

	/// <summary>
	/// Non-async Loads the requested asset bundles.
	/// </summary>
	/// <returns>The assets bundles.</returns>
	/// <param name="assetsToLoad">Queue of Assets to load will load in that order</param>
	private static List<AssetBundle> LoadAssetBundles(Queue<string> assetsToLoad)
	{
		List<AssetBundle> bundles = new List<AssetBundle>();
		do
		{
			string assetName = Application.dataPath + AssetBundleLocation + assetsToLoad.Dequeue();
			AssetBundle assetBundle = AssetBundle.LoadFromFile(assetName);

			if(assetBundle != null)
			{
				bundles.Add(assetBundle);
				Debug.Log("Loaded AssetBundle at " + assetName);
			}
			else
			{
				Debug.LogError("Failed to load AssetBundle at " + assetName);
			}

			//	NOTE we are not calling assetBundle.Unload
			//	TODO figure out how to load out the asset so we can Unload or maybe not? 
		}
		while(assetsToLoad.Count != 0);

		return bundles;
	}
}
