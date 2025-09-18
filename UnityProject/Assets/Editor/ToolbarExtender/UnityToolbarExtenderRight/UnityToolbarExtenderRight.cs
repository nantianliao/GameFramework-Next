﻿using UnityEditor;
using UnityToolbarExtender;

namespace UnityGameFramework.Editor
{
    [InitializeOnLoad]
    public partial class UnityToolbarExtenderRight
    {
        
        static UnityToolbarExtenderRight()
        {
            // 添加自定义按钮到右上工具栏
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI_SceneSwitch);
            // 订阅项目变化事件
            EditorApplication.projectChanged += UpdateScenes;
            UpdateScenes();
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI_EditorPlayMode);
            _resourceModeIndex = EditorPrefs.GetInt("EditorPlayMode", 0);
        }
    }
}