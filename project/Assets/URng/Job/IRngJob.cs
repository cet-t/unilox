using System;

namespace Cet.Rng.Job
{
    public interface IRng32Job : IDisposable
    {
        void Init(uint seed);
        uint NextU();
        float NextF();
        int RandI(int min, int max);
        float RandF(float min, float max);
    }

    public interface IRng64Job : IDisposable
    {
        void Init(ulong seed);
        ulong NextU();
        double NextF();
        long RandI(long min, long max);
        double RandF(double min, double max);
    }
}