# Unity_AssetBundles

This is an example project used to show how to use the main AssetBundle created to then load all other asset bundles that exist. In AssetBundleManager.cs it loads the AssetBundle named "AssetBundles" then loads the Manifest from that bundle. It then stores out the Manifest and on any Load calls it references that manifest to grab AssetBundle Information such as name.

In order to run the project you need to open the Main scene that is under Resources/Scenes/Main.scene. You must also build the AssetBundles via the Menu Custom/Build AssetBundles. Then when you run the scene you will notice the console log loading in the asset bundle. Then if you press Q it will load in all other remaining Bundles. There is code in Test.cs that then searches for the Player prefab and loads it for a nice visual to make sure everything is working.

If you see Aladdin all is correct.

NOTE: AssetBundle creation is not the best currently, I will be updating this. However right now all assets that you want bundled need to be set up via the Asset Bundle Labeling system.
