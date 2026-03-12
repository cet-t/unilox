# Ultimate RNG - Asset Store Description (Japanese)

## 概要

Unity向けの超高速乱数生成器。Burst/Job System、Compute Shader、Rust を活用し、膨大なバッチ生成やシミュレーションをサポートします。

## 詳細説明

**Ultimate RNG** は、大規模なバッチ生成におけるボトルネックを解消するためにゼロから設計されました。Unity の Job System（Burst コンパイラ）、GPU オフロード用 Compute Shader、高度に最適化された Rust ネイティブプラグインという 3 つの強力なバックエンドを活用することで、数百万個の乱数を 1 ミリ秒未満で生成することが可能です。

本パッケージは、Xoshiro256、Philox、Mersenne Twister (MT19937, SFMT)、SFC64 など、最先端の RNG アルゴリズムを幅広く提供しており、圧倒的な速度と品質のバランスを自由に選択できます。

**最適な用途:**

- プロシージャルな地形およびワールド生成
- 複雑なパーティクルシステムと VFX
- 大規模シミュレーション（群衆、交通、流体解析）
- Unity 内での機械学習およびモンテカルロ法シミュレーション
- 大量かつ高速なサンプリングが求められるあらゆる DOTS ワークフロー（Data-Oriented Design）

_注意: Ultimate RNG はバッチ生成に特化して設計されています。通常の Update ループ内での単純な `UnityEngine.Random` の代替品（シングルスレッドでの使用など）を意図したものではありません。_

## 技術仕様

**主な特徴:**

- **圧倒的なパフォーマンス**: 標準的な Unity/C# 生成器と比較して、最大 40 倍の生成速度を達成。ネイティブプラグインは **AVX-512** SIMD 命令と並列処理を活用し、対応ハードウェア上で次世代の CPU パフォーマンスを引き出します。
- **マルチバックエンド対応**:
  - **Job System / Burst Compiler**: ネイティブ級の C# 高速生成（Xoshiro256\*\*、Xoshiro256++、SplitMix）。
  - **Compute Shader**: CPU 負荷ゼロで GPU 上で直接生成（Philox32）。
  - **Rust Native Plugins**: 統計的品質とパフォーマンスの究極のゴールドスタンダード（MT19937、SFMT、SFC64、PCG32 等）。**SIMD Auto-Dispatching** 機能を備え、実行時に **AVX-512**、AVX2、または SSE を自動検出し、あらゆるマシンで最大のスループットを発揮します。内部状態は SplitMix32/64 を用いて安全に初期化されます。
- **データ指向設計**: `NativeArray`、`Span<T>`、Unity Job System での使用を前提とした設計。
- **統合型インターフェース**: Job、Compute Shader、ネイティブプラグインのいずれを使用する場合でも、クリーンで一貫した API を提供。
- **広範なベンチマーク**: 独自の開発環境でパフォーマンスを測定できる統合デモシーンが付属。

**収録アルゴリズム:**

- Xoshiro256++ / Xoshiro256\*\*
- Philox32 / Philox64
- Mersenne Twister (Mt19937 / Mt19937-64 / Sfmt19937 / Sfmt19937-64)
- Sfc64
- SplitMix32 / SplitMix64
- Pcg32
- Xorshift variants
- Squares32
- Threefry32
