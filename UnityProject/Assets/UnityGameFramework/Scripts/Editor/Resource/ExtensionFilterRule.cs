using System.IO;
using YooAsset.Editor;

namespace UnityGameFramework.Editor
{
    [DisplayName("收集着色器")]
    public class CollectShader : IFilterRule
    {
        public string FindAssetType
        {
            get { return EAssetSearchType.Shader.ToString(); }
        }

        public bool IsCollectAsset(FilterRuleData data)
        {
            return Path.GetExtension(data.AssetPath) == ".shader";
        }
    }
}