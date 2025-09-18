using System;
using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

namespace GameFramework.Resource
{
    [Serializable]
    public struct AssetsRefInfo
    {
        public int instanceId;

        public Object refAsset;

        public AssetsRefInfo(Object refAsset)
        {
            this.refAsset = refAsset;
            instanceId = this.refAsset.GetInstanceID();
        }
    }

    [DisallowMultipleComponent]
    public sealed class AssetsReference : MonoBehaviour
    {
        [SerializeField]
        private GameObject _sourceGameObject;

        [SerializeField]
        private List<AssetsRefInfo> _refAssetInfoList;

        private IResourceManager _resourceManager;

        private static Dictionary<GameObject, AssetsReference> _originalRefs = new Dictionary<GameObject, AssetsReference>();

        private void CheckInit()
        {
            if (_resourceManager != null)
            {
                return;
            }
            else
            {
                _resourceManager = GameFrameworkSystem.GetModule<IResourceManager>();
            }

            if (_resourceManager == null)
            {
                throw new GameFrameworkException($"resourceModule is null.");
            }
        }

        private void CheckRelease()
        {
            if (_sourceGameObject != null)
            {
                _resourceManager.UnloadAsset(_sourceGameObject);
            }
            else
            {
                Log.Warning($"sourceGameObject is not invalid.");
            }
        }

        private void Awake()
        {
            // If it is a clone, clear the reference records before cloning
            if (!IsOriginalInstance())
            {
                ClearCloneReferences();
            }
        }

        private bool IsOriginalInstance()
        {
            return _originalRefs.TryGetValue(gameObject, out var originalComponent) &&
                   originalComponent == this;
        }

        private void ClearCloneReferences()
        {
            _sourceGameObject = null;
            _refAssetInfoList?.Clear();
        }

        private void OnDestroy()
        {
            CheckInit();
            if (_sourceGameObject != null)
            {
                CheckRelease();
            }

            ReleaseRefAssetInfoList();
        }

        private void ReleaseRefAssetInfoList()
        {
            if (_refAssetInfoList != null)
            {
                foreach (var refInfo in _refAssetInfoList)
                {
                    _resourceManager.UnloadAsset(refInfo.refAsset);
                }

                _refAssetInfoList.Clear();
            }
        }

        public AssetsReference Ref(GameObject source, IResourceManager resourceManager = null)
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new GameFrameworkException($"Source gameObject is in scene.");
            }

            _resourceManager = resourceManager;
            _sourceGameObject = source;

            if (!_originalRefs.ContainsKey(gameObject))
            {
                _originalRefs.Add(gameObject, this);
            }

            return this;
        }

        public AssetsReference Ref<T>(T source, IResourceManager resourceManager = null) where T : Object
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            _resourceManager = resourceManager;
            if (_refAssetInfoList == null)
            {
                _refAssetInfoList = new List<AssetsRefInfo>();
            }
            _refAssetInfoList.Add(new AssetsRefInfo(source));
            return this;
        }

        public static AssetsReference Instantiate(GameObject source, Transform parent = null, IResourceManager resourceManager = null)
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new GameFrameworkException($"Source gameObject is in scene.");
            }

            GameObject instance = Object.Instantiate(source, parent);
            return instance.AddComponent<AssetsReference>().Ref(source, resourceManager);
        }

        public static AssetsReference Ref(GameObject source, GameObject instance, IResourceManager resourceManager = null)
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            if (source.scene.name != null)
            {
                throw new GameFrameworkException($"Source gameObject is in scene.");
            }

            var comp = instance.GetComponent<AssetsReference>();
            return comp ? comp.Ref(source, resourceManager) : instance.AddComponent<AssetsReference>().Ref(source, resourceManager);
        }

        public static AssetsReference Ref<T>(T source, GameObject instance, IResourceManager resourceManager = null) where T : UnityEngine.Object
        {
            if (source == null)
            {
                throw new GameFrameworkException($"Source gameObject is null.");
            }

            var comp = instance.GetComponent<AssetsReference>();
            return comp ? comp.Ref(source, resourceManager) : instance.AddComponent<AssetsReference>().Ref(source, resourceManager);
        }
    }
}