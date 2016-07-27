using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class AssetViewerManifest
{
	public List<string> Assets { get; private set; }
	public List<AssetViewerInfo> Dependencies { get; private set; }

	public List<string> Text { get; private set; }

	public AssetViewerManifest(string path, string assetsToBundleDirectory, int lenghtOfProjectPath, int assetBundleOffset)
	{
		Assets = new List<string>();
		Dependencies = new List<AssetViewerInfo>();
		Text = new List<string>();

		//	parse everything from the file into a list of strings
		using(StreamReader reader = new StreamReader(path))
		{
			string line = string.Empty;

			while((line = reader.ReadLine()) != null)
			{
				Text.Add(line);
			}
		}

		//	find the start and end based on how the text file is laid out
		int assetStartIndex = Text.FindIndex(o => o.Equals("Assets:")) + 1;
		int assetEndIndex = assetStartIndex + 1;
		int dependenciesStartIndex = Text.FindIndex(o => o.Equals("Dependencies:")) + 1;
		int dependenciesEndIndex = dependenciesStartIndex + 1;

		//	all assets!
		for(int i = assetStartIndex; i < assetEndIndex; ++i)
		{
			Assets.Add(Text[i].Substring(2));
		}

		//	all dependencies!!
		for(int i = dependenciesStartIndex; i < dependenciesEndIndex; ++i)
		{
			string parse = Text[i].Substring(2).Replace('.', Path.DirectorySeparatorChar);
			parse = Application.dataPath.Substring(0, lenghtOfProjectPath) + assetsToBundleDirectory + "/" + parse.Substring(assetBundleOffset);

			FileInfo file = new FileInfo(parse);
			Dependencies.Add(new AssetViewerInfo(file, file.FullName.Replace("~", string.Empty)));
		}

		/*
		for(int i = 0; i < Assets.Count; ++i)
		{
			Debug.Log("ASSET " + Assets[i]);
		}

		for(int i = 0; i < Dependencies.Count; ++i)
		{
			Debug.Log("DEPENDENCY " + Dependencies[i]);
			DirectoryInfo d = new DirectoryInfo(Dependencies[i]);
			if(d.Exists)
			{
				Debug.Log("EXISTS " + Dependencies[i]);
			}
		}*/
	}
}