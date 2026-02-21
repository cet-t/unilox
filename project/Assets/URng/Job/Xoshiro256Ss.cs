using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Xoshiro256Ss : IRng64Job
    {
        private ulong a, b, c, d;

        public void Init(ulong seed)
        {
            this.a = seed ^ 0x0123;
            this.b = seed ^ 0x4567;
            this.c = seed ^ 0x89ab;
            this.d = seed ^ 0xcdef;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU()
        {
            var res = (this.a + this.c).RotateLeft(23) + this.a;
            var t = this.b << 17;

            this.c ^= this.a;
            this.d ^= this.b;
            this.b ^= this.c;
            this.a ^= this.d;
            this.a ^= t;
            this.c = this.c.RotateLeft(45);
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
            var range = (ulong)(max - min + 1);
            return (long)((NextU() * range) >> 32) + min;
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
