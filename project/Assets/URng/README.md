# Ultimate RNG

**Ultimate RNG** is a high-speed, high-performance random number generator package specifically optimized for batch generation (procedural generation) in Unity.

## Table of Contents

1. [Features](#1-features)
2. [Setup Guide](#2-setup-guide)
3. [Supported Algorithms and Use Cases](#3-supported-algorithms-and-use-cases)
4. [Known Issues](#4-known-issues)
5. [Quick Start / Usage Examples](#5-quick-start--usage-examples)
6. [Benchmarks](#6-benchmarks)

---

## 1. Features

- **High Speed & Efficiency**: Achieves blazingly fast random number generation by utilizing Unity's C# Job System (Burst Compiler), Compute Shaders, and native Rust plugins.
- **Optimized for Procedural Generation**: Designed for batch processing where massive amounts of random numbers need to be generated simultaneously, making it ideal for terrain generation, particle systems, and large-scale simulations.

> [!IMPORTANT]
> **Not a Drop-in Replacement for Unity.Mathematics.Random or System.Random**
> Ultimate RNG is specialized for batch generation and data-oriented design. It is _not_ intended to replace the standard Unity or C# random number generators for simple, everyday gameplay logic (like a simple dice roll in an `Update` loop).

---

## 2. Setup Guide

Follow these steps to fully integrate Ultimate RNG into your Unity project.

### Step 1: Import the Package

1. Download the Ultimate RNG package from the Unity Asset Store.
2. In Unity, select `Assets` > `Import Package` > `Custom Package...` and choose the downloaded file.
3. Import all files into your project.

### Step 2: Install Required Dependencies

To achieve the maximum performance for Job-based RNGs, ensure that the **Burst** package is installed in your project.

1. Open the Package Manager (`Window` > `Package Manager`).
2. Search for the **Burst** package under `Packages: Unity Registry`.
3. Click `Install` if it is not already installed.

### Step 3: Reference the Namespaces

In your C# scripts, include the appropriate namespaces depending on the algorithm you choose to use:

- `using Cet.Rng;` for Native Rust PRNGs.
- `using Cet.Rng.Job;` for C# Job System and Burst Compiler PRNGs.
- `using Cet.Rng.Gpu;` for Compute Shader based PRNGs.

You are now ready to start using Ultimate RNG algorithms. Please refer to Section 5 for code examples.

---

## 3. Supported Algorithms and Use Cases

Ultimate RNG provides a variety of algorithms, allowing you to choose the best tool for your specific performance and quality requirements.

### 3.1. Speed-Optimized: Xoshiro256\*\* / Xoshiro256++ (Burst / Job)

- **Use Case**: When maximum generation speed is required on the CPU.
- **Description**: Implemented using Unity's C# Job System and Burst Compiler. These generators provide remarkable single-thread and multi-thread performance, offering a perfect balance between speed and statistical quality for most game development needs.

### 3.2. CPU-Offloading: Philox32 (Compute Shader)

- **Use Case**: When you want to alleviate CPU load by moving generation to the GPU.
- **Description**: A counter-based PRNG implemented in Compute Shaders. It allows you to generate millions of random numbers directly on the GPU, keeping the CPU entirely free for main game logic. Perfect for GPU-based particle simulations and compute-heavy terrain generation.

### 3.3. High Quality: Mt19937 & Sfmt19937 (Rust Native)

- **Use Case**: When strict statistical quality and extensively long periods are paramount.
- **Description**: The standard Mersenne Twister (`Mt19937`) and the SIMD-oriented Fast Mersenne Twister (`Sfmt19937`), implemented in highly optimized native Rust. These provide the cryptographic-like gold standard for PRNG predictability and distribution.

### 3.4. Fast & Chaotic: Sfc64 (Rust Native)

- **Use Case**: When a strong balance of high speed, small state size, and chaotic properties is needed.
- **Description**: Small Fast Chaotic PRNG implemented in Rust. Excellent for initializing other states or when you need a fast native generator with a smaller memory footprint than Mersenne Twister.

### 3.5. Ultimate Speed: Philox32x4x4 / Squares32x8 (Rust Native)

- **Use Case**: When you need the absolute fastest generation speeds possible for extreme batch workloads.
- **Description**: Highly optimized implementations of Philox and Squares algorithms in native Rust. These currently deliver the highest performance of all available generators in the suite, utilizing advanced SIMD and unrolling techniques.

> [!NOTE]
> **Rust Native Seed Initialization**
> All Rust native plugin implementations utilize the `SplitMix32` or `SplitMix64` algorithm to robustly initialize their internal state from the provided seed value.

---

## 4. Known Issues

> [!WARNING]
> **Identical Patterns in Specific Implementations**
> Under certain conditions, specifically when using the Rust native implementations of **`Philox32`** and **`Pcg32`**, they may generate identical random number sequences or identical patterns depending on the initial seed and internal state configuration. Please be cautious when using these specific implementations if strict uniqueness across different seeds is required.

---

## 5. Quick Start / Usage Examples

Below are simplified examples of how to generate batches of random numbers using the three different backend approaches provided by Ultimate RNG.

### 5.1. Job System / Burst Compiler (High Performance C#)

This approach is highly recommended for most Unity standard use cases where you need heavy random generation without leaving C#.

```csharp
using Cet.Rng.Job;
using Unity.Collections;
using UnityEngine;

public class JobRngExample : MonoBehaviour
{
    void Start()
    {
        int count = 1_000_000;
        uint seed = 12345;

        // Create a sequence generator with Persistent allocator
        using var rng = new Xoshiro256SsSeq(count, Allocator.Persistent);

        // Fill the internal buffer with random numbers.
        // This is extremely fast and can also be run parallel (FillParallel).
        rng.Fill(count, seed);

        // The numbers are now ready to be used from the NativeArray inside the sequence.
        // For instance, passing rng.Result to other IJobParallelFor jobs.
    }
}
```

### 5.2. Rust Native Plugin (Cryptographic-like Quality)

When you need the absolute highest statistical quality or specific algorithms like SFMT.

```csharp
using Cet.Rng;
using UnityEngine;

public class RustNativeExample : MonoBehaviour
{
    void Start()
    {
        int count = 1_000_000;
        uint seed = 42;

        // Initialize Native PRNG (e.g. SIMD-oriented Fast MT)
        using var sfmt = new Sfmt1993764(seed);

        // Generate a large batch into a NativeArray/Span efficiently
        var randomFloats = sfmt.NextF64s(count);

        Debug.Log($"Generated {randomFloats.Length} random numbers!");
    }
}
```

### 5.3. Compute Shader (GPU Generation)

Ideal for offloading extreme workloads to the GPU.

```csharp
using Cet.Rng.Gpu;
using UnityEngine;

public class GpuRngExample : MonoBehaviour
{
    // Assign via Inspector
    public Philox32Gpu philox32;

    void Start()
    {
        int count = 1_000_000;
        uint seed = 999;
        var results = new float[count];

        // Initialize with seed
        philox32.Init(seed);

        // Dispatch Compute Shader to generate random floats natively on GPU
        philox32.GetRandomFloats(results, 0, 1);

        Debug.Log($"First GPU Random: {results[0]}");
    }
}
```

---

## 6. Benchmarks

Performance comparison for generating random numbers based on latest benchmark results.
Tested environments may vary, but this illustrates the relative speed of each implementation compared to standard Unity/C# generators.

| Generator                        |    Time (ms) |         Relative Speed |
| :------------------------------- | -----------: | ---------------------: |
| **URng.Philox32x4x4** (Rust)     | **18.16 ms** | **100.00% (Baseline)** |
| **URng.Squares32x8** (Rust)      |     18.50 ms |                101.88% |
| **URng.Philox32** (Rust)         |     21.42 ms |                117.97% |
| **URng.Job.Philox32x4 Parallel** |     52.19 ms |                287.40% |
| **URng.Xorshift32** (Rust)       |     70.38 ms |                387.55% |
| **URng.SplitMix32** (Rust)       |     79.44 ms |                437.44% |
| **URng.Job.SplitMix32**          |     83.06 ms |                457.35% |
| **URng.Mt19937** (Rust)          |     87.66 ms |                482.69% |
| **URng.Sfmt19937** (Rust)        |     88.68 ms |                488.30% |
| **URng.Pcg32** (Rust)            |    121.46 ms |                668.81% |
| **URng.Job.Pcg32**               |    155.39 ms |                855.66% |
| **URng.Job.Philox32x4 IJob**     |    155.98 ms |                858.87% |
| **URng.Job.Mt19937**             |    201.54 ms |               1109.77% |
| **URng.Xorshift128** (Rust)      |    300.46 ms |               1654.43% |
| `UnityEngine.Random`             |    692.93 ms |               3815.53% |
| `Unity.Mathematics.Random`       |    695.53 ms |               3829.83% |
| `System.Random`                  |    852.72 ms |               4695.38% |

> [!NOTE]
> Lower time is better. `URng` implementations utilizing native Rust plugins and the Burst compiler (`URng.Job`) show massive performance gains, up to **~47x faster** than standard `System.Random` and **~38x faster** than `UnityEngine.Random` for massive bulk generation workloads!
