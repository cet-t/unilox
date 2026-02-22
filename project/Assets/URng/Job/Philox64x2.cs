using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Philox64x2 : IRng64Job
    {
        private const ulong PHILOX_M = 0xD2E7470EE14C6C93uL;
        private const ulong PHILOX_W = 0x9E3779B97F4A7C15uL;

        private ulong _c0, _k0;
        private ulong _buf0, _buf1;
        private int _bufIdx;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(ulong seed)
        {
            _k0 = seed;
            _c0 = 0;
            _bufIdx = 2;
            _buf0 = _buf1 = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong MulHi64(ulong a, ulong b)
        {
            ulong a_lo = (uint)a, a_hi = a >> 32;
            ulong b_lo = (uint)b, b_hi = b >> 32;
            ulong p0 = a_lo * b_lo;
            ulong p1 = a_lo * b_hi;
            ulong p2 = a_hi * b_lo;
            ulong p3 = a_hi * b_hi;
            ulong mid = (p0 >> 32) + (uint)p1 + (uint)p2;
            return p3 + (p1 >> 32) + (p2 >> 32) + (mid >> 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Bijection10(
            ulong c0, ulong c1, ulong k0,
            out ulong o0, out ulong o1)
        {
            ulong hi, lo;

            // Round 0
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 1
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 2
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 3
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 4
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 5
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 6
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 7
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 8
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo; k0 += PHILOX_W;
            // Round 9
            hi = MulHi64(PHILOX_M, c0); lo = PHILOX_M * c0;
            c0 = hi ^ c1 ^ k0; c1 = lo;

            o0 = c0; o1 = c1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU()
        {
            if (_bufIdx >= 2)
            {
                Bijection10(_c0, 0, _k0, out _buf0, out _buf1);
                _c0++;
                _bufIdx = 0;
            }
            if (_bufIdx == 0) { _bufIdx = 1; return _buf0; }
            else { _bufIdx = 2; return _buf1; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextF() => (NextU() >> 11) * (1.0 / (1L << 53));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long RandI(long min, long max)
        {
            var range = (ulong)(max - min + 1);
            return (long)((NextU() * range) >> 32) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double RandF(double min, double max) => NextF() * (max - min) + min;

        public void Dispose() { }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Philox64x2Job : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ulong> Results;
        public ulong Key0;
        public int Count;

        public unsafe void Execute()
        {
            ulong* ptr = (ulong*)Results.GetUnsafePtr();
            ulong k0 = Key0;
            int blocks = Count >> 1;

            for (int i = 0; i < blocks; i++)
            {
                Philox64x2.Bijection10((ulong)i, 0uL, k0, out ulong o0, out ulong o1);
                int b = i << 1;
                ptr[b]     = o0;
                ptr[b + 1] = o1;
            }
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Philox64x2ParallelJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ulong> Results;
        public ulong Key0;

        public unsafe void Execute(int index)
        {
            ulong* ptr = (ulong*)Results.GetUnsafePtr();
            Philox64x2.Bijection10((ulong)index, 0uL, Key0, out ulong o0, out ulong o1);
            int b = index << 1;
            ptr[b]     = o0;
            ptr[b + 1] = o1;
        }
    }

    public struct Philox64x2Seq : IDisposable
    {
        public NativeArray<ulong> Results;

        public Philox64x2Seq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
        }

        public NativeArray<ulong> Fill(int count, uint seed)
        {
            new Philox64x2Job
            {
                Results = Results, Key0 = seed, Count = count
            }.Schedule().Complete();
            return Results;
        }

        public NativeArray<ulong> FillParallel(int count, uint seed, int batchSize = 256)
        {
            new Philox64x2ParallelJob
            {
                Results = Results, Key0 = seed
            }.Schedule(count >> 1, batchSize).Complete();
            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated) Results.Dispose();
        }
    }
}
