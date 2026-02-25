using UnityEngine;
using UER = UnityEngine.Random;
using UR = Unity.Mathematics.Random;
using SR = System.Random;
using System;
using System.IO;
using System.Text;
using Cet.Rng;
using Job = Cet.Rng.Job;
using Cet.Rng.Gpu;

namespace Cet.Rng.Tests
{
    /// <summary>
    /// A MonoBehaviour that performs quality tests on each random number generator.
    /// Test items:
    ///   1. Chi-Square Goodness-of-Fit Test
    ///   2. Monte Carlo estimation of Pi
    ///   3. Generation of a noise image (256x256 grayscale PNG)
    /// Results are output to the console and also saved in URng/Demo/QualityResults.
    /// </summary>
    public class URngQualityTest : MonoBehaviour
    {
        [Header("GPU RNG")]
        public Philox32Gpu philox32Gpu;

        // ── Test Parameters ──
        const int CHI_SQUARE_SAMPLES = 1_000_000;
        const int CHI_SQUARE_BINS = 100;
        const int MONTE_CARLO_SAMPLES = 1_000_000;
        const int NOISE_SIZE = 256;

        // ── Chi-Square Critical Value (df=99, α=0.05) ──
        const double CHI_SQUARE_CRITICAL_005 = 123.225;

        // ── Result Output Directory ──
        static string ResultsDir =>
            Path.Combine(Application.dataPath, "URng", "Demo", "QualityResults");

        // ================================================================
        //  Result Structures
        // ================================================================
        struct ChiSquareResult
        {
            public double Statistic;
            public int DegreesOfFreedom;
            public double CriticalValue;
            public bool Passed;
        }

        struct MonteCarloResult
        {
            public double EstimatedPi;
            public double Error;
            public int InsideCount;
            public int TotalCount;
        }

        // ================================================================
        //  Entry Point
        // ================================================================
        void Start()
        {
            if (!Directory.Exists(ResultsDir))
                Directory.CreateDirectory(ResultsDir);

            Debug.Log("========================================");
            Debug.Log("  URng Quality Test — Start");
            Debug.Log("========================================");

            // ── Cet.Rng Native (Rust) 32-bit RNGs ──
            TestNativeRng32("URng.Mt19937", () => new Cet.Rng.Mt19937(42));
            TestNativeRng32("URng.Sfmt19937", () => new Sfmt19937(42));
            TestNativeRng32("URng.Xorshift32", () => new Xorshift32(42));
            TestNativeRng32("URng.Xorshift128", () => new Xorshift128(1, 2, 3, 4));
            TestNativeRng32("URng.Pcg32", () => new Pcg32(42));
            TestNativeRng32("URng.Philox32", () => new Cet.Rng.Philox32(42, 1));
            TestNativeRng32("URng.SplitMix32", () => new Cet.Rng.SplitMix32(42));

            // ── Cet.Rng Native (Rust) 64-bit RNGs ──
            TestNativeRng64("URng.Mt19937-64", () => new Mt1993764(42));
            TestNativeRng64("URng.Sfmt19937-64", () => new Sfmt1993764(42));
            TestNativeRng64("URng.Xorshift64", () => new Xorshift64(42));
            TestNativeRng64("URng.Sfc64", () => new Sfc64(42));
            TestNativeRng64("URng.Xoshiro256Ss", () => new Cet.Rng.Xoshiro256Ss(42));
            TestNativeRng64("URng.Xoshiro256Pp", () => new Cet.Rng.Xoshiro256Pp(42));
            TestNativeRng64("URng.Philox64", () => new Cet.Rng.Philox64(42));
            TestNativeRng64("URng.SplitMix64", () => new Cet.Rng.SplitMix64(42));

            // ── Job-based 32-bit RNGs ──
            TestRng32<Job.SplitMix32>("Job.SplitMix32", 42);
            TestRng32<Job.Mt19937>("Job.Mt19937", 42);
            TestRng32<Job.Philox32x4>("Job.Philox32x4", 42);

            // ── Job-based 64-bit RNGs ──
            TestRng64<Job.SplitMix64>("Job.SplitMix64", 42);
            TestRng64<Job.Xoshiro256Ss>("Job.Xoshiro256Ss", 42);
            TestRng64<Job.Xoshiro256Pp>("Job.Xoshiro256Pp", 42);
            TestRng64<Job.Philox64x2>("Job.Philox64x2", 42);

            // ── GPU RNG ──
            TestGpuRng();

            // ── System.Random ──
            TestSystemRandom();

            // ── UnityEngine.Random ──
            TestUnityEngineRandom();

            // ── Unity.Mathematics.Random ──
            TestUnityMathRandom();

            Debug.Log("========================================");
            Debug.Log("  URng Quality Test — Complete");
            Debug.Log("========================================");
        }

        // ================================================================
        //  Cet.Rng Native 32-bit RNG Test (IRng32)
        //  Uses batch APIs NextF32s / RandF32s
        // ================================================================
        void TestNativeRng32(string name, Func<IRng32> factory)
        {
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                using var rng = factory();
                var samples = rng.NextF32s(CHI_SQUARE_SAMPLES);
                var chi = ChiSquareTestSpan(samples);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                using var rng = factory();
                var samples = rng.NextF32s(MONTE_CARLO_SAMPLES * 2);
                var pi = MonteCarloPiSpan(samples);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                using var rng = factory();
                var samples = rng.NextF32s(NOISE_SIZE * NOISE_SIZE);
                var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
                for (int i = 0; i < pixels.Length; i++)
                {
                    float v = samples[i];
                    pixels[i] = new Color(v, v, v, 1f);
                }
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  Cet.Rng Native 64-bit RNG Test (IRng64)
        //  Uses batch API NextF64s and converts to float
        // ================================================================
        void TestNativeRng64(string name, Func<IRng64> factory)
        {
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                using var rng = factory();
                var samples = rng.NextF64s(CHI_SQUARE_SAMPLES);
                var bins = new int[CHI_SQUARE_BINS];
                for (int i = 0; i < samples.Length; i++)
                {
                    float v = (float)samples[i];
                    int bin = Mathf.Clamp((int)(v * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                    bins[bin]++;
                }
                var chi = CalcChiSquare(bins);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                using var rng = factory();
                var samples = rng.NextF64s(MONTE_CARLO_SAMPLES * 2);
                int inside = 0;
                for (int i = 0; i < MONTE_CARLO_SAMPLES; i++)
                {
                    double x = samples[i * 2];
                    double y = samples[i * 2 + 1];
                    if (x * x + y * y <= 1.0)
                        inside++;
                }
                var pi = CalcMonteCarlo(inside);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                using var rng = factory();
                var samples = rng.NextF64s(NOISE_SIZE * NOISE_SIZE);
                var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
                for (int i = 0; i < pixels.Length; i++)
                {
                    float v = (float)samples[i];
                    pixels[i] = new Color(v, v, v, 1f);
                }
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  32-bit Job RNG Test
        //  Since structs are copied when captured by lambdas,
        //  they are called directly in a loop.
        // ================================================================
        void TestRng32<T>(string name, uint seed) where T : struct, Job.IRng32Job
        {
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                var gen = default(T);
                gen.Init(seed);
                var chi = ChiSquareTest32(ref gen);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                var gen = default(T);
                gen.Init(seed);
                var pi = MonteCarloPi32(ref gen);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                var gen = default(T);
                gen.Init(seed);
                var pixels = GenerateNoise32(ref gen);
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  64-bit Job RNG Test
        // ================================================================
        void TestRng64<T>(string name, ulong seed) where T : struct, Job.IRng64Job
        {
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                var gen = default(T);
                gen.Init(seed);
                var chi = ChiSquareTest64(ref gen);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                var gen = default(T);
                gen.Init(seed);
                var pi = MonteCarloPi64(ref gen);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                var gen = default(T);
                gen.Init(seed);
                var pixels = GenerateNoise64(ref gen);
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  GPU RNG Test
        // ================================================================
        void TestGpuRng()
        {
            if (philox32Gpu == null)
            {
                Debug.LogWarning("Philox32Gpu is not assigned in the Inspector. Skipping GPU test.");
                return;
            }

            const string name = "Philox32Gpu";
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) For Chi-Square Test
            {
                philox32Gpu.Init(42);
                var samples = new float[CHI_SQUARE_SAMPLES];
                philox32Gpu.GetRandomFloats(samples, 0f, 1f);
                var chi = ChiSquareTestArray(samples);
                LogChiSquare(name, chi, sb);
            }

            // 2) For Monte Carlo Pi
            {
                philox32Gpu.Init(42);
                var samples = new float[MONTE_CARLO_SAMPLES * 2];
                philox32Gpu.GetRandomFloats(samples, 0f, 1f);
                var pi = MonteCarloPiArray(samples);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) For Noise Image
            {
                philox32Gpu.Init(42);
                var samples = new float[NOISE_SIZE * NOISE_SIZE];
                philox32Gpu.GetRandomFloats(samples, 0f, 1f);
                var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
                for (int i = 0; i < pixels.Length; i++)
                {
                    float v = samples[i];
                    pixels[i] = new Color(v, v, v, 1f);
                }
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  System.Random Test
        // ================================================================
        void TestSystemRandom()
        {
            const string name = "System.Random";
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                var rng = new SR(42);
                var bins = new int[CHI_SQUARE_BINS];
                for (int i = 0; i < CHI_SQUARE_SAMPLES; i++)
                {
                    float v = (float)rng.NextDouble();
                    int bin = Mathf.Clamp((int)(v * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                    bins[bin]++;
                }
                var chi = CalcChiSquare(bins);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                var rng = new SR(42);
                int inside = 0;
                for (int i = 0; i < MONTE_CARLO_SAMPLES; i++)
                {
                    float x = (float)rng.NextDouble();
                    float y = (float)rng.NextDouble();
                    if (x * x + y * y <= 1f) inside++;
                }
                var pi = CalcMonteCarlo(inside);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                var rng = new SR(42);
                var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
                for (int i = 0; i < pixels.Length; i++)
                {
                    float v = (float)rng.NextDouble();
                    pixels[i] = new Color(v, v, v, 1f);
                }
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  UnityEngine.Random Test
        // ================================================================
        void TestUnityEngineRandom()
        {
            const string name = "UnityEngine.Random";
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                UER.InitState(42);
                var bins = new int[CHI_SQUARE_BINS];
                for (int i = 0; i < CHI_SQUARE_SAMPLES; i++)
                {
                    float v = UER.value;
                    int bin = Mathf.Clamp((int)(v * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                    bins[bin]++;
                }
                var chi = CalcChiSquare(bins);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                UER.InitState(42);
                int inside = 0;
                for (int i = 0; i < MONTE_CARLO_SAMPLES; i++)
                {
                    float x = UER.value;
                    float y = UER.value;
                    if (x * x + y * y <= 1f) inside++;
                }
                var pi = CalcMonteCarlo(inside);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                UER.InitState(42);
                var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
                for (int i = 0; i < pixels.Length; i++)
                {
                    float v = UER.value;
                    pixels[i] = new Color(v, v, v, 1f);
                }
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  Unity.Mathematics.Random Test
        // ================================================================
        void TestUnityMathRandom()
        {
            const string name = "Unity.Mathematics.Random";
            Debug.Log($"── {name} ──");
            var sb = new StringBuilder();
            WriteHeader(sb, name);

            // 1) Chi-Square Test
            {
                var rng = new UR(42);
                var bins = new int[CHI_SQUARE_BINS];
                for (int i = 0; i < CHI_SQUARE_SAMPLES; i++)
                {
                    float v = rng.NextFloat();
                    int bin = Mathf.Clamp((int)(v * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                    bins[bin]++;
                }
                var chi = CalcChiSquare(bins);
                LogChiSquare(name, chi, sb);
            }

            // 2) Monte Carlo Pi
            {
                var rng = new UR(42);
                int inside = 0;
                for (int i = 0; i < MONTE_CARLO_SAMPLES; i++)
                {
                    float x = rng.NextFloat();
                    float y = rng.NextFloat();
                    if (x * x + y * y <= 1f) inside++;
                }
                var pi = CalcMonteCarlo(inside);
                LogMonteCarlo(name, pi, sb);
            }

            // 3) Noise Image
            {
                var rng = new UR(42);
                var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
                for (int i = 0; i < pixels.Length; i++)
                {
                    float v = rng.NextFloat();
                    pixels[i] = new Color(v, v, v, 1f);
                }
                SaveNoiseImage(name, pixels, sb);
            }

            SaveResult(name, sb);
        }

        // ================================================================
        //  Chi-Square Test — for 32-bit struct RNG
        // ================================================================
        ChiSquareResult ChiSquareTest32<T>(ref T gen) where T : struct, Job.IRng32Job
        {
            var bins = new int[CHI_SQUARE_BINS];
            for (int i = 0; i < CHI_SQUARE_SAMPLES; i++)
            {
                float v = gen.NextF();
                int bin = Mathf.Clamp((int)(v * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                bins[bin]++;
            }
            return CalcChiSquare(bins);
        }

        // ================================================================
        //  Chi-Square Test — for 64-bit struct RNG
        // ================================================================
        ChiSquareResult ChiSquareTest64<T>(ref T gen) where T : struct, Job.IRng64Job
        {
            var bins = new int[CHI_SQUARE_BINS];
            for (int i = 0; i < CHI_SQUARE_SAMPLES; i++)
            {
                // 64-bit uint -> float [0,1): use upper 24 bits
                float v = (float)((gen.NextU() >> 40) * (1.0 / (1L << 24)));
                int bin = Mathf.Clamp((int)(v * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                bins[bin]++;
            }
            return CalcChiSquare(bins);
        }

        // ================================================================
        //  Chi-Square Test — for array (GPU)
        // ================================================================
        ChiSquareResult ChiSquareTestArray(float[] samples)
        {
            var bins = new int[CHI_SQUARE_BINS];
            int count = Mathf.Min(samples.Length, CHI_SQUARE_SAMPLES);
            for (int i = 0; i < count; i++)
            {
                int bin = Mathf.Clamp((int)(samples[i] * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                bins[bin]++;
            }
            return CalcChiSquare(bins);
        }

        // ================================================================
        //  Chi-Square Test — for ReadOnlySpan<float> (Native RNG)
        // ================================================================
        ChiSquareResult ChiSquareTestSpan(ReadOnlySpan<float> samples)
        {
            var bins = new int[CHI_SQUARE_BINS];
            int count = Mathf.Min(samples.Length, CHI_SQUARE_SAMPLES);
            for (int i = 0; i < count; i++)
            {
                int bin = Mathf.Clamp((int)(samples[i] * CHI_SQUARE_BINS), 0, CHI_SQUARE_BINS - 1);
                bins[bin]++;
            }
            return CalcChiSquare(bins);
        }

        // ================================================================
        //  Calculation of Chi-Square Statistic
        // ================================================================
        ChiSquareResult CalcChiSquare(int[] bins)
        {
            int total = 0;
            for (int i = 0; i < bins.Length; i++) total += bins[i];

            double expected = (double)total / bins.Length;
            double chiSq = 0;
            for (int i = 0; i < bins.Length; i++)
            {
                double diff = bins[i] - expected;
                chiSq += (diff * diff) / expected;
            }

            return new ChiSquareResult
            {
                Statistic = chiSq,
                DegreesOfFreedom = bins.Length - 1,
                CriticalValue = CHI_SQUARE_CRITICAL_005,
                Passed = chiSq < CHI_SQUARE_CRITICAL_005,
            };
        }

        // ================================================================
        //  Monte Carlo Pi — for 32-bit struct RNG
        // ================================================================
        MonteCarloResult MonteCarloPi32<T>(ref T gen) where T : struct, Job.IRng32Job
        {
            int inside = 0;
            for (int i = 0; i < MONTE_CARLO_SAMPLES; i++)
            {
                float x = gen.NextF();
                float y = gen.NextF();
                if (x * x + y * y <= 1f)
                    inside++;
            }
            return CalcMonteCarlo(inside);
        }

        // ================================================================
        //  Monte Carlo Pi — for 64-bit struct RNG
        // ================================================================
        MonteCarloResult MonteCarloPi64<T>(ref T gen) where T : struct, Job.IRng64Job
        {
            int inside = 0;
            for (int i = 0; i < MONTE_CARLO_SAMPLES; i++)
            {
                float x = (float)((gen.NextU() >> 40) * (1.0 / (1L << 24)));
                float y = (float)((gen.NextU() >> 40) * (1.0 / (1L << 24)));
                if (x * x + y * y <= 1f)
                    inside++;
            }
            return CalcMonteCarlo(inside);
        }

        // ================================================================
        //  Monte Carlo Pi — for array (GPU)
        // ================================================================
        MonteCarloResult MonteCarloPiArray(float[] samples)
        {
            int inside = 0;
            int count = Mathf.Min(samples.Length / 2, MONTE_CARLO_SAMPLES);
            for (int i = 0; i < count; i++)
            {
                float x = samples[i * 2];
                float y = samples[i * 2 + 1];
                if (x * x + y * y <= 1f)
                    inside++;
            }
            return CalcMonteCarlo(inside);
        }

        // ================================================================
        //  Monte Carlo Pi — for ReadOnlySpan<float> (Native RNG)
        // ================================================================
        MonteCarloResult MonteCarloPiSpan(ReadOnlySpan<float> samples)
        {
            int inside = 0;
            int count = Mathf.Min(samples.Length / 2, MONTE_CARLO_SAMPLES);
            for (int i = 0; i < count; i++)
            {
                float x = samples[i * 2];
                float y = samples[i * 2 + 1];
                if (x * x + y * y <= 1f)
                    inside++;
            }
            return CalcMonteCarlo(inside);
        }

        // ================================================================
        //  Calculation of Monte Carlo Result
        // ================================================================
        MonteCarloResult CalcMonteCarlo(int inside)
        {
            double pi = 4.0 * inside / MONTE_CARLO_SAMPLES;
            return new MonteCarloResult
            {
                EstimatedPi = pi,
                Error = Math.Abs(pi - Math.PI),
                InsideCount = inside,
                TotalCount = MONTE_CARLO_SAMPLES,
            };
        }

        // ================================================================
        //  Noise Image Pixel Generation — for 32-bit struct RNG
        // ================================================================
        Color[] GenerateNoise32<T>(ref T gen) where T : struct, Job.IRng32Job
        {
            var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
            for (int i = 0; i < pixels.Length; i++)
            {
                float v = gen.NextF();
                pixels[i] = new Color(v, v, v, 1f);
            }
            return pixels;
        }

        // ================================================================
        //  Noise Image Pixel Generation — for 64-bit struct RNG
        // ================================================================
        Color[] GenerateNoise64<T>(ref T gen) where T : struct, Job.IRng64Job
        {
            var pixels = new Color[NOISE_SIZE * NOISE_SIZE];
            for (int i = 0; i < pixels.Length; i++)
            {
                float v = (float)((gen.NextU() >> 40) * (1.0 / (1L << 24)));
                pixels[i] = new Color(v, v, v, 1f);
            }
            return pixels;
        }

        // ================================================================
        //  Common: Save Noise Image
        // ================================================================
        void SaveNoiseImage(string name, Color[] pixels, StringBuilder sb)
        {
            var tex = new Texture2D(NOISE_SIZE, NOISE_SIZE, TextureFormat.RGB24, false);
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            // Sanitize filename to remove dots and spaces
            string safeName = name.Replace(".", "_").Replace(" ", "_");
            string path = Path.Combine(ResultsDir, $"{safeName}_noise.png");
            File.WriteAllBytes(path, png);
            DestroyImmediate(tex);

            string line = $"  [Noise] {NOISE_SIZE}x{NOISE_SIZE} PNG saved → {safeName}_noise.png";
            Debug.Log(line);

            sb.AppendLine("## Noise Image");
            sb.AppendLine($"Size  : {NOISE_SIZE}x{NOISE_SIZE}");
            sb.AppendLine($"File  : {safeName}_noise.png");
            sb.AppendLine();
        }

        // ================================================================
        //  Common: Log Output
        // ================================================================
        void WriteHeader(StringBuilder sb, string name)
        {
            sb.AppendLine($"# Quality Test Results: {name}");
            sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
        }

        void LogChiSquare(string name, ChiSquareResult r, StringBuilder sb)
        {
            string status = r.Passed ? "PASS ✓" : "FAIL ✗";
            string line = $"  [Chi²] χ² = {r.Statistic:F4},  df = {r.DegreesOfFreedom}, " +
                          $" Critical Value (α=0.05) = {r.CriticalValue:F3}  →  {status}";
            Debug.Log(line);

            sb.AppendLine("## Chi-Square Goodness-of-Fit Test");
            sb.AppendLine($"Samples     : {CHI_SQUARE_SAMPLES:#,#}");
            sb.AppendLine($"Bins        : {CHI_SQUARE_BINS}");
            sb.AppendLine($"χ² Statistic: {r.Statistic:F4}");
            sb.AppendLine($"df          : {r.DegreesOfFreedom}");
            sb.AppendLine($"Critical(5%): {r.CriticalValue:F3}");
            sb.AppendLine($"Result      : {status}");
            sb.AppendLine();
        }

        void LogMonteCarlo(string name, MonteCarloResult r, StringBuilder sb)
        {
            string line = $"  [Pi Estimation] π ≈ {r.EstimatedPi:F6},  Error = {r.Error:F6}" +
                          $"  (in={r.InsideCount:#,#} / total={r.TotalCount:#,#})";
            Debug.Log(line);

            sb.AppendLine("## Monte Carlo Pi Estimation");
            sb.AppendLine($"Samples    : {r.TotalCount:#,#}");
            sb.AppendLine($"Inside     : {r.InsideCount:#,#}");
            sb.AppendLine($"π Estimate : {r.EstimatedPi:F6}");
            sb.AppendLine($"True π     : {Math.PI:F6}");
            sb.AppendLine($"Abs Error  : {r.Error:F6}");
            sb.AppendLine();
        }

        void SaveResult(string name, StringBuilder sb)
        {
            string safeName = name.Replace(".", "_").Replace(" ", "_");
            File.WriteAllText(Path.Combine(ResultsDir, $"{safeName}.txt"), sb.ToString());
            Debug.Log($"  → Saved results to {safeName}.txt\n");
        }
    }
}
