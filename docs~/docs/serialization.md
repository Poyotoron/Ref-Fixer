# Force Text への変換

このツールは、プロジェクトの Asset Serialization が Force Text のときだけ使えます。Binary や Mixed のときは、ウィンドウの上部に警告が出て、「参照を検査」を押せません。

壊れた参照がどの GUID を指していたかは、Unity の API からは取れません。Force Text の Prefab ファイルには、次のように壊れた参照がそのまま残っているので、このツールはこの行を読みます。

```yaml
m_SourcePrefab: {fileID: 100100000, guid: 3f2b1c..., type: 3}
```

## 変換する

ウィンドウの警告の下にある「このプロジェクトを Force Text に変換する」を押し、確認ダイアログで「変換する」を押します。

Unity の設定から変えても同じです。

```
Edit > Project Settings > Editor > Asset Serialization > Mode: Force Text
```

!!! warning
    変換すると、Unity がプロジェクトのアセットを保存し直します。大きなプロジェクトでは時間がかかり、Git などには大量の変更が出ます。変換する前にコミットするか、プロジェクトのバックアップを取ってください。変換の途中で Unity を強制終了しないでください。

保存し直しが終わり、警告が消えて「参照を検査」が押せるようになれば準備完了です。
