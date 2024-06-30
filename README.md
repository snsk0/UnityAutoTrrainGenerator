# UnityAutoTerrainGenerator
※開発中につきテストコードが含まれています<br>

## 概要
- 地形の自動生成を主目的としたツールです<br>
- 現在はUnityのMathf.PerlinNoise関数を使用したHeightMapの生成のみが可能です
## 使い方
### 1. 基本ツールの使い方
#### エディターの開き方
- Window -> AutoTerrainGenerator を選択し、専用ウィンドウを開く
#### 入力パラメータ詳細
- アルゴリズム
  - プルダウン形式でアルゴリズムを選択できます
  - 後述の方法で独自に作成したアルゴリズムを追加することができます
  - デフォルトではfbm, turbulence, ridgeを選択することができます<br>
    > 詳しくは以下から違いを参照してください<br>
    https://thebookofshaders.com/13/?lan=jp
- 線形スケーリング
  - 無効の場合、振幅をそのまま設定できます
  - 有効の場合、振幅の最低値と最大値を設定することができます
- オクターヴ
  - ノイズの重ねる回数を指定します
  - 0の場合、ノイズはそのまま出力されます
  - 有効にする場合、1~10程度の数値で調整してください
- 設定値の出力
  - 基本実装されている3つのアルゴリズムの統一パラメータをアセットとして出力出来ます
  - 出力されたアセットは「入力」からインプットが可能です
- Asset保存を有効化(推奨)
  - Terrain DataのAssetを「Terrain.asset」ファイルとして保存する設定です<br>通常UnityでTerrainを生成した場合自動で生成されるアセットファイルです
  - 保存先を選択するからTerrain Dataの保存先を選択できます<br>(選択していない場合、生成時に正しくTerrain Dataのアセットが保存されません)

![ATG_1](https://github.com/snsk0/ImageRepository/blob/main/ATG_1.png)
![ATG_2](https://github.com/snsk0/ImageRepository/blob/main/ATG_2.png)<br>
※上記内容は24/06/14時点のものです
