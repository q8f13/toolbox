using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 批量替换Shader
/// </summary>
public class MaterialBatchReplaceWindow : EditorWindow {

	private Material _mat;
	private GameObject[] _selected;
	
	private Vector2 _scrollView;

	[MenuItem("Tools/MaterialBatchReplaceWindow")]
	private static void ShowWindow() {
		var window = GetWindow<MaterialBatchReplaceWindow>();
		window.titleContent = new GUIContent("MaterialBatchReplaceWindow");
		window.Show();
	}

	private void OnGUI() {
		EditorGUILayout.BeginVertical();
		GUILayout.Label("当前选中目标", new GUIStyle("IN Title"));
		_selected = Selection.gameObjects;

		if(_selected == null || _selected.Length == 0)
			return;
		
		_scrollView = EditorGUILayout.BeginScrollView(_scrollView);
		foreach(GameObject go in _selected)
		{
			GUILayout.Label(go.name);
		}
		EditorGUILayout.EndScrollView();

		
		GUILayout.Label("批量替换材质球", new GUIStyle("IN Title"));

		_mat = EditorGUILayout.ObjectField("TargetMaterial:", _mat,typeof(Material), false) as Material;
		if(_mat != null && GUILayout.Button("Batch Replace"))
		{
			List<Material> all_mats = new List<Material>();
			List<MeshRenderer> all_mrs = new List<MeshRenderer>();
			string all_mat_names = "";
			int count = 0;
			foreach(GameObject go in _selected)
			{
				GameObject targetGo = go;
				MeshRenderer[] mrs = targetGo.GetComponentsInChildren<MeshRenderer>();
				all_mrs.AddRange(mrs);
				foreach(MeshRenderer mr in mrs)
				{
					foreach(Material m in mr.sharedMaterials)
					{
						if(!all_mats.Contains(m))
						{
							if(count > 0)
								all_mat_names += "\n";
							all_mats.Add(m);
							all_mat_names += m.name;
							count++;
						}
					}
				}
			}

			if(EditorUtility.DisplayDialog("确认替换"
				, string.Format("确认要将物体{0}的所有材质球\n----------\n{1}\n----------\n替换为\n----------\n{2}\n----------\n吗？"
					, Selection.activeGameObject.name
					, all_mat_names
					,_mat.name)
				, "OK", "Cancel"))
			{
				// Debug.Log("123");
				try
				{
					ReplaceToMat(_mat, all_mrs);
				}
				catch(System.Exception e)
				{
					Debug.LogFormat("set shaders failed");
				}

				EditorUtility.DisplayDialog("修改完成","修改完成", "OK", null);
			}
		}

		EditorGUILayout.EndVertical();
	}

	void ReplaceToMat(Material target_mat, List<MeshRenderer> mrs)
	{
		foreach(MeshRenderer mr in mrs)
		{
			Material[] mats = mr.sharedMaterials;
			for(int i=0;i<mats.Length;i++)
			{
				mats[i] = target_mat;
			}

			mr.sharedMaterials = mats;
		}
	}
}