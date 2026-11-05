# Poyo's Reference Fixer

Prefab の参照切れ（Missing）を調べて直す Unity エディタ拡張です。壊れた参照が指していた GUID を Prefab のファイルから読み取り、プロジェクト・`.unitypackage`・OS のごみ箱から元のアセットを探します。

使い方: <https://ghp.maaaaa.net/Ref-Fixer/>

## 動作環境

| 項目 | 要件 |
|---|---|
| Unity | 2022.3 |
| Asset Serialization | Force Text（ツールから変換できます） |
| VRChat SDK | 不要です |

## インストール

1. VCC / ALCOM に `https://vpm.maaaaa.net/index.json` を追加します。
2. プロジェクトの Manage Packages で「Poyo's Reference Fixer」を追加します。

VCC を使わない場合は、[リリースページ](https://github.com/Poyotoron/Ref-Fixer/releases)の `.unitypackage` をインポートしてください。

メニューの `Tools > Poyo's Reference Fixer` でウィンドウが開きます。

## できること

- Prefab の参照切れを見つけ、原因ごとに分類する（override の参照、親 Prefab の参照、Missing Script も含む）
- つなぎ直す候補を、プロジェクト内・`.unitypackage`・ごみ箱から探す。GUID が一致する候補と、名前で選んだ候補（推測）を分けて表示する
- 選んだ候補で参照を書き換える。書き換える前に元のファイルを `RefFixer_Backup/` にコピーし、確認ダイアログで承認したときだけ書き換える
- 複数の Prefab やフォルダをまとめて検査する

## 注意

- 修復では `.prefab` ファイルを書き換えます。一部の参照は YAML を直接書き換えて直します。これは Unity が用意している方法ではないため、今後の Unity で使えなくなることがあります。
- 推測の候補は、別のアセットのことがあります。
- Missing Script を直したとき、コンポーネントの設定値が残るとは限りません。
- `.unitypackage` はインポートしません。どれをインポートすればよいかを表示します。
- 元のアセットがどこにも無いときは直せません。

## 不具合の報告

[GitHub Issues](https://github.com/Poyotoron/Ref-Fixer/issues) に送ってください。

## ライセンス

MIT License（[LICENSE](LICENSE)）
