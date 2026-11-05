using System.Collections.Generic;

namespace Poyo.RefFixer.Editor.Core
{
    internal static class ReferenceClassifier
    {
        public static void ClassifyAll(IEnumerable<ReferenceRecord> records)
        {
            foreach (var record in records) Classify(record);
        }

        private static void Classify(ReferenceRecord record)
        {
            record.Issue = IssueKind.None;
            record.Severity = IssueSeverity.None;
            if (record.FileId == 0)
            {
                record.Issue = IssueKind.EmptyReference;
                record.Severity = IssueSeverity.Warning;
                record.CollapsedByDefault = true;
            }
            else if (!record.GuidResolved)
            {
                record.Issue = record.IsScript ? IssueKind.MissingScript : IssueKind.GuidMissing;
                record.Severity = IssueSeverity.Error;
            }
            else if (record.BackingFileMissing)
            {
                record.Issue = IssueKind.FileIdMissing;
                record.Severity = IssueSeverity.Error;
            }
            else if (!record.FileIdResolved)
            {
                record.Issue = IssueKind.FileIdMissing;
                // Unity 内部で吸収される可能性があるため、F-3-2 に従い警告とする。
                record.Severity = IssueSeverity.Warning;
            }
            else if (record.ExpectedType != null && record.ResolvedType != null && !record.ExpectedType.IsAssignableFrom(record.ResolvedType))
            {
                record.Issue = IssueKind.TypeMismatch;
                record.Severity = IssueSeverity.Warning;
            }
        }

        public static string Label(IssueKind kind)
        {
            switch (kind)
            {
                case IssueKind.GuidMissing: return "T-A GUID 解決不可";
                case IssueKind.FileIdMissing: return "T-B fileID 解決不可";
                case IssueKind.MissingScript: return "T-C Missing Script";
                case IssueKind.EmptyReference: return "T-D 空参照";
                case IssueKind.TypeMismatch: return "T-E 型不一致";
                default: return "正常";
            }
        }

        public static string Label(ReferenceRecord record)
        {
            return record.BackingFileMissing ? "T-B 参照先のファイルがありません" : Label(record.Issue);
        }

        public static string Description(ReferenceRecord record)
        {
            return record.BackingFileMissing
                ? "T-B 参照先のファイルがありません: GUID からパスは分かりましたが、Assets/ または Packages/ に参照先のファイルがありません。"
                : Description(record.Issue);
        }

        public static string Description(IssueKind kind)
        {
            switch (kind)
            {
                case IssueKind.GuidMissing: return "T-A GUID 解決不可: guid がプロジェクト内にありません。パッケージが入っていない場合や、meta が作り直された場合などに起きます。";
                case IssueKind.FileIdMissing: return "T-B fileID 解決不可: guid は見つかりましたが、アセット内に fileID がありません。FBX の再インポートなどでサブアセットの構成と fileID が変わった場合などに起きます。";
                case IssueKind.MissingScript: return "T-C Missing Script: m_Script の guid が見つかりません。スクリプトやパッケージがなくなった場合や、Dynamic Bone の参照が残っている場合などに起きます。";
                case IssueKind.EmptyReference: return "T-D 空参照: fileID が 0 です。意図して未設定にしている可能性があり、誤検知を多く含みます。";
                case IssueKind.TypeMismatch: return "T-E 型不一致: 参照先のアセットの型が、フィールドに必要な型と違います。手動の書き換えに失敗した場合などに起きます。";
                default: return "問題は検出されていません。";
            }
        }
    }
}
