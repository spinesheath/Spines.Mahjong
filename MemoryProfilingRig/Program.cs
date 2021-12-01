using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Profiler.Api;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Shanten;

namespace MemoryProfilingRig
{
  class Program
  {
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

      // Do stuff

      MemoryProfiler.GetSnapshot();
      MemoryProfiler.Detach();

      stopwatch.Stop();

      Console.WriteLine(sum);
      Console.WriteLine(stopwatch.Elapsed);
    }
  }
}
