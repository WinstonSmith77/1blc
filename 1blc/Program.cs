using _1blc;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<ToBench>();
//var dummy = new ToBench();
////
////
//dummy.DoItFloat();
//dummy.DoItDouble();

//	const string path = @"C:\Users\henning\source\1brc\data\measurements.txt";
//	var part = new FilePart { FileName = path, Start = 0, End = 700 };

//	part.GetLines().ToList().Dump();