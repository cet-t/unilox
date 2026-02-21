using System.Runtime.CompilerServices;

namespace Cet.Rng.Job
{
    public readonly struct Consts
    {
        public const float U2F = 1f / 4294967296f;
    }

    public static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(this ulong value, int shift)
        {
            return (value << shift) | (value >> (64 - shift));
        }
    }
}