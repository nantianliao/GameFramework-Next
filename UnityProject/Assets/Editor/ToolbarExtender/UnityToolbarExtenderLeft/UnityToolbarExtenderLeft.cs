using UnityEditor;
using UnityToolbarExtender;

namespace UnityGameFramework.Editor
{
    [InitializeOnLoad]
    public partial class UnityToolbarExtenderLeft
    {
        static UnityToolbarExtenderLeft()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI_SceneLauncher);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuit;
        }
    }
}