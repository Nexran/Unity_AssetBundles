using UnityEditor;

public class CreateAssetBundles
{
	[MenuItem ("Custom/Build AssetBundles")]
	private static void BuildAllAssetBundles()
	{
		//	TODO check for this folder existing if it doesn't exist auto build it
		BuildPipeline.BuildAssetBundles("Assets/~/AssetBundles");
	}
}