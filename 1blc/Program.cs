using _1blc;
using BenchmarkDotNet.Running;
using System.Diagnostics;

//var summary = BenchmarkRunner.Run<ToBench>();
var dummy = new ToBench();
////
////


var stopwatch = Stopwatch.StartNew();
dummy.DoItFloat();
stopwatch.Stop();

Console.WriteLine($"DoItFloat runtime: {stopwatch.ElapsedMilliseconds} ms");//dummy.DoItDouble();

//	const string path = @"C:\Users\henning\source\1brc\data\measurements.txt";
//	var part = new FilePart { FileName = path, Start = 0, End = 700 };

//	part.GetLines().ToList().Dump();