using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;

[TestFixture]
[Category("Asset Viewer Directory Tests")]
internal class AssetViewerDirectoryTest 
{
	private string _testDirectory = Application.dataPath + "/Editor/UnitTests/Test";
	private string _openDirectory = Application.dataPath + "/Editor/UnitTests/Test/Open";
	private string _closedDirectory = Application.dataPath + "/Editor/UnitTests/Test/~Closed";

	[TestFixtureSetUp]
	public void Init()
	{
		Debug.Log("setup");
		Directory.CreateDirectory(_testDirectory);
		Directory.CreateDirectory(_openDirectory);
		Directory.CreateDirectory(_closedDirectory);
	}

	[TestFixtureTearDown]
	public void Destroy()
	{
		Debug.Log("TearDown");
		if(Directory.Exists(_openDirectory)) Directory.Delete(_openDirectory);
		if(Directory.Exists(_closedDirectory)) Directory.Delete(_closedDirectory);
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

		Assert.That(dir.ContainsSubDirectories);
	}

	[Test]
	public void InitAssetManifest()
	{
		//	test null 
		AssetViewerDirectory dir = new AssetViewerDirectory(_testDirectory);
		dir.InitAssetManifest();

		Assert.That(dir.AssetViewerManifest == null);
	}

	[Test]
	public void InitDependencyDirectories()
	{
		AssetViewerDirectory dir = new AssetViewerDirectory(_testDirectory);
		dir.InitDependencyDirectories();

		Assert.AreEqual(dir.DependencyDirectories[0], "Assets/Editor");
		Assert.AreEqual(dir.DependencyDirectories[1], "Assets/Editor/UnitTests");
	}

	[Test]
	public void OpenDirectory()
	{
		AssetViewerDirectory dir = new AssetViewerDirectory(_closedDirectory);
		dir.Init();

		dir.OpenDirectory();

		Assert.That(dir.Directory.FullName.Contains("~"));
	}

	[Test]
	public void CloseDirectory()
	{
		AssetViewerDirectory dir = new AssetViewerDirectory(_openDirectory);
		dir.Init();

		dir.CloseDirectory();

		Assert.That(!dir.Directory.FullName.Contains("~"));
	}
}
