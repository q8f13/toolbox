using UnityEditor;

namespace qbfox
{
    // add "SearchThisType" contextMenu item to any component
    public class InspectorEnhance : EditorWindow
    {
        [MenuItem("CONTEXT/Component/SearchThisType")]
        static void SearchThisType(MenuCommand command)
        {
            System.Type t = command.context.GetType();
            UnityEditor.SceneModeUtility.SearchForType(t);
        }
    }
}