using System;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Cet.Rng.Job
{
    [BurstCompile]
    public struct Sfc32Job
    {
        private uint a, b, c;
        private uint counter;

        public Sfc32Job(uint seed)
        {
            seed |= 1;
            a = seed;
            b = seed >> 3;
            c = seed ^ 0xfa03f90d;
            counter = 1;

            // warm-up
            for (int i = 0; i < 10; i++)
            {
                _ = NextU32();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32()
        {
            unchecked
            {
                var res = a + b + counter++;
                a = b ^ (b >> 9);
                b = c + (c << 3);
                c = res + (c << 21 | c >> 11); // 21+11 rotate
                return res;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextF32()
        {
            return NextU32() * (1f / (uint.MaxValue + 1f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int RandI32(int min, int max)
        {
            var range = max - min + 1;
            return (int)((NextU32() * range) >> 64) + min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float RandF32(float min, float max)
        {
            var range = max - min;
            var scale = range * (1f / (uint.MaxValue + 1f));
            return NextU32() * scale + min;
        }
    }

    [BurstCompile]
    public struct Philox32Job
    {
        // private uint4 c;
        private uint c0, c1, c2, c3;
        private uint k0, k1;

        private const uint m0 = 0xd2511f53;
        private const uint m1 = 0xcd9e8d57;

        public Philox32Job(uint seed1, uint seed2)
        {
            c0 = 1;
            c1 = c2 = c3 = 0;
            // c = new(1, 0, 0, 0);
            k0 = seed1;
            k1 = seed2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint4 NextU32()
        {
            unchecked
            {
                // var p = new uint4(c.a * m0, 0, c.c * m1, 0);
                // var o = c;
                // o.a = p.a ^ c.b ^ k0;
                // o.b = p.a;
                // o.c = p.c ^ c.d ^ k1;
                // o.d = p.c;
                // c++;
                // return o;
                var p0 = c0 * m0;
                var p1 = c2 * m1;
                uint o0 = p0 ^ c1 ^ k0;
                uint o1 = p0;
                uint o2 = p1 ^ c3 ^ k1;
                uint o3 = p1;
                c0++;
                c1++;
                c2++;
                c3++;
                return new(o0, o1, o2, o3);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float4 NextF32()
        {
            return NextU32() * (1f / (uint.MaxValue + 1f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int4 RandI32(int min, int max)
        {
            var o = NextU32() * (max - min + 1);
            o.a = (o.a >> 32) + min;
            o.b = (o.b >> 32) + min;
            o.c = (o.c >> 32) + min;
            o.d = (o.d >> 32) + min;
            return new((int)o.a, (int)o.b, (int)o.c, (int)o.d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float4 RandF32(float min, float max)
        {
            var scale = (max - min) * (1f / (uint.MaxValue + 1f));
            var o = NextU32() * scale;
            o.a += min;
            o.b += min;
            o.c += min;
            o.d += min;
            return o;
        }

        public void Dispose()
        {
            // c.Dispose();
            // k.Dispose();
        }
    }
}
