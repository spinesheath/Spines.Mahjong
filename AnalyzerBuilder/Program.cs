using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Creators;

namespace AnalyzerBuilder
{
  internal class TenpaiShapeClassifier
  {
    private readonly int[] _transitions;
    private long[] _values;

    public TenpaiShapeClassifier(string workingDirectory)
    {
      var transitionsPath = Path.Combine(workingDirectory, "TenpaiShapeTransitions.txt");
      var valuesPath = Path.Combine(workingDirectory, "TenpaiShapeValues.txt");

      _transitions = File.ReadAllLines(transitionsPath).Select(line => Convert.ToInt32(line)).ToArray();
      _values = File.ReadAllLines(valuesPath).Select(line => Convert.ToInt64(line)).ToArray();
    }

    public long Classify(IEnumerable<int> tiles)
    {
      var c = 0;
      foreach (var tile in tiles)
      {
        c = _transitions[c + tile];
      }

      return _values[c];
    }
  }

  class Program
  {
    static void Main(string[] args)
    {
      var workingDirectory = @"C:\temp\mahjong\tenpaiShapes";
      var x = new TenpaiShapeTransitionsCreator(workingDirectory);
      x.Create();
      var c = new TenpaiShapeClassifier(workingDirectory);
      var r = c.Classify(new[] {1, 1, 1, 2, 1, 1, 2, 1, 1});

      var bits = Convert.ToString(r, 2);

      var ittsuu0 = (r & 1L << 0) != 0;
      var ittsuu3 = (r & 1L << 1) != 0;
      var ittsuu6 = (r & 1L << 2) != 0;
      var junchan = (r & 1L << 3) != 0;
      var iipeikou = (r & 1L << 4) != 0;
      var pinfu = (r & 1L << 5) != 0;
      var pinfu9 = Convert.ToString((r >> 6) & 0b111111111L, 2);
      var sanshoku7 = Convert.ToString((r >> 15) & 0b1111111L, 2);
      var sandoukou9 = Convert.ToString((r >> 22) & 0b111111111L, 2);
      var ankou = (r >> 31) & 0b111L;
      var ankou9 = string.Join(",", Enumerable.Range(0, 9).Select(i => (r >> (34 + 3 * i)) & 0b111L));

      // 3 ittsuu
      // 1 junchan
      // 1 iipeikou
      // 1 pinfu if wait not in this suit
      // 9 pinfu if wait in this suit, indexed by wait
      // 7 shuntsu presence for sanshoku doujun
      // 9 koutsu presence for sanshoku doukou
      // 3 ankou count if wait not in this suit
      // 3*9 ankou count if wait in this suit, indexed by wait
    }
  }
}
