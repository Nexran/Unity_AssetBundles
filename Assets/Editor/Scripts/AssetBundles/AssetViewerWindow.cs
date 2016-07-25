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
	private static string _defaultFolderName = "Assets";
	private static float _dividerWidth = 2f;
	private static char _smallRightArrowUnicode = '\u25B8';
	private static float _folderXOffset = 17f;
	private static string _noFilesOrSubDir = "This folder is empty";

	private static string _tildeFolderTextureName = "TildeFolderIcon.png";
	private static string _selectionTextureName = "Title.png";
	private static string _selectedTextureName = "SelectedTexture.png";

	private static Texture _selectionTexture;
	private static Texture _selectedTexture;
	private static Texture _dividerTexture;
	private static Texture _folderTexture;
	private static Texture _tildeFolderTexture;

	private List<AssetViewerDirectory> _viewerDirectories = new List<AssetViewerDirectory>();
	private List<string> _expandedDirectories = new List<string>();
	private List<string> _removedDirectories = new List<string>();
	private List<string> _openedDirectories = new List<string>();

	private Vector2 _leftScroll;
	private Vector2 _rightScroll;

	private float _currentViewWidth;
	private bool _isResizing;
	private Rect _dividerRect;

	private Rect _selectionRect;
	private string _directoryDisplayName;

	private string _searchText;
	private string _currentSearchText;
	private bool _searchTextChanged;
	private bool _clearSearchText;

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
		_selectedTexture = EditorGUIUtility.Load(_selectedTextureName) as Texture;
		_tildeFolderTexture = EditorGUIUtility.Load(_tildeFolderTextureName) as Texture;
		_folderTexture = AssetDatabase.GetCachedIcon(_defaultFolderName) as Texture;
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
		Vector2? leftMousePosition = null;
		Vector2? rightMousePosition = null;
		if(Event.current.type == EventType.MouseDown)
		{
			//	adjust for if the user has scrolled the view down the page
			leftMousePosition = Event.current.mousePosition + _leftScroll;
			rightMousePosition = Event.current.mousePosition + _rightScroll;
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

		RenderSearchArea();

		//	check to see if the left side of the screen has a mouse press 
		//	and select / highlight the correct path
		if(leftMousePosition.HasValue)
		{
			//	cycle through all directories and check to see if a directory has been clicked on
			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				if(_viewerDirectories[i].SelectionRect.Contains(leftMousePosition.Value))
				{
					//	if we switch one to true lets reset all others to false
					ResetViewDirectories();

					_clearSearchText = true;
					_currentSearchText = string.Empty;
					_viewerDirectories[i].IsSelected = true;
				}
			}
		}

		//	check to see if the right side of the screen has a mouse press 
		//	and select / highlight the correct path
		if(rightMousePosition.HasValue)
		{
			bool switchDirectory = false;

			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				if(_viewerDirectories[i].IsSelected == false && _viewerDirectories[i].IsSearched == false)
					continue;

				for(int j = 0; j < _viewerDirectories[i].AssetInfo.Count; ++j)
				{				
					//	if this is toggled we have just switche directories
					//	we do not care about displaying anymore directories
					//	just exit out
					if(switchDirectory == true)
						return;	
					
					if(_viewerDirectories[i].AssetInfo[j].SelectionRect.Contains(rightMousePosition.Value))
					{
						//	if we switch one to true lets reset all others sub assets to false
						for(int a = 0; a < _viewerDirectories.Count; ++a)
						{
							for(int b = 0; b < _viewerDirectories[a].AssetInfo.Count; ++b)
							{
								_viewerDirectories[a].AssetInfo[b].IsSelected = false;
							}
						}

						//	if it has we set it as selected and update the Selection.activeobject so its visible in the inspector
						Object obj = AssetDatabase.LoadAssetAtPath(_viewerDirectories[i].GetProjectPathFileLocation(j), typeof(Object));
						if(obj != null) { Selection.activeObject = obj; }

						//	 a user double clicks on the folder 
						//	select the folder and expand the folder to mimic project folder hierarchy
						if(Event.current.type == EventType.MouseDown && Event.current.clickCount == 2)
						{
							//	reset all serach / selected non-sense
							ResetViewDirectories();

							//	if there is no file extension which means its a folder expand it! 
							if(string.IsNullOrEmpty(_viewerDirectories[i].AssetInfo[j].FileSystemInfo.Extension))
							{
								//	clear out the search field
								_clearSearchText = true;
								_currentSearchText = string.Empty;
								switchDirectory = true;

								_viewerDirectories[i].IsSelected = false;
								_viewerDirectories[i].IsExpanded = true;

								//	go through and select the correct directory
								for(int a = 0; a < _viewerDirectories.Count; ++a)
								{
									if(_viewerDirectories[a].ExpandedDirectoryName == _viewerDirectories[i].AssetInfo[j].FileSystemName)
									{
										_viewerDirectories[a].IsSelected = true;
										break;
									}
								}
							}
							else
							{
								_viewerDirectories[i].AssetInfo[j].IsSelected = true;
								if(obj != null) { AssetDatabase.OpenAsset(obj); }
							}
						}
						else
						{
							_viewerDirectories[i].AssetInfo[j].IsSelected = true;
						}
					}
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
				viewerDirectory.Clone(_viewerDirectories[index]);
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

						//	if its selected also draw the selection texture
						//	draw this first so the label/ foldout / image are rendered over
						if(_viewerDirectories[i].IsSelected && _selectedTexture != null)
						{
							GUI.DrawTexture(_viewerDirectories[i].SelectionRect, _selectedTexture);
						}

						//	show a foldout if the directory has sub directories
						//	we add in extra spaces for the folder to be placed inbetween
						if(_viewerDirectories[i].ContainsSubDirectories)
						{
							_viewerDirectories[i].IsExpanded = EditorGUILayout.Foldout(_viewerDirectories[i].IsExpanded,
								string.Format("      {0}", _viewerDirectories[i].Directory.Name));
						}
						else
						{
							EditorGUILayout.LabelField(string.Format("          {0}", _viewerDirectories[i].Directory.Name));
						}

						Rect lastRect = GUILayoutUtility.GetLastRect();
						//	for X reason the rect Width will sometimes give us default values of 0, 0, 1, 1
						//	we check to make sure its a valid size, anything larger than default 
						if(lastRect.width > 1f)
						{
							_viewerDirectories[i].SelectionRect = new Rect(0, lastRect.y, lastRect.width + lastRect.x, lastRect.height);
						}

						//	render the folder texture
						Rect folderRect = EditorGUI.IndentedRect(lastRect);
						float folderOffset = (folderRect.x - lastRect.x) + _folderXOffset;
						Texture tex = AssetDatabase.GetCachedIcon(_viewerDirectories[i].ProjectPathFolderLocation);
						if(tex != null) 
						{
							GUI.DrawTexture(new Rect(folderOffset, lastRect.y, 16, 16), tex);
						}
						else if(_folderTexture != null)
						{
							GUI.DrawTexture(new Rect(folderOffset, lastRect.y, 16, 16), _folderTexture);
						}

						GUILayout.EndHorizontal();
					}
				}
			}
		}

		GUILayout.EndScrollView();

		//	reset the indent level
		EditorGUI.indentLevel = 0;
	}

	/// <summary>
	/// Renders the folder view, for the right side of the screen.
	/// </summary>
	private void RenderFolderView()
	{
		_rightScroll = GUILayout.BeginScrollView(_rightScroll, false, false);

		//	cycle through and show all files and subdirectories of the currently selected viewerDirectory
		for(int i = 0; i < _viewerDirectories.Count; ++i)
		{
			if(_viewerDirectories[i].IsSelected == true || _viewerDirectories[i].IsSearched)
			{
				//	render the label at the top, with a selection texture behind it
				//	for examples Assets->AssetsToBundle->Globals
				GUILayout.Label(_viewerDirectories[i].ProjectPathDisplayName, EditorStyles.boldLabel);
				if(_selectionTexture != null) GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _selectionTexture);

				//	if there are either sub directories or files to show
				if(_viewerDirectories[i].AssetInfo != null && _viewerDirectories[i].AssetInfo.Count > 0)
				{
					for(int j = 0; j < _viewerDirectories[i].AssetInfo.Count; ++j)
					{
						//	if we are displaying search results check to see if this current asset is being searched
						//	if not continue looking;
						if(_viewerDirectories[i].IsSearched == true && _viewerDirectories[i].AssetInfo[j].IsSearched == false)
							continue;
						
						//	if its selected also draw the selection texture 
						if(_viewerDirectories[i].AssetInfo[j].IsSelected && _selectedTexture != null)
						{
							GUI.DrawTexture(_viewerDirectories[i].AssetInfo[j].DrawRect, _selectedTexture);
						}

						GUILayout.Label(string.Format("     {0}", _viewerDirectories[i].AssetInfo[j].FileSystemInfo.Name));
						Rect lastRect = GUILayoutUtility.GetLastRect();
						Texture tex = AssetDatabase.GetCachedIcon(_viewerDirectories[i].GetProjectPathFileLocation(j));
						if(tex != null) 
						{
							GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, 16, 16), tex);
						}
						else if(_tildeFolderTexture != null)
						{
							GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, 16, 16), _tildeFolderTexture);
						}

						//	for X reason the rect Width will sometimes give us default values of 0, 0, 1, 1
						//	we check to make sure its a valid size, anything larger than default 
						if(lastRect.width > 1f)
						{
							_viewerDirectories[i].AssetInfo[j].SelectionRect = new Rect(lastRect.x + _currentViewWidth, lastRect.y, lastRect.width, lastRect.height);
							_viewerDirectories[i].AssetInfo[j].DrawRect = lastRect;
						}
					}
				}
				else
				{
					GUILayout.Label(_noFilesOrSubDir);
				}
			}
		}

		GUILayout.EndScrollView();
	}

	/// <summary>
	/// Renders the search area, and handles initial logic to switch directories to searched.
	/// </summary>
	private void RenderSearchArea()
	{
		_searchTextChanged = false;

		//	mimic Unitys search bar as close as possible
		GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
		GUI.SetNextControlName("SearchToolBarField");
		_currentSearchText = GUILayout.TextField(_currentSearchText, GUI.skin.FindStyle("ToolbarSeachTextField"));
		if(GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
		{
			_currentSearchText = string.Empty;
			GUI.FocusControl(null);
		}
		GUILayout.EndHorizontal();

		//	check to see if the search text has changed
		//	if it has reset all search results
		if(_currentSearchText != _searchText && _clearSearchText == false)
		{
			_searchTextChanged = true;
			ResetViewDirectories();
		}

		_searchText = _currentSearchText;
		_clearSearchText = false;

		//	check to see if we found any search results
		if(string.IsNullOrEmpty(_currentSearchText) == false && _searchTextChanged == true)
		{
			//	we make sure every thing is lower case to ignore any case sensitivity issues
			string toLowerSearch = _currentSearchText.ToLower();

			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				for(int j = 0; j < _viewerDirectories[i].AssetInfo.Count; ++j)
				{
					string directoryName = _viewerDirectories[i].AssetInfo[j].FileSystemInfo.Name.ToLower();

					if(directoryName.Contains(toLowerSearch))
					{
						_viewerDirectories[i].IsSearched = true;
						_viewerDirectories[i].AssetInfo[j].IsSearched = true;
					}
				}
			}
		}
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
	/// Resets the view directories, search and selected status to false.
	/// </summary>
	private void ResetViewDirectories()
	{
		for(int i = 0; i < _viewerDirectories.Count; ++i)
		{
			_viewerDirectories[i].IsSearched = false;
			_viewerDirectories[i].IsSelected = false;
			for(int j = 0; j < _viewerDirectories[i].AssetInfo.Count; ++j)
			{
				_viewerDirectories[i].AssetInfo[j].IsSearched = false;
				_viewerDirectories[i].AssetInfo[j].IsSelected = false;
			}
		}
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
					//	try to destroy and meta files that exist
					//	meta files are used to show folders in Unity the tilde property ensures meta files are NOT created
					//	if we did not destroy all meta files here if you close a folder at the top of a directory all the meta files
					//	will not get destroyed which will cause errors to pop up in the editor
					FileInfo metaFile = new FileInfo(_viewerDirectories[i].Directory.FullName + ".meta");
					if(metaFile != null && metaFile.Exists == true)
					{
						metaFile.Delete();
					}

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
