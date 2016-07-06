using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
			string [] scenePaths = bundles[i].GetAllScenePaths();

			if(scenePaths != null && scenePaths.Length > 0)
			{
				string [] splits = scenePaths[0].Split('/');
				string [] splitAgain = splits[splits.Length - 1].Split('.');

				Debug.Log("Loaded Scene " + splitAgain[0]);
				SceneManager.LoadScene(splitAgain[0], LoadSceneMode.Additive);
			}
		}
	}
}
