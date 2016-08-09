using UnityEngine;

/// <summary>
/// Extends the String class.
/// </summary>
public static class StringExtensions
{
	/// <summary>
	///	calculate the start index to get the length of the project path
	///	this is used to get where in the local project Asset folder the path is
	/// </summary>
	/// <value>The length of project path.</value>
	public static int LengthOfProjectPath
	{
		get { return Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length; }
	}

	/// <summary>
	/// Removes the project path from the passed in path location. 
	/// /Users/meh/Unity_AssetBundles/Assets becomes Assets
	/// </summary>
	/// <returns>The project path.</returns>
	/// <param name="path">Path</param>
	public static string RemoveProjectPath(this string path)
	{
		string newString = string.Empty;
		if(string.IsNullOrEmpty(path) == false && path.Length > LengthOfProjectPath)
		{
			newString = path.Substring(LengthOfProjectPath);
		}
		return newString;
	}
}