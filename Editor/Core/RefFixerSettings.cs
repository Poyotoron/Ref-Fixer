using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Poyo.RefFixer.Editor.Core
{
    /// <summary>プロジェクト単位の探索設定。兄弟ツールと同じ zzz_pytr 配下へ保存する。</summary>
    internal sealed class RefFixerSettings : ScriptableObject
    {
        public const string SettingsFolder = "Assets/zzz_pytr/Ref-Fixer";
        public const string SettingsPath = SettingsFolder + "/Settings.asset";
        // 以前の版の設定フォルダ。移行のためだけに残す。
        public const string LegacySettingsFolder = "Assets/zzz_pytr/Asset-Sanshounaosu-Nator";
        public const string LegacySettingsPath = LegacySettingsFolder + "/Settings.asset";

        public List<string> unityPackageFolders = new List<string>();
        public bool projectSearchEnabled = true;
        public bool unityPackageSearchEnabled = true;
        public bool recycleBinSearchEnabled = true;

        private static RefFixerSettings _cached;

        public static RefFixerSettings GetOrCreate()
        {
            if (_cached != null) return _cached;
            _cached = AssetDatabase.LoadAssetAtPath<RefFixerSettings>(SettingsPath);
            if (_cached != null) return _cached;
            var legacy = AssetDatabase.LoadAssetAtPath<RefFixerSettings>(LegacySettingsPath);
            if (legacy != null)
            {
                // 旧版が使っている設定を動かしたり、空の設定で置き換えたりしない。
                if (RefFixerLegacy.IsInstalled) return _cached = legacy;
                EnsureFolder();
                var error = AssetDatabase.MoveAsset(LegacySettingsPath, SettingsPath);
                if (string.IsNullOrEmpty(error))
                {
                    _cached = AssetDatabase.LoadAssetAtPath<RefFixerSettings>(SettingsPath);
                    if (_cached == null)
                    {
                        _cached = legacy;
                        Debug.LogWarning("[Poyo's Reference Fixer] 設定の移行後に新しいパスから読み直せませんでした。移行した設定をそのまま使います。");
                    }
                    Debug.Log($"[Poyo's Reference Fixer] 設定を {LegacySettingsPath} から {SettingsPath} へ移しました。");
                    CleanupLegacyFolder();
                }
                else
                {
                    _cached = legacy;
                    Debug.LogWarning($"[Poyo's Reference Fixer] 設定を移せませんでした。旧パスの設定をそのまま使います: {error}");
                }
                return _cached;
            }
            EnsureFolder();
            _cached = CreateInstance<RefFixerSettings>();
            AssetDatabase.CreateAsset(_cached, SettingsPath);
            AssetDatabase.SaveAssets();
            return _cached;
        }

        private static void CleanupLegacyFolder()
        {
            if (!AssetDatabase.IsValidFolder(LegacySettingsFolder)) return;
            var absolutePath = Path.Combine(AssetPathUtility.ProjectRoot, LegacySettingsFolder);
            if (!Directory.Exists(absolutePath)) return;
            using (var entries = Directory.EnumerateFileSystemEntries(absolutePath).GetEnumerator())
            {
                if (entries.MoveNext()) return;
            }
            AssetDatabase.DeleteAsset(LegacySettingsFolder);
        }

        private static void EnsureFolder()
        {
            const string authorFolder = "Assets/zzz_pytr";
            if (!AssetDatabase.IsValidFolder(authorFolder)) AssetDatabase.CreateFolder("Assets", "zzz_pytr");
            if (!AssetDatabase.IsValidFolder(SettingsFolder)) AssetDatabase.CreateFolder(authorFolder, "Ref-Fixer");
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
