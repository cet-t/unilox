using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct SplitMix32 : IRng32Job
    {
        private uint state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(uint seed)
        {
            state = seed | 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU()
        {
            state += 0x9E3779B9;

            var z = state;
            z = (z ^ (z >> 16)) + 0x85EBCA6B;
            z = (z ^ (z >> 13)) + 0xC2B2AE35;
            return z ^ (z >> 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF()
        {
            return NextU() * Consts.U2F;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int RandI(int min, int max)
        {
            var range = (ulong)(max - min + 1);
            return (int)((NextU() * range) >> 32) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RandF(float min, float max)
        {
            var range = max - min;
            var scale = range * Consts.U2F;
            return (NextU() * scale) + min;
        }

        public void Dispose()
        {
        }
    }
}
