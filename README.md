# UnityAutoTerrainGenerator
※開発中につきテストコードが含まれています<br>

## 概要
- 地形の自動生成を主目的としたツールです<br>
- 現在はUnityのMathf.PerlinNoise関数を使用したHeightMapの生成のみが可能です
- ノイズの追加は自由に行えるため、追加実装が可能です
- ノイズを利用して生成する段階のアルゴリズムも自由に追加可能です
- アルゴリズムについてはUnityの提供するInspector拡張に対応しています
## 使い方
### 1. 基本ツールの使い方
#### エディターの開き方
- Window -> AutoTerrainGenerator を選択し、専用ウィンドウを開く<br>
![ATG_1](https://github.com/snsk0/ImageRepository/blob/main/ATG_1.png)
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
- オクターブ
  - ノイズを重ねる回数を指定します
  - 0の場合、ノイズはそのまま出力されます
  - 有効にする場合、1~10程度の数値で調整してください
- 設定値の出力
  - 基本実装されている3つのアルゴリズムの統一パラメータをアセットとして出力出来ます
  - 出力されたアセットは「入力」からインプットが可能です
- Asset保存を有効化(推奨)
  - Terrain DataのAssetを「Terrain.asset」ファイルとして保存する設定です<br>通常UnityでTerrainを生成した場合自動で生成されるアセットファイルです
  - 保存先を選択するからTerrain Dataの保存先を選択できます<br>(選択していない場合、生成時に正しくTerrain Dataのアセットが保存されません)<br>
![ATG_2](https://github.com/snsk0/ImageRepository/blob/main/ATG_2.png)

### 2. Sampleについて
#### 設定値
/Assets/Samples/SampleSettings フォルダーに、出力されたいくつかの設定値が保存されています
入力からインプットすることでこれらの地形を生成することができます
#### コーディング(Sample01フォルダ)
コーディングを行うことで、アルゴリズムの追加を行うことができます
1. AutoTerrainGenerator.HeightMapGeneratorBaseを継承したクラスを作成します
2. /Assets/Samples/SampleArlg01.cs のように、アルゴリズムを作成します<br>
※この時、[SerializeField]を付与しておくことで、AutoTerrainGeneratorからパラメータの入力が可能になります
3. 編集 -> プロジェクト設定から、プロジェクト設定を開きます
4. AutoTerrainGeneratorsのHeight Map Generatorsの項目に、作成したスクリプトを追加します
5. AutoTerrainGeneratorを開きなおし、アルゴリズムから作成したスクリプトを選択します
![ATG_3](https://github.com/snsk0/ImageRepository/blob/main/ATG_3.png)<br>

#### 他各種サンプルについて
- Sample02は、GeneratorのパラメータをScriptableObjectにまとめた例です
- Sample03は、Generatorのパラメータを自作クラスにまとめた例です<br>
  ※この際、自作クラスをWindowに表示するには、自作クラスに[System.Serializable]を付与することを忘れないでください
- Sample04は、Generatorに対してEditor拡張でWindowに表示されるパラメータを示した例です<br>
  ※window上に表示されるEditor拡張については、UnityのInspectorのカスタマイズと全く同じ手段で可能です<br>
  そのため、[CustomEditor]を使用し、Editorクラスを拡張してください<br>

※上記内容は24/06/30時点のものです
