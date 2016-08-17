using UnityEngine;
using UnityEditor;

namespace FrameworkEditor
{	
	public class BaseEditorView : EditorWindow
	{
		/// <summary>
		/// Unity function OnEnable, called when the window gets initialized. 
		/// </summary>
		internal virtual void OnEnable()
		{
		}

		/// <summary>
		/// Unity function OnFocus, Called when the window gets keyboard focus.
		/// </summary>
		internal virtual void OnFocus()
		{
		}

		/// <summary>
		/// Unity function OnGUI, might be called several times per frame (one call per event). 
		/// </summary>
		internal virtual void OnGUI()
		{
			if(Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
			{
				RepaintLayoutGUI();
			}
		}
			
		/// <summary>
		/// Unity function OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
		/// Unity recommends all Repaints to occur in this.
		/// </summary>
		internal virtual void OnInspectorUpdate() 
		{
			Repaint();
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Update method for GUI, when the Event is either Repaint or Layout update so it doesn't change while calculating what to render.
		/// Use this for updating logic that may change visuals.
		/// </summary>
		internal virtual void RepaintLayoutGUI()
		{

		}
	}
}