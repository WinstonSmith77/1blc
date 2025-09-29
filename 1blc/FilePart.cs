namespace _1blc;

public struct FilePart
{
    public string FileName;
    public long Start { get; set; }
    public long End { get; set; }
    public long Lenght => End - Start;
}
