using System;
using System.IO;
using System.Linq;
using System.Text;
using AnalyzerBuilder.Combinations;
using AnalyzerBuilder.Creators.Shared;

namespace AnalyzerBuilder.Creators.B9Ukeire
{
  public class B9UkeireCreator
  {
    public static void Create(string directory)
    {
      Console.WriteLine("Suit");
      CreateSuit(Path.Combine(directory, "suit.dat"));
      Console.WriteLine("Honor");
      CreateHonor(Path.Combine(directory, "honor.dat"));
    }

    private static void CreateHonor(string path)
    {
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);

      var row = new ushort[16];

      var analyzer = new CachingAnalyzer(7);
      var it = new PartialHandIterator(7);
      while (it.HasNext())
      {
        Array.Clear(row, 0, row.Length);

        if (it.TileCount < 15)
        {
          CalculateHonorRow(row, it, analyzer);
        }

        Write(row, writer);
        ReportProgress(it);
        it.MoveNext();
      }
    }

    private static void CreateSuit(string path)
    {
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);

      var row = new ushort[16];

      var analyzer = new CachingAnalyzer(9);
      var it = new PartialHandIterator(9);
      while (it.HasNext())
      {
        Array.Clear(row, 0, row.Length);

        if (it.TileCount < 15)
        {
          CalculateSuitRow(row, it, analyzer);
        }

        Write(row, writer);
        ReportProgress(it);
        it.MoveNext();
      }
    }

    private static void CalculateHonorRow(ushort[] row, PartialHandIterator it, CachingAnalyzer analyzer)
    {
      var counts = it.Counts;

      var results = analyzer.Analyze(it);

      foreach (var arrangement in results)
      {
        var index = IndexInRow(arrangement);
        row[index] = (byte)Math.Max(row[index], arrangement.TotalValue);
      }
      
      CopyValuesFromLowerGroupCounts(row);

      CalculateHonorChiitoiKokushi(counts, row);

      for (var tileIndex = 0; tileIndex < counts.Length; tileIndex++)
      {
        var b9Results = analyzer.AnalyzeWithExtraTile(it, tileIndex);

        foreach (var arrangement in b9Results)
        {
          UpdateB9(row, tileIndex, arrangement);
        }
      }
    }

    private static void CalculateSuitRow(ushort[] row, PartialHandIterator it, CachingAnalyzer analyzer)
    {
      var counts = it.Counts;

      var results = analyzer.Analyze(it);

      foreach (var arrangement in results)
      {
        var index = IndexInRow(arrangement);
        row[index] = (byte)Math.Max(row[index], arrangement.TotalValue);
      }

      CopyValuesFromLowerGroupCounts(row);

      CalculateSuitChiitoiKokushi(counts, row);

      for (var tileIndex = 0; tileIndex < counts.Length; tileIndex++)
      {
        var b9Results = analyzer.AnalyzeWithExtraTile(it, tileIndex);

        foreach (var arrangement in b9Results)
        {
          UpdateB9(row, tileIndex, arrangement);
        }
      }
    }

    private static void UpdateB9(ushort[] row, int tileIndex, Arrangement arrangement)
    {
      var index = IndexInRow(arrangement);
      var value = arrangement.TotalValue;

      var targetValue = row[index] & 15;

      if (targetValue >= value)
      {
        // tile does not improve hand with this arrangement
        return;
      }

      var bit = (ushort)(1 << (tileIndex + B9Shift));
      row[index] |= bit;

      for (var i = index; i < 4; i++)
      {
        if ((row[i + 1] & 15) == targetValue)
        {
          row[i + 1] |= bit;
        }

        if ((row[i + 5] & 15) == targetValue)
        {
          row[i + 5] |= bit;
        }

        if ((row[i + 6] & 15) == targetValue)
        {
          row[i + 6] |= bit;
        }
      }

      for (var i = index; i > 4 && i < 9; i++)
      {
        if ((row[i + 1] & 15) == targetValue)
        {
          row[i + 1] |= bit;
        }
      }
    }

    private static void CalculateSuitChiitoiKokushi(byte[] counts, ushort[] row)
    {
      var c0 = counts[0];
      var c8 = counts[8];

      var kokushi0 = (c0 > 0 ? 1 : 0) + (c8 > 0 ? 1 : 0);
      // any missing terminal is an ukeire for kokushi
      var b9Kokushi0 = (c0 == 0 ? 1 : 0) | (c8 == 0 ? 1 << 8 : 0);

      var kokushi1 = c0 > 1 || c8 > 1 ? 1 : 0;
      // a single terminal can become a kokushi pair
      var b9Kokushi1 = (c0 == 1 ? 1 : 0) | (c8 == 1 ? 1 << 8 : 0);

      // if any terminal pair is present, no more ukeire for kokushi pairs
      if (kokushi1 != 0)
      {
        b9Kokushi1 = 0;
      }

      var chiitoi = 0;
      var b9Chiitoi = 0;

      for (var i = 0; i < counts.Length; i++)
      {
        chiitoi += counts[i] > 1 ? 1 : 0;
        // a single honor can become a chiitoi pair
        b9Chiitoi |= counts[i] == 1 ? 1 << i : 0;
      }

      row[13] = (ushort)(kokushi0 | (b9Kokushi0 << B9Shift));
      row[14] = (ushort)(kokushi0 + kokushi1 | ((b9Kokushi0 | b9Kokushi1) << B9Shift));
      row[15] = (ushort)(chiitoi | (b9Chiitoi << B9Shift));
    }

    private static void CalculateHonorChiitoiKokushi(byte[] counts, ushort[] row)
    {
      var kokushi0 = 0;
      var b9Kokushi0 = 0;

      var kokushi1 = 0;
      var b9Kokushi1 = 0;

      var chiitoi = 0;
      var b9Chiitoi = 0;
      for (var i = 0; i < counts.Length; i++)
      {
        var c = counts[i];

        kokushi0 += c > 0 ? 1 : 0;
        // any missing honor is an ukeire for kokushi
        b9Kokushi0 |= c == 0 ? 1 << i : 0;

        kokushi1 = c > 1 ? 1 : kokushi1;
        // a single honor can become a kokushi pair
        b9Kokushi1 |= c == 1 ? 1 << i : 0;

        chiitoi += c > 1 ? 1 : 0;
        // a single honor can become a chiitoi pair
        b9Chiitoi |= c == 1 ? 1 << i : 0;
      }

      // if any honor pair is present, no more ukeire for kokushi pairs
      if (kokushi1 != 0)
      {
        b9Kokushi1 = 0;
      }

      row[13] = (ushort) (kokushi0 | (b9Kokushi0 << B9Shift));
      row[14] = (ushort) (kokushi0 + kokushi1 | ((b9Kokushi0 | b9Kokushi1) << B9Shift));
      row[15] = (ushort) (chiitoi | (b9Chiitoi << B9Shift));
    }

    private const int B9Shift = 4;

    private static void CopyValuesFromLowerGroupCounts(ushort[] row)
    {
      for (var i = 0; i < 4; i++)
      {
        row[i + 1] = Max(row[i], row[i + 1]);
        row[i + 6] = Max(row[i + 1], row[i + 5], row[i + 6]);
      }
    }

    private static int IndexInRow(Arrangement arrangement)
    {
      return arrangement.HasJantou ? 5 + arrangement.MentsuCount : arrangement.MentsuCount;
    }

    private static ushort Max(ushort a, ushort b)
    {
      return Math.Max(a, b);
    }

    private static ushort Max(ushort a, ushort b, ushort c)
    {
      return Max(Max(a, b), c);
    }

    private static void ReportProgress(PartialHandIterator it)
    {
      if (it.Base5Hash % 10000 == 9999)
      {
        Console.WriteLine($"{(double)it.Base5Hash / it.Max:P}");
      }
    }

    private static void Write(ushort[] row, BinaryWriter writer)
    {
      for (var i = 0; i < row.Length; i++)
      {
        writer.Write(row[i]);
      }
    }

    private static string DebugString(byte[] counts, ushort[] row)
    {
      string[] configurations = { "00", "01", "02", "03", "04", "10", "11", "12", "13", "14", "__", "__", "__", "k0", "k1", "cc" };

      var sb = new StringBuilder();
      var hand = "hand: ".PadLeft(17 - counts.Length) + string.Join("", counts.Reverse());
      sb.AppendLine(hand);

      for (var i = 0; i < row.Length; i++)
      {
        var b9 = Convert.ToString(row[i] >> 4, 2).PadLeft(9, '0');
        var v = (row[i] & 15).ToString().PadLeft(2, ' ');
        sb.AppendLine($"{configurations[i]}: {v}, {b9}");
      }

      return sb.ToString();
    }
  }
}