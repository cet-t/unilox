using UnityEngine;
using System;
using System.IO;
using System.Linq;

namespace Cet.Rng.Tests
{
    /// <summary>
    /// A simple visual and numerical test for PRNG quality.
    /// It generates 2D plots for visual inspection and calculates basic statistics
    /// (Mean, Variance, Chi-Squared) for an objective measure of randomness.
    /// </summary>
    public class URNGQualityTest : MonoBehaviour
    {
        private const int ImgSize = 512;
        private const int NumPoints = 200_000;
        private const int NumSamplesForStats = NumPoints * 2;

        #region Helper Classes
        /// <summary>
        /// A simple Linear Congruential Generator (LCG). Included as a visual baseline for a "bad" PRNG.
        /// </summary>
        private class SimpleLcg
        {
            private ulong _state;
            public SimpleLcg(ulong seed) { _state = seed; }
            private uint Next()
            {
                _state = 6364136223846793005 * _state + 1442695040888963407; // Knuth's MMIX
                return (uint)(_state >> 32);
            }
            public float NextF32() => (float)Next() / uint.MaxValue;
        }

        /// <summary>
        /// Calculates and logs basic statistical properties of a random number sample.
        /// </summary>
        private class NumericQualityAssessor
        {
            private const int ChiSquaredBins = 100;

            public void Assess(string rngName, float[] randoms)
            {
                int n = randoms.Length;
                if (n == 0) return;

                // --- 1. Mean ---
                double sum = 0;
                for (int i = 0; i < n; i++) sum += randoms[i];
                double mean = sum / n;

                // --- 2. Variance ---
                double varianceSum = 0;
                for (int i = 0; i < n; i++) varianceSum += (randoms[i] - mean) * (randoms[i] - mean);
                double variance = varianceSum / n;

                // --- 3. Chi-Squared Test ---
                int[] counts = new int[ChiSquaredBins];
                for (int i = 0; i < n; i++)
                {
                    int bin = (int)(randoms[i] * ChiSquaredBins);
                    if (bin >= 0 && bin < ChiSquaredBins)
                    {
                        counts[bin]++;
                    }
                }

                double expectedCount = (double)n / ChiSquaredBins;
                double chiSquared = 0;
                for (int i = 0; i < ChiSquaredBins; i++)
                {
                    chiSquared += ((counts[i] - expectedCount) * (counts[i] - expectedCount)) / expectedCount;
                }

                Debug.Log($"--- Stats for {rngName} ({n} samples) ---\n" +
                          $"Mean: {mean:F6} (Expected: ~0.5)\n" +
                          $"Variance: {variance:F6} (Expected: ~{1.0 / 12.0:F6})\n" +
                          $"Chi-Squared ({ChiSquaredBins} bins): {chiSquared:F2} (Expected: ~{ChiSquaredBins}.0)");
            }
        }
        #endregion

        void Start()
        {
            Debug.Log($"Generating {NumPoints} points and calculating stats from {NumSamplesForStats} samples for each RNG.");
            var assessor = new NumericQualityAssessor();

            // --- High-Quality PRNGs from Ultimate RNG (using batch generation) ---
            RunAllTests(assessor);

            // --- Debug & Non-Batch PRNGs ---
            Debug.Log("--- Testing Non-Ultimate and Debug RNGs ---");

            // Debugging Philox32 with single-shot generation to isolate the bug
            using (var rng = new Philox32(1, 2))
            {
                var randoms = GenerateSamplesFromFunc(() => (float)rng.Next() / uint.MaxValue, NumSamplesForStats);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Philox32_Debug_SingleShot", randoms);
                assessor.Assess("Philox32 (Single-Shot)", randoms);
            }

            var lcg = new SimpleLcg(1);
            var lcgRandoms = GenerateSamplesFromFunc(lcg.NextF32, NumSamplesForStats);
            GenerateAndSavePlotFromBatch("Unilox_Quality_SimpleLCG", lcgRandoms);
            assessor.Assess("SimpleLCG", lcgRandoms);

            var unityMathRandom = new Unity.Mathematics.Random(1);
            var unityRandoms = GenerateSamplesFromFunc(unityMathRandom.NextFloat, NumSamplesForStats);
            GenerateAndSavePlotFromBatch("Unilox_Quality_UnityMathematics", unityRandoms);
            assessor.Assess("Unity.Mathematics.Random", unityRandoms);

            Debug.Log("Quality test plots and stats are complete. Plots saved to project root.");
        }

        private void RunAllTests(NumericQualityAssessor assessor)
        {
            Debug.Log("--- Testing Ultimate RNGs ---");

            using (var rng = new Mt19937(1))
            {
                var randoms = rng.NextF32s(NumSamplesForStats).ToArray();
                GenerateAndSavePlotFromBatch("Unilox_Quality_Mt19937", randoms);
                assessor.Assess("Mt19937", randoms);
            }
            using (var rng = new Mt1993764(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Mt1993764", randoms);
                assessor.Assess("Mt1993764", randoms);
            }
            using (var rng = new Pcg32(1))
            {
                var randoms = rng.NextF32s(NumSamplesForStats).ToArray();
                GenerateAndSavePlotFromBatch("Unilox_Quality_Pcg32", randoms);
                assessor.Assess("Pcg32", randoms);
            }
            using (var rng = new Philox32(1, 2))  //<-- REMOVED FOR DEBUGGING
            {
                var randoms = rng.NextF32s(NumSamplesForStats).ToArray();
                GenerateAndSavePlotFromBatch("Unilox_Quality_Philox32", randoms);
                assessor.Assess("Philox32", randoms);
            }
            using (var rng = new Philox64(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Philox64", randoms);
                assessor.Assess("Philox64", randoms);
            }
            using (var rng = new Sfc64(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Sfc64", randoms);
                assessor.Assess("Sfc64", randoms);
            }
            using (var rng = new Sfmt19937(1))
            {
                var randoms = rng.NextF32s(NumSamplesForStats).ToArray();
                GenerateAndSavePlotFromBatch("Unilox_Quality_Sfmt19937", randoms);
                assessor.Assess("Sfmt19937", randoms);
            }
            using (var rng = new Sfmt1993764(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Sfmt1993764", randoms);
                assessor.Assess("Sfmt1993764", randoms);
            }
            using (var rng = new Xorshift128(1, 2, 3, 4))
            {
                var randoms = Array.ConvertAll(rng.NextF32s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Xorshift128", randoms);
                assessor.Assess("Xorshift128", randoms);
            }
            using (var rng = new Xorshift32(1))
            {
                var randoms = rng.NextF32s(NumSamplesForStats).ToArray();
                GenerateAndSavePlotFromBatch("Unilox_Quality_Xorshift32", randoms);
                assessor.Assess("Xorshift32", randoms);
            }
            using (var rng = new Xorshift64(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Xorshift64", randoms);
                assessor.Assess("Xorshift64", randoms);
            }
            using (var rng = new Xoshiro256Ss(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Xoshiro256Ss", randoms);
                assessor.Assess("Xoshiro256Ss", randoms);
            }
            using (var rng = new Xoshiro256Pp(1))
            {
                var randoms = Array.ConvertAll(rng.NextF64s(NumSamplesForStats).ToArray(), x => (float)x);
                GenerateAndSavePlotFromBatch("Unilox_Quality_Xoshiro256Pp", randoms);
                assessor.Assess("Xoshiro256Pp", randoms);
            }
        }

        private float[] GenerateSamplesFromFunc(Func<float> rngFunc, int count)
        {
            var randoms = new float[count];
            for (int i = 0; i < count; i++)
            {
                randoms[i] = rngFunc();
            }
            return randoms;
        }

        private void GenerateAndSavePlotFromBatch(string fileName, float[] randoms)
        {
            var texture = CreateBlackTexture();
            for (int i = 0; i < NumPoints; i++)
            {
                float x = randoms[i * 2];
                float y = randoms[i * 2 + 1];
                int px = (int)(x * ImgSize);
                int py = (int)(y * ImgSize);
                if (px >= 0 && px < ImgSize && py >= 0 && py < ImgSize)
                {
                    texture.SetPixel(px, py, Color.white);
                }
            }
            SaveTexture(fileName, texture);
        }

        private Texture2D CreateBlackTexture()
        {
            var texture = new Texture2D(ImgSize, ImgSize, TextureFormat.RGB24, false);
            var pixels = new Color[ImgSize * ImgSize];
            Array.Fill(pixels, Color.black);
            texture.SetPixels(pixels);
            return texture;
        }

        private void SaveTexture(string fileName, Texture2D texture)
        {
            texture.Apply();
            byte[] bytes = texture.EncodeToPNG();
            var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, fileName + ".png");
            File.WriteAllBytes(path, bytes);
            Destroy(texture);
        }
    }
}
