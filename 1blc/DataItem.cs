using System.Numerics;

namespace _1blc;

public struct DataItem<T> where T : IFloatingPoint<T>
{
    public string Name;
    public T Min;
    public T Max;
    public T Sum;
    public ulong Count;
    public T Mean;
}