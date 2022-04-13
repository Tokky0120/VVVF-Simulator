# VVVF Simulator
PC上で、VVVFの音を再現します。<br>

# 使い方
このプログラムは、VisualStudio上のC#コンソールアプリ用です。<br>

# 使用上の注意
このプログラムを使用した　作品(動画や、解説動画）等は作ることを歓迎します。<br>

免責事項<br>
- このソフトを使用して出た損害などに関する責任は一切負いません<br>

次の点は行ってください。<br>
- このGitHubのURLを貼り付ける。<br>

次の点はしないでください<br>
- このGitHubのURLを参照せずに、改造したコードを他の場所で公開する。<br>

# 機能
## VVVF音声出力
このアプリケーションは、再現された音声データを wav 形式で出力します。<br>
特に変更がなければ、 192kHz でサンプリングされた wav ファイルになります。<br>

## 波形動画出力
このアプリケーションは、VVVFの波形を動画で出力できます。形式は .avi です。<br>
![2022-02-14](https://user-images.githubusercontent.com/77259842/153803020-6615bcce-22a6-4839-b919-ea114dc12d03.png)

## 電圧ベクトル動画出力
このアプリケーションは、例の六角形を動画で出力できます。形式は .avi です。<br>

## マスコン状況出力
このアプリケーションは、マスコンの操作状況を動画で出力できます。形式は .avi です。<br>
![2022-02-14 (3)](https://user-images.githubusercontent.com/77259842/153803208-18692183-b1ae-4251-96dc-ccc4ce8b3c10.png)

## リアルタイム音声生成
いろいろ、リアルタイムで遊べます。<br>
キー操作<br>
```
W - 変化大
S - 変化中
X - 変化小
B - ブレーキON/OFF
N - マスコンON/OFF
R - VVVF音再選択
Enter - 終了
```

# 親プロジェクト
このプログラムは、Raspberry pi zero vvvf から派生しました。
https://github.com/JOTAN-0655/RPi-Zero-VVVF


# 貢献者
・Thunderfeng<br>
https://github.com/Leifengfengfeng

・Geek of the Week<br>
https://github.com/geekotw
