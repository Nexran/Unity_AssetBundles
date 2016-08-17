using UnityEngine;
using UnityEditor;

namespace FrameworkEditor
{	
	/// <summary>
	/// Split editor view, used for windows that need to display information on two different parts of a window.
	/// Shows split either HORIZONTAL or VERTICAL with a divider that the user can position.
	/// </summary>
	public class SplitEditorView : BaseEditorView
	{
		protected enum SplitType
		{
			HORIZONTAL,
			VERTICAL
		}

		private static float _dividerSize = 2f;
		private static string _dividerTextureName = "Title.png";
		private static Texture _dividerTexture;

		private Vector2 _viewOneScroll;
		private Vector2 _viewTwoScroll;

		private bool _isResizing;
		private Rect _dividerRect;
		private Rect _windowRect;

		protected EventType _eventType;
		protected Vector2 _mousePosition;
		protected int _clickCount;

		protected float _currentViewSize;
		protected SplitType _splitType;

		/// <summary>
		/// Unity function OnEnable, called when the window gets initialized. 
		/// </summary>
		internal override void OnEnable()
		{
			base.OnEnable();

			_dividerTexture = EditorGUIUtility.Load(_dividerTextureName) as Texture;
			_windowRect = this.position;
		}
			
		/// <summary>
		/// Unity function OnGUI, might be called several times per frame (one call per event). 
		/// </summary>
		internal override void OnGUI()
		{
			base.OnGUI();

			//	EVENT data is only available in OnGUI store out event data
			//	used in Update methods
			_eventType = Event.current.type;
			_mousePosition = Event.current.mousePosition;
			_clickCount = Event.current.clickCount;

			if(_splitType == SplitType.VERTICAL)
			{
				GUILayout.BeginVertical();
			}
			else
			{
				GUILayout.BeginHorizontal();
			}

			//	view One, set the WIDTH|HEIGHT otherwise it will change based on current selection
			if(_splitType == SplitType.VERTICAL)
			{				
				_viewOneScroll = GUILayout.BeginScrollView(_viewOneScroll, false, false, GUILayout.Height(_currentViewSize));
			}
			else
			{
				_viewOneScroll = GUILayout.BeginScrollView(_viewOneScroll, false, false, GUILayout.Width(_currentViewSize));
			}
			RenderViewOne();
			GUILayout.EndScrollView();

			RenderDivider();

			//	view Two
			_viewTwoScroll = GUILayout.BeginScrollView(_viewTwoScroll);
			RenderViewTwo();
			GUILayout.EndScrollView();

			if(_splitType == SplitType.VERTICAL)
			{
				GUILayout.EndVertical();
			}
			else
			{
				GUILayout.EndHorizontal();
			}
		}

		/// <summary>
		/// Unity function Update.
		/// </summary>				
		internal override void Update() 
		{
			base.Update();

			if(_eventType == EventType.MouseDown)
			{
				HandleInputViewOne(_mousePosition + _viewOneScroll);
				HandleInputViewTwo(_mousePosition + _viewTwoScroll);
			}
		}

		/// <summary>
		/// Unity function OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
		/// Unity recommends all Repaints to occur in this.
		/// </summary>
		internal override void OnInspectorUpdate() 
		{
			base.OnInspectorUpdate();
				
			UpdateDivider();

			if(this.position != _windowRect)
			{
				WindowRectChanged();
			}
		}

		internal virtual void RenderViewOne()
		{

		}

		internal virtual void RenderViewTwo()
		{

		}

		internal virtual void HandleInputViewOne(Vector2 mousePosition)
		{

		}

		internal virtual void HandleInputViewTwo(Vector2 mousePosition)
		{

		}

		/// <summary>
		/// Called when the size of the window changes. 
		/// </summary>
		private void WindowRectChanged()
		{
			InitDivider();
		}

		/// <summary>
		/// Inits the divider, sets up all base variables for the divider.
		/// </summary>
		private void InitDivider()
		{
			_windowRect = this.position;
			_isResizing = false;

			if(_splitType == SplitType.VERTICAL)
			{
				_currentViewSize = _windowRect.height / 2;
				_dividerRect = new Rect(0, _currentViewSize, _windowRect.width, _dividerSize);
			}
			else
			{
				_currentViewSize = _windowRect.width / 2;
				_dividerRect = new Rect(_currentViewSize, 0, _dividerSize, _windowRect.height);
			}
		}

		/// <summary>
		/// Renders the divider, based on how the screen is positioned.
		/// </summary>
		private void RenderDivider()
		{
			if(_dividerTexture != null) { GUI.DrawTexture(_dividerRect, _dividerTexture); }
			EditorGUIUtility.AddCursorRect(_dividerRect, (_splitType == SplitType.VERTICAL) ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal);
		}

		/// <summary>
		/// Updates the divider, based on mouse and sets the new divider rect based on user input.
		/// </summary>
		private void UpdateDivider()
		{
			if(_eventType == EventType.MouseDown && _dividerRect.Contains(_mousePosition))
			{
				_isResizing = true;
			}
			else if(_eventType == EventType.MouseUp)
			{
				_isResizing = false;
			}

			if(_isResizing)
			{
				if(_splitType == SplitType.VERTICAL)
				{
					_currentViewSize = _mousePosition.y;
					_dividerRect.Set(_dividerRect.x, _currentViewSize, _dividerRect.width, _dividerRect.height);
				}
				else
				{
					_currentViewSize = _mousePosition.x;
					_dividerRect.Set(_currentViewSize, _dividerRect.y, _dividerRect.width, _dividerRect.height);
				}
			}

		}
	}
}