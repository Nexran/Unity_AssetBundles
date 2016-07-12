using System;
using UnityEngine;

/// <summary>
/// Variant data used to configure unique auto generated variant types.
/// </summary>
[Serializable]
public class Variant 
{
	public enum TextureCompressionType
	{
		NONE,
		HALF
	}

	[SerializeField]
	private string _name;

	[SerializeField]
	private TextureCompressionType _textureCompression;

	[SerializeField]
	private bool _mipMaps;

	/// <summary>
	/// Gets the name of the variant folder.
	/// </summary>
	/// <value>The name.</value>
	public string Name
	{
		get { return _name; }
		private set { _name = value; }
	}

	/// <summary>
	/// Gets the texture compression on this variant type.
	/// </summary>
	/// <value>The texture compression.</value>
	public TextureCompressionType TextureCompression
	{
		get { return _textureCompression; }
		private set { _textureCompression = value; }
	}

	/// <summary>
	/// Gets a value indicating whether this variant has mip maps enabled. 
	/// </summary>
	/// <value><c>true</c> if mip maps; otherwise, <c>false</c>.</value>
	public bool MipMaps
	{
		get { return _mipMaps; }
		private set { _mipMaps = value; }
	}

	/// <summary>
	/// Gets a value indicating whether this instance is valid ONLY based on NAME.
	/// </summary>
	/// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
	public bool IsValid
	{
		get { return !string.IsNullOrEmpty(_name); }
	}
}