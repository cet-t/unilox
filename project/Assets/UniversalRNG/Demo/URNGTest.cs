using Cet.Rng;
using UnityEngine;
using UER = UnityEngine.Random;
using UR = Unity.Mathematics.Random;
using SR = System.Random;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.Jobs;
using Cet.Rng.Job;
using Unity.Collections;

namespace Cet.Rng.Tests
{
    public struct JobTest : IJobParallelFor
    {
        public NativeArray<Vector3> Positions;

        public JobTest(NativeArray<Vector3> positions)
        {
            Positions = positions;
        }

        public void Execute(int index)
        {
            // var rng = new Philox32Job((uint)index | 1, (uint)index | 1 / 2);
            // var v = rng.NextF32();
            // Positions[index] = new(v.a, v.b, v.c);

            var rng = new Sfc32Job((uint)index + 1);
            Positions[index] = new(rng.NextF32(), rng.NextF32(), rng.NextF32());
        }
    }

    public class URNGTest : MonoBehaviour
    {
        const int N = 100_000_000;
        const int M = 10;

        private readonly Dictionary<string, TimeSpan> times = new(3);

        void Start()
        {
            using var cet = new Cet64(1);
            Debug.Assert(cet.Next() == 15169567334506313593);

            using var mt = new Mt19937(1);
            Debug.Assert(mt.Next() == 1811243163);
            Debug.Assert(mt.NextU32s(3).SequenceEqual(new uint[] { 3170722558, 139297365, 4293579128 }));

            using var sfc = new Sfc64(1);
            Debug.Assert(sfc.Next() == 5761717516557699369);
            Debug.Assert(sfc.NextU64s(3).SequenceEqual(new ulong[] { 8947820368297942538, 13441617627871919236, 17462243833413595086 }));

            using var pcg = new Pcg32(1);
            Debug.Assert(pcg.Next() == 1299482704);
            Debug.Assert(pcg.NextU32s(3).SequenceEqual(new uint[] { 3096917925, 1071401340, 2195517225 }));

            Debug.Log($"N: {N:#,#}");
            Debug.Log($"M: {M:#,#}");

            var sw = new System.Diagnostics.Stopwatch();

            // {
            //     cet.NextU64s(1024);
            //     sw.Restart();
            //     for (int i = 0; i < M; i++)
            //     {
            //         _ = cet.NextF64s(N);
            //     }
            //     sw.Stop();
            //     times.Add("Cet.Rng.Cet64", sw.Elapsed);
            // }

            {
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = mt.NextF32s(N);
                }
                sw.Stop();
                times.Add("Cet.Rng.Mt19937", sw.Elapsed);
            }

            {
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = sfc.NextF64s(N);
                }
                sw.Stop();
                times.Add("Cet.Rng.Sfc64", sw.Elapsed);
            }

            {
                sw.Restart();
                for (int i = 0; i < M; i++)
                {
                    _ = pcg.NextF32s(N);
                }
                sw.Stop();
                times.Add("Cet.Rng.Pcg32", sw.Elapsed);
            }

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

            // var buffer = new Vector3[N];
            // using var positions = new NativeArray<Vector3>(buffer, Allocator.Persistent);
            // var job = new JobTest(positions);
            // sw.Restart();
            // for (int i = 0; i < M; i++)
            // {
            //     job.Execute(i);
            //     job.Schedule(N, 64).Complete();
            // }
            // sw.Stop();
            // times.Add("Cet.Rng.Sfc32Job", sw.Elapsed);

            var sortedTimes = times.OrderBy(x => x.Value);
            var fastest = sortedTimes.First().Value;
            foreach (var (key, value) in sortedTimes)
            {
                Debug.Log($"{key,-8}: {(value.TotalMilliseconds / M).ToString("F4"):+12} ms (+{((value.TotalMilliseconds / fastest.TotalMilliseconds) * 100).ToString("F2"):+4+1+2}%)");
            }
        }
    }
}