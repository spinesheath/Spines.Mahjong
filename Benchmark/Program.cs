using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Spines.Mahjong.Analysis.Replay;

namespace Benchmark
{
  public class Shanten
  {
    private const string ReplaysFolder = @"C:\Users\Johannes\source\repos\MahjongAiTrainer\Data\tenhou";
    private readonly List<string> _files;

    public Shanten()
    {
      _files = Directory.EnumerateFiles(ReplaysFolder).Take(1000).Select(File.ReadAllText).ToList();
    }

    private int RunShanten1()
    {
      return _files.Sum(ReplayParser.Parse);
    }

    [Benchmark]
    public int Shanten1() => RunShanten1();
  }

  public class Program
  {
    public static void Main(string[] args)
    {
      var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
  }
}
