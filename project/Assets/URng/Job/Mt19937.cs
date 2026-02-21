using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public unsafe struct Mt19937 : IRng32Job
    {
        public fixed uint mt[MT32_N];
        private int mti;

        const int MT32_N = 624;
        const int MT32_M = 397;
        const uint MT32_MATRIX_A = 0x9908B0DF;
        const uint MT32_UPPER_MASK = 0x80000000;
        const uint MT32_LOWER_MASK = 0x7FFFFFFF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(uint seed)
        {
            mt[0] = seed;
            for (uint i = 1; i < MT32_N; i++)
            {
                mt[i] = 1812433253u * (mt[i - 1] ^ (mt[i - 1] >> 30)) + i;
            }

            mti = MT32_N;
        }

        private void Twist()
        {
            for (int i = 0; i < MT32_N - MT32_M; i++)
            {
                var x = (mt[i] & MT32_UPPER_MASK) | (mt[i + 1] & MT32_LOWER_MASK);
                var x_a = x >> 1;
                if ((x & 1) != 0)
                {
                    x_a ^= MT32_MATRIX_A;
                }
                mt[i] = mt[i + MT32_M] ^ x_a;
            }
            for (int i = MT32_N - MT32_M; i < MT32_N - 1; i++)
            {
                var x = (mt[i] & MT32_UPPER_MASK) | (mt[i + 1] & MT32_LOWER_MASK);
                var x_a = x >> 1;
                if ((x & 1) != 0)
                {
                    x_a ^= MT32_MATRIX_A;
                }
                mt[i] = mt[i + MT32_M - MT32_N] ^ x_a;
            }
            {
                var x = (mt[MT32_N - 1] & MT32_UPPER_MASK) | (mt[0] & MT32_LOWER_MASK);
                var x_a = x >> 1;
                if ((x & 1) != 0)
                {
                    x_a ^= MT32_MATRIX_A;
                }
                mt[MT32_N - 1] = mt[MT32_M - 1] ^ x_a;
                mti = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU()
        {
            if (mti >= MT32_N)
            {
                Twist();
            }
            var y = mt[mti];
            mti += 1;
            y = unchecked(y ^ y >> 11);
            y = unchecked(y ^ (y << 7) & 0x9D2C5680);
            y = unchecked(y ^ (y << 15) & 0xEFC60000);
            y = unchecked(y ^ y >> 18);
            return y;
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
