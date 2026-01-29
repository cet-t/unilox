using System;
using Cet.Rng;
using Unity.Collections;
using UnityEngine;

namespace Cet.Rng.Tests
{
    public class PointGenerator : MonoBehaviour
    {
        [SerializeField]
        private GameObject pointObj;

        [SerializeField]
        private Transform pointParent;

        [SerializeField]
        [Range(1, 1_000)]
        private int pointCount = 50;

        private readonly Vector2 MAX = new(9f, 5f);

        private void Start()
        {
            var sw = new System.Diagnostics.Stopwatch();

            // Pcg32
            using (IRng32 pcg32 = new Pcg32(0))
            {
                _ = pcg32.NextU32s(1);
                using var x = new NativeArray<float>(pointCount, Allocator.Temp);
                using var y = new NativeArray<float>(pointCount, Allocator.Temp);

                sw.Restart();
                pcg32.RandF32s(x, -MAX.x, 0);
                pcg32.RandF32s(y, 0, MAX.y);
                sw.Stop();
                Debug.Log($"{nameof(Pcg32)}: {sw.Elapsed.TotalMilliseconds} ms");

                for (int i = 0; i < pointCount; i++)
                {
                    var point = Instantiate(pointObj, pointParent);
                    point.transform.position = new(x[i], y[i]);
                }
            }

            // Mt19937
            using (IRng32 mt32 = new Mt19937(0))
            {
                _ = mt32.NextU32s(1);
                using var x = new NativeArray<float>(pointCount, Allocator.Temp);
                using var y = new NativeArray<float>(pointCount, Allocator.Temp);

                sw.Restart();
                mt32.RandF32s(x, 0, MAX.x);
                mt32.RandF32s(y, 0, MAX.y);
                sw.Stop();
                Debug.Log($"{nameof(Mt19937)}: {sw.Elapsed.TotalMilliseconds} ms");

                for (int i = 0; i < pointCount; i++)
                {
                    var point = Instantiate(pointObj, pointParent);
                    point.transform.position = new(x[i], y[i]);
                }
            }

            // Sfc64
            using (IRng64 sfc64 = new Sfc64(0))
            {
                _ = sfc64.NextU64s(1);
                using var x = new NativeArray<double>(pointCount, Allocator.Temp);
                using var y = new NativeArray<double>(pointCount, Allocator.Temp);

                sw.Restart();
                sfc64.RandF64s(x, -MAX.x, 0);
                sfc64.RandF64s(y, -MAX.y, 0);
                sw.Stop();
                Debug.Log($"{nameof(Sfc64)}: {sw.Elapsed.TotalMilliseconds} ms");

                for (int i = 0; i < pointCount; i++)
                {
                    var point = Instantiate(pointObj, pointParent);
                    point.transform.position = new((float)x[i], (float)y[i]);
                }
            }

            // Xorshift64
            using (IRng64 xs64 = new Xorshift64(1))
            {
                _ = xs64.NextU64s(1);
                using var x = new NativeArray<double>(pointCount, Allocator.Temp);
                using var y = new NativeArray<double>(pointCount, Allocator.Temp);

                sw.Restart();
                xs64.RandF64s(x, 0, MAX.x);
                xs64.RandF64s(y, -MAX.y, 0);
                sw.Stop();
                Debug.Log($"{nameof(Xorshift64)}: {sw.Elapsed.TotalMilliseconds} ms");

                for (int i = 0; i < pointCount; i++)
                {
                    var point = Instantiate(pointObj, pointParent);
                    point.transform.position = new((float)x[i], (float)y[i]);
                }
            }
        }
    }
}