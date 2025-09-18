using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using GameFramework;
using UnityGameFramework.Runtime;
using Cysharp.Threading.Tasks;

public partial class GameApp : Singleton<GameApp>
{
    private static List<Assembly> s_HotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        s_HotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Instance.InitSystem();
        Instance.Start();
        Utility.Unity.AddUpdateListener(Instance.Update);
        Utility.Unity.AddFixedUpdateListener(Instance.FixedUpdate);
        Utility.Unity.AddLateUpdateListener(Instance.LateUpdate);
        Utility.Unity.AddDestroyListener(Instance.OnDestroy);
        Utility.Unity.AddOnDrawGizmosListener(Instance.OnDrawGizmos);
        Utility.Unity.AddOnApplicationPauseListener(Instance.OnApplicationPause);
        Instance.StartGameLogic();
    }

    /// <summary>
    /// 开始游戏业务层逻辑。
    /// <remarks>显示UI、加载场景等。</remarks>
    /// </summary>
    private void StartGameLogic()
    {
        
    }

    /// <summary>
    /// 关闭游戏。
    /// </summary>
    /// <param name="shutdownType">关闭游戏框架类型。</param>
    public static void Shutdown(ShutdownType shutdownType)
    {
        if (shutdownType == ShutdownType.None)
        {
            return;
        }

        if (shutdownType == ShutdownType.Restart)
        {
            Utility.Unity.RemoveUpdateListener(Instance.Update);
            Utility.Unity.RemoveFixedUpdateListener(Instance.FixedUpdate);
            Utility.Unity.RemoveLateUpdateListener(Instance.LateUpdate);
            Utility.Unity.RemoveDestroyListener(Instance.OnDestroy);
            Utility.Unity.RemoveOnDrawGizmosListener(Instance.OnDrawGizmos);
            Utility.Unity.RemoveOnApplicationPauseListener(Instance.OnApplicationPause);
            return;
        }

        if (shutdownType == ShutdownType.Quit)
        {
            UnityEngine.Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void Start()
    {
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            logic.OnStart();
        }
    }

    private void Update()
    {
        TProfiler.BeginFirstSample("Update");
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            TProfiler.BeginSample(logic.GetType().FullName);
            logic.OnUpdate();
            TProfiler.EndSample();
        }
        TProfiler.EndFirstSample();
    }

    private void FixedUpdate()
    {
        TProfiler.BeginFirstSample("FixedUpdate");
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            TProfiler.BeginSample(logic.GetType().FullName);
            logic.OnFixedUpdate();
            TProfiler.EndSample();
        }
        TProfiler.EndFirstSample();
    }

    private void LateUpdate()
    {
        TProfiler.BeginFirstSample("LateUpdate");
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            TProfiler.BeginSample(logic.GetType().FullName);
            logic.OnLateUpdate();
            TProfiler.EndSample();
        }
        TProfiler.EndFirstSample();
    }

    private void OnDestroy()
    {
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            logic.OnDestroy();
        }
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            logic.OnDrawGizmos();
        }
#endif
    }

    private void OnApplicationPause(bool isPause)
    {
        var listLogic = m_ListLogicMgr;
        var logicCnt = listLogic.Count;
        for (int i = 0; i < logicCnt; i++)
        {
            var logic = listLogic[i];
            logic.OnApplicationPause(isPause);
        }
    }
}
