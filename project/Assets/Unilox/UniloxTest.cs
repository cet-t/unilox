using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;
using System.Collections.Generic;
using UR = UnityEngine.Random;
using System.Linq;

namespace Cet.Rng.Test
{
    class UniloxTest : MonoBehaviour
    {
        private void Start()
        {
            const int N = 100_000_000;
            const int M = 20;

            var times = new Dictionary<string, TimeSpan>();

            Unilox.Init(N, warm: 8);
            UR.InitState(0);
            var SR = new System.Random(0);

            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < M; i++)
                    _ = Unilox.Randfloats(N, 0, 1);
                sw.Stop();
                times["Unilox.Randfloats"] = sw.Elapsed;
            }

            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < M; i++)
                    for (int j = 0; j < N; j++)
                        _ = Unilox.Randfloat(0, 1);
                sw.Stop();
                times["Unilox.Randfloat"] = sw.Elapsed;
            }

            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < M; i++)
                    for (int j = 0; j < N; j++)
                        _ = UR.Range(0f, 1);
                sw.Stop();
                times["Unity.Range"] = sw.Elapsed;
            }

            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < M; i++)
                    for (int j = 0; j < N; j++)
                        _ = SR.NextDouble();
                sw.Stop();
                times["System.NextDouble"] = sw.Elapsed;
            }

            Debug.Log($"N: {N:#,#}");
            Debug.Log($"M: {M:#,#}");
            foreach (var (key, value) in times.OrderBy(x => x.Value))
            {
                Debug.Log($"{key,-20}: {value.TotalMilliseconds / M:F3} ms");
            }

            Unilox.Free();
        }
    }
}