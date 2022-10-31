using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UGUI个sb居然不支持通过key读取sprite
/// </summary>
public class qfSpriteLoader : MonoBehaviour
{
	private static qfSpriteLoader _instance;
	public static qfSpriteLoader Instance{get { return _instance; }}

	public Sprite[] AllSprites; 

	private Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

	void Start()
	{
		if(AllSprites != null)
		{
			for (int i = 0; i < AllSprites.Length; i++)
			{
				if (_cache.ContainsKey(AllSprites[i].name))
				{
					Debug.LogError(string.Format("duplicate key {0} in all sprites", AllSprites[i].name));
					continue;
				}

				_cache.Add(AllSprites[i].name, AllSprites[i]);

				AllSprites[i] = null;
			}
		}
	}

	void Awake()
	{
		_instance = this;
	}

	public Sprite LoadSprite(string key)
	{
		if (!_cache.ContainsKey(key))
		{
			Debug.LogError(string.Format("fail to find sprite with key {0}", key));
			return null;
		}

		return _cache[key];
	}
}
