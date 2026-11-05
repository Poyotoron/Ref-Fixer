using System;
using System.Collections.Generic;
using System.IO;
using Poyo.RefFixer.Editor.Yaml;
using UnityEditor;
using UnityEngine;

namespace Poyo.RefFixer.Editor.Core
{
    internal static class PrefabReferenceScanner
    {
        public static InspectionResult Inspect(string assetPath, bool showProgress)
        {
            return Inspect(assetPath, showProgress, new Dictionary<string, List<ReferenceRecord>>(StringComparer.OrdinalIgnoreCase));
        }

        private static InspectionResult Inspect(string assetPath, bool showProgress, Dictionary<string, List<ReferenceRecord>> scanCache)
        {
            var result = new InspectionResult { RootAssetPath = assetPath };
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var backingFileExistence = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            try { InspectRecursive(assetPath, result, visited, backingFileExistence, scanCache, showProgress, true); }
            catch (Exception exception) { result.Errors.Add(exception.Message); }
            finally { if (showProgress) EditorUtility.ClearProgressBar(); }
            ReferenceClassifier.ClassifyAll(result.References);
            return result;
        }

        public static BatchInspectionResult InspectBatch(IReadOnlyList<string> assetPaths)
        {
            var batch = new BatchInspectionResult();
            var paths = new List<string>(assetPaths ?? new string[0]);
            var cache = new Dictionary<string, List<ReferenceRecord>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                for (var index = 0; index < paths.Count; index++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(RefFixerText.WindowTitle,
                            (index + 1) + " / " + paths.Count + ": " + paths[index],
                            paths.Count == 0 ? 1f : (float)index / paths.Count))
                    {
                        batch.Cancelled = true;
                        break;
                    }
                    try { batch.Results.Add(Inspect(paths[index], false, cache)); }
                    catch (Exception exception) { batch.Errors.Add(paths[index] + ": " + exception.Message); }
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            return batch;
        }

        private static void InspectRecursive(string assetPath, InspectionResult result, HashSet<string> visited,
            Dictionary<string, bool> backingFileExistence, Dictionary<string, List<ReferenceRecord>> scanCache, bool showProgress, bool isRoot)
        {
            if (string.IsNullOrEmpty(assetPath) || !visited.Add(assetPath)) return;
            if (showProgress) EditorUtility.DisplayProgressBar(RefFixerText.WindowTitle, "参照を調べています: " + assetPath, Mathf.Clamp01(visited.Count / 20f));
            var absolutePath = AssetPathUtility.ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                if (isRoot)
                {
                    result.RootFileMissing = true;
                    result.Errors.Add("検査対象のファイルがありません: " + assetPath);
                }
                return;
            }

            if (!scanCache.TryGetValue(assetPath, out var records))
            {
                records = PrefabYamlParser.Parse(assetPath, absolutePath);
                foreach (var record in records) Resolve(record, backingFileExistence);
                scanCache[assetPath] = records;
            }
            result.References.AddRange(records);

            // Prefab Variant の親および Nested Prefab は m_SourcePrefab として表れる。
            foreach (var record in records)
                if (record.PropertyName == "m_SourcePrefab" && record.GuidResolved && record.ResolvedAssetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    InspectRecursive(record.ResolvedAssetPath, result, visited, backingFileExistence, scanCache, showProgress, false);
        }

        private static void Resolve(ReferenceRecord record, Dictionary<string, bool> backingFileExistence)
        {
            record.ExpectedType = InferExpectedType(record);
            if (record.FileId == 0) return;
            if (string.IsNullOrEmpty(record.Guid)) return;
            record.ResolvedAssetPath = AssetDatabase.GUIDToAssetPath(record.Guid);
            record.GuidResolved = !string.IsNullOrEmpty(record.ResolvedAssetPath);
            if (!record.GuidResolved) return;
            if (record.ResolvedAssetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                record.ReferencedName = Path.GetFileNameWithoutExtension(record.ResolvedAssetPath);
            record.BackingFileMissing = IsBackingFileMissing(record.ResolvedAssetPath, backingFileExistence);

            var resolvedAssets = AssetDatabase.LoadAllAssetsAtPath(record.ResolvedAssetPath);
            var hasResolvedAsset = false;
            foreach (var asset in resolvedAssets)
            {
                if (asset == null) continue;
                hasResolvedAsset = true;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId) &&
                    string.Equals(guid, record.Guid, StringComparison.OrdinalIgnoreCase) && localId == record.FileId)
                {
                    record.FileIdResolved = true;
                    record.ResolvedType = asset.GetType();
                    break;
                }
            }

            if (!record.FileIdResolved && record.FileId == 100100000)
            {
                var main = AssetDatabase.LoadMainAssetAtPath(record.ResolvedAssetPath);
                hasResolvedAsset |= main != null;
                record.FileIdResolved = main != null;
                record.ResolvedType = main != null ? main.GetType() : null;
            }
            if (record.BackingFileMissing)
            {
                // Library の import キャッシュが解決できても、物理ファイル欠落を優先する。
                record.FileIdResolved = false;
                record.TypeAssessment = "参照先のファイルがありません。";
            }
            else if (!hasResolvedAsset)
                record.TypeAssessment = "GUID からパスは分かりましたが、参照先のファイルがありません。";
            else
                record.TypeAssessment = record.ExpectedType == null ? "YAML から必要な型が分からないため判定できません" : null;
        }

        internal static bool IsBackingFileMissing(string assetPath)
        {
            return IsBackingFileMissing(assetPath, null);
        }

        private static bool IsBackingFileMissing(string assetPath, Dictionary<string, bool> existenceCache)
        {
            if (string.IsNullOrEmpty(assetPath)) return true;
            if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) &&
                !assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
                return false;
            bool exists;
            if (existenceCache == null || !existenceCache.TryGetValue(assetPath, out exists))
            {
                try
                {
                    var absolutePath = AssetPathUtility.ToAbsolutePath(assetPath);
                    exists = File.Exists(absolutePath) || Directory.Exists(absolutePath);
                }
                catch
                {
                    // パスを安全に確認できない場合は、欠落と断定して誤検知させない。
                    exists = true;
                }
                if (existenceCache != null) existenceCache[assetPath] = exists;
            }
            return !exists;
        }

        private static Type InferExpectedType(ReferenceRecord record)
        {
            var name = record.PropertyName ?? string.Empty;
            if (name == "m_SourcePrefab") return typeof(GameObject);
            if (record.IsScript) return typeof(MonoScript);
            if (name.IndexOf("Material", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(Material);
            if (name.IndexOf("Sprite", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(Sprite);
            if (name.IndexOf("Texture", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(Texture);
            if (name.IndexOf("Mesh", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(Mesh);
            if (name.IndexOf("Clip", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("Animation", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(AnimationClip);
            if (name.IndexOf("Controller", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(RuntimeAnimatorController);
            if (name.IndexOf("Avatar", StringComparison.OrdinalIgnoreCase) >= 0) return typeof(Avatar);
            return null;
        }
    }

    internal static class AssetPathUtility
    {
        public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        public static string ToAbsolutePath(string assetPath)
        {
            if (assetPath.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
            {
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(assetPath);
                if (package != null && !string.IsNullOrEmpty(package.resolvedPath))
                {
                    var relativePath = assetPath.Length > package.assetPath.Length
                        ? assetPath.Substring(package.assetPath.Length).TrimStart('/', '\\')
                        : string.Empty;
                    return Path.GetFullPath(Path.Combine(package.resolvedPath, relativePath));
                }
            }
            return Path.GetFullPath(Path.Combine(ProjectRoot, assetPath));
        }
    }
}
