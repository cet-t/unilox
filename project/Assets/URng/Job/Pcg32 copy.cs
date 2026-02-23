using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Sfc64 : IRng64Job
    {
        private ulong a;
        private ulong b;
        private ulong c;
        private ulong counter;

        public void Init(ulong seed)
        {
            this.a = seed ^ (seed >> 30);
            this.b = seed ^ (seed >> 27);
            this.c = seed ^ (seed >> 23);
            this.counter = seed | 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU()
        {
            var res = unchecked(unchecked(this.a + this.b) + this.counter);
            this.a = this.b ^ (this.b >> 11);
            this.b = unchecked(this.c + (this.c << 3));
            this.c = res.RotateLeft(24);

            this.counter = unchecked(this.counter + 1);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextF()
        {
            return NextU() * Consts.U2F;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long RandI(long min, long max)
        {
            var range = (uint)(max - min + 1);
            return (int)((NextU() * range) >> 32) + min;
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
