using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum UpdateType
{
    None = 0,

    //底包更新
    PackageUpdate = 1,

    //资源更新
    ResourceUpdate = 2,
}

/// <summary>
/// 强制更新类型。
/// </summary>
public enum UpdateStyle
{
    /// <summary>
    /// 强制更新(不更新无法进入游戏。)
    /// </summary>
    Force = 1,

    /// <summary>
    /// 非强制(不更新可以进入游戏。)
    /// </summary>
    Optional = 2,
}

/// <summary>
/// 是否提示更新。
/// </summary>
public enum UpdateNotice
{
    /// <summary>
    /// 更新存在提示。
    /// </summary>
    Notice = 1,
    /// <summary>
    /// 更新非提示。
    /// </summary>
    NoNotice = 2,
}

public enum GameStatus
{
    First = 0,
    AssetLoad = 1
}

/// <summary>
/// WebGL平台下，
/// StreamingAssets：跳过远程下载资源直接访问StreamingAssets
/// Remote：访问远程资源
/// </summary>
public enum LoadResWayWebGL
{
    Remote,
    StreamingAssets,
}

[Serializable]
public class ServerIpAndPort
{
    public string ServerName;
    public string Ip;
    public int Port;
}

[Serializable]
public class ServerChannelInfo
{
    public string ChannelName;
    public string CurUseServerName;
    public List<ServerIpAndPort> ServerIpAndPorts;
}

[Serializable]
public class FrameworkGlobalSettings
{
    [Header("Resources")]
    /// <summary>
    /// 项目名称。
    /// </summary>
    [SerializeField]
    private string m_ProjectName = "Demo";
    public string ProjectName
    {
        get { return m_ProjectName; }
    }

    /// <summary>
    /// 资源服务器地址。
    /// </summary>
    [SerializeField]
    private string m_MainResDownLoadPath = "http://127.0.0.1:8081";
    public string MainResDownLoadPath
    {
        get { return m_MainResDownLoadPath; }
    }

    /// <summary>
    /// 资源服务备用地址。
    /// </summary>
    [SerializeField]
    private string m_FallbackResDownLoadPath = "http://127.0.0.1:8082";
    public string FallbackResDownLoadPath
    {
        get { return m_FallbackResDownLoadPath; }
    }

    [Header("Update")]
    [Tooltip("更新设置")]
    [SerializeField]
    private UpdateStyle m_UpdateStyle = UpdateStyle.Force;
    public UpdateStyle UpdateStyle
    {
        get { return m_UpdateStyle; }
    }

    [SerializeField]
    private UpdateNotice m_UpdateNotice = UpdateNotice.Notice;
    public UpdateNotice UpdateNotice
    {
        get { return m_UpdateNotice; }
    }

    [Header("Server")]
    [SerializeField]
    private string m_CurUseServerChannel;
    public string CurUseServerChannel
    {
        get => m_CurUseServerChannel;
    }

    [SerializeField]
    private List<ServerChannelInfo> m_ServerChannelInfos;
    public List<ServerChannelInfo> ServerChannelInfos
    {
        get => m_ServerChannelInfos;
    }

    [SerializeField]
    private string @namespace = "GameLogic";

    [Header("PreLoad")]
    [SerializeField]
    private string[] m_PreLoadTags = new[] { "PRELOAD" };
    public string[] PreLoadTags => m_PreLoadTags;

    [Header("WebGL PreLoad")]
    [SerializeField]
    private string[] m_WebGLPreLoadTags = new[] { "WEBGL_PRELOAD" };
    public string[] WebGLPreLoadTags => m_WebGLPreLoadTags;

    /// <summary>
    /// WebGL平台加载本地资源/加载远程资源。
    /// </summary>
    [Header("WebGL LoadMode")]
    [Tooltip("WebGL设置")]
    [SerializeField]
    private LoadResWayWebGL m_LoadResWayWebGL = LoadResWayWebGL.Remote;
    public LoadResWayWebGL LoadResWayWebGL
    {
        get => m_LoadResWayWebGL;
    }

    /// <summary>
    /// 是否自动你讲打包资源复制到打包后的StreamingAssets地址
    /// </summary>
    [Header("构建资源设置")]
    [SerializeField]
    private bool isAutoAssetCopeToBuildAddress = false;
    /// <summary>
    /// 是否自动你讲打包资源复制到打包后的StreamingAssets地址
    /// </summary>
    /// <returns></returns>
    public bool IsAutoAssetCopeToBuildAddress
    {
        get => isAutoAssetCopeToBuildAddress;
    }

    /// <summary>
    /// 打包程序资源地址
    /// </summary>
    [SerializeField]
    private string buildAddress = "../../Builds/Unity_Data/StreamingAssets";
    /// <summary>
    /// 获取打包程序资源地址
    /// </summary>
    /// <returns></returns>
    public string BuildAddress
    {
        get => buildAddress;
    }

    public string NameSpace => @namespace;
}