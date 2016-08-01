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

	public static string RemoveProjectPath(this string path)
	{
		return path.Substring(LengthOfProjectPath);
	}
}