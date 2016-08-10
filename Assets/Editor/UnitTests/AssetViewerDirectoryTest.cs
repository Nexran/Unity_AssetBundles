using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

[TestFixture]
[Category("Asset Viewer Directory Tests")]
internal class AssetViewerDirectoryTest 
{
	private string _testDirectory = Application.dataPath + "/Editor/UnitTests/Test";
	private string _subDirectory = Application.dataPath + "/Editor/UnitTests/Test/Subdirectory";

	[TestFixtureSetUp]
	public void Init()
	{
		Debug.Log("setup");
		Directory.CreateDirectory(_testDirectory);
		Directory.CreateDirectory(_subDirectory);
	}

	[TestFixtureTearDown]
	public void Destroy()
	{
		Debug.Log("TearDown");
		if(Directory.Exists(_subDirectory)) Directory.Delete(_subDirectory);
		if(Directory.Exists(_testDirectory)) Directory.Delete(_testDirectory);
	}

	[Test]
	[ExpectedException(typeof(DirectoryNotFoundException), ExpectedMessage = "Directory not found")]
	public void IsDirectoryFound()
	{
		//	test null 
		new AssetViewerDirectory(string.Empty);

		//	test not created directory
		new AssetViewerDirectory("Assets/Blah/NotReal");
	}

	[Test]
	public void InitAssetInfo()
	{
		AssetViewerDirectory dir = new AssetViewerDirectory(_testDirectory);
		dir.InitAssetInfo();

		Assert.True(dir.ContainsSubDirectories);
	}

	[Test]
	[ExpectedException(typeof(FileNotFoundException), ExpectedMessage = "Unable to find the specified file.")]
	public void InitAssetManifest()
	{
		//	test null 
		AssetViewerDirectory dir = new AssetViewerDirectory(_testDirectory);
		dir.InitAssetManifest();
	}

	[Test]
	public void InitDependencyDirectories()
	{
		AssetViewerDirectory dir = new AssetViewerDirectory(_testDirectory);
		dir.InitDependencyDirectories();

		Assert.AreEqual(dir.DependencyDirectories[0], "Assets/Editor");
		Assert.AreEqual(dir.DependencyDirectories[1], "Assets/Editor/UnitTests");
	}
}
