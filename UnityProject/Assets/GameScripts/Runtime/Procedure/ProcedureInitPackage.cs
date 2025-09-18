﻿using System;
using Cysharp.Threading.Tasks;
using GameFramework.Resource;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 流程 => 初始化Package。
    /// </summary>
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            //Fire Forget立刻触发UniTask初始化Package
            InitPackage(procedureOwner).Forget();
        }

        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            try
            {
                var initializationOperation = await _resourceManager.InitPackage(_resourceManager.DefaultPackageName);

                if (initializationOperation.Status == EOperationStatus.Succeed)
                {
                    //热更新阶段文本初始化
                    LoadText.Instance.InitConfigData(null);

                    EPlayMode playMode = _resourceManager.PlayMode;

                    // 编辑器模式。
                    if (playMode == EPlayMode.EditorSimulateMode)
                    {
                        Log.Info("Editor resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 单机模式。
                    else if (playMode == EPlayMode.OfflinePlayMode)
                    {
                        Log.Info("Package resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 可更新模式。
                    else if (playMode == EPlayMode.HostPlayMode ||
                             playMode == EPlayMode.WebPlayMode)
                    {
                        // 打开启动UI。
                        UILoadMgr.Show(UIDefine.UILoadUpdate);

                        Log.Info("Updatable resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    else
                    {
                        Log.Error("UnKnow resource mode detected Please check???");
                    }
                }
                else
                {
                    // 打开启动UI。
                    UILoadMgr.Show(UIDefine.UILoadUpdate);

                    Log.Error($"{initializationOperation.Error}");

                    // 打开启动UI。
                    UILoadMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");

                    UILoadTip.ShowMessageBox(
                        $"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{initializationOperation.Error}</color>",
                        MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_Retry
                        , () => { Retry(procedureOwner); }, UnityEngine.Application.Quit);
                }
            }
            catch (Exception e)
            {
                OnInitPackageFailed(procedureOwner, e.Message);
            }
        }

        private void OnInitPackageFailed(ProcedureOwner procedureOwner, string message)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate);

            Log.Error($"{message}");

            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");

            if (message.Contains("PackageManifest_DefaultPackage.version Error : HTTP/1.1 404 Not Found"))
            {
                message = "请检查StreamingAssets/package/DefaultPackage/PackageManifest_DefaultPackage.version是否存在";
            }

            UILoadTip.ShowMessageBox($"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{message}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { Retry(procedureOwner); },
                GameModule.QuitApplication);
        }

        private void Retry(ProcedureOwner procedureOwner)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, $"重新初始化资源中...");

            InitPackage(procedureOwner).Forget();
        }
    }
}