using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameBase;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameLogic
{
    /// <summary>
    /// UI系统。
    /// </summary>
    public sealed partial class UISystem : BaseLogicSys<UISystem>
    {
        // 核心字段
        private static Transform _instanceRoot = null;          // UI根节点变换组件
        private bool m_EnableErrorLog = true;                    // 是否启用错误日志
        private Camera _uiCamera = null;                        // UI专用摄像机
        private readonly List<UIWindow> _uiStack = new List<UIWindow>(128); // 窗口堆栈
        private ErrorLogger _errorLogger;                       // 错误日志记录器

        // 常量定义
        internal const int LAYER_DEEP = 2000;
        internal const int WINDOW_DEEP = 100;
        internal const int WINDOW_HIDE_LAYER = 2; // Ignore Raycast
        internal const int WINDOW_SHOW_LAYER = 5; // UI

        /// <summary>
        /// UI根节点。
        /// </summary>
        public Transform UICanvasTransform { private set; get; }

        /// <summary>
        /// UI根节点Canvas。
        /// </summary>
        public Canvas UICanvas { private set; get; }

        /// <summary>
        /// UI根节点。
        /// </summary>
        public Camera UICamera { private set; get; }

        /// <summary>
        /// 模块初始化（自动调用）。
        /// 1. 查找场景中的UIRoot
        /// 2. 初始化资源加载器
        /// 3. 配置错误日志系统
        /// </summary>
        public override bool OnInit()
        {
            base.OnInit();

            Log.Info("OnInit UISystem");

            UICanvasTransform = GameObject.Find("UIRoot/UICanvas").transform;

            UICanvas = UICanvasTransform.GetComponent<Canvas>();

            UICamera = GameObject.Find("UIRoot/UICamera").GetComponent<Camera>();

            UICanvasTransform.gameObject.layer = LayerMask.NameToLayer("UI");

            switch (GameModule.Debugger.ActiveWindowType)
            {
                case DebuggerActiveWindowType.AlwaysOpen:
                    m_EnableErrorLog = true;
                    break;

                case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                    m_EnableErrorLog = Debug.isDebugBuild;
                    break;

                case DebuggerActiveWindowType.OnlyOpenInEditor:
                    m_EnableErrorLog = Application.isEditor;
                    break;

                default:
                    m_EnableErrorLog = false;
                    break;
            }

            if (m_EnableErrorLog)
            {
                _errorLogger = new ErrorLogger();
            }

            UIController.RegisterAllController();

            return true;
        }

        public override void OnDestroy()
        {
            if (_errorLogger != null)
            {
                _errorLogger.Dispose();
                _errorLogger = null;
            }

            CloseAll();
        }

        /// <summary>
        /// 获取所有层级下顶部的窗口。
        /// </summary>
        public UIWindow GetTopWindow()
        {
            if (_uiStack.Count == 0)
            {
                return null;
            }

            UIWindow topWindow = _uiStack[_uiStack.Count - 1];
            return topWindow;
        }

        /// <summary>
        /// 获取指定层级下顶部的窗口。
        /// </summary>
        public UIWindow GetTopWindow(int layer)
        {
            UIWindow lastOne = null;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack[i].WindowLayer == layer)
                {
                    lastOne = _uiStack[i];
                }
            }

            if (lastOne == null)
            {
                return null;
            }

            return lastOne;
        }

        /// <summary>
        /// 获取指定层级下顶部的窗口。
        /// </summary>
        public UIWindow GetTopWindow(UILayer layer)
        {
            return GetTopWindow((int)layer);
        }

        /// <summary>
        /// 是否有任意窗口正在加载。
        /// </summary>
        public bool IsAnyLoading()
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                var window = _uiStack[i];
                if (window.IsLoadDone == false)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 查询窗口是否存在。
        /// </summary>
        /// <typeparam name="T">界面类型。</typeparam>
        /// <returns>是否存在。</returns>
        public bool HasWindow<T>()
        {
            return HasWindow(typeof(T));
        }

        /// <summary>
        /// 查询窗口是否存在。
        /// </summary>
        /// <param name="type">界面类型。</param>
        /// <returns>是否存在。</returns>
        public bool HasWindow(Type type)
        {
            return IsContains(type.FullName);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void ShowUIAsync<T>(params System.Object[] userDatas) where T : UIWindow, new()
        {
            ShowUIImp<T>(true, userDatas);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="type">界面类型。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void ShowUIAsync(Type type, params System.Object[] userDatas)
        {
            ShowUIImp(type, true, userDatas);
        }

        /// <summary>
        /// 同步打开窗口。
        /// </summary>
        /// <typeparam name="T">窗口类。</typeparam>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void ShowUI<T>(params System.Object[] userDatas) where T : UIWindow, new()
        {
            ShowUIImp<T>(false, userDatas);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public async UniTask<T> ShowUIAsyncAwait<T>(params System.Object[] userDatas) where T : UIWindow, new()
        {
            return await ShowUIAwaitImp<T>(true, userDatas) as T;
        }

        /// <summary>
        /// 同步打开窗口。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userDatas"></param>
        /// <returns>打开窗口操作句柄。</returns>
        public void ShowUI(Type type, params System.Object[] userDatas)
        {
            ShowUIImp(type, false, userDatas);
        }

        private void ShowUIImp(Type type, bool isAsync, params System.Object[] userDatas)
        {
            string windowName = type.FullName;

            if (!TryGetWindow(windowName, out UIWindow window, userDatas))
            {
                window = CreateInstance(type);
                Push(window); //首次压入
                window.InternalLoad(window.AssetName, OnWindowPrepare, isAsync, userDatas).Forget();
            }
        }

        private void ShowUIImp<T>(bool isAsync, params System.Object[] userDatas) where T : UIWindow, new()
        {
            Type type = typeof(T);
            string windowName = type.FullName;

            if (!TryGetWindow(windowName, out UIWindow window, userDatas))
            {
                window = CreateInstance<T>();
                Push(window); //首次压入
                window.InternalLoad(window.AssetName, OnWindowPrepare, isAsync, userDatas).Forget();
            }
        }

        private bool TryGetWindow(string windowName, out UIWindow window, params System.Object[] userDatas)
        {
            window = null;
            if (IsContains(windowName))
            {
                window = GetWindow(windowName);
                Pop(window); //弹出窗口
                Push(window); //重新压入
                window.TryInvoke(OnWindowPrepare, userDatas);

                return true;
            }
            return false;
        }

        private async UniTask<T> ShowUIAwaitImp<T>(bool isAsync, params System.Object[] userDatas) where T : UIWindow, new()
        {
            Type type = typeof(T);
            string windowName = type.FullName;

            if (TryGetWindow(windowName, out UIWindow window, userDatas))
            {
                return window as T;
            }
            else
            {
                window = CreateInstance<T>();
                Push(window); //首次压入
                window.InternalLoad(window.AssetName, OnWindowPrepare, isAsync, userDatas).Forget();
                float time = 0f;
                while (!window.IsLoadDone)
                {
                    time += Time.deltaTime;
                    if (time > 60f)
                    {
                        break;
                    }
                    await UniTask.Yield();
                }
                return window as T;
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseUI<T>() where T : UIWindow
        {
            CloseUI(typeof(T));
        }

        public void CloseUI(Type type)
        {
            string windowName = type.FullName;
            UIWindow window = GetWindow(windowName);
            if (window == null)
                return;

            window.InternalDestroy();
            Pop(window);
            OnSortWindowDepth(window.WindowLayer);
            OnSetWindowVisible();
        }

        public void HideUI<T>() where T : UIWindow
        {
            HideUI(typeof(T));
        }

        public void HideUI(Type type)
        {
            string windowName = type.FullName;
            UIWindow window = GetWindow(windowName);
            if (window == null)
            {
                return;
            }

            if (window.HideTimeToClose <= 0)
            {
                CloseUI(type);
                return;
            }

            window.CancelHideToCloseTimer();
            window.Visible = false;
            window.IsHide = true;
            window.HideTimerId = GameModule.Timer.AddOnceTimer(window.HideTimeToClose * 1000, () =>
            {
                CloseUI(type);
            });

            if (window.FullScreen)
            {
                OnSetWindowVisible();
            }
        }

        /// <summary>
        /// 关闭所有窗口。
        /// </summary>
        public void CloseAll()
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                UIWindow window = _uiStack[i];
                window.InternalDestroy();
            }

            _uiStack.Clear();
        }

        /// <summary>
        /// 关闭所有窗口除了。
        /// </summary>
        public void CloseAllWithOut(UIWindow withOut)
        {
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (window == withOut)
                {
                    continue;
                }

                window.InternalDestroy();
                _uiStack.RemoveAt(i);
            }
        }

        /// <summary>
        /// 关闭所有窗口除了。
        /// </summary>
        public void CloseAllWithOut<T>() where T : UIWindow
        {
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (window.GetType() == typeof(T))
                {
                    continue;
                }

                window.InternalDestroy();
                _uiStack.RemoveAt(i);
            }
        }

        private void OnWindowPrepare(UIWindow window)
        {
            OnSortWindowDepth(window.WindowLayer);
            window.InternalCreate();
            window.InternalRefresh();
            OnSetWindowVisible();
        }

        private void OnSortWindowDepth(int layer)
        {
            int depth = layer * LAYER_DEEP;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack[i].WindowLayer == layer)
                {
                    _uiStack[i].Depth = depth;
                    depth += WINDOW_DEEP;
                }
            }
        }

        private void OnSetWindowVisible()
        {
            bool isHideNext = false;
            for (int i = _uiStack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _uiStack[i];
                if (isHideNext == false)
                {
                    if (window.IsHide)
                    {
                        continue;
                    }
                    window.Visible = true;
                    if (window.IsPrepare && window.FullScreen)
                    {
                        isHideNext = true;
                    }
                }
                else
                {
                    window.Visible = false;
                }
            }
        }

        private UIWindow CreateInstance<T>() where T : UIWindow, new()
        {
            Type type = typeof(T);
            UIWindow window = new T();
            WindowAttribute attribute = Attribute.GetCustomAttribute(type, typeof(WindowAttribute)) as WindowAttribute;

            if (window == null)
                throw new GameFrameworkException($"Window {type.FullName} create instance failed.");

            if (attribute != null)
            {
                string assetName = string.IsNullOrEmpty(attribute.Location) ? type.Name : attribute.Location;
                window.Init(type.FullName, attribute.WindowLayer, attribute.FullScreen, assetName, attribute.FromResources, attribute.HideTimeToClose);
            }
            else
            {
                window.Init(type.FullName, (int)UILayer.UI, fullScreen: window.FullScreen, assetName: type.Name, fromResources: false, hideTimeToClose: 10);
            }

            return window;
        }

        private UIWindow CreateInstance(Type type)
        {
            UIWindow window = Activator.CreateInstance(type) as UIWindow;
            WindowAttribute attribute = Attribute.GetCustomAttribute(type, typeof(WindowAttribute)) as WindowAttribute;

            if (window == null)
                throw new GameFrameworkException($"Window {type.FullName} create instance failed.");

            if (attribute != null)
            {
                string assetName = string.IsNullOrEmpty(attribute.Location) ? type.Name : attribute.Location;
                window.Init(type.FullName, attribute.WindowLayer, attribute.FullScreen, assetName, attribute.FromResources, attribute.HideTimeToClose);
            }
            else
            {
                window.Init(type.FullName, (int)UILayer.UI, fullScreen: window.FullScreen, assetName: type.Name, fromResources: false, hideTimeToClose: 10);
            }

            return window;
        }

        /// <summary>
        /// 异步获取窗口。
        /// </summary>
        /// <returns>打开窗口操作句柄。</returns>
        public async UniTask<T> GetUIAsyncAwait<T>(CancellationToken cancellationToken = default) where T : UIWindow
        {
            string windowName = typeof(T).FullName;
            var window = GetWindow(windowName);
            if (window == null)
            {
                return null;
            }

            var ret = window as T;

            if (ret == null)
            {
                return null;
            }

            if (ret.IsLoadDone)
            {
                return ret;
            }

            float time = 0f;
            while (!ret.IsLoadDone)
            {
                time += Time.deltaTime;
                if (time > 60f)
                {
                    break;
                }
                await UniTask.Yield(cancellationToken: cancellationToken);
            }
            return ret;
        }

        /// <summary>
        /// 异步获取窗口。
        /// </summary>
        /// <param name="callback">回调。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public void GetUIAsync<T>(Action<T> callback) where T : UIWindow
        {
            string windowName = typeof(T).FullName;
            var window = GetWindow(windowName);
            if (window == null)
            {
                return;
            }

            var ret = window as T;

            if (ret == null)
            {
                return;
            }

            GetUIAsyncImp(callback).Forget();

            async UniTaskVoid GetUIAsyncImp(Action<T> ctx)
            {
                float time = 0f;
                while (!ret.IsLoadDone)
                {
                    time += Time.deltaTime;
                    if (time > 60f)
                    {
                        break;
                    }
                    await UniTask.Yield();
                }
                ctx?.Invoke(ret);
            }
        }

        private UIWindow GetWindow(string windowName)
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                UIWindow window = _uiStack[i];
                if (window.WindowName == windowName)
                {
                    return window;
                }
            }

            return null;
        }

        private bool IsContains(string windowName)
        {
            for (int i = 0; i < _uiStack.Count; i++)
            {
                UIWindow window = _uiStack[i];
                if (window.WindowName == windowName)
                {
                    return true;
                }
            }

            return false;
        }

        private void Push(UIWindow window)
        {
            // 如果已经存在
            if (IsContains(window.WindowName))
                throw new GameFrameworkException($"Window {window.WindowName} is exist.");

            // 获取插入到所属层级的位置
            int insertIndex = -1;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (window.WindowLayer == _uiStack[i].WindowLayer)
                {
                    insertIndex = i + 1;
                }
            }

            // 如果没有所属层级，找到相邻层级
            if (insertIndex == -1)
            {
                for (int i = 0; i < _uiStack.Count; i++)
                {
                    if (window.WindowLayer > _uiStack[i].WindowLayer)
                    {
                        insertIndex = i + 1;
                    }
                }
            }

            // 如果是空栈或没有找到插入位置
            if (insertIndex == -1)
            {
                insertIndex = 0;
            }

            // 最后插入到堆栈
            _uiStack.Insert(insertIndex, window);
        }

        private void Pop(UIWindow window)
        {
            // 从堆栈里移除
            _uiStack.Remove(window);
        }

        public override void OnUpdate()
        {
            if (_uiStack == null)
            {
                return;
            }

            int count = _uiStack.Count;
            for (int i = 0; i < _uiStack.Count; i++)
            {
                if (_uiStack.Count != count)
                {
                    break;
                }

                var window = _uiStack[i];
                window.InternalUpdate();
            }
        }
    }
}
