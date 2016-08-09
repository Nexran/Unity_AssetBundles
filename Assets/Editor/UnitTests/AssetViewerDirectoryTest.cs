using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

[TestFixture]
[Category("Asset Viewer Directory Tests")]
internal class AssetViewerDirectoryTest 
{
	private string _testDirectory = Application.dataPath + "/Editor/UnitTests/Test";

	[SetUp]
	public void Init()
	{
		Directory.CreateDirectory(_testDirectory);
	}

	[TearDown]
	public void Destroy()
	{
		Directory.Delete(_testDirectory);
	}

	[Test]
	[ExpectedException(typeof(DirectoryNotFoundException), ExpectedMessage = "Directory not found")]
	public void IsDirectoryFound()
	{
		new AssetViewerDirectory(string.Empty);
		new AssetViewerDirectory("Assets/Blah/NotReal");
	}
}
