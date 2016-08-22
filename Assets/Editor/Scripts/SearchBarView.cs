using UnityEngine;
using System;

public class SearchBarView
{
	private static string _controlName = "SearchToolBarField";
	private static string _toolbarStyle = "Toolbar";
	private static string _searchTextFieldStyle = "ToolbarSeachTextField";
	private static string _searchCancelButtonStyle = "ToolbarSeachCancelButton";

	private string _searchText;
	private string _textFieldSearchText;

	public Action OnSearchChanged;

	public string SearchText
	{
		get { return _searchText; }
	}

	public string ControlName
	{
		get { return _controlName; }
	}

	public SearchBarView()
	{
		_searchText = string.Empty;
		_textFieldSearchText = string.Empty;
	}

	public void Update()
	{
		if(_textFieldSearchText != _searchText)
		{
			_searchText = _textFieldSearchText;

			if(OnSearchChanged != null)
			{
				OnSearchChanged();
			}
		}
	}

	public void Render()
	{
		//	mimic Unitys search bar as close as possible
		GUILayout.BeginHorizontal(GUI.skin.FindStyle(_toolbarStyle));
		GUI.SetNextControlName(_controlName);
		_textFieldSearchText = GUILayout.TextField(_textFieldSearchText, GUI.skin.FindStyle(_searchTextFieldStyle));
		if(GUILayout.Button(string.Empty, GUI.skin.FindStyle(_searchCancelButtonStyle)))
		{
			_textFieldSearchText = string.Empty;
			GUI.FocusControl(null);
		}
		GUILayout.EndHorizontal();
	}

	public void Clear()
	{
		_textFieldSearchText = string.Empty;
	}
}