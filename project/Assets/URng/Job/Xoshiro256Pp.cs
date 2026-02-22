using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Xoshiro256Pp : IRng64Job
    {
        private ulong a, b, c, d;

        public void Init(ulong seed)
        {
            this.a = seed ^ 0xaf6bcb117fc38b58;
            this.b = seed ^ 0x8634ddd9ad84f19f;
            this.c = seed ^ 0x277bff772d63c7e2;
            this.d = seed ^ 0x4b575eff6b159516;
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

            this.c ^= t;
            this.d = this.c.RotateLeft(45);

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
