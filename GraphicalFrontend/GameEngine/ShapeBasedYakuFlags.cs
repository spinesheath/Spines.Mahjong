using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GraphicalFrontend.GameEngine
{
  internal class ShapeBasedYakuFlags
  {
    static ShapeBasedYakuFlags()
    {
      Transitions = LoadArray("TenpaiShapeTransitions.txt", x => Convert.ToInt32(x, CultureInfo.InvariantCulture));
      Values = LoadArray("TenpaiShapeValues.txt", x => Convert.ToInt64(x, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// TileType counts to bitfield with information relevant for yaku check
    /// Bits, from least significant to highest:
    /// 3 ittsuu (123, 456, 789)
    /// 1 junchan
    /// 1 iipeikou
    /// 1 pinfu if wait not in this suit
    /// 9 pinfu if wait in this suit, indexed by wait
    /// 7 shuntsu presence for sanshoku doujun
    /// 9 koutsu presence for sanshoku doukou
    /// 3 ankou count if wait not in this suit
    /// 3*9 ankou count if wait in this suit, indexed by wait
    /// </summary>
    public static long GetFlagsForSuit(IList<int> tileTypeCounts)
    {
      var c = 0;
      foreach (var tile in tileTypeCounts)
      {
        c = Transitions[c + tile];
      }

      return Values[c];
    }

    private static readonly int[] Transitions;
    private static readonly long[] Values;

    private static T[] LoadArray<T>(string resourceName, Func<string, T> converter)
    {
      var fullResourceName = "GraphicalFrontend.Resources." + resourceName;
      var assembly = Assembly.GetExecutingAssembly();
      Stream? stream = null;
      try
      {
        stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
        {
          throw new FileNotFoundException($"Resource {fullResourceName} is missing.");
        }

        using var reader = new StreamReader(stream);
        stream = null;
        var result = reader.ReadToEnd();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return lines.Select(converter).ToArray();
      }
      finally
      {
        stream?.Dispose();
      }
    }
  }
}