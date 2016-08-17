using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace FrameworkEditor
{	
	public class SampleSplitEditorView : SplitEditorView
	{
		/// <summary>
		/// Shows the window.
		/// </summary>
		[MenuItem("Custom/Help Viewer")]
		public static void ShowWindow()
		{
			GetWindow<SampleSplitEditorView>("Help");
		}
	}
}