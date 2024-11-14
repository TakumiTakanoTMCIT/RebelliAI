# CameraSwitcherEndFallingBreakZoneについて

## 概要

イントロステージにおいて落ちるゾーンがあり、そこを降りたあとに動くカメラの切り替えを行うスクリプトです

## 仕組み

CameraSwitcherBaseを継承しています。StartをUpdateもプレイヤーが自分自身のx座標を超えたら次のカメラに切り替える処理となっています。

## 発展

これからはカメラを切り替えたい処理を行いときは、座標関係なく、CameraSwitcherBaseという抽象クラスを継承したクラスを作成すればおっけー！！
