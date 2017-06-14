using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;

/// <summary>
/// 反查场景中对选中物体的引用情况
/// http://answers.unity3d.com/questions/155746/how-do-i-find-which-objects-are-referencing-anothe.html
/// </summary>
public class BacktraceReference : EditorWindow
{
	private Component _theObject;
	bool specificComponent = false;


	[MenuItem("GameObject/What Objects Reference this?")]
	public static void Init()
	{
		GetWindow(typeof(BacktraceReference));
	}

	public void OnGUI()
	{

		if (specificComponent = GUILayout.Toggle(specificComponent, "Match a single specific component"))
		{
			_theObject = EditorGUILayout.ObjectField("Component referenced : ", _theObject, typeof(Component), true) as Component;
			if (_theObject == null)
				return;

			if (GUILayout.Button("Find Objects Referencing it"))
				FindObjectsReferencing(_theObject);
		}
		else if (GUILayout.Button("Find Objects Referencing Selected GameObjects"))
		{
			GameObject[] objects = Selection.gameObjects;
			if (objects == null || objects.Length < 1)
			{
				GUILayout.Label("Select source object/s in Hierarchy.");
				return;
			}
			foreach (GameObject go in objects)
			{
				foreach (Component c in go.GetComponents(typeof(Component)))
				{
					FindObjectsReferencing(c);
				}
			}
		}
	}

	private static void FindObjectsReferencing<T>(T mb) where T : Component
	{
		var objs = Resources.FindObjectsOfTypeAll(typeof(Component)) as Component[];
		if (objs == null) return;
		foreach (Component obj in objs)
		{
			FieldInfo[] fields =
				obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
										BindingFlags.Static);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (FieldReferencesComponent(obj, fieldInfo, mb))
				{
					Debug.Log("Ref: Component " + obj.GetType() + " from Object " + obj.name + " references source component " + mb.GetType(), obj.gameObject);
				}
			}
		}
	}

	private static bool FieldReferencesComponent<T>(Component obj, FieldInfo fieldInfo, T mb) where T : Component
	{
		if (fieldInfo.FieldType.IsArray)
		{
			var arr = fieldInfo.GetValue(obj) as Array;
			if (arr == null)
				return false;
			foreach (object elem in arr)
			{
				if (elem != null && mb != null && elem.GetType() == mb.GetType())
				{
					var o = elem as T;
					if (o == mb)
						return true;
				}
			}
		}
		else
		{
			if (fieldInfo.FieldType == mb.GetType())
			{
				var o = fieldInfo.GetValue(obj) as T;
				if (o == mb)
					return true;
			}
		}
		return false;
	}
}
