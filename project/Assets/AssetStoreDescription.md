# Ultimate RNG - Asset Store Description

## Summary

A lightning-fast random number generator suite for Unity. Utilizing Burst/Job, Compute Shaders, and Rust, it empowers massive batch generation and simulations.

## Description

Unlock unprecedented random generation speeds for your Unity projects. **Ultimate RNG** is designed from the ground up to solve the bottleneck of massive batch generations. By utilizing three powerful backends—Unity's C# Job System (Burst Compiler), Compute Shaders for GPU offloading, and highly optimized Rust native plugins—you can generate millions of random numbers in a fraction of a millisecond.

This package provides a wide array of state-of-the-art RNG algorithms, including Xoshiro256, Philox, Mersenne Twister (MT19937, SFMT), SFC64, and more, allowing you to choose the exact balance between blinding speed and cryptographic-like quality.

**Perfect for:**

- Procedural Terrain & World Generation
- Complex Particle Systems and VFX
- Large-scale simulations (flocking, traffic, fluid dynamics)
- Machine Learning and Monte Carlo simulations within Unity
- Any Data-Oriented (DOTS) workflow demanding heavy random sampling

_Note: Ultimate RNG is strictly specialized for batch generation and is not intended as a drop-in replacement for simple `UnityEngine.Random` calls in everyday, single-threaded Update logic._

## Technical details

**Key Features:**

- **Blazing Fast Performance**: Achieve up to 67x faster generation speeds compared to standard Unity or C# generators on bulk workloads.
- **Multiple Backends**:
  - **Job System / Burst Compiler**: Native C# high-speed generation (Xoshiro256\*\*, Xoshiro256++, SplitMix).
  - **Compute Shader**: Direct-to-GPU generation (Philox32) for zero CPU overhead.
  - **Rust Native Plugins**: The absolute gold standard in statistical quality and performance (MT19937, SFMT, SFC64, PCG32, etc.).
- **Data-Oriented Design**: Built natively for use with `NativeArray`, `Span<T>`, and the Unity Job System.
- **Unified Interface**: Clean, consistent API whether generating from Jobs, Compute Shaders, or Native Plugins.
- **Extensive Benchmarks**: Comes with an integrated Demo scene to benchmark the performance in your own specific environment.

**Included Algorithms:**

- Xoshiro256++ / Xoshiro256\*\*
- Philox32 / Philox64
- Mersenne Twister (Mt19937 / Mt19937-64 / Sfmt19937 / Sfmt19937-64)
- Sfc64
- SplitMix32
- Pcg32
- Xorshift variants
