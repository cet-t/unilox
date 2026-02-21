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

namespace Cet.Rng.Tests
{
    public class URNGTest : MonoBehaviour
    {
        public Philox32Gpu philox32;

        const int N = 10_000_000;
        const int M = 10;

        private readonly Dictionary<string, TimeSpan> times = new(3);

        void Start()
        {
            Debug.Log($"N: {N:#,#}");
            Debug.Log($"M: {M:#,#}");

            var sw = new System.Diagnostics.Stopwatch();

            {
                using var rng = new URng32Seq<Job.SplitMix32>(N, Allocator.Persistent);
                sw.Restart();
                for (int i = 0; i < 10; i++)
                {
                    _ = rng.Fill(N, (uint)i);
                }
                sw.Stop();
                times.Add("URng.Job.SplitMix32", sw.Elapsed);
            }

            {
                using var rng = new URng64Seq<Job.Xoshiro256Ss>(N, Allocator.Persistent);
                sw.Restart();
                for (int i = 0; i < 10; i++)
                {
                    _ = rng.Fill(N, (uint)i);
                }
                sw.Stop();
                times.Add("URng.Job.Xoshiro256**", sw.Elapsed);
            }

            {
                using var rng = new URng32Seq<Job.Mt19937>(N, Allocator.Persistent);
                sw.Restart();
                for (int i = 0; i < 10; i++)
                {
                    _ = rng.Fill(N, (uint)i);
                }
                sw.Stop();
                times.Add("URng.Job.Mt19937", sw.Elapsed);
            }

            {
                var results = new float[N];
                philox32.Init(1);
                sw.Restart();
                for (int i = 0; i < 10; i++)
                {
                    philox32.RandF(results, 0, 1);
                }
                sw.Stop();
                results = null;
                times.Add("URng.Gpu.Philox32", sw.Elapsed);
            }

            {
                using var mt32 = new Mt19937(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = mt32.NextF32s(N);
                }
                sw.Stop();
                times.Add("URng.Mt19937", sw.Elapsed);
            }

            {
                using var mt64 = new Mt1993764(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = mt64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Mt19937-64", sw.Elapsed);
            }

            {
                using var sfmt32 = new Sfmt19937(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = sfmt32.NextF32s(N);
                }
                sw.Stop();
                times.Add("URng.Sfmt19937", sw.Elapsed);
            }

            {
                using var sfmt64 = new Sfmt1993764(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = sfmt64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Sfmt19937-64", sw.Elapsed);
            }

            {
                using var xs32 = new Xorshift32(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = xs32.NextF32s(N);
                }
                sw.Stop();
                times.Add("URng.Xorshift32", sw.Elapsed);
            }

            {
                using var xs64 = new Xorshift64(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = xs64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Xorshift64", sw.Elapsed);
            }

            {
                using var xs128 = new Xorshift128(1, 2, 3, 4);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = xs128.NextF32s(N);
                }
                sw.Stop();
                times.Add("URng.Xorshift128", sw.Elapsed);
            }

            {
                using var sfc64 = new Sfc64(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = sfc64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Sfc64", sw.Elapsed);
            }

            {
                using var pcg32 = new Pcg32(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = pcg32.NextF32s(N);
                }
                sw.Stop();
                times.Add("URng.Pcg32", sw.Elapsed);
            }

            {
                using var xsss64 = new Xoshiro256Ss(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = xsss64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Xoshiro256**", sw.Elapsed);
            }

            {
                using var xspp64 = new Xoshiro256Pp(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = xspp64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Xoshiro256++", sw.Elapsed);
            }

            {
                using var philox32 = new Philox32(1, 2);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = philox32.NextF32s(N);
                }
                sw.Stop();
                times.Add("URng.Philox32", sw.Elapsed);
            }

            {
                using var philox64 = new Philox64(1);
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = philox64.NextF64s(N);
                }
                sw.Stop();
                times.Add("URng.Philox64", sw.Elapsed);
            }

            {
                var system = new SR(1);
                sw.Restart();
                for (int j = 0; j < M; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        _ = system.NextDouble();
                    }
                }
                sw.Stop();
                times.Add("System.Random", sw.Elapsed);
            }

            {
                UER.InitState(1);
                sw.Restart();
                for (int j = 0; j < M; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        _ = UER.value;
                    }
                }
                sw.Stop();
                times.Add("UnityEngine.Random", sw.Elapsed);
            }

            {
                var ur = new UR(1);
                sw.Restart();
                for (int j = 0; j < M; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        _ = ur.NextFloat();
                    }
                }
                sw.Stop();
                times.Add("Unity.Mathematics.Random", sw.Elapsed);
            }

            var sortedTimes = times.OrderBy(x => x.Value);
            var fastest = sortedTimes.First().Value;
            foreach (var (key, value) in sortedTimes)
            {
                Debug.Log($"{key,-8}: {(value.TotalMilliseconds / M).ToString("F4"):+12} ms (+{(value.TotalMilliseconds / fastest.TotalMilliseconds * 100.0).ToString("F2"):+4+1+2}%)");
            }
        }
    }
}