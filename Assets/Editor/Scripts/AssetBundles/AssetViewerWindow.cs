using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;

public class ImportDirectory
{
	public DirectoryInfo parentDir { get; set; }
	public DirectoryInfo dir { get; set; }

	public DirectoryInfo [] subDirectories { get; set; }

	public FileInfo [] files { get; set; }

	public string expandedDirName { get; set; } 
	public string expandedParentName { get; set; }

	public bool expanded { get; set; }
	public int indentLevel { get; set; } 
	public bool selected { get; set; }
	public Rect controlRect { get; set; }
	public bool setRect { get; set; }

	public ImportDirectory()
	{
		expanded = false;
		setRect = true;
	}
}
	
public class AssetViewerWindow : EditorWindow
{
	/// <summary>
	///	calculate the start index to get the length of the project path
	///	this is used to get where in the local project Asset folder the path is
	/// </summary>
	/// <value>The lenght of project path.</value>
	private static int LenghtOfProjectPath
	{
		get { return Application.dataPath.Remove(Application.dataPath.Length - 6, 6).Length; }
	}

	private static string _windowName = "Asset Viewer";
	private static string _selectionTextureName = "opacity.png";
	private static string _dividerTextureName = "Test.png";
	private static string _folderTextureName = "FolderIcon.png";

	private static Texture _selectionTexture;
	private static Texture _dividerTexture;
	private static Texture2D _folderTexture;

	private static GUIStyle _foldoutStyle;

	private List<ImportDirectory> _directories = new List<ImportDirectory>();
	private List<string> _expandedDirectories = new List<string>();
	private List<string> _removedDirectories= new List<string>();
	private List<string> _openedDirectories = new List<string>();

	private Vector2 _scroll;
	private Vector2 _rightScroll;

	private float currentScrollViewWidth;
	private bool resize;
	private Rect cursorChangeRect;

	private Rect _selectionRect;

	[MenuItem("Custom/Asset Viewer")]
	public static void ShowWindow()
	{
		GetWindow<AssetViewerWindow>(_windowName, typeof(SceneView));
		_foldoutStyle = new GUIStyle(EditorStyles.foldout);
	}

	public void OnEnable()
	{
		//	this searches in the default folder Editor Default Resources for the texture
		_selectionTexture = EditorGUIUtility.Load(_selectionTextureName) as Texture;
		_dividerTexture = EditorGUIUtility.Load(_dividerTextureName) as Texture;
		_folderTexture = AssetDatabase.GetCachedIcon("Assets") as Texture2D;//EditorGUIUtility.Load(_folderTextureName) as Texture2D;

		resize = false;
		currentScrollViewWidth = this.position.width / 2;
		cursorChangeRect = new Rect(currentScrollViewWidth, 0, 2f, this.position.height);
	}

	public void OnFocus()
	{
		if(AssetBundleSettings.Instance == null)
			return;

		//	get all Files that are under the main Asset TO Bundle directory 
		DirectoryInfo directory = new DirectoryInfo(AssetBundleSettings.Instance.AssetsToBundleDirectory);
		DirectoryInfo [] dirs = directory.GetDirectories("*", SearchOption.AllDirectories);

		//	we always want the main directory expanded
		if(_expandedDirectories.Count == 0)
		{
			_expandedDirectories.Add(directory.FullName);
		}
		int baseIndentCount = directory.FullName.Split(Path.DirectorySeparatorChar).Length;

		foreach(DirectoryInfo dir in dirs)
		{
			ImportDirectory importDirectory = new ImportDirectory();

			importDirectory.parentDir = dir.Parent;
			importDirectory.dir = dir;

			importDirectory.expandedDirName = dir.FullName;
			importDirectory.expandedParentName = dir.Parent.FullName;
			if(importDirectory.expandedDirName.Contains("~"))
			{
				importDirectory.expandedDirName = importDirectory.expandedDirName.Replace("~", string.Empty);
			}
			if(importDirectory.expandedParentName.Contains("~"))
			{
				importDirectory.expandedParentName = importDirectory.expandedParentName.Replace("~", string.Empty);
			}

			importDirectory.indentLevel = importDirectory.dir.FullName.Split(Path.DirectorySeparatorChar).Length - baseIndentCount;
			importDirectory.subDirectories = dir.GetDirectories("*", SearchOption.TopDirectoryOnly);
			importDirectory.files = dir.GetFiles("*", SearchOption.TopDirectoryOnly).Where(o => !o.Name.EndsWith(".meta") && !o.Name.EndsWith(".DS_Store")).ToArray();

			if(importDirectory.subDirectories.Length == 0)
			{
				importDirectory.expanded = false;
			}
			else
			{
				if(!importDirectory.subDirectories.Any(o => o.FullName.Contains("~")))
				{
					importDirectory.expanded = true;
				}
				else
				{
					importDirectory.expanded = false;
				}
			}

			int index = _directories.FindIndex(o => o.expandedDirName.Equals(importDirectory.expandedDirName));
			if(index == -1)
			{
				Debug.Log("ADD " + importDirectory.expandedDirName + importDirectory.expanded);
				_directories.Add(importDirectory);
			}
			else
			{
				Debug.Log("REPLACE " + index + " ");
				Debug.Log("OLD " + _directories[index].expandedDirName +  " " + _directories[index].expanded);
				Debug.Log("NEW " + importDirectory.expandedDirName + " " + importDirectory.expanded);

				_directories.RemoveAt(index);
				_directories.Insert(index, importDirectory);
			}
		}

		//	sort based on the directory name 
		//	this will get all the directories in a correct visible order
		_directories.Sort((x, y) =>
		{
			return x.expandedDirName.CompareTo(y.expandedDirName);
		});	
	}

	public void OnGUI()
	{
		if(AssetBundleSettings.Instance == null)
			return;

		Vector2? _mousePosition = null;

		if(Event.current.type == EventType.MouseDown)
		{
			_mousePosition = Event.current.mousePosition;
		}

		GUILayout.BeginHorizontal();

		_scroll = GUILayout.BeginScrollView(_scroll, false, false, GUILayout.Width(currentScrollViewWidth));

		if(_directories != null)
		{
			if(Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
			{
				UpdateExpandedDirectories();
			}

			for(int i = 0; i < _directories.Count; ++i)
			{
				EditorGUI.indentLevel = _directories[i].indentLevel;

				for(int j = 0; j < _expandedDirectories.Count; ++j)
				{
					if(_directories[i].expandedParentName == _expandedDirectories[j])
					{
						GUILayout.BeginHorizontal();

						if(_directories[i].subDirectories.Length == 0)
						{
							EditorGUILayout.LabelField(string.Format("          {0}", _directories[i].dir.Name));
						}
						else
						{
							_directories[i].expanded = EditorGUILayout.Foldout(_directories[i].expanded, string.Format("      {0}", _directories[i].dir.Name));
						}
						Rect r = GUILayoutUtility.GetLastRect();

						if(_directories[i].setRect == true && r.width > 10f)
						{
							_directories[i].controlRect = new Rect(0, r.y, r.width + r.x, r.height);
							_directories[i].setRect = false;
						}

						Rect a = EditorGUI.IndentedRect(r);
						float x = (a.x - r.x) + 17f;
						if(_folderTexture != null)	GUI.DrawTexture(new Rect(x, r.y, 16, 16), _folderTexture);

						if(_directories[i].selected)
						{
							DrawSelectedItem(_directories[i].controlRect);
						}
							
						GUILayout.EndHorizontal();
					}
				}
			}
		}

		GUILayout.EndScrollView();

		ResizeScrollView();

		EditorGUI.indentLevel = 0;
		_rightScroll = GUILayout.BeginScrollView(_rightScroll, false, false);

		for(int i = 0; i < _directories.Count; ++i)
		{
			if(_directories[i].selected == true)
			{
				if(_directories[i].subDirectories.Length > 0 || _directories[i].files.Length > 0)
				{
					for(int j = 0; j < _directories[i].subDirectories.Length; ++j)
					{
						EditorGUILayout.LabelField(string.Format("     {0}", _directories[i].subDirectories[j].Name));
						Rect r = GUILayoutUtility.GetLastRect();
						if(_folderTexture != null) GUI.DrawTexture(new Rect(r.x, r.y, 16, 16), _folderTexture);
					}

					for(int j = 0; j < _directories[i].files.Length; ++j)
					{
						EditorGUILayout.LabelField(string.Format("     {0}", _directories[i].files[j].Name));
						Rect r = GUILayoutUtility.GetLastRect();

						string fileLocation = _directories[i].files[j].FullName.Substring(LenghtOfProjectPath);
						Texture tex = AssetDatabase.GetCachedIcon(fileLocation);
						if(tex != null) GUI.DrawTexture(new Rect(r.x, r.y, 16, 16), tex);
					}
				}
				else
				{
					EditorGUILayout.LabelField("This folder is empty");
				}
			}
		}

		GUILayout.EndScrollView();

		GUILayout.EndHorizontal();

		for(int i = 0; i < _directories.Count; ++i)
		{
			if(_mousePosition.HasValue && _directories[i].controlRect.Contains(_mousePosition.Value))
			{
				//	if we switch one to true lets reset all others to false
				for(int j = 0; j < _directories.Count; ++j)
				{
					_directories[j].selected = false;
				}

				string fileLocation = _directories[i].dir.FullName.Substring(LenghtOfProjectPath);
				Debug.Log("selected " + fileLocation);
				Object obj = AssetDatabase.LoadAssetAtPath(fileLocation, typeof(Object));
				Selection.activeObject = obj;

				_directories[i].selected = true;
			}
		}
	}

	public void OnInspectorUpdate() 
	{
		Repaint();
		AssetDatabase.Refresh();
	}

	private void UpdateExpandedDirectories()
	{
		for(int i = 0; i < _directories.Count; ++i)
		{
			if(_directories[i].expanded == true && !_expandedDirectories.Contains(_directories[i].expandedDirName))
			{
				_expandedDirectories.Add(_directories[i].expandedDirName);
				_openedDirectories.Add(_directories[i].expandedDirName);

				for(int j = 0; j < _directories.Count; ++j)
				{
					_directories[j].setRect = true;
				}
			}
			else if(_directories[i].expanded == false && _expandedDirectories.Contains(_directories[i].expandedDirName))
			{
				_removedDirectories.Add(_directories[i].expandedDirName);
				_expandedDirectories.Remove(_directories[i].expandedDirName);

				for(int j = 0; j < _directories.Count; ++j)
				{
					_directories[j].setRect = true;
				}
			}
		}

		for(int j = _openedDirectories.Count - 1; j >= 0; j--)
		{
			for(int i = 0; i < _directories.Count; ++i)
			{
				if(_openedDirectories[j] == _directories[i].expandedParentName)
				{
					Debug.Log("OPEN " + i + " " + _directories[i].dir.FullName);
					if(_directories[i].dir.FullName.Contains("~"))
					{
						string newPath = _directories[i].dir.FullName.Replace("~", string.Empty);

						_directories[i].dir.MoveTo(newPath);
						//_directories[i].dir = new DirectoryInfo(newPath);

						OnFocus();
					}
				}
			}
		}
		_openedDirectories.Clear();

		//	we store out all recently removed directories
		//	if the directory was closed we then check to see if any diretories share a parent
		//	if they share a parent we force the directory closed
		for(int j = _removedDirectories.Count - 1; j >= 0; j--)
		{
			for(int i = 0; i < _directories.Count; ++i)
			{
				if(_removedDirectories[j] == _directories[i].expandedParentName )
				{
					string newPath = _directories[i].dir.FullName + "~";
					bool exists = Directory.Exists(newPath);
					Debug.Log("CLOSED " + i + " " + _directories[i].expandedDirName + " " + exists);

					if(exists == false)
					{
						_directories[i].dir.MoveTo(newPath);
						//_directories[i].dir = new DirectoryInfo(newPath);
						OnFocus();

					}
					_directories[i].expanded = false;
				}
			}
		}
		_removedDirectories.Clear();
	}

	private bool ToggleTildeButton(ImportDirectory dir)
	{
		if(dir.dir.Name.Contains("~"))
		{
			if(GUILayout.Button("Show", GUILayout.Width(75), GUILayout.Height(16)))
			{
				dir.dir.MoveTo(dir.dir.FullName.Replace("~", string.Empty));
				return true;
			}
		}
		else
		{
			if(GUILayout.Button("Hide", GUILayout.Width(75), GUILayout.Height(16)))
			{
				dir.dir.MoveTo(dir.dir.FullName + "~");
				return true;
			}
		}
		return false;

	}

	private void DrawSelectedItem(Rect rect)
	{
		//rect.Set(0, rect.y, rect.width + rect.x, rect.height);

		GUI.DrawTexture(rect, _selectionTexture);
	//	EditorGUIUtility.AddCursorRect(rect, MouseCursor.RotateArrow);
	}

	private void ResizeScrollView()
	{
		GUI.DrawTexture(cursorChangeRect, _dividerTexture);
		EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeHorizontal);

		if( Event.current.type == EventType.mouseDown && cursorChangeRect.Contains(Event.current.mousePosition))
		{
			resize = true;
		}

		if(resize)
		{
			currentScrollViewWidth = Event.current.mousePosition.x;
			cursorChangeRect.Set(currentScrollViewWidth, cursorChangeRect.y, cursorChangeRect.width, cursorChangeRect.height);
		}

		if(Event.current.type == EventType.MouseUp)
		{
			resize = false;        
		}
	}
}
