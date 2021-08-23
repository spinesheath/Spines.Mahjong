using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class ScoreTests
  {
    [Fact]
    public void BundlesWithVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ScoreCalculatingVisitor();
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
      Assert.Equal(0, visitor.CalculationCount);
    }

    private static readonly string[] BundlesFolders = 
    {
      @"C:\tenhou\compressed\2014\yonma\bundles",
      @"C:\tenhou\compressed\2015\yonma\bundles",
      @"C:\tenhou\compressed\2016\yonma\bundles"
    };

    [Fact]
    public void Count()
    {
      var count = 0;
      var worst = 0;
      var sum = 0;
      var b = new int[421];
      
      for (var i0 = 0; i0 < 9; i0++)
      {
        for (var i1 = 0; i1 <= i0; i1++)
        {
          for (var i2 = 0; i2 <= i1; i2++)
          {
            for (var i3 = 0; i3 <= i2; i3++)
            {
              if (i0 + i1 + i2 + i3 <= 14)
              {
                count++;

                var a1 = OutOf(i1, i0);
                var a2 = OutOf(i2, i1);
                var a3 = OutOf(i3, i2);
                var a = a1 * a2 * a3;
                sum += a;
                worst = Math.Max(worst, a);
                b[a] += 1;
              }
            }
          }
        }
      }

      var d = new Dictionary<int, int>();
      for (var i = 0; i < 421; i++)
      {
        if (b[i] != 0)
        {
          var bits = (int) Math.Ceiling(Math.Log2(i));
          if (!d.ContainsKey(bits))
          {
            d[bits] = 0;
          }
          d[bits] += b[i];
        }
      }

      var average = sum / (double)count;

      Assert.Equal(0, count);
    }

    private int OutOf(int k, int n)
    {
      var c = 1;
      for (var i = n; i >= n - k + 1; i--)
      {
        c *= i;
      }

      for (var i = 2; i <= k; i++)
      {
        c /= i;
      }

      return c;
    }
  }
}
