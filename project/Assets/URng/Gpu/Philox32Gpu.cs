using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Cet.Rng.Gpu
{
    /// <summary>
    /// Philox 4x32-10 counter-based RNG implemented on GPU via ComputeShader.
    /// Each dispatch produces 4 uint per thread using the Philox algorithm.
    /// </summary>
    [Serializable]
    public class Philox32Gpu : IDisposable
    {
        [SerializeField] ComputeShader shader;

        const int ThreadGroupSize = 256;
        const int CacheSize = 4096;

        // --- internal state ---
        uint _key0, _key1;
        uint _counterBase;

        // single-value cache
        uint[] _cache;
        int _cachePos;
        ComputeBuffer _cacheBuf;

        // reusable batch buffers (grow-only)
        ComputeBuffer _batchBuf;
        int _batchBufLen;
        uint[] _uintPool;
        float[] _floatPool;
        int[] _intPool;

        // kernel ids
        int _kUint, _kFloat, _kInt;
        bool _initialized;

        /// <summary>Initializes with a single seed.</summary>
        public void Init(uint seed)
        {
            Init(seed, seed ^ 0xBB67AE85u);
        }

        /// <summary>Initializes with two seed values used as the Philox key.</summary>
        public void Init(uint seed0, uint seed1)
        {
            _key0 = seed0;
            _key1 = seed1;
            _counterBase = 0;
            _cachePos = CacheSize; // force refill

            _kUint  = shader.FindKernel("CSGenerateUint");
            _kFloat = shader.FindKernel("CSGenerateRangeFloat");
            _kInt   = shader.FindKernel("CSGenerateRangeInt");

            _cache = new uint[CacheSize];
            _cacheBuf?.Release();
            _cacheBuf = new ComputeBuffer(CacheSize, sizeof(uint));

            _initialized = true;
        }

        // ======== Single value ========

        /// <summary>Returns a single random uint from the GPU cache.</summary>
        public uint GetRandomUint()
        {
            if (_cachePos >= CacheSize) RefillCache();
            return _cache[_cachePos++];
        }

        /// <summary>Returns a single random float in [0, 1).</summary>
        public float GetRandomFloat()
        {
            return GetRandomUint() * (1f / 4294967296f);
        }

        // ======== Batch ========

        /// <summary>Fills span with random ints in [min, max] (inclusive).</summary>
        public void GetRandomInts(Span<int> output, int min, int max)
        {
            int count = output.Length;
            if (count == 0) return;

            EnsureBatchBuffer(count);
            EnsurePool(ref _intPool, count);

            uint range = (uint)(max - min + 1);
            SetCommon(count);
            shader.SetInt("_RangeI", unchecked((int)range));
            shader.SetInt("_MinI", min);
            shader.SetBuffer(_kInt, "_Result", _batchBuf);
            Dispatch(_kInt, count);

            _batchBuf.GetData(_intPool, 0, 0, count);
            _intPool.AsSpan(0, count).CopyTo(output);
        }

        /// <summary>Fills span with random floats in [min, max).</summary>
        public void GetRandomFloats(Span<float> output, float min, float max)
        {
            int count = output.Length;
            if (count == 0) return;

            EnsureBatchBuffer(count);
            EnsurePool(ref _floatPool, count);

            SetCommon(count);
            shader.SetFloat("_MinF", min);
            shader.SetFloat("_MaxF", max);
            shader.SetBuffer(_kFloat, "_Result", _batchBuf);
            Dispatch(_kFloat, count);

            // asuint in shader → raw bits → reinterpret as float via GetData
            _batchBuf.GetData(_floatPool, 0, 0, count);
            _floatPool.AsSpan(0, count).CopyTo(output);
        }

        // ======== Internal ========

        void RefillCache()
        {
            SetCommon(CacheSize);
            shader.SetBuffer(_kUint, "_Result", _cacheBuf);
            Dispatch(_kUint, CacheSize);
            _cacheBuf.GetData(_cache);
            _cachePos = 0;
        }

        void SetCommon(int count)
        {
            shader.SetInt("_Key0", unchecked((int)_key0));
            shader.SetInt("_Key1", unchecked((int)_key1));
            shader.SetInt("_CounterBase", unchecked((int)_counterBase));
            shader.SetInt("_Count", count);
        }

        void Dispatch(int kernel, int count)
        {
            int threads = (count + 3) / 4;
            int groups = (threads + ThreadGroupSize - 1) / ThreadGroupSize;
            shader.Dispatch(kernel, Mathf.Max(groups, 1), 1, 1);
            _counterBase += (uint)threads;
        }

        void EnsureBatchBuffer(int minSize)
        {
            if (_batchBuf != null && _batchBufLen >= minSize) return;
            _batchBuf?.Release();
            _batchBufLen = Mathf.NextPowerOfTwo(Mathf.Max(256, minSize));
            _batchBuf = new ComputeBuffer(_batchBufLen, sizeof(uint));
        }

        static void EnsurePool<T>(ref T[] pool, int minSize)
        {
            if (pool != null && pool.Length >= minSize) return;
            pool = new T[Mathf.NextPowerOfTwo(Mathf.Max(256, minSize))];
        }

        // ======== Cleanup ========

        public void Dispose()
        {
            _cacheBuf?.Release();
            _cacheBuf = null;
            _batchBuf?.Release();
            _batchBuf = null;
            _initialized = false;
        }
    }
}
