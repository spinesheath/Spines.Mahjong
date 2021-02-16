using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Profiler.Api;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace MemoryProfilingRig
{
  class Program
  {
    private const string BundlesFolder = @"C:\tenhou\compressed\2014\yonma\bundles";

    static void Main(string[] args)
    {
      var sum = 0;

      var loadStatics = new HandCalculator();
      loadStatics.Init(Enumerable.Range(0, 13).Select(TileType.FromTileTypeId));
      sum += loadStatics.Shanten < 100 ? 0 : 1;

      var stopwatch = new Stopwatch();
      stopwatch.Start();

      MemoryProfiler.CollectAllocations(true);
      MemoryProfiler.GetSnapshot();

      var files = Directory.EnumerateFiles(BundlesFolder).Take(10);
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        var r = ReplayParser.Parse(fileStream);
        sum += r;
      }

      MemoryProfiler.GetSnapshot();
      MemoryProfiler.Detach();

      stopwatch.Stop();

      Console.WriteLine(sum);
      Console.WriteLine(stopwatch.Elapsed);
    }
  }
}
