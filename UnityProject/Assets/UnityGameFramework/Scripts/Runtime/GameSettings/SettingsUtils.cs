using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

public static class SettingsUtils
{
    private static readonly string GlobalSettingsPath = $"Settings/GameFrameworkGlobalSettings";
    private static GameFrameworkSettings m_EngineGlobalSettings;

    public static GameFrameworkSettings GlobalSettings
    {
        get
        {
            if (m_EngineGlobalSettings == null)
            {
                m_EngineGlobalSettings = GetSingletonAssetsByResources<GameFrameworkSettings>(GlobalSettingsPath);
            }

            return m_EngineGlobalSettings;
        }
    }

    public static FrameworkGlobalSettings FrameworkGlobalSettings
    {
        get { return GlobalSettings.FrameworkGlobalSettings; }
    }

    public static HybridCLRCustomGlobalSettings HybridCLRCustomGlobalSettings
    {
        get { return GlobalSettings.HybridClrCustomGlobalSettings; }
    }

    /// <summary>
    /// 是否强制更新
    /// </summary>
    /// <returns></returns>
    public static UpdateStyle UpdateStyle
    {
        get { return GlobalSettings.FrameworkGlobalSettings.UpdateStyle; }
    }

    /// <summary>
    /// 是否提示更新
    /// </summary>
    /// <returns></returns>
    public static UpdateNotice UpdateNotice
    {
        get { return GlobalSettings.FrameworkGlobalSettings.UpdateNotice; }
    }

    public static void SetHybridCLRHotUpdateAssemblies(List<string> hotUpdateAssemblies)
    {
        HybridCLRCustomGlobalSettings.HotUpdateAssemblies.Clear();
        HybridCLRCustomGlobalSettings.HotUpdateAssemblies.AddRange(hotUpdateAssemblies);
    }

    public static void SetHybridCLRAOTMetaAssemblies(List<string> aOTMetaAssemblies)
    {
        HybridCLRCustomGlobalSettings.AOTMetaAssemblies.Clear();
        HybridCLRCustomGlobalSettings.AOTMetaAssemblies.AddRange(aOTMetaAssemblies);
    }

    /// <summary>
    /// 是否加载远程资源
    /// </summary>
    /// <returns></returns>
    public static LoadResWayWebGL GetLoadResWayWebGL()
    {
        return FrameworkGlobalSettings.LoadResWayWebGL;
    }

    /// <summary>
    /// 是否加载远程资源
    /// </summary>
    /// <returns></returns>
    public static bool IsAutoAssetCopeToBuildAddress()
    {
        return FrameworkGlobalSettings.IsAutoAssetCopeToBuildAddress;
    }

    /// <summary>
    /// 是否加载远程资源
    /// </summary>
    /// <returns></returns>
    public static string GetBuildAddress()
    {
        return FrameworkGlobalSettings.BuildAddress;
    }

    /// <summary>
    /// 获取资源下载路径。
    /// </summary>
    public static string GetMainResDownLoadPath()
    {
        return Path.Combine(FrameworkGlobalSettings.MainResDownLoadPath, FrameworkGlobalSettings.ProjectName, GetPlatformName()).Replace("\\", "/");
    }

    /// <summary>
    /// 获取备用资源下载路径。
    /// </summary>
    public static string GetFallbackResDownLoadPath()
    {
        return Path.Combine(FrameworkGlobalSettings.FallbackResDownLoadPath, FrameworkGlobalSettings.ProjectName, GetPlatformName()).Replace("\\", "/");
    }

    private static ServerIpAndPort FindServerIpAndPort(string channelName = "")
    {
        if (string.IsNullOrEmpty(channelName))
        {
            channelName = FrameworkGlobalSettings.CurUseServerChannel;
        }

        foreach (var serverChannelInfo in FrameworkGlobalSettings.ServerChannelInfos)
        {
            if (serverChannelInfo.ChannelName.Equals(channelName))
            {
                foreach (var serverIpAndPort in serverChannelInfo.ServerIpAndPorts)
                {
                    if (serverIpAndPort.ServerName.Equals(serverChannelInfo.CurUseServerName))
                    {
                        return serverIpAndPort;
                    }
                }
            }
        }

        return null;
    }

    public static string GetServerIp(string channelName = "")
    {
        ServerIpAndPort serverIpAndPort = FindServerIpAndPort(channelName);
        if (serverIpAndPort != null)
        {
            return serverIpAndPort.Ip;
        }

        return string.Empty;
    }

    public static int GetServerPort(string channelName = "")
    {
        ServerIpAndPort serverIpAndPort = FindServerIpAndPort(channelName);
        if (serverIpAndPort != null)
        {
            return serverIpAndPort.Port;
        }

        return 0;
    }

    private static T GetSingletonAssetsByResources<T>(string assetsPath) where T : ScriptableObject, new()
    {
        string assetType = typeof(T).Name;
#if UNITY_EDITOR
        string[] globalAssetPaths = UnityEditor.AssetDatabase.FindAssets($"t:{assetType}");
        if (globalAssetPaths.Length > 1)
        {
            foreach (var assetPath in globalAssetPaths)
            {
                Debug.LogError($"Could not had Multiple {assetType}. Repeated Path: {UnityEditor.AssetDatabase.GUIDToAssetPath(assetPath)}");
            }

            throw new Exception($"Could not had Multiple {assetType}");
        }
#endif
        T customGlobalSettings = Resources.Load<T>(assetsPath);
        if (customGlobalSettings == null)
        {
            Log.Error($"Could not found {assetType} asset，so auto create:{assetsPath}.");
            return null;
        }

        return customGlobalSettings;
    }

    /// <summary>
    /// 平台名字
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformName()
    {
#if UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "IOS";
#else
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
                return "Windows64";
            case RuntimePlatform.WindowsPlayer:
                return "Windows64";

            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
                return "MacOS";

            case RuntimePlatform.IPhonePlayer:
                return "IOS";

            case RuntimePlatform.Android:
                return "Android";

            case RuntimePlatform.WebGLPlayer:
                return "WebGL";

            default:
                return Application.platform.ToString();
        }
#endif
    }

    public static string GetDictionaryAsset(string assetName, bool fromBytes)
    {
        return Utility.Text.Format("Assets/GameMain/Localization/{0}/Dictionaries/{1}.{2}",
            GameSystem.GetComponent<LocalizationComponent>().Language.ToString(), assetName, fromBytes ? "bytes" : "xml");
    }

    public static string[] GetPreLoadTags()
    {
        return FrameworkGlobalSettings.PreLoadTags;
    }

    public static string[] GetWebGLPreLoadTags()
    {
        return FrameworkGlobalSettings.WebGLPreLoadTags;
    }
}