using UnityEngine;
using UnityEditor;

namespace FrameworkEditor
{	
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

		protected float _currentViewWidth;
		protected EventType _eventType;
		protected SplitType _splitType;

		/// <summary>
		/// Unity function OnEnable, called when the window gets initialized. 
		/// </summary>
		internal override void OnEnable()
		{
			base.OnEnable();

			_dividerTexture = EditorGUIUtility.Load(_dividerTextureName) as Texture;
		}

		/// <summary>
		/// Unity function OnFocus, Called when the window gets keyboard focus.
		/// </summary>
		internal override void OnFocus()
		{
			base.OnFocus();

			_isResizing = false;

			if(_splitType == SplitType.VERTICAL)
			{
				_currentViewWidth = this.position.height / 2;
				_dividerRect = new Rect(0, _currentViewWidth, this.position.width, _dividerSize);
			}
			else
			{
				_currentViewWidth = this.position.width / 2;
				_dividerRect = new Rect(_currentViewWidth, 0, _dividerSize, this.position.height);
			}
		}

		/// <summary>
		/// Unity function OnGUI, might be called several times per frame (one call per event). 
		/// </summary>
		internal override void OnGUI()
		{
			base.OnGUI();

			//	calculate mouse position before any other part due to other logic potentially 
			//	altering the mouse position and setting the Event.current.type to USED instead of Mouse Down
			//	mouse position being null means the mouse wasn't pressed down
			Vector2? oneMousePosition = null;
			Vector2? twoMousePosition = null;
			//	store out the mouseDown event because other events can alter the type and switch it to 
			//	used instead of the proper EventType
			_eventType = Event.current.type;
			if(_eventType == EventType.MouseDown)
			{
				//	adjust for if the user has scrolled the view down the page
				oneMousePosition = Event.current.mousePosition + _viewOneScroll;
				twoMousePosition = Event.current.mousePosition + _viewTwoScroll;
			}

			if(_splitType == SplitType.VERTICAL)
			{
				GUILayout.BeginVertical();
			}
			else
			{
				GUILayout.BeginHorizontal();
			}

			//	view One, set the width otherwise it will change based on current selection
			if(_splitType == SplitType.VERTICAL)
			{				
				_viewOneScroll = GUILayout.BeginScrollView(_viewOneScroll, false, false, GUILayout.Height(_currentViewWidth));
			}
			else
			{
				_viewOneScroll = GUILayout.BeginScrollView(_viewOneScroll, false, false, GUILayout.Width(_currentViewWidth));
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


			//	check to see if the left side of the screen has a mouse press 
			if(oneMousePosition.HasValue)
			{
				HandleInputViewOne(oneMousePosition.Value);
			}

			//	check to see if the right side of the screen has a mouse press 
			if(twoMousePosition.HasValue)
			{
				HandleInputViewTwo(twoMousePosition.Value);
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
		/// Renders the divider, based on how the screen is positioned and updates the split view display.
		/// </summary>
		private void RenderDivider()
		{
			if(_eventType == EventType.mouseDown && _dividerRect.Contains(Event.current.mousePosition))
			{
				_isResizing = true;
			}

			if(_isResizing)
			{
				if(_splitType == SplitType.VERTICAL)
				{
					_currentViewWidth = Event.current.mousePosition.y;
					_dividerRect.Set(_dividerRect.x, _currentViewWidth, _dividerRect.width, _dividerRect.height);
				}
				else
				{
					_currentViewWidth = Event.current.mousePosition.x;
					_dividerRect.Set(_currentViewWidth, _dividerRect.y, _dividerRect.width, _dividerRect.height);
				}
			}

			if(_eventType == EventType.MouseUp)
			{
				_isResizing = false;        
			}

			MouseCursor mouseCursor = (_splitType == SplitType.VERTICAL) ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal;
			if(_dividerTexture != null) { GUI.DrawTexture(_dividerRect, _dividerTexture); }
			EditorGUIUtility.AddCursorRect(_dividerRect, mouseCursor);
		}
	}
}