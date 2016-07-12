using UnityEditor;
using UnityEngine;

/// <summary>
/// Variant drawer, used to show the variant in the inspector in a more friendly manner.
/// </summary>
[CustomPropertyDrawer(typeof(Variant))]
public class VariantDrawer : PropertyDrawer 
{	
	/// <summary>
	/// Draw the property inside the given rect
	/// </summary>
	/// <param name="position">Position.</param>
	/// <param name="property">Property.</param>
	/// <param name="label">Label.</param>
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
	{
		//	find the properties 
		SerializedProperty name = property.FindPropertyRelative("_name");
		SerializedProperty mipMaps = property.FindPropertyRelative("_mipMaps");
		SerializedProperty textureCompression = property.FindPropertyRelative("_textureCompression");

		EditorGUI.BeginProperty(position, label, property);

		// Draw label
		position = EditorGUI.PrefixLabel(position, label);

		EditorGUI.PropertyField(new Rect(position.x - 110, position.y + 20, position.width + 110, 20), name);
		EditorGUI.PropertyField(new Rect(position.x - 110, position.y + 40, position.width + 110, 20), mipMaps);
		EditorGUI.PropertyField(new Rect(position.x - 110, position.y + 60, position.width + 110, 20), textureCompression);

		EditorGUI.EndProperty();
	}

	/// <summary>
	/// Gets the height of the property, used to determine how much space in the inspector this will take up.
	/// </summary>
	/// <returns>The property height.</returns>
	/// <param name="property">Property.</param>
	/// <param name="label">Label.</param>
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return 80f;
	}
}