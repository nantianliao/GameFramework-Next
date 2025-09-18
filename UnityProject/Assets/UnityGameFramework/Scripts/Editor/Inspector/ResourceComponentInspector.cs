﻿using System;
using UnityEditor;
using GameFramework;
using UnityGameFramework.Runtime;
using System.Collections.Generic;
using YooAsset.Editor;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    internal sealed class ResourceComponentInspector : GameFrameworkInspector
    {
        private static readonly string[] _resourceModeNames = new string[]
        {
            "EditorSimulateMode (模拟模式)",
            "OfflinePlayMode (单机模式)",
            "HostPlayMode (联机模式)",
            "WebPlayMode (WebGL模式)"
        };

        private SerializedProperty m_PlayMode = null;
        private SerializedProperty m_EncryptionType = null;
        private SerializedProperty m_UpdatableWhilePlaying = null;
        private SerializedProperty m_Milliseconds = null;
        private SerializedProperty m_ReadWritePathType = null;
        private SerializedProperty m_MinUnloadUnusedAssetsInterval = null;
        private SerializedProperty m_MaxUnloadUnusedAssetsInterval = null;
        private SerializedProperty m_UseSystemUnloadUnusedAssets = null;
        private SerializedProperty m_AssetAutoReleaseInterval = null;
        private SerializedProperty m_AssetCapacity = null;
        private SerializedProperty m_AssetExpireTime = null;
        private SerializedProperty m_AssetPriority = null;
        private SerializedProperty m_DownloadingMaxNum = null;
        private SerializedProperty m_FailedTryAgain = null;
        private SerializedProperty m_PackageName = null;
        private int m_ResourceModeIndex = 0;

        private int _packageNameIndex = 0;
        private string[] _packageNames;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceComponent t = (ResourceComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Play Mode", t.PlayMode);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("Play Mode", m_ResourceModeIndex, _resourceModeNames);
                    if (selectedIndex != m_ResourceModeIndex)
                    {
                        m_ResourceModeIndex = selectedIndex;
                        m_PlayMode.enumValueIndex = selectedIndex;
                    }
                }

                m_ReadWritePathType.enumValueIndex = (int)(ReadWritePathType)EditorGUILayout.EnumPopup("Read-Write Path Type", t.ReadWritePathType);

                EditorGUILayout.PropertyField(m_EncryptionType);
            }

            EditorGUILayout.PropertyField(m_UpdatableWhilePlaying);

            EditorGUI.EndDisabledGroup();

            _packageNames = GetBuildPackageNames().ToArray();
            _packageNameIndex = Array.IndexOf(_packageNames, m_PackageName.stringValue);
            if (_packageNameIndex < 0)
            {
                _packageNameIndex = 0;
            }
            _packageNameIndex = EditorGUILayout.Popup("Package Name", _packageNameIndex, _packageNames);
            if (m_PackageName.stringValue != _packageNames[_packageNameIndex])
            {
                m_PackageName.stringValue = _packageNames[_packageNameIndex];
            }

            int milliseconds = EditorGUILayout.DelayedIntField("Milliseconds", m_Milliseconds.intValue);
            if (milliseconds != m_Milliseconds.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.Milliseconds = milliseconds;
                }
                else
                {
                    m_Milliseconds.longValue = milliseconds;
                }
            }

            EditorGUILayout.PropertyField(m_UseSystemUnloadUnusedAssets);

            float minUnloadUnusedAssetsInterval =
                EditorGUILayout.Slider("Min Unload Unused Assets Interval", m_MinUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(minUnloadUnusedAssetsInterval - m_MinUnloadUnusedAssetsInterval.floatValue) > 0.01f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MinUnloadUnusedAssetsInterval = minUnloadUnusedAssetsInterval;
                }
                else
                {
                    m_MinUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
                }
            }

            float maxUnloadUnusedAssetsInterval =
                EditorGUILayout.Slider("Max Unload Unused Assets Interval", m_MaxUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(maxUnloadUnusedAssetsInterval - m_MaxUnloadUnusedAssetsInterval.floatValue) > 0.01f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.MaxUnloadUnusedAssetsInterval = maxUnloadUnusedAssetsInterval;
                }
                else
                {
                    m_MaxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
                }
            }

            float downloadingMaxNum = EditorGUILayout.Slider("Max Downloading Num", m_DownloadingMaxNum.intValue, 1f, 48f);
            if (Math.Abs(downloadingMaxNum - m_DownloadingMaxNum.intValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.DownloadingMaxNum = (int)downloadingMaxNum;
                }
                else
                {
                    m_DownloadingMaxNum.intValue = (int)downloadingMaxNum;
                }
            }

            float failedTryAgain = EditorGUILayout.Slider("Max FailedTryAgain Count", m_FailedTryAgain.intValue, 1f, 48f);
            if (Math.Abs(failedTryAgain - m_FailedTryAgain.intValue) > 0.001f)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FailedTryAgain = (int)failedTryAgain;
                }
                else
                {
                    m_FailedTryAgain.intValue = (int)failedTryAgain;
                }
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                float assetAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Asset Auto Release Interval", m_AssetAutoReleaseInterval.floatValue);
                if (Math.Abs(assetAutoReleaseInterval - m_AssetAutoReleaseInterval.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetAutoReleaseInterval = assetAutoReleaseInterval;
                    }
                    else
                    {
                        m_AssetAutoReleaseInterval.floatValue = assetAutoReleaseInterval;
                    }
                }

                int assetCapacity = EditorGUILayout.DelayedIntField("Asset Capacity", m_AssetCapacity.intValue);
                if (assetCapacity != m_AssetCapacity.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetCapacity = assetCapacity;
                    }
                    else
                    {
                        m_AssetCapacity.intValue = assetCapacity;
                    }
                }

                float assetExpireTime = EditorGUILayout.DelayedFloatField("Asset Expire Time", m_AssetExpireTime.floatValue);
                if (Math.Abs(assetExpireTime - m_AssetExpireTime.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetExpireTime = assetExpireTime;
                    }
                    else
                    {
                        m_AssetExpireTime.floatValue = assetExpireTime;
                    }
                }

                int assetPriority = EditorGUILayout.DelayedIntField("Asset Priority", m_AssetPriority.intValue);
                if (assetPriority != m_AssetPriority.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetPriority = assetPriority;
                    }
                    else
                    {
                        m_AssetPriority.intValue = assetPriority;
                    }
                }
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Unload Unused Assets",
                    Utility.Text.Format("{0:F2} / {1:F2}", t.LastUnloadUnusedAssetsOperationElapseSeconds, t.MaxUnloadUnusedAssetsInterval));
                EditorGUILayout.LabelField("Read-Only Path", t?.ReadOnlyPath?.ToString());
                EditorGUILayout.LabelField("Read-Write Path", t?.ReadWritePath?.ToString());
                EditorGUILayout.LabelField("Applicable Game Version", t.ApplicableGameVersion ?? "<Unknwon>");
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_PlayMode = serializedObject.FindProperty("playMode");
            m_EncryptionType = serializedObject.FindProperty("encryptionType");
            m_UpdatableWhilePlaying = serializedObject.FindProperty("m_UpdatableWhilePlaying");
            m_Milliseconds = serializedObject.FindProperty("Milliseconds");
            m_ReadWritePathType = serializedObject.FindProperty("m_ReadWritePathType");
            m_MinUnloadUnusedAssetsInterval = serializedObject.FindProperty("m_MinUnloadUnusedAssetsInterval");
            m_MaxUnloadUnusedAssetsInterval = serializedObject.FindProperty("m_MaxUnloadUnusedAssetsInterval");
            m_UseSystemUnloadUnusedAssets = serializedObject.FindProperty("m_UseSystemUnloadUnusedAssets");
            m_AssetAutoReleaseInterval = serializedObject.FindProperty("m_AssetAutoReleaseInterval");
            m_AssetCapacity = serializedObject.FindProperty("m_AssetCapacity");
            m_AssetExpireTime = serializedObject.FindProperty("m_AssetExpireTime");
            m_AssetPriority = serializedObject.FindProperty("m_AssetPriority");
            m_DownloadingMaxNum = serializedObject.FindProperty("m_DownloadingMaxNum");
            m_FailedTryAgain = serializedObject.FindProperty("m_FailedTryAgain");
            m_PackageName = serializedObject.FindProperty("m_packageName");

            RefreshModes();
            RefreshTypeNames();
        }

        private void RefreshModes()
        {
            m_ResourceModeIndex = m_PlayMode.enumValueIndex > 0 ? m_PlayMode.enumValueIndex : 0;
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// 获取构建包名称列表，用于下拉可选择
        /// </summary>
        /// <returns></returns>
        private List<string> GetBuildPackageNames()
        {
            List<string> result = new List<string>();
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                result.Add(package.PackageName);
            }
            return result;
        }
    }
}
