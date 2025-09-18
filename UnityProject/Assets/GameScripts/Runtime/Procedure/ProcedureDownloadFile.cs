﻿using System;
using Cysharp.Threading.Tasks;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;
using YooAsset;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureDownloadFile : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        private float _lastUpdateDownloadedSize;
        private float _totalSpeed;
        private int _speedSampleCount;

        private float CurrentSpeed
        {
            get
            {
                float interval = Time.deltaTime;
                var sizeDiff = _resourceManager.Downloader.CurrentDownloadBytes - _lastUpdateDownloadedSize;
                _lastUpdateDownloadedSize = _resourceManager.Downloader.CurrentDownloadBytes;
                var speed = sizeDiff / interval;

                // 使用滑动窗口计算平均速度
                _totalSpeed += speed;
                _speedSampleCount++;
                return _totalSpeed / _speedSampleCount;
            }
        }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;

            Log.Info("开始下载更新文件！");

            UILoadMgr.Show(UIDefine.UILoadUpdate, $"开始下载更新文件...");

            BeginDownload().Forget();
        }

        private async UniTaskVoid BeginDownload()
        {
            var downloader = _resourceManager.Downloader;

            // 注册下载回调
            downloader.DownloadErrorCallback = OnDownloadErrorCallback;
            downloader.DownloadUpdateCallback = OnDownloadProgressCallback;
            downloader.BeginDownload();
            await downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeed)
                return;

            ChangeState<ProcedureDownloadOver>(_procedureOwner);
        }

        private void OnDownloadErrorCallback(DownloadErrorData downloadErrorData)
        {
            UILoadTip.ShowMessageBox($"Failed to download file : {downloadErrorData.FileName}", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Default
                , () => { ChangeState<ProcedureCreateDownloader>(_procedureOwner); }, UnityEngine.Application.Quit);
        }

        private void OnDownloadProgressCallback(DownloadUpdateData downloadUpdateData)
        {
            string currentSizeMb = (downloadUpdateData.CurrentDownloadBytes / 1048576f).ToString("f1");
            string totalSizeMb = (downloadUpdateData.TotalDownloadBytes / 1048576f).ToString("f1");
            float progressPercentage = _resourceManager.Downloader.Progress * 100;
            string speed = Utility.File.GetLengthString((int)CurrentSpeed);

            string line1 = Utility.Text.Format("正在更新，已更新 {0}/{1} ({2:F2}%)", downloadUpdateData.CurrentDownloadCount,
                downloadUpdateData.TotalDownloadCount, progressPercentage);
            string line2 = Utility.Text.Format("已更新大小 {0}MB/{1}MB", currentSizeMb, totalSizeMb);
            string line3 = Utility.Text.Format("当前网速 {0}/s，剩余时间 {1}", speed,
                GetRemainingTime(downloadUpdateData.TotalDownloadBytes, downloadUpdateData.CurrentDownloadBytes,
                    CurrentSpeed));
            GameEvent.Send(StringId.StringToHash("DownProgress"), _resourceManager.Downloader.Progress);
            UILoadMgr.Show(UIDefine.UILoadUpdate, $"{line1}\n{line2}\n{line3}");

            Log.Info($"{line1} {line2} {line3}");
        }

        private string GetRemainingTime(long totalBytes, long currentBytes, float speed)
        {
            int needTime = 0;
            if (speed > 0)
            {
                needTime = (int)((totalBytes - currentBytes) / speed);
            }

            TimeSpan ts = new TimeSpan(0, 0, needTime);
            return ts.ToString(@"mm\:ss");
        }
    }
}