using System.Numerics;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace _1blc;

public class ToBench
{
    const string path = @"C:\Users\henning\source\1brc\data\measurements.txt";
    //const string path = @"C:\Users\matze\Desktop\measurements_m.txt";
    //const string path = @"/Volumes/Matze/matze/Desktop/measurements.txt";
    //
    //	[Benchmark]
    //	public void DoItDouble()
    //	{
    //		OpenAndClose(path);
    //
    //		var parts = SplitFile(path, Environment.ProcessorCount);
    //		var result = parts.AsParallel().Select(p => ProcessPart<double>(p)).Merge();
    //
    //		result.Dump();
    //	}

    [Benchmark]
    public void DoItFloat()
    {
        DoItInner<float>();
    }

    public ToBench()
    {
        SizeInMB = 16;
    }

    [Params(1, 2, 4, 8, 16, 32)]
    public int SizeInMB { get; set; }

    private void DoItInner<T>() where T : IFloatingPoint<T>, IMinMaxValue<T>
    {
        Helpers.OpenAndClose(path);

        var fileSize = new FileInfo(path).Length;
        var maxBlock = SizeInMB * 1024 * 1024;
        var minBlocks = fileSize / maxBlock;

        var numberOfBlocks = Math.Max(Environment.ProcessorCount, (int)minBlocks);


        var parts = Helpers.SplitFile(path, numberOfBlocks);


        var result = parts.Select((p, i) => p.ReadBuffer(i)).AsParallel().Select((b, i) => ProcessPart<T>(b, i)).Merge();

        //result.Dump();
    }
    
    public static Dictionary<int, DataItem<T>> ProcessPart<T>(byte[] bufferAsBytes, int index) where T : IFloatingPoint<T>
    {
#if OUTPUT
	$"Start process {index}".Dump();
#endif
        var nameToValues = new Dictionary<int, DataItem<T>>();

        var parser = new CustomParser<T>();

        ReadOnlySpan<char> buffer = Encoding.UTF8.GetChars(bufferAsBytes);

        var e = buffer.Split('\n');

        foreach (var lineRange in e)
        {
            (var hash, var name, var value) = parser.SplitTextAndParseDoubleStateMachine(buffer, lineRange);
            if (!nameToValues.TryGetValue(hash, out var data))
            {
                data = new DataItem<T>() { Name = name, Sum = value, Min = value, Max = value, Count = 1 };
            }
            else
            {
                data.Sum += value;
                data.Min = T.Min(value, data.Min);
                data.Max = T.Max(value, data.Max);
                data.Count++;
            }
            nameToValues[hash] = data;
        }
#if OUTPUT
	$"End process {index}".Dump();
#endif
        return nameToValues;
    }
}