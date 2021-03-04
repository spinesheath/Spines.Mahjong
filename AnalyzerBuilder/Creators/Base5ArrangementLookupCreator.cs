using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Combinations;
using Spines.Mahjong.Analysis.Shanten;

namespace AnalyzerBuilder.Creators
{
  public class Base5ArrangementLookupCreator
  {
    public void Run()
    {
      var cc = ConcealedCombinationCreator.ForSuits();
      var sc = new SuitClassifier();
      var dict = new Dictionary<int, int>();
      for (var i = 0; i < 15; i++)
      {
        var combinations = cc.Create(i);
        foreach (var combination in combinations)
        {
          var c = combination.Counts.Select(x => (byte)x).ToArray();
          var rc = c.Reverse().ToArray();

          var h1 = Hash(c);
          var a1 = sc.GetValue(c, 0, new []{h1});
          AddToDictionary(dict, h1, a1);

          var h2 = Hash(rc);
          var a2 = sc.GetValue(rc, 0, new[] { h1 });
          AddToDictionary(dict, h2, a2);
        }
      }

      var max = dict.Keys.Max();
      var data = new byte[max + 1];
      foreach (var p in dict)
      {
        data[p.Key] = (byte)p.Value;
      }

      using var f = File.OpenWrite(@"C:\Shanten2\suitArrangementsBase5NoMelds.dat");
      f.Write(data);
    }

    private static void AddToDictionary(Dictionary<int, int> dict, int h, int a)
    {
      if (dict.TryGetValue(h, out var e))
      {
        if (e != a)
        {

        }
      }
      else
      {
        dict[h] = a;
      }
    }

    private static int Hash(byte[] tiles)
    {
      var r = 0;

      foreach (var t in tiles)
      {
        r *= 5;
        r += t;
      }

      return r;
    }
  }
}
