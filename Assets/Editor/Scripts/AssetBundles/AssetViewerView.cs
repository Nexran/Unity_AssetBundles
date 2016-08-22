using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace FrameworkEditor
{	
	public class AssetViewerView : SplitEditorView
	{
		private static string _windowName = "Asset Viewer";
		private static string _defaultFolderName = "Assets";
		private static char _smallRightArrowUnicode = '\u25B8';
		private static string _noFilesOrSubDir = "This folder is empty";
		private static string _folderDependencies = "Folder Dependencies:";
		private static string _none = "None";

		private static string _tildeFolderTextureName = "TildeFolderIcon.png";
		private static string _selectionTextureName = "Title.png";
		private static string _selectedTextureName = "SelectedTexture.png";
		private static string _warningTextureName = "Info.png";
		private static string _fileTextureName = "File.png";

		private static Texture _selectionTexture;
		private static Texture _selectedTexture;
		private static Texture _folderTexture;
		private static Texture _tildeFolderTexture;
		private static Texture _warningTexture;
		private static Texture _fileTexture;

		private List<AssetViewerDirectory> _viewerDirectories = new List<AssetViewerDirectory>();
		private List<string> _expandedDirectories = new List<string>();
		private List<string> _removedDirectories = new List<string>();
		private List<string> _openedDirectories = new List<string>();

		private Rect _selectionRect;
		private string _directoryDisplayName;

		private SearchBarView _searchBar;

		/// <summary>
		/// Shows the window.
		/// </summary>
		[MenuItem("Custom/Asset Viewer")]
		public static void ShowWindow()
		{
			GetWindow<AssetViewerView>(_windowName);
		}

		/// <summary>
		/// Unity function OnEnable, called when the window gets initialized. 
		/// </summary>
		internal override void OnEnable()
		{
			base.OnEnable();

			_searchBar = new SearchBarView();
			_searchBar.OnSearchChanged += SearchChanged;

			//	this searches in the default folder Editor Default Resources for the texture
			_selectionTexture = EditorGUIUtility.Load(_selectionTextureName) as Texture;
			_selectedTexture = EditorGUIUtility.Load(_selectedTextureName) as Texture;
			_tildeFolderTexture = EditorGUIUtility.Load(_tildeFolderTextureName) as Texture;
			_folderTexture = AssetDatabase.GetCachedIcon(_defaultFolderName) as Texture;
			_warningTexture = EditorGUIUtility.Load(_warningTextureName) as Texture;
			_fileTexture = EditorGUIUtility.Load(_fileTextureName) as Texture;
		}

		/// <summary>
		/// Unity function OnFocus, Called when the window gets keyboard focus.
		/// </summary>
		internal override void OnFocus()
		{
			SetSplitViewType(SplitType.HORIZONTAL);

			base.OnFocus();

			if(AssetBundleSettings.Instance == null || string.IsNullOrEmpty(AssetBundleSettings.Instance.AssetsToBundleDirectory))
				return;

			//	initialize the Directory Display Name
			_directoryDisplayName = AssetBundleSettings.Instance.AssetsToBundleDirectory;
			_directoryDisplayName = _directoryDisplayName.Replace(Path.DirectorySeparatorChar, _smallRightArrowUnicode);

			InitViewerWindow();
		}

		internal override void Update()
		{
			base.Update();

			_searchBar.Update();
		}

		/// <summary>
		/// Unity function OnGUI, might be called several times per frame (one call per event). 
		/// </summary>
		internal override void OnGUI()
		{
			base.OnGUI();

			if(AssetBundleSettings.Instance == null)
				return;

			_searchBar.Render();
		}

		internal override void RenderViewOne(float viewSize)
		{
			base.RenderViewOne(viewSize);

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
						_viewerDirectories[i].Render(_expandedDirectories[j], _selectionTexture, _folderTexture, _fileTexture);
					}
				}
			}

			//	reset the indent level
			EditorGUI.indentLevel = 0;
		}

		internal override void RenderViewTwo(float viewSize)
		{
			base.RenderViewTwo(viewSize);

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

							_viewerDirectories[i].AssetInfo[j].Render(_selectedTexture, _tildeFolderTexture, _warningTexture, viewSize, false, EditorStyles.label);
						}
					}
					else
					{
						GUILayout.Label(_noFilesOrSubDir);
					}
				}
				else if(_viewerDirectories[i].ShowAssetBundleDependencies == true)
				{
					//	render the label at the top, with a selection texture behind it
					//	for examples Assets->AssetsToBundle->Globals
					GUILayout.Label(_viewerDirectories[i].ProjectPathDisplayName, EditorStyles.boldLabel);
					if(_selectionTexture != null) GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _selectionTexture);

					GUILayout.Label(_folderDependencies, EditorStyles.boldLabel);

					if(_viewerDirectories[i].AssetViewerManifest != null)
					{
						for(int j = 0; j < _viewerDirectories[i].AssetViewerManifest.Dependencies.Count; ++j)
						{
							_viewerDirectories[i].AssetViewerManifest.Dependencies[j].Render(_selectedTexture, _tildeFolderTexture, _warningTexture, viewSize, true, EditorStyles.miniLabel);
						}
					}
					else
					{
						GUILayout.Label(_none, EditorStyles.miniLabel);
					}
				}
			}
		}

		internal override void HandleInputViewOne(Vector2 mousePosition, EventType eventType, int clickCount)
		{
			base.HandleInputViewOne(mousePosition, eventType, clickCount);

			//	cycle through all directories and check to see if a directory has been clicked on
			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				if(_viewerDirectories[i].SelectionRect.Contains(mousePosition))
				{
					//	if we switch one to true lets reset all others to false
					ResetViewDirectories();

					_searchBar.Clear();

					//	on double click attempt to show the asset bundle folder dependencies
					if(eventType == EventType.MouseDown && clickCount == 2)
					{
						_viewerDirectories[i].ShowAssetBundleDependencies = true;
					}
					else
					{
						_viewerDirectories[i].IsSelected = true;
					}
				}
			}
		}

		internal override void HandleInputViewTwo(Vector2 mousePosition, EventType eventType, int clickCount)
		{
			base.HandleInputViewTwo(mousePosition, eventType, clickCount);

			//	bool switchDirectory = false;
			string folderName = string.Empty;

			for(int i = 0; i < _viewerDirectories.Count; ++i)
			{
				switch(_viewerDirectories[i].CheckMousePress(eventType, clickCount, mousePosition, out folderName))
				{
				case AssetViewerInfo.ClickType.CLICK:
					{
						//	if we switch one to true lets reset all others sub assets to false
						for(int a = 0; a < _viewerDirectories.Count; ++a)
						{
							for(int b = 0; b < _viewerDirectories[a].AssetInfo.Count; ++b)
							{
								if(_viewerDirectories[a].AssetInfo[b].FolderName != folderName)
								{
									_viewerDirectories[a].AssetInfo[b].IsSelected = false;
								}
							}
						}
					}
					break;

				case AssetViewerInfo.ClickType.EXPAND:
					{
						//	reset all serach / selected non-sense
						ResetViewDirectories();

						_searchBar.Clear();

						//	go through and select the correct directory
						for(int a = 0; a < _viewerDirectories.Count; ++a)
						{
							if(_viewerDirectories[i].DependencyDirectories.Contains(_viewerDirectories[a].ExpandedDirectoryName))
							{
								_viewerDirectories[a].IsExpanded = true;
							}

							if(_viewerDirectories[a].ExpandedDirectoryName == folderName)
							{
								_viewerDirectories[a].IsSelected = true;
								break;
							}
						}
					}
					break;
				}
			}
		}

		/// <summary>
		/// Inits the viewer window, and sets up all the AssetViewerDirectories to show.
		/// </summary>
		private void InitViewerWindow()
		{
			//	get all Files that are under the main Asset TO Bundle directory 
			DirectoryInfo directory = new DirectoryInfo(AssetBundleSettings.Instance.AssetsToBundleDirectory);
			DirectoryInfo [] dirs = directory.GetDirectories("*", SearchOption.AllDirectories);

			//	we always want the main directory expanded
			if(_expandedDirectories.Count == 0)
			{
				_expandedDirectories.Add(directory.FullName);
			}

			//	cycle through all directories and create the custom viewer directory
			//	only add a new directory to the list if it has been modified
			//	we check to see if its been modified based on its directory name
			for(int i = 0; i < dirs.Length; ++i)
			{
				AssetViewerDirectory viewerDirectory = new AssetViewerDirectory(dirs[i].FullName);
				viewerDirectory.Init();

				//	if the name has changed it means we need to update it in the list
				int index = _viewerDirectories.FindIndex(o => o.ExpandedDirectoryName.Equals(viewerDirectory.ExpandedDirectoryName));
				if(index == -1)
				{
					_viewerDirectories.Add(viewerDirectory);
				}
				else
				{
					viewerDirectory.CloneFlags(_viewerDirectories[index]);
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

		private void SearchChanged()
		{
			ResetViewDirectories();
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
				_viewerDirectories[i].ShowAssetBundleDependencies = false;
				for(int j = 0; j < _viewerDirectories[i].AssetInfo.Count; ++j)
				{
					_viewerDirectories[i].AssetInfo[j].IsSearched = false;
					_viewerDirectories[i].AssetInfo[j].IsSelected = false;
				}
			}
		}

		/// <summary>
		/// Update method for GUI, when the Event is either Repaint or Layout update so it doesn't change while calculating what to render.
		/// Use this for updating logic that may change visuals.
		/// </summary>
		internal override void RepaintLayoutGUI()
		{
			base.RepaintLayoutGUI();

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
			for(int j = 0; j < _openedDirectories.Count; ++j)				
			{
				for(int i = 0; i < _viewerDirectories.Count; ++i)
				{
					if(_openedDirectories[j] == _viewerDirectories[i].ExpandedParentName)
					{
						_viewerDirectories[i].OpenDirectory();
					}
				}
				//	re-init every directory if multiple directories just opened
				InitViewerWindow();
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
					if(_removedDirectories[j] == _viewerDirectories[i].ExpandedParentName)
					{
						if(_viewerDirectories[i].CloseDirectory())
						{
							updated = true;
						}
					}
				}
			}
			_removedDirectories.Clear();

			//	we make sure every thing is lower case to ignore any case sensitivity issues
			string toLowerSearch = _searchBar.SearchText.ToLower();

			if(!string.IsNullOrEmpty(toLowerSearch))
			{
				bool resetFocusControl = false;

				for(int i = 0; i < _viewerDirectories.Count; ++i)
				{
					for(int j = 0; j < _viewerDirectories[i].AssetInfo.Count; ++j)
					{
						string directoryName = _viewerDirectories[i].AssetInfo[j].FileSystemInfo.Name.ToLower();

						if(directoryName.Contains(toLowerSearch))
						{
							_viewerDirectories[i].IsSearched = true;
							_viewerDirectories[i].AssetInfo[j].IsSearched = true;
							resetFocusControl = true;
						}
					}
				}

				if(resetFocusControl) 
				{
					GUI.FocusControl(_searchBar.ControlName);
				}
			}

			//	update all the ViewerDirectories!
			if(updated)
			{
				InitViewerWindow();
				updated = false;
			}
		}
	}
}