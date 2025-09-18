using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// 获取资源路径相关的实用函数。
    /// </summary>
    public static class GetAssetHelper
    {
        [MenuItem("Assets/Log Asset Path")]
        static void GetAssetPath()
        {
            UnityEngine.Object selObj = Selection.activeObject;

            if (selObj != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selObj);
                EditorGUIUtility.systemCopyBuffer = assetPath;
                Debug.Log($"Asset path is {assetPath}");
            }
        }
        
        [MenuItem("Assets/Log Addressable Path")]
        static void GetAddressablePath()
        {
            UnityEngine.Object selObj = Selection.activeObject;

            if (selObj != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selObj);
                var split = assetPath.Split('/');
                var name = split.Last();
                assetPath = name.Split('.').First();
                EditorGUIUtility.systemCopyBuffer = assetPath;
                Debug.Log($"Addressable path is {assetPath}");
            }
        }
    }
}
