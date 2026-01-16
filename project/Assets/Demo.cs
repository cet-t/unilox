using System;
using System.Linq;
using Cet.Rng;
using UnityEngine;

public class UniloxNoiseDemo : MonoBehaviour
{
    [SerializeField] private int textureSize = 512;

    void Start()
    {
        GenerateSeededNoise(12345, 67890, "SeedA");
        GenerateSeededNoise(99999, 11111, "SeedB");
        GenerateSeededNoise(314159, 271828, "SeedC");
    }

    public void GenerateSeededNoise(uint seed1, uint seed2, string filename)
    {
        int count = 256 * 256;
        const int BufferSize = 4096;
        Unilox.Init(BufferSize, seed1, seed2);

        var noise = new float[count];
        for (int i = 0; i < count; i += BufferSize)
        {
            Unilox.Randfloats(Math.Min(BufferSize, count - i), 0f, 1f).CopyTo(noise.AsSpan(i));
        }

        var indices = ShuffleArray(count);
        Unilox.Free();

        var tex = new Texture2D(256, 256);
        var pixels = new Color[256 * 256];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(noise[indices[i]], noise[indices[i]], noise[indices[i]], 1f);
        }

        tex.SetPixels(pixels);
        tex.Apply();

        System.IO.File.WriteAllBytes($"Unilox_Seed_{seed1}_{seed2}.png", tex.EncodeToPNG());
    }

    int[] ShuffleArray(int length)
    {
        var array = new int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = i;
        }

        var randoms = new float[length];
        const int BufferSize = 4096;
        for (int k = 0; k < length; k += BufferSize)
        {
            Unilox.Randfloats(Math.Min(BufferSize, length - k), 0f, 1f).CopyTo(randoms.AsSpan(k));
        }

        for (int i = length - 1; i > 0; i--)
        {
            int j = (int)(randoms[i] * (i + 1));
            (array[j], array[i]) = (array[i], array[j]);
        }
        return array;
    }
}
