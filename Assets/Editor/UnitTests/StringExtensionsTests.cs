using UnityEngine;
using UnityEditor;
using NUnit.Framework;

[TestFixture]
[Category("Extension Tests")]
internal class StringExtensionsTests 
{
	[Test]
	public void RemoveProjectPathTest()
	{
		//	null test
		string nullString = null;
		nullString = nullString.RemoveProjectPath();
		Assert.AreEqual(string.Empty, nullString);

		//	test a small string, that is smaller than the amount that will be 
		//	removed from the string to check out of bounds
		string smallString = " ";
		smallString = smallString.RemoveProjectPath();
		Assert.AreEqual(string.Empty, nullString);

		//	standard case
		string testString = Application.dataPath;
		testString = testString.RemoveProjectPath();
		Assert.AreEqual("Assets", testString);
	}
}