using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;
	
/// <summary>
/// Asset viewer window, shows all directories that are listed under the Asset To Bundle Directory set in Asset Bundle Settings.
/// </summary>
public class AssetViewerWindow : EditorWindow
{
	private static string _windowName = "Asset Viewer";
	private static string _selectionTextureName = "opacity.png";
	private static string _defaultFolderName = "Assets";
	private static float _dividerWidth = 2f;
	private static char _smallRightArrowUnicode = '\u25B8';
	private static float _folderXOffset = 17f;
	private static string _noFilesOrSubDir = "This folder is empty";

	private static Texture _selectionTexture;
	private static Texture _dividerTexture;
	private static Texture2D _folderTexture;

	private List<AssetViewerDirectory> _viewerDirectories;
	private List<string> _expandedDirectories;
	private List<string> _removedDirectories;
	private List<string> _openedDirectories;

	private Vector2 _leftScroll;
	private Vector2 _rightScroll;

	private float _currentViewWidth;
	private bool _isResizing;
	private Rect _dividerRect;

	private Rect _selectionRect;
	private string _directoryDisplayName;

	/// <summary>
	/// Shows the window.
	/// </summary>
	[MenuItem("Custom/Asset Viewer")]
	public static void ShowWindow()
	{
		GetWindow<AssetViewerWindow>(_windowName);
	}

	/// <summary>
	/// Unity function OnEnable, called when the window gets initialized. 
	/// </summary>
	public void OnEnable()
	{
		//	this searches in the default folder Editor Default Resources for the texture
		_selectionTexture = EditorGUIUtility.Load(_selectionTextureName) as Texture;
		_dividerTexture = EditorGUIUtility.Load(_selectionTextureName) as Texture;
		_folderTexture = AssetDatabase.GetCachedIcon(_defaultFolderName) as Texture2D;

		_viewerDirectories = new List<AssetViewerDirectory>();
		_expandedDirectories = new List<string>();
		_removedDirectories= new List<string>();
		_openedDirectories = new List<string>();
	}

	/// <summary>
	/// Unity function OnFocus, Called when the window gets keyboard focus.
	/// </summary>
	public void OnFocus()
	{
		if(AssetBundleSettings.Instance == null || string.IsNullOrEmpty(AssetBundleSettings.Instance.AssetsToBundleDirectory))
			return;
			
		//	initialize the Directory Display Name
		_directoryDisplayName = AssetBundleSettings.Instance.AssetsToBundleDirectory;
		_directoryDisplayName = _directoryDisplayName.Replace(Path.DirectorySeparatorChar, _smallRightArrowUnicode);

		InitViewerWindow();
	}

	/// <summary>
	/// Unity function OnGUI, might be called several times per frame (one call per event). 
	/// </summary>
	public void OnGUI()
	{
		if(AssetBundleSettings.Instance == null)
			return;

		//	calculate mouse position before any other part due to other logic potentially 
		//	altering the mouse position and setting the Event.current.type to USED instead of Mouse Down
		//	mouse position being null means the mouse wasn't pressed down
		Vector2? _mousePosition = null;
		if(Event.current.type == EventType.MouseDown)
		{
			//	adjust for if the user has scrolled the view down the page
			_mousePosition = Event.current.mousePosition + _leftScroll;
		}
		//	only update directories when the Event is either Repaint or Layout so it doesn't change while calculating what to render
		else if(Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
		{
			if(_viewerDirectories != null) { UpdateViewerDirectories(); }
		}

		GUILayout.BeginHorizontal();

		//	render the left side of the screen
		RenderViewerDirectories();
	
		//	render the divider used to click and reposition the size of the 2 halves of the screen
		RenderDivider();

		//	render the right side of the screen
		RenderFolderView();

		GUILayout.EndHorizontal();

		Vector2? _rightMousePosition = null;

		if(_mousePosition.HasValue)
		{
			_rightMousePosition = new Vector2?(new Vector2(_mousePosition.Value.x + _currentViewWidth, _mousePosition.Value.y));
		}

		if(_rightMousePosition.HasValue)
		{
			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				if(_viewerDirectories[i].IsSelected == true && _viewerDirectories[i].FileRects != null)
				{
					Debug.Log(_viewerDirectories[i].FileRects.Count + " " + _mousePosition.Value);
					for(int j = 0; j < _viewerDirectories[i].FileRects.Count; ++j)
					{
						Debug.Log(_viewerDirectories[i].FileRects[j]);

						if(_viewerDirectories[i].FileRects[j].Contains(_mousePosition.Value))
						{
							Object obj = AssetDatabase.LoadAssetAtPath(_viewerDirectories[i].GetProjectPathFileLocation(j), typeof(Object));
							if(obj != null) { Selection.activeObject = obj; }
						}
					}
				}
			}
		}

		//	cycle through all directories and check to see if a directory has been clicked on
		for(int i = 0; i < _viewerDirectories.Count; ++i)
		{
			if(_mousePosition.HasValue)
			{
				if(_viewerDirectories[i].SelectionRect.Contains(_mousePosition.Value))
				{
					//	if we switch one to true lets reset all others to false
					for(int j = 0; j < _viewerDirectories.Count; ++j)
					{
						_viewerDirectories[j].IsSelected = false;
					}

					//	if it has we set it as selected and update the Selection.activeobject so its visible in the inspector
					Object obj = AssetDatabase.LoadAssetAtPath(_viewerDirectories[i].ProjectPathFolderLocation, typeof(Object));
					if(obj != null) { Selection.activeObject = obj; }
					_viewerDirectories[i].IsSelected = true;
				}
			}
		}
	}

	/// <summary>
	/// Unity function OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Unity recommends all Repaints to occur in this.
	/// </summary>
	public void OnInspectorUpdate() 
	{
		Repaint();
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// Inits the viewer window, and sets up all the AssetViewerDirectories to show.
	/// </summary>
	private void InitViewerWindow()
	{
		//	set up Divider rect 
		_isResizing = false;
		_currentViewWidth = this.position.width / 2;
		_dividerRect = new Rect(_currentViewWidth, 0, _dividerWidth, this.position.height);

		//	get all Files that are under the main Asset TO Bundle directory 
		DirectoryInfo directory = new DirectoryInfo(AssetBundleSettings.Instance.AssetsToBundleDirectory);
		DirectoryInfo [] dirs = directory.GetDirectories("*", SearchOption.AllDirectories);

		//	we always want the main directory expanded
		if(_expandedDirectories.Count == 0)
		{
			_expandedDirectories.Add(directory.FullName);
		}
		int baseIndentCount = directory.FullName.Split(Path.DirectorySeparatorChar).Length + 1;

		//	cycle through all directories and create the custom viewer directory
		//	only add a new directory to the list if it has been modified
		//	we check to see if its been modified based on its directory name
		for(int i = 0; i < dirs.Length; ++i)
		{
			AssetViewerDirectory viewerDirectory = new AssetViewerDirectory(dirs[i], baseIndentCount);

			//	if the name has changed it means we need to update it in the list
			int index = _viewerDirectories.FindIndex(o => o.ExpandedDirectoryName.Equals(viewerDirectory.ExpandedDirectoryName));
			if(index == -1)
			{
				_viewerDirectories.Add(viewerDirectory);
			}
			else
			{
				_viewerDirectories.RemoveAt(index);
				_viewerDirectories.Insert(index, viewerDirectory);
			}
		}

		//	sort based on the directory name 
		//	this will get all the directories in a correct visible order
		_viewerDirectories.Sort((x, y) =>
		{
			return x.ExpandedDirectoryName.CompareTo(y.ExpandedDirectoryName);
		});	
	}

	/// <summary>
	/// Renders the viewer directories, for the left side of the screen.
	/// </summary>
	private void RenderViewerDirectories()
	{
		_leftScroll = GUILayout.BeginScrollView(_leftScroll, false, false, GUILayout.Width(_currentViewWidth));

		if(_viewerDirectories != null)
		{
			//	render the label at the top, with a selection texture behind it
			//	for examples Assets->AssetsToBundle
			EditorGUILayout.LabelField(_directoryDisplayName, EditorStyles.boldLabel);
			if(_selectionTexture != null) GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _selectionTexture);

			//	cycle through all directories 
			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				EditorGUI.indentLevel = _viewerDirectories[i].IndentLevel;

				//	now we check all expanded directories if its expanded we show it!
				for(int j = 0; j < _expandedDirectories.Count; ++j)
				{
					if(_viewerDirectories[i].ExpandedParentName == _expandedDirectories[j])
					{
						GUILayout.BeginHorizontal();

						//	show a foldout if the directory has sub directories
						//	we add in extra spaces for the folder to be placed inbetween
						if(_viewerDirectories[i].SubDirectories.Length == 0)
						{
							EditorGUILayout.LabelField(string.Format("          {0}", _viewerDirectories[i].Directory.Name));
						}
						else
						{
							_viewerDirectories[i].IsExpanded = EditorGUILayout.Foldout(_viewerDirectories[i].IsExpanded,
								string.Format("      {0}", _viewerDirectories[i].Directory.Name));
						}

						Rect lastRect = GUILayoutUtility.GetLastRect();
						//	TODO look into a better way to set these rects
						if(Event.current.type == EventType.MouseDown) 
						{
							_viewerDirectories[i].SelectionRect = new Rect(0, lastRect.y, lastRect.width + lastRect.x, lastRect.height);
						}

						//	render the folder texture
						Rect folderRect = EditorGUI.IndentedRect(lastRect);
						float folderOffset = (folderRect.x - lastRect.x) + _folderXOffset;
						if(_folderTexture != null)	GUI.DrawTexture(new Rect(folderOffset, lastRect.y, 16, 16), _folderTexture);

						//	if its selected also draw the selection texture 
						if(_viewerDirectories[i].IsSelected && _selectionTexture != null)
						{
							GUI.DrawTexture(_viewerDirectories[i].SelectionRect, _selectionTexture);
						}

						GUILayout.EndHorizontal();
					}
				}
			}
		}

		GUILayout.EndScrollView();
	}

	/// <summary>
	/// Renders the folder view, for the right side of the screen.
	/// </summary>
	private void RenderFolderView()
	{
		_rightScroll = GUILayout.BeginScrollView(_rightScroll, false, false);

		//	we dont' care about indent on the right side
		EditorGUI.indentLevel = 0;

		//	cycle through and show all files and subdirectories of the currently selected viewerDirectory
		for(int i = 0; i < _viewerDirectories.Count; ++i)
		{
			if(_viewerDirectories[i].IsSelected == true)
			{
				//	render the label at the top, with a selection texture behind it
				//	for examples Assets->AssetsToBundle->Globals
				EditorGUILayout.LabelField(_viewerDirectories[i].ProjectPathDisplayName, EditorStyles.boldLabel);
				if(_selectionTexture != null) GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _selectionTexture);

				//	if there are either sub directories or files to show
				if(_viewerDirectories[i].SubDirectories.Length > 0 || _viewerDirectories[i].Files.Length > 0)
				{
					//	TODO don't recreate this list every frame
					_viewerDirectories[i].FileRects = new List<Rect>();

					//	show all sub directories first
					for(int j = 0; j < _viewerDirectories[i].SubDirectories.Length; ++j)
					{
						EditorGUILayout.LabelField(string.Format("     {0}", _viewerDirectories[i].SubDirectories[j].Name));
						Rect lastRect = GUILayoutUtility.GetLastRect();
						if(_folderTexture != null) GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, 16, 16), _folderTexture);

						_viewerDirectories[i].FileRects.Add(new Rect(lastRect.x + _currentViewWidth, lastRect.y, lastRect.width, lastRect.height));
					}


					//	show dem files!
					for(int j = 0; j < _viewerDirectories[i].Files.Length; ++j)
					{
						EditorGUILayout.LabelField(string.Format("     {0}", _viewerDirectories[i].Files[j].Name));
						Rect lastRect = GUILayoutUtility.GetLastRect();
						Texture tex = AssetDatabase.GetCachedIcon(_viewerDirectories[i].GetProjectPathFileLocation(j));
						if(tex != null) GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, 16, 16), tex);

						_viewerDirectories[i].FileRects.Add(new Rect(lastRect.x + _currentViewWidth, lastRect.y, lastRect.width, lastRect.height));
					}
				}
				else
				{
					EditorGUILayout.LabelField(_noFilesOrSubDir);
				}
			}
		}

		GUILayout.EndScrollView();
	}

	/// <summary>
	/// Renders the divider, based on how the screen is positioned and updates the split view display.
	/// </summary>
	private void RenderDivider()
	{
		if(Event.current.type == EventType.mouseDown && _dividerRect.Contains(Event.current.mousePosition))
		{
			_isResizing = true;
		}

		if(_isResizing)
		{
			_currentViewWidth = Event.current.mousePosition.x;
			_dividerRect.Set(_currentViewWidth, _dividerRect.y, _dividerRect.width, _dividerRect.height);
		}

		if(Event.current.type == EventType.MouseUp)
		{
			_isResizing = false;        
		}

		if(_dividerTexture != null) { GUI.DrawTexture(_dividerRect, _dividerTexture); }
		EditorGUIUtility.AddCursorRect(_dividerRect, MouseCursor.ResizeHorizontal);
	}

	/// <summary>
	/// Updates the viewer directories based on if they are expanded or not. 
	/// </summary>
	private void UpdateViewerDirectories()
	{
		//	we store out an updated variable to detect if we have to move folders / files
		//	only update the ViewerDirectory list if we had to perform a move and only do it once!
		bool updated = false;

		//	cycle through and see if any viewer directories have had their expanded status changed
		for(int i = 0; i < _viewerDirectories.Count; ++i)
		{
			//	we just opened something!
			if(_viewerDirectories[i].IsExpanded == true && !_expandedDirectories.Contains(_viewerDirectories[i].ExpandedDirectoryName))
			{
				_expandedDirectories.Add(_viewerDirectories[i].ExpandedDirectoryName);
				_openedDirectories.Add(_viewerDirectories[i].ExpandedDirectoryName);
			}
			//	we just closed something!
			else if(_viewerDirectories[i].IsExpanded == false && _expandedDirectories.Contains(_viewerDirectories[i].ExpandedDirectoryName))
			{
				_expandedDirectories.Remove(_viewerDirectories[i].ExpandedDirectoryName);
				_removedDirectories.Add(_viewerDirectories[i].ExpandedDirectoryName);
			}
		}

		//	cycle through all opened directories and set up the tilde!
		for(int j = _openedDirectories.Count - 1; j >= 0; j--)
		{
			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				if(_openedDirectories[j] == _viewerDirectories[i].ExpandedParentName)
				{
					//	found one to open!
					if(_viewerDirectories[i].Directory.FullName.Contains("~"))
					{
						string newPath = _viewerDirectories[i].Directory.FullName.Replace("~", string.Empty);
						bool exists = Directory.Exists(newPath);

						if(exists == false)
						{
							_viewerDirectories[i].Directory.MoveTo(newPath);
							updated = true;
						}
					}
				}
			}
		}
		_openedDirectories.Clear();

		//	we store out all recently removed directories
		//	if the directory was closed we then check to see if any directories share a parent
		//	if they share a parent we force the directory closed
		for(int j = _removedDirectories.Count - 1; j >= 0; j--)
		{
			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				//	found one to close!
				if(_removedDirectories[j] == _viewerDirectories[i].ExpandedParentName )
				{
					string newPath = _viewerDirectories[i].Directory.FullName + "~";
					bool exists = Directory.Exists(newPath);

					if(exists == false)
					{
						_viewerDirectories[i].Directory.MoveTo(newPath);
						updated = true;
					}
					_viewerDirectories[i].IsExpanded = false;
				}
			}
		}
		_removedDirectories.Clear();

		//	update all the ViewerDirectories!
		if(updated)
		{
			InitViewerWindow();
			updated = false;
		}
	}
}
