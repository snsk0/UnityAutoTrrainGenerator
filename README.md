# UnityAutoTerrainGenerator
※開発中につきテストコードが含まれています<br>

### 概要
- 地形の自動生成を主目的としたツールです<br>
- 現在はUnityのMathf.PerlinNoise関数を使用したHeightMapの生成のみが可能です

### 使い方
- Window -> AutoTerrainGenerator を選択する
- AutoTerrainGenerator から設定値を設定し、「テレインを生成する」を押す
- Asset保存を有効化(推奨)
  - Terrain DataのAssetを「Terrain.asset」ファイルとして保存する設定です<br>通常UnityでTerrainを生成した場合自動で生成されるアセットファイルです
  - 保存先を選択するからTerrain Dataの保存先を選択できます<br>(選択していない場合、生成時に正しくTerrain Dataのアセットが保存されません)<br>
![ATG_1](https://github.com/snsk0/ImageRepository/blob/main/ATG_1.png)
![ATG_2](https://github.com/snsk0/ImageRepository/blob/main/ATG_2.png)
