namespace Poyo.RefFixer.Editor
{
    internal static class RefFixerText
    {
        public const string WindowTitle = "Reference Fixer";
        public const string FirstRunWarning = "このツールは Prefab を書き換える場合があります。修復前にバックアップを作成します。コミットするか、手動でもバックアップを取っておくことをおすすめします。";
        public const string GuessWarning = "これは推測です。誤ったアセットを参照先に選ぶ可能性があります。";
        public const string ForceTextReason = "このツールは Prefab の YAML を直接読み、壊れた参照の GUID を調べます。Binary / Mixed には対応していません。";
        public const string ForceTextCost = "変換すると、すべてのアセットが保存し直されます。大きなプロジェクトでは時間がかかり、バージョン管理に多くの差分が出ます。先にコミットするか、バックアップを取ってください。";
        public const string RepairWarning = "修復前にバックアップを作成します。コミットしておくこともおすすめします。修復しても、コンポーネントの設定値がすべて元に戻る保証はありません。";
        public const string MissingScriptRepairWarning = "Missing Script の m_Script を YAML で直接書き換えます。同じ型に戻す場合も、別の型に差し替える場合も、既存の設定値がどう扱われるかは実機で確認していません。YAML に残っている値が読み込まれる可能性はありますが、対応しないフィールドなどの値が失われるおそれもあります。設定値がすべて元に戻る保証はありません。必ずバックアップと Diff を確認してください。";
        public const string RecycleBinLimitWarning = "ごみ箱に入れずに削除した場合、ごみ箱を空にした場合、OS が自動で削除した後は、ごみ箱から回収できません。.meta がない場合は GUID を保てないため、名前の一致から探す推測の候補になります。";
        public const string UnityPackageImportGuide = "この .unitypackage を手動でインポートすると、元の GUID が戻り、参照が直る可能性があります。影響する範囲を予測できないため、自動ではインポートしません。インポート後に再検査してください。";
        public const string UnityPackageNameMatchGuide = "この .unitypackage に同名または部分一致のアセットがあります。名前一致は推測であり、GUID が異なるためインポートだけでは参照は直りません。インポート後、再検査してプロジェクト内の候補として選び直してください。";
    }
}
