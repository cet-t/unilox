using Cet.Rng;
using UnityEngine;
using System;

public class UniloxBenchmark : MonoBehaviour
{
    const int N = 100_000_000;
    const int M = 100;

    void Start()
    {
        Unilox.Init();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < M; i++)
            _ = Unilox.Randfloats(N, 0f, 1f);
        sw.Stop();
        Debug.Log($"Unilox.Randfloats   : {sw.Elapsed.TotalMilliseconds / M:f4} ms");

        Unilox.Free();
        Unilox.Init(N);

        var systemResults = new double[N];
        var rng = new System.Random(0);

        sw.Restart();
        for (int j = 0; j < M; j++)
            for (int i = 0; i < systemResults.Length; i++)
                systemResults[i] = rng.NextDouble();
        _ = Unilox.Randfloats(N, 0f, 1f);
        sw.Stop();
        Debug.Log($"System.Random.NextDouble: {sw.Elapsed.TotalMilliseconds / M:f4} ms");

        Unilox.Free();

        var unityResults = new float[N];
        UnityEngine.Random.InitState(0);

        sw.Restart();
        for (int j = 0; j < M; j++)
            for (int i = 0; i < unityResults.Length; i++)
                unityResults[i] = UnityEngine.Random.Range(0f, 1f);
        sw.Stop();

        Debug.Log($"Unity.Random.Range  : {sw.Elapsed.TotalMilliseconds / M:f4} ms");
    }
}