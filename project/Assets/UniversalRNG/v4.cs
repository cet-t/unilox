using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cet.Rng.Job
{
    [StructLayout(LayoutKind.Sequential)]
    public struct uint4
    {
        public uint a, b, c, d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint4(uint a, uint b, uint c, uint d)
        {
            this.a = a; this.b = b; this.c = c; this.d = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint4(uint v)
        {
            a = b = c = d = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator +(uint4 x, uint4 y)
        {
            x.a += y.a; x.b += y.b; x.c += y.c; x.d += y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator -(uint4 x, uint4 y)
        {
            x.a -= y.a; x.b -= y.b; x.c -= y.c; x.d -= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator *(uint4 x, uint4 y)
        {
            x.a *= y.a; x.b *= y.b; x.c *= y.c; x.d *= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator /(uint4 x, uint4 y)
        {
            x.a /= y.a; x.b /= y.b; x.c /= y.c; x.d /= y.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator &(uint4 x, uint4 y)
        {
            x.a &= y.a; x.b &= y.b; x.c &= y.c; x.d &= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator |(uint4 x, uint4 y)
        {
            x.a |= y.a; x.b |= y.b; x.c |= y.c; x.d |= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator ^(uint4 x, uint4 y)
        {
            x.a ^= y.a; x.b ^= y.b; x.c ^= y.c; x.d ^= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator ~(uint4 x)
        {
            x.a = ~x.a; x.b = ~x.b; x.c = ~x.c; x.d = ~x.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator <<(uint4 x, int n)
        {
            x.a <<= n; x.b <<= n; x.c <<= n; x.d <<= n;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator >>(uint4 x, int n)
        {
            x.a >>= n; x.b >>= n; x.c >>= n; x.d >>= n;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator ++(uint4 x)
        {
            x.a++; x.b++; x.c++; x.d++;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator --(uint4 x)
        {
            x.a--; x.b--; x.c--; x.d--;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator *(uint4 x, uint y)
        {
            x.a *= y; x.b *= y; x.c *= y; x.d *= y;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint4 operator *(uint x, uint4 y) => y * x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator *(uint4 x, int y) => new long4(x.a * y, x.b * y, x.c * y, x.d * y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator *(int x, uint4 y) => y * x;



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(uint4 x, float4 y)
        {
            y.a *= x.a; y.b *= x.b; y.c *= x.c; y.d *= x.d;
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(uint4 x, float y) => new float4(x.a * y, x.b * y, x.c * y, x.d * y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(float x, uint4 y) => y * x;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct int4
    {
        public int a, b, c, d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int4(int a, int b, int c, int d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int4(int v)
        {
            a = b = c = d = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator +(int4 x, int4 y)
        {
            x.a += y.a; x.b += y.b; x.c += y.c; x.d += y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator -(int4 x, int4 y)
        {
            x.a -= y.a; x.b -= y.b; x.c -= y.c; x.d -= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator *(int4 x, int4 y)
        {
            x.a *= y.a; x.b *= y.b; x.c *= y.c; x.d *= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator /(int4 x, int4 y)
        {
            x.a /= y.a; x.b /= y.b; x.c /= y.c; x.d /= y.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator -(int4 x)
        {
            x.a = -x.a; x.b = -x.b; x.c = -x.c; x.d = -x.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator &(int4 x, int4 y)
        {
            x.a &= y.a; x.b &= y.b; x.c &= y.c; x.d &= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator |(int4 x, int4 y)
        {
            x.a |= y.a; x.b |= y.b; x.c |= y.c; x.d |= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator ^(int4 x, int4 y)
        {
            x.a ^= y.a; x.b ^= y.b; x.c ^= y.c; x.d ^= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator ~(int4 x)
        {
            x.a = ~x.a; x.b = ~x.b; x.c = ~x.c; x.d = ~x.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator <<(int4 x, int n)
        {
            x.a <<= n; x.b <<= n; x.c <<= n; x.d <<= n;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator >>(int4 x, int n)
        {
            x.a >>= n; x.b >>= n; x.c >>= n; x.d >>= n;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator ++(int4 x)
        {
            x.a++; x.b++; x.c++; x.d++;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator --(int4 x)
        {
            x.a--; x.b--; x.c--; x.d--;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator *(int4 x, int y)
        {
            x.a *= y; x.b *= y; x.c *= y; x.d *= y;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 operator *(int x, int4 y) => y * x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(int4 x, float4 y)
        {
            y.a *= x.a; y.b *= x.b; y.c *= x.c; y.d *= x.d;
            return y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(int4 x, float y) => new float4(x.a * y, x.b * y, x.c * y, x.d * y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(float x, int4 y) => y * x;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct float4
    {
        public float a, b, c, d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float4(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float4(float v)
        {
            a = b = c = d = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator +(float4 x, float4 y)
        {
            x.a += y.a; x.b += y.b; x.c += y.c; x.d += y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator -(float4 x, float4 y)
        {
            x.a -= y.a; x.b -= y.b; x.c -= y.c; x.d -= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(float4 x, float4 y)
        {
            x.a *= y.a; x.b *= y.b; x.c *= y.c; x.d *= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator /(float4 x, float4 y)
        {
            x.a /= y.a; x.b /= y.b; x.c /= y.c; x.d /= y.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator -(float4 x)
        {
            x.a = -x.a; x.b = -x.b; x.c = -x.c; x.d = -x.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(float4 x, float y)
        {
            x.a *= y; x.b *= y; x.c *= y; x.d *= y;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(float x, float4 y) => y * x;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct long4
    {
        public long a, b, c, d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long4(long a, long b, long c, long d)
        {
            this.a = a; this.b = b; this.c = c; this.d = d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long4(long v)
        {
            a = b = c = d = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator +(long4 x, long4 y)
        {
            x.a += y.a; x.b += y.b; x.c += y.c; x.d += y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator -(long4 x, long4 y)
        {
            x.a -= y.a; x.b -= y.b; x.c -= y.c; x.d -= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator *(long4 x, long4 y)
        {
            x.a *= y.a; x.b *= y.b; x.c *= y.c; x.d *= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator /(long4 x, long4 y)
        {
            x.a /= y.a; x.b /= y.b; x.c /= y.c; x.d /= y.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator -(long4 x)
        {
            x.a = -x.a; x.b = -x.b; x.c = -x.c; x.d = -x.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator &(long4 x, long4 y)
        {
            x.a &= y.a; x.b &= y.b; x.c &= y.c; x.d &= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator |(long4 x, long4 y)
        {
            x.a |= y.a; x.b |= y.b; x.c |= y.c; x.d |= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator ^(long4 x, long4 y)
        {
            x.a ^= y.a; x.b ^= y.b; x.c ^= y.c; x.d ^= y.d;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator ~(long4 x)
        {
            x.a = ~x.a; x.b = ~x.b; x.c = ~x.c; x.d = ~x.d;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator <<(long4 x, int n)
        {
            x.a <<= n; x.b <<= n; x.c <<= n; x.d <<= n;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator >>(long4 x, int n)
        {
            x.a >>= n; x.b >>= n; x.c >>= n; x.d >>= n;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator ++(long4 x)
        {
            x.a++; x.b++; x.c++; x.d++;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator --(long4 x)
        {
            x.a--; x.b--; x.c--; x.d--;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator *(long4 x, long y)
        {
            x.a *= y; x.b *= y; x.c *= y; x.d *= y;
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long4 operator *(long x, long4 y) => y * x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(long4 x, float4 y)
        {
            y.a *= x.a; y.b *= x.b; y.c *= x.c; y.d *= x.d;
            return y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(long4 x, float y) => new float4(x.a * y, x.b * y, x.c * y, x.d * y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 operator *(float x, long4 y) => y * x;
    }
}