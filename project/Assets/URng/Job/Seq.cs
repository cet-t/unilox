using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Cet.Rng.Job
{
    public struct URng32Seq<T> : IDisposable where T : struct, IRng32Job
    {
        public NativeArray<uint> Results;
        private Allocator _allocator;

        public URng32Seq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
            _allocator = allocator;
        }

        public NativeArray<uint> Fill(int count, uint seed)
        {
            var job = new URng32Job<T>
            {
                Results = Results,
                Seed = seed | 1
            };

            job.Schedule(count, 64).Complete();

            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated)
            {
                Results.Dispose();
            }
        }
    }

    [BurstCompile]
    struct URng32Job<T> : IJobParallelFor
        where T : struct, IRng32Job, IDisposable
    {
        [WriteOnly] public NativeArray<uint> Results;
        public uint Seed;

        public void Execute(int index)
        {
            using T gen = default;
            gen.Init(Seed + (uint)index);
            Results[index] = gen.NextU();
        }
    }

    public struct URng64Seq<T> : IDisposable where T : struct, IRng64Job
    {
        public NativeArray<ulong> Results;
        private Allocator _allocator;

        public URng64Seq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
            _allocator = allocator;
        }

        public NativeArray<ulong> Fill(int count, ulong seed)
        {
            var job = new URng64Job<T>
            {
                Results = Results,
                Seed = seed | 1
            };

            job.Schedule(count, 64).Complete();

            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated)
            {
                Results.Dispose();
            }
        }
    }

    [BurstCompile]
    struct URng64Job<T> : IJobParallelFor
    where T : struct, IRng64Job, IDisposable
    {
        [WriteOnly] public NativeArray<ulong> Results;
        public ulong Seed;

        public void Execute(int index)
        {
            using T gen = default;
            gen.Init(Seed + (ulong)index);
            Results[index] = gen.NextU();
        }
    }

}