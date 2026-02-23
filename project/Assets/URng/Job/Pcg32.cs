using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Pcg32 : IRng32Job
    {
        private ulong state;
        private ulong inc;

        public void Init(uint seed)
        {
            this.state = seed;
            this.inc = seed | 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU()
        {
            var oldstate = this.state;
            this.state = unchecked(oldstate * 6364136223846793005 + this.inc);
            var xorshifted = (uint)unchecked(((oldstate >> 18) ^ oldstate) >> 27);
            var rot = (int)unchecked(oldstate >> 59);
            return (xorshifted >> rot) | (xorshifted << (-rot & 31));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF()
        {
            return NextU() * Consts.U2F;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int RandI(int min, int max)
        {
            var range = (uint)(max - min + 1);
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
