using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Cet.Rng.Job
{
    // =========================================================================
    //  Burst-compiled concrete Job structs for each 32-bit RNG
    // =========================================================================

    [BurstCompile]
    public struct SplitMix32Job : IJobParallelFor
    {
        [WriteOnly] public NativeArray<uint> Results;
        public uint Seed;

        public void Execute(int index)
        {
            var gen = default(SplitMix32);
            gen.Init(Seed + (uint)index);
            Results[index] = gen.NextU();
        }
    }

    [BurstCompile]
    public struct Mt19937Job : IJob
    {
        [WriteOnly] public NativeArray<uint> Results;
        public uint Seed;
        public int Count;

        public void Execute()
        {
            var gen = default(Mt19937);
            gen.Init(Seed);
            for (int i = 0; i < Count; i++)
            {
                Results[i] = gen.NextU();
            }
        }
    }

    // =========================================================================
    //  Burst-compiled concrete Job structs for each 64-bit RNG
    // =========================================================================

    [BurstCompile]
    public struct Xoshiro256PpJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<ulong> Results;
        public ulong Seed;

        public void Execute(int index)
        {
            var gen = default(Xoshiro256Pp);
            gen.Init(Seed + (ulong)index);
            Results[index] = gen.NextU();
        }
    }

    [BurstCompile]
    public struct Xoshiro256SsJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<ulong> Results;
        public ulong Seed;

        public void Execute(int index)
        {
            var gen = default(Xoshiro256Ss);
            gen.Init(Seed + (ulong)index);
            Results[index] = gen.NextU();
        }
    }

    // =========================================================================
    //  Seq wrappers — thin API layer over the concrete Burst Jobs
    // =========================================================================

    public struct SplitMix32Seq : IDisposable
    {
        public NativeArray<uint> Results;

        public SplitMix32Seq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
        }

        public NativeArray<uint> Fill(int count, uint seed)
        {
            new SplitMix32Job { Results = Results, Seed = seed | 1 }
                .Schedule(count, 64).Complete();
            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated) Results.Dispose();
        }
    }

    public struct Mt19937Seq : IDisposable
    {
        public NativeArray<uint> Results;

        public Mt19937Seq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
        }

        public NativeArray<uint> Fill(int count, uint seed)
        {
            new Mt19937Job { Results = Results, Seed = seed | 1, Count = count }
                .Schedule().Complete();
            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated) Results.Dispose();
        }
    }

    public struct Xoshiro256PpSeq : IDisposable
    {
        public NativeArray<ulong> Results;

        public Xoshiro256PpSeq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
        }

        public NativeArray<ulong> Fill(int count, ulong seed)
        {
            new Xoshiro256PpJob { Results = Results, Seed = seed | 1 }
                .Schedule(count, 64).Complete();
            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated) Results.Dispose();
        }
    }

    public struct Xoshiro256SsSeq : IDisposable
    {
        public NativeArray<ulong> Results;

        public Xoshiro256SsSeq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
        }

        public NativeArray<ulong> Fill(int count, ulong seed)
        {
            new Xoshiro256SsJob { Results = Results, Seed = seed | 1 }
                .Schedule(count, 64).Complete();
            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated) Results.Dispose();
        }
    }
}