using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Spines.Mahjong.Analysis.Replay;

namespace Benchmark
{
  public class Shanten
  {
    private const string ReplaysFolder = @".\..\..\..\..\..\..\..\..\Data\tenhou";
    private readonly List<string> _files;

    public Shanten()
    {
      _files = Directory.EnumerateFiles(ReplaysFolder).Take(10000).Select(File.ReadAllText).ToList();
    }

    private int RunShanten1()
    {
      return _files.Sum(ReplayParser.Parse);
    }

    [Benchmark]
    public int Shanten1() => RunShanten1();
  }
}