using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		AssetBundleManager.Initialize();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Q))
		{
			DoLoad(AssetBundleManager.LoadAllAssetBundles());
		}
	}

	void DoLoad(List<AssetBundle> bundles)
	{
		if(bundles == null)
			return;

		//	cycle through all asset bundles and find the player to load
		//	TODO this is awful but just wanted to make sure it worked
		//	probably would want the AssetBundleManager to store the bundles in a factory then be able to request? 
		//	not sure will think about it more
		for(int i = 0; i < bundles.Count; ++i)
		{
			GameObject prefab = bundles[i].LoadAsset<GameObject>("Player");
			if(prefab != null)
			{
				Instantiate(prefab);
			}
		}
	}
}
