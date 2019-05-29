using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 批量替换Shader
/// </summary>
public class ShaderBatchSwitchWindow : EditorWindow {

	private Shader _shader;
	private GameObject[] _selected;
	
	private Vector2 _scrollView;

	[MenuItem("Tools/ShaderBatchSwitchWindow")]
	private static void ShowWindow() {
		var window = GetWindow<ShaderBatchSwitchWindow>();
		window.titleContent = new GUIContent("ShaderBatchSwitchWindow");
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

		
		GUILayout.Label("批量修改Shader", new GUIStyle("IN Title"));

		_shader = EditorGUILayout.ObjectField("TargetShader:", _shader,typeof(Shader), false) as Shader;
		if(_shader != null && GUILayout.Button("Batch Replace"))
		{
			List<Material> all_mats = new List<Material>();
			string all_mat_names = "";
			int count = 0;
			foreach(GameObject go in _selected)
			{
				GameObject targetGo = go;
				MeshRenderer[] mrs = targetGo.GetComponentsInChildren<MeshRenderer>();
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
				, string.Format("确认要将物体{0}的所有材质球\n----------\n{1}\n----------\n替换为使用Shader\n----------\n{2}\n----------\n吗？"
					, Selection.activeGameObject.name
					, all_mat_names
					,_shader.name)
				, "OK", "Cancel"))
			{
				// Debug.Log("123");
				try
				{
					SetShaderToMats(_shader, all_mats);
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

	void SetShaderToMats(Shader shader, List<Material> mats)
	{
		foreach(Material m in mats)
		{
			m.shader = shader;
		}
	}
}