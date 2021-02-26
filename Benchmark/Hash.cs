using System.Collections.Generic;
using System.Linq;
using AnalyzerBuilder.Combinations;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
  public class Hash
  {
    private static readonly List<int[]> _data = new List<int[]>();

    static Hash()
    {
      var cc = ConcealedCombinationCreator.ForSuits();
      for (var i = 0; i < 15; i++)
      {
        var combinations = cc.Create(i);
        foreach (var combination in combinations)
        {
          var c = combination.Counts.ToArray();
          _data.Add(c);
        }
      }
    }


    private int RunMultiplyAndAdd()
    {
      var r = 0;
      for (var i = 0; i < _data.Count; i++)
      {
        var d = _data[i];
        var h = 0;
        for (var j = 0; j < d.Length; j++)
        {
          h *= 5;
          h += d[j];
        }

        r ^= h;
      }

      return r;
    }

    private int RunForeach()
    {
      var r = 0;
      for (var i = 0; i < _data.Count; i++)
      {
        var d = _data[i];
        var h = 0;
        foreach (var t in d)
        {
          h *= 5;
          h += t;
        }

        r ^= h;
      }

      return r;
    }

    [Benchmark]
    public int MultiplyAndAdd() => RunMultiplyAndAdd();

    [Benchmark]
    public int Foreach() => RunForeach();
  }
}
