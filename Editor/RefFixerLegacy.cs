using UnityEditor;
using UnityEngine;

namespace Poyo.RefFixer.Editor
{
    internal static class RefFixerLegacy
    {
        // if の条件で使うため、到達できないコードの警告を避ける。
        internal static readonly bool IsInstalled =
#if REF_FIXER_HAS_LEGACY
            true;
#else
            false;
#endif

#if REF_FIXER_HAS_LEGACY
        [InitializeOnLoadMethod]
        private static void Notify()
        {
            const string key = "net.maaaaa.ref-fixer.legacyNoticeShown";
            if (Application.isBatchMode || SessionState.GetBool(key, false)) return;
            SessionState.SetBool(key, true);
            Debug.LogWarning("[Poyo's Reference Fixer] 旧名のアセット参照直すネーター（net.maaaaa.asset-sanshounaosu-nator）が同じプロジェクトに入っています。Poyo's Reference Fixer はアセット参照直すネーターの新しい名前です。VCC などのパッケージ管理ツールでアセット参照直すネーターを削除してください。");
        }
#endif
    }
}
