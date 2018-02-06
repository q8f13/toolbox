using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// UE4场景导出纯文本导入unity的工具面板
/// </summary>
public class UE4SceneDataImporter : EditorWindow
{
	private TextAsset _dataFile ;

	private char lineSeperater = '\n'; // It defines line seperate character
	private char fieldSeperator = ','; // It defines field seperate chracter

	private UE4LevelUnitData[] _dataParsed;

	private GameObject[] _assetFound;

	private string[] _folderFilter;

	private Vector2 _scrollPos;
	[MenuItem("Custom/UE4 Scene Data Import")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof (UE4SceneDataImporter));
	}

	void OnGUI()
	{
		_dataFile = EditorGUILayout.ObjectField("DataFile: ", _dataFile, typeof (TextAsset), false) as TextAsset;

//		_obj = EditorGUILayout.ObjectField("Test: ", _obj, typeof (Object), false);

		if (_dataFile == null)
			return;

		GUILayout.BeginVertical();
		if (GUILayout.Button("解析"))
		{
			_assetFound = null;
			DoParse();
		}

		EditorGUILayout.Separator();

		if(_folderFilter == null)
			_folderFilter = new string[1];

		if (GUILayout.Button("指定文件夹："))
		{
//			EditorUtility.fol
			_folderFilter[0] = EditorUtility.OpenFolderPanel("请指定所用到的替换资源所在文件夹", _folderFilter[0],null);
		}
//		_folderFilter[0] = EditorGUILayout.ObjectField("Folder: ",)

		EditorGUILayout.Separator();
		bool folderSpecified = ! string.IsNullOrEmpty(_folderFilter[0]);
		if(folderSpecified)
		{
			GUILayout.Label(string.Format("指定文件夹：{0}", _folderFilter[0]), new GUIStyle("HelpBox"));
		}
		EditorGUILayout.Separator();

		if (folderSpecified && _dataParsed != null && _dataParsed.Length > 0)
		{
			if (GUILayout.Button("查找项目同名文件"))
			{
				DoSearchAssets();
			}

			int validCount = 0;

			if (_assetFound != null)
			{
				validCount = _assetFound.Length;
				_scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false, GUILayout.Height(300));
//					Debug.Log(_assetFound[0].name);
				for (int i = 0; i < _assetFound.Length; i++)
				{
					if(_dataParsed[i] == null)
						continue;
					if (_assetFound[i] == null)
					{
						string reasonText = _dataParsed[i].InDuplicateError ? "存在重复资源" : "未找到资源";
						GUILayout.Label(string.Format("!!! {1}: {0}", _dataParsed[i].AssetName, reasonText), new GUIStyle("MiniBoldLabel"));
						validCount--;
					}
					else
						_assetFound[i] = EditorGUILayout.ObjectField(_assetFound[i].name, _assetFound[i], typeof (Object)) as GameObject;
				}
				GUILayout.EndScrollView();
			}

			EditorGUILayout.Separator();

			if (_assetFound != null && validCount > 0)
			{
				if (GUILayout.Button("Build scenes"))
				{
					GameObject root = GameObject.Find("importRoot");
					if(root != null)
						DestroyImmediate(root);

					Transform pRoot = new GameObject("importRoot").transform;
					for (int i = 0; i < _assetFound.Length; i++)
					{
						if(_assetFound[i] == null)
							continue;

						GameObject g = Instantiate(_assetFound[i]);
						try
						{
							g.name = _dataParsed[i].PawnName;
							g.transform.position = _dataParsed[i].Position;
							g.transform.eulerAngles += _dataParsed[i].Euler;
							g.transform.localScale = _dataParsed[i].Scale;
							g.transform.parent = pRoot;
						}
						catch (Exception e)
						{
							throw e;
						}
					}
				}
			}
		}


		GUILayout.EndVertical();
	}

	/// <summary>
	/// 数组内string内容是否都一致
	/// 且符合给定参数
	/// </summary>
	/// <param name="strArr">目标string数组</param>
	/// <param name="match">需要符合的内容</param>
	/// <returns></returns>
	public static bool AllTheSameAssetID_AndMatchName(string[] strArr, string match)
	{
		if (strArr == null || strArr.Length == 0)
			return false;
		if (strArr.Length == 1)
			return true;
		bool result = true;
		string s = null;
		for (int i =0; i < strArr.Length; i++)
		{
			string sname = AssetDatabase.GUIDToAssetPath(strArr[i]);
			if(sname != match)
				continue;
			if (string.IsNullOrEmpty(s))
			{
				s = strArr[i];
				continue;
			}
			if (s != strArr[i])
			{
				result = false;
				break;
			}
		}

		return result;
	}

	private void DoSearchAssets()
	{
		_assetFound =new GameObject[_dataParsed.Length];
		for (int i = 0; i < _dataParsed.Length; i++)
		{
			if(_dataParsed[i] == null)
				continue;

			string targetName =_dataParsed[i].AssetName;
			string[] folder = new[] {_folderFilter[0].Substring(_folderFilter[0].LastIndexOf("Assets"))};
			string[] result = AssetDatabase.FindAssets(string.Format("t:Model t:Mesh {0}",_dataParsed[i].AssetName), folder);
			if(result.Length == 0)
				continue;
			_dataParsed[i].InDuplicateError = false;
			// 如果发现有重复资源则将其标记，之后统一提示
			if (result.Length > 1 && !AllTheSameAssetID_AndMatchName(result, _dataParsed[i].AssetName))
			{
				_dataParsed[i].InDuplicateError = true;
				_assetFound[i] = null;
				continue;
			}

			// 找到对应的资源并记录
			string path = AssetDatabase.GUIDToAssetPath(result[0]);
			Object obj = AssetDatabase.LoadAssetAtPath(path, typeof (Object));
			if (obj is GameObject)
				_assetFound[i] = obj as GameObject;
			Object[] oArray = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (Object o in oArray)
			{
				if (o.name == targetName)
				{
					_assetFound[i] = o as GameObject;
					break;
				}
			}

		}
	}

	private void DoParse()
	{
		ReadData(_dataFile);
	}

	/// <summary>
	/// 从csv中读取所需信息
	/// </summary>
	/// <param name="csvFile"></param>
	private void ReadData(TextAsset csvFile)
	{
		string[] records = csvFile.text.Split(lineSeperater);
		_dataParsed = new UE4LevelUnitData[records.Length];

		for(int i=0;i<records.Length;i++)
		{
			records[i] = records[i].Replace("\r", "");
			string[] fields = records[i].Split(fieldSeperator);
			if (fields.Length < 5)
			{
				Debug.LogError(string.Format("line {0} corrupted", i));
				continue;
			}
			UE4LevelUnitData data = new UE4LevelUnitData();
			data.RawData = new string[fields.Length];
			for (int j = 0; j < fields.Length; j++)
			{
				data.RawData[j] = fields[j];
			}

			_dataParsed[i] = data;
			data.Parse();
		}
	}
}

/// <summary>
/// 从UE4 csv文件中解析获得的场景内单位信息
/// </summary>
public class UE4LevelUnitData
{
	public Vector3 Position;
	public Vector3 Euler;
	public Vector3 Scale;
	public string PawnName;
	public string AssetName;
	public bool InDuplicateError = false;

	public string[] RawData;

	/// <summary>
	/// 因为UE4对mesh的组织名称为 '文件名_mesh名'的方式
	/// 所以这里需要单独处理一下
	/// 规范这边可能出现几种mesh命名包括不限于：
	///		以'_SM"开头
	///		以'SM'开头
	///		无特殊前缀
	/// </summary>
	/// <param name="n"></param>
	/// <returns>过滤后的mesh资源名称</returns>
	string GetValidName(string n)
	{
		string fullName = n.Split(' ')[1];
		int sepIdx = fullName.IndexOf("_SM");
		int offset = 0;
		if (sepIdx < 0)
		{
			sepIdx = fullName.IndexOf("SM");
		}
		else
		{
			offset = 1;
		}

		if (sepIdx < 0)
		{
			Debug.LogWarning("find mesh name without 'SM' or '_SM'");
			sepIdx = 0;
			offset = 0;
		}

		return fullName.Substring(sepIdx + offset);
	}

	public void Parse()
	{
		PawnName = RawData[3];
//		string fullName = RawData[4].Split(' ')[1];

		AssetName = GetValidName(RawData[4]);
//		AssetName = fullName.Substring(sepIdx + offset);

//		Debug.Assert(sepIdx >= 0, "'_SM' or 'SM' should appear in file name");

		Position = ParseVector(RawData[0], false, true);
		Euler = ParseVector(RawData[1], true, false);
		Scale = ParseVector(RawData[2], false, false);

		InDuplicateError = false;
	}

	Vector3 ParseVector(string s, bool isRotate, bool isPosition)
	{
		string[] raw = s.Split(' ');
		Debug.Assert(raw.Length == 3, "ParseVector target string should be split to 3 parts");
		Vector3 v;
		if (!isRotate)
		{
			float unitFix = isPosition ? 0.01f : 1.0f;
			try
			{
				v.x = -float.Parse(raw[0].Substring(2))*unitFix;
				v.z = float.Parse(raw[1].Substring(2))*unitFix;
				v.y = float.Parse(raw[2].Substring(2))*unitFix;
			}
			catch (System.Exception)
			{
				throw new Exception("parse failed");
			}
		}
		else
		{
			try
			{
				v.x = float.Parse(raw[0].Substring(2));
				v.y = float.Parse(raw[1].Substring(2));
				v.z = -float.Parse(raw[2].Substring(2));
			}
			catch (System.Exception)
			{
				throw new Exception("parse failed");
			}
		}
		return v;
	}
}
