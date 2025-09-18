using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor;

namespace GameScripts.Editor
{
    public static class LubanTools
    {
        [MenuItem("GameFramework/Tools/Luban 转表")]
        public static void BuildLubanExcel()
        {
            Application.OpenURL(Application.dataPath + @"/../../Configs/GameConfig/gen_code_bin_to_project_lazyload.bat");
        }
        
        [MenuItem("GameFramework/Tools/打开表格目录")]
        public static void OpenConfigFolder()
        {
            OpenFolder.Execute(Application.dataPath + @"/../../Configs/GameConfig");
        }
    }
}