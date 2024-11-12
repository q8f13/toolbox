using UnityEngine;

public class qfGizmoUtil {
	const string ICON_MEMBER = "person-fill";

	public static void DrawGizmoPoint(Transform t, Color c, float radius)
	{
		if(!t.gameObject.activeInHierarchy)
			return;
		if(t == null)
			return;
		Gizmos.color = c;
		Gizmos.DrawWireSphere(t.position, radius);
		Gizmos.DrawRay(t.position, t.forward * radius * 2.0f);
	}

	public static void DrawSpawnPoint(Transform t, Color c, float radius, string pname = ICON_MEMBER)
	{
		if(!t.gameObject.activeInHierarchy)
			return;
		if(t == null)
			return;
		Gizmos.color = c;
		Gizmos.DrawIcon(t.position, pname);
		Gizmos.DrawRay(t.position, t.forward * radius * 2.0f);
	}

	static public void DrawString(string text, Vector3 worldPos, Color? colour = null) {
		UnityEditor.Handles.BeginGUI();

		var restoreColor = GUI.color;

		if (colour.HasValue) GUI.color = colour.Value;
		var view = UnityEditor.SceneView.currentDrawingSceneView;
		Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

		if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
		{
			GUI.color = restoreColor;
			UnityEditor.Handles.EndGUI();
			return;
		}

		Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
		GUI.color = restoreColor;
		UnityEditor.Handles.EndGUI();

	}
}