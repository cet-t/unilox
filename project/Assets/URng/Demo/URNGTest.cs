using UnityEngine;
using UER = UnityEngine.Random;
using UR = Unity.Mathematics.Random;
using SR = System.Random;
using System.Collections.Generic;
using System;
using System.Linq;
using Cet.Rng.Job;
using Unity.Collections;
using Cet.Rng.Gpu;
using CCs = Cet.Rng.Cs;

namespace Cet.Rng.Tests
{
    public class URngTest : MonoBehaviour
    {
        public Philox32Gpu philox32Gpu;

        const int N = 10_000_000;
        const int M = 10;

        private readonly Dictionary<string, TimeSpan> times = new(32);
        private readonly System.Diagnostics.Stopwatch sw = new();

        void Start()
        {
            Debug.Log($"N: {N:#,#}");
            Debug.Log($"M: {M:#,#}");

            // Cet.Rng.Cs (C#)
            TestCsRng64("URng.C#.SplitMix64", () => new CCs.SplitMix64(0));
            TestCsRng64("URng.C#.Xoshiro256**", () => new CCs.Xoshiro256pp());

            // Cet.Rng.Job (C#)
            TestJobRng32("URng.Job.SplitMix32", () => new SplitMix32Seq(N));
            TestJobRng32("URng.Job.Mt19937", () => new Mt19937Seq(N));
            TestJobRng32("URng.Job.Pcg32", () => new Pcg32Seq(N));
            TestJobRng32("URng.Job.Philox32x4 IJob", () => new Philox32x4Seq(N));
            TestParJobRng32("URng.Job.Philox32x4 Parallel", () => new Philox32x4Seq(N));

            TestJobRng64("URng.Job.SplitMix64", () => new SplitMix64Seq(N));
            TestJobRng64("URng.Job.Sfc64", () => new Sfc64Seq(N));
            TestJobRng64("URng.Job.Xoshiro256**", () => new Xoshiro256SsSeq(N));
            TestJobRng64("URng.Job.Xoshiro256++", () => new Xoshiro256PpSeq(N));
            TestJobRng64("URng.Job.Philox64x2 IJob", () => new Philox64x2Seq(N));
            TestJobRng64("URng.Job.Philox64x2 Parallel", () => new Philox64x2Seq(N));

            // Cet.Rng.Gpu (C#)
            TestGpuRng();

            // Cet.Rng (Rust)
            TestNativeRng32("URng.SplitMix32", () => new SplitMix32(0));
            TestNativeRng32("URng.Mt19937", () => new Mt19937(0));
            TestNativeRng32("URng.Pcg32", () => new Pcg32(0));
            TestNativeRng32("URng.Xorshift32", () => new Xorshift32(0));
            TestNativeRng32("URng.Xorshift128", () => new Xorshift128(1, 2, 3, 4));
            TestNativeRng32("URng.Philox32", () => new Philox32(0, 1));
            TestNativeRng32("URng.Sfmt19937", () => new Sfmt19937(0));

            TestNativeRng64("URng.SplitMix64", () => new SplitMix64(0));
            TestNativeRng64("URng.Sfc64", () => new Sfc64(0));
            TestNativeRng64("URng.Xoshiro256**", () => new Xoshiro256Ss(0));
            TestNativeRng64("URng.Xoshiro256++", () => new Xoshiro256Pp(0));
            TestNativeRng64("URng.Mt19937-64", () => new Mt1993764(0));
            TestNativeRng64("URng.Philox64", () => new Philox64(0));
            TestNativeRng64("URng.Sfmt19937-64", () => new Sfmt1993764(0));

            // Standard
            TestSystemRandom();
            TestUnityEngineRandom();
            TestUnityMathRandom();

            var sortedTimes = times.OrderBy(x => x.Value);
            var fastest = sortedTimes.First().Value;
            foreach (var (key, value) in sortedTimes)
            {
                Debug.Log($"{key,-30}: {value.TotalMilliseconds / M,12:F4} ms ({value.TotalMilliseconds / fastest.TotalMilliseconds * 100.0,6:F2} %)");
            }
        }

        void TestNativeRng32(string name, Func<IRng32> factory)
        {
            using var rng = factory();
            var a = new float[N];
            sw.Restart();
            for (int i = 0; i < M; i++)
            {
                a = rng.NextF32s(N).ToArray();
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestNativeRng64(string name, Func<IRng64> factory)
        {
            using var rng = factory();
            var a = new double[N];
            sw.Restart();
            for (int i = 0; i < M; i++)
            {
                a = rng.NextF64s(N).ToArray();
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestCsRng32(string name, Func<CCs.IRng32> factory)
        {
            var rng = factory();
            var a = new float[N];
            sw.Restart();
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    a[j] = rng.NextF();
                }
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestCsRng64(string name, Func<CCs.IRng64> factory)
        {
            var rng = factory();
            var a = new double[N];
            sw.Restart();
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    a[j] = rng.NextF();
                }
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestJobRng32<R>(string name, Func<R> r) where R : ISeq32
        {
            using var rng = r();
            var a = new uint[N];
            sw.Restart();
            for (uint i = 0; i < M; i++)
            {
                a = rng.Fill(N, i).ToArray();
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestParJobRng32<R>(string name, Func<R> r) where R : IParSeq32
        {
            using var rng = r();
            sw.Restart();
            for (uint i = 0; i < M; i++)
            {
                var a = rng.FillParallel(N, i).ToArray();
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestJobRng64<R>(string name, Func<R> r) where R : ISeq64
        {
            using var rng = r();
            var a = new ulong[N];
            sw.Restart();
            for (ulong i = 0; i < M; i++)
            {
                a = rng.Fill(N, i).ToArray();
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestParJobRng64<R>(string name, Func<R> r) where R : IParSeq64
        {
            using var rng = r();
            sw.Restart();
            for (ulong i = 0; i < M; i++)
            {
                var a = rng.FillParallel(N, i);
            }
            sw.Stop();
            times.Add(name, sw.Elapsed);
        }

        void TestGpuRng()
        {
            if (philox32Gpu == null)
            {
                Debug.LogWarning("Philox32Gpu is not assigned. Skipping GPU test.");
                return;
            }
            var results = new float[N];
            philox32Gpu.Init(1);
            sw.Restart();
            for (int i = 0; i < M; i++)
            {
                philox32Gpu.GetRandomFloats(results, 0, 1);
                _ = results.ToArray(); // ToArray to ensure execution
            }
            sw.Stop();
            times.Add("URng.Gpu.Philox32", sw.Elapsed);
        }

        void TestSystemRandom()
        {
            var system = new SR(1);
            var f = new double[N];
            sw.Restart();
            for (int j = 0; j < M; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    f[i] = system.NextDouble();
                }
            }
            sw.Stop();
            times.Add("System.Random", sw.Elapsed);
        }

        void TestUnityEngineRandom()
        {
            UER.InitState(1);
            var f = new float[N];
            sw.Restart();
            for (int j = 0; j < M; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    f[i] = UER.value;
                }
            }
            sw.Stop();
            times.Add("UnityEngine.Random", sw.Elapsed);
        }

        void TestUnityMathRandom()
        {
            var ur = new UR(1);
            var f = new float[N];
            sw.Restart();
            for (int j = 0; j < M; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    f[i] = ur.NextFloat();
                }
            }
            sw.Stop();
            times.Add("Unity.Mathematics.Random", sw.Elapsed);
        }
    }
}