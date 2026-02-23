using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct SplitMix64 : IRng64Job
    {
        private ulong s;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(ulong seed)
        {
            s = seed | 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ulong NextU()
        {
            this.s += 0x9E3779B97F4A7C15;
            var result = this.s;
            result = unchecked((result ^ (result >> 30)) * 0xBF58476D1CE4E5B9);
            result = unchecked((result ^ (result >> 27)) * 0x94D049BB133111EB);
            return result ^ (result >> 31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextF()
        {
            return NextU() * Consts.U2F;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long RandI(long min, long max)
        {
            var range = (ulong)(max - min + 1);
            return (long)((NextU() * range) >> 64) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double RandF(double min, double max)
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
