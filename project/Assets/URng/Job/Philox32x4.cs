using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Philox32x4 : IRng32Job
    {
        private const uint PHILOX_M0 = 0xD2511F53u;
        private const uint PHILOX_M1 = 0xCD9E8D57u;
        private const uint PHILOX_W0 = 0x9E3779B9u;
        private const uint PHILOX_W1 = 0xBB67AE85u;

        private uint _c0, _k0, _k1;
        private uint _buf0, _buf1, _buf2, _buf3;
        private int _bufIdx;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(uint seed)
        {
            _k0 = seed;
            _k1 = seed ^ 0xBB67AE85u;
            _c0 = 0;
            _bufIdx = 4;
            _buf0 = _buf1 = _buf2 = _buf3 = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Bijection10(
            uint c0, uint c1, uint c2, uint c3,
            uint k0, uint k1,
            out uint o0, out uint o1, out uint o2, out uint o3)
        {
            ulong p0, p1;
            uint h0, l0, h1, l1;

            // Round 0
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 1
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 2
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 3
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 4
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 5
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 6
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 7
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 8
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;
            k0 += PHILOX_W0; k1 += PHILOX_W1;
            // Round 9
            p0 = (ulong)PHILOX_M0 * c0; h0 = (uint)(p0 >> 32); l0 = (uint)p0;
            p1 = (ulong)PHILOX_M1 * c2; h1 = (uint)(p1 >> 32); l1 = (uint)p1;
            c0 = h1 ^ c1 ^ k0; c1 = l1; c2 = h0 ^ c3 ^ k1; c3 = l0;

            o0 = c0; o1 = c1; o2 = c2; o3 = c3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU()
        {
            if (_bufIdx >= 4)
            {
                Bijection10(_c0, 0, 0, 0, _k0, _k1,
                    out _buf0, out _buf1, out _buf2, out _buf3);
                _c0++;
                _bufIdx = 0;
            }
            switch (_bufIdx)
            {
                case 0: _bufIdx = 1; return _buf0;
                case 1: _bufIdx = 2; return _buf1;
                case 2: _bufIdx = 3; return _buf2;
                default: _bufIdx = 4; return _buf3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF() => NextU() * Consts.U2F;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int RandI(int min, int max)
        {
            var range = (ulong)(max - min + 1);
            return (int)((NextU() * range) >> 32) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RandF(float min, float max)
        {
            return (NextU() * ((max - min) * Consts.U2F)) + min;
        }

        public void Dispose() { }
    }

    // ==== IJob: unsafe pointer で境界チェックを完全排除 ====
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Philox32x4Job : IJob
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<uint> Results;
        public uint Key0, Key1;
        public int Count;

        public unsafe void Execute()
        {
            uint* ptr = (uint*)Results.GetUnsafePtr();
            uint k0 = Key0, k1 = Key1;
            int blocks = Count >> 2;

            for (int i = 0; i < blocks; i++)
            {
                Philox32x4.Bijection10(
                    (uint)i, 0u, 0u, 0u, k0, k1,
                    out uint o0, out uint o1, out uint o2, out uint o3);

                int b = i << 2;
                ptr[b]     = o0;
                ptr[b + 1] = o1;
                ptr[b + 2] = o2;
                ptr[b + 3] = o3;
            }
        }
    }

    // ==== IJobParallelFor: 各indexで4 uint生成 ====
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct Philox32x4ParallelJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<uint> Results;
        public uint Key0, Key1;

        public unsafe void Execute(int index)
        {
            uint* ptr = (uint*)Results.GetUnsafePtr();
            Philox32x4.Bijection10(
                (uint)index, 0u, 0u, 0u, Key0, Key1,
                out uint o0, out uint o1, out uint o2, out uint o3);

            int b = index << 2;
            ptr[b]     = o0;
            ptr[b + 1] = o1;
            ptr[b + 2] = o2;
            ptr[b + 3] = o3;
        }
    }

    // ==== Seq wrapper ====
    public struct Philox32x4Seq : IDisposable
    {
        public NativeArray<uint> Results;

        public Philox32x4Seq(int maxCount, Allocator allocator = Allocator.TempJob)
        {
            Results = new(maxCount, allocator);
        }

        public NativeArray<uint> Fill(int count, uint seed)
        {
            uint k0 = seed;
            uint k1 = seed ^ 0xBB67AE85u;
            new Philox32x4Job
            {
                Results = Results, Key0 = k0, Key1 = k1, Count = count
            }.Schedule().Complete();
            return Results;
        }

        public NativeArray<uint> FillParallel(int count, uint seed, int batchSize = 256)
        {
            uint k0 = seed;
            uint k1 = seed ^ 0xBB67AE85u;
            new Philox32x4ParallelJob
            {
                Results = Results, Key0 = k0, Key1 = k1
            }.Schedule(count >> 2, batchSize).Complete();
            return Results;
        }

        public void Dispose()
        {
            if (Results.IsCreated) Results.Dispose();
        }
    }
}
