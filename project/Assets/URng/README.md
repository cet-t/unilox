# Ultimate RNG

**Ultimate RNG** is a high-speed, high-performance random number generator package specifically optimized for batch generation (procedural generation) in Unity.

## Features

- **High Speed & Efficiency**: Achieves blazingly fast random number generation by utilizing Unity's C# Job System (Burst Compiler), Compute Shaders, and native Rust plugins.
- **Optimized for Procedural Generation**: Designed for batch processing where massive amounts of random numbers need to be generated simultaneously, making it ideal for terrain generation, particle systems, and large-scale simulations.

> [!IMPORTANT]
> **Not a Drop-in Replacement for Unity.Mathematics.Random or System.Random**
> Ultimate RNG is specialized for batch generation and data-oriented design. It is _not_ intended to replace the standard Unity or C# random number generators for simple, everyday gameplay logic (like a simple dice roll in an `Update` loop).

## Supported Algorithms and Use Cases

Ultimate RNG provides a variety of algorithms, allowing you to choose the best tool for your specific performance and quality requirements.

### ⚡ Speed-Optimized: Xoshiro256\*\* / Xoshiro256++ (Burst / Job)

- **Use Case**: When maximum generation speed is required on the CPU.
- **Description**: Implemented using Unity's C# Job System and Burst Compiler. These generators provide remarkable single-thread and multi-thread performance, offering a perfect balance between speed and statistical quality for most game development needs.

### 🎮 CPU-Offloading: Philox32 (Compute Shader)

- **Use Case**: When you want to alleviate CPU load by moving generation to the GPU.
- **Description**: A counter-based PRNG implemented in Compute Shaders. It allows you to generate millions of random numbers directly on the GPU, keeping the CPU entirely free for main game logic. Perfect for GPU-based particle simulations and compute-heavy terrain generation.

### 💎 High Quality: Mt19937 & Sfmt19937 (Rust Native)

- **Use Case**: When strict statistical quality and extensively long periods are paramount.
- **Description**: The standard Mersenne Twister (`Mt19937`) and the SIMD-oriented Fast Mersenne Twister (`Sfmt19937`), implemented in highly optimized native Rust. These provide the cryptographic-like gold standard for PRNG predictability and distribution.

### ⚖️ Fast & Chaotic: Sfc64 (Rust Native)

- **Use Case**: When a strong balance of high speed, small state size, and chaotic properties is needed.
- **Description**: Small Fast Chaotic PRNG implemented in Rust. Excellent for initializing other states or when you need a fast native generator with a smaller memory footprint than Mersenne Twister.

## Known Issues

> [!WARNING]
> **Identical Patterns in Specific Implementations**
> Under certain conditions, specifically when using the Rust native implementations of **`Philox32`** and **`Pcg32`**, they may generate identical random number sequences or identical patterns depending on the initial seed and internal state configuration. Please be cautious when using these specific implementations if strict uniqueness across different seeds is required.

## Quick Start / Usage Examples

Below are simplified examples of how to generate batches of random numbers using the three different backend approaches provided by Ultimate RNG.

### 1. Job System / Burst Compiler (High Performance C#)

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

### 2. Rust Native Plugin (Cryptographic-like Quality)

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

### 3. Compute Shader (GPU Generation)

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

## Benchmarks

Performance comparison for generating **10,000,000** random numbers.
Tested environments may vary, but this illustrates the relative speed of each implementation compared to standard Unity/C# generators.

| Generator                        |   Time (ms) |         Relative Speed |
| :------------------------------- | ----------: | ---------------------: |
| **URng.Job.Xoshiro256++**        | **1.35 ms** | **100.00% (Baseline)** |
| **URng.Job.Xoshiro256\*\***      |     1.49 ms |                109.80% |
| **URng.Job.SplitMix32**          |     1.52 ms |                111.98% |
| **URng.Job.Philox32x4 Parallel** |     1.77 ms |                130.77% |
| **URng.Gpu.Philox32**            |     5.28 ms |                390.20% |
| **URng.Sfc64** (Rust)            |     5.85 ms |                432.27% |
| **URng.Xorshift64** (Rust)       |     6.45 ms |                476.39% |
| **URng.Sfmt19937** (Rust)        |     6.71 ms |                495.82% |
| **URng.Philox32** (Rust)         |     7.45 ms |                550.46% |
| **URng.Xoshiro256++** (Rust)     |     7.45 ms |                550.66% |
| **URng.Xoshiro256\*\*** (Rust)   |     7.74 ms |                572.02% |
| **URng.Mt19937** (Rust)          |     8.22 ms |                607.42% |
| **URng.Pcg32** (Rust)            |     8.80 ms |                650.43% |
| **URng.Sfmt19937-64** (Rust)     |     9.73 ms |                719.38% |
| **URng.Job.Philox32x4 IJob**     |    11.58 ms |                856.02% |
| **URng.Mt19937-64** (Rust)       |    12.60 ms |                931.12% |
| **URng.Job.Mt19937**             |    12.68 ms |                937.22% |
| **URng.Philox64** (Rust)         |    24.49 ms |               1809.77% |
| **URng.Xorshift32** (Rust)       |    27.94 ms |               2064.63% |
| **URng.Xorshift128** (Rust)      |    28.94 ms |               2138.87% |
| **URng.Job.Philox64x2 IJob**     |    32.15 ms |               2376.07% |
| `Unity.Mathematics.Random`       |    40.21 ms |               2971.53% |
| `UnityEngine.Random`             |    71.46 ms |               5281.39% |
| `System.Random`                  |    90.74 ms |               6705.98% |

> [!NOTE]
> Lower time is better. `URng` implementations using the Burst compiler (`URng.Job`) show massive performance gains, up to **~67x faster** than standard `System.Random` and **~52x faster** than `UnityEngine.Random` for massive bulk generation workloads!
