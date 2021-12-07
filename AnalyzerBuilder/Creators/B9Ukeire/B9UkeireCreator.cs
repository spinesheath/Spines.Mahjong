using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AnalyzerBuilder.Combinations;
using Spines.Mahjong.Analysis;

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

      var counts = new int[7];
      var tileCount = 0;
      var row = new ushort[16];

      for (var base5Hash = 0; base5Hash < Base5.MaxFor7Digits; base5Hash++)
      {
        Array.Clear(row, 0, row.Length);

        if (tileCount < 15)
        {
          CalculateHonorRow(counts, row);
        }

        for (var i = 0; i < row.Length; i++)
        {
          writer.Write(row[i]);
        }

        // move to next tile sequence
        counts[0] += 1;
        tileCount += 1;
        for (var j = 0; j < counts.Length - 1; j++)
        {
          var carry = counts[j] == 5 ? 1 : 0;
          counts[j + 1] += carry;
          counts[j] -= 5 * carry;
          tileCount -= 4 * carry;
        }

        if (base5Hash % 10000 == 9999)
        {
          Console.WriteLine($"{(double)base5Hash / Base5.MaxFor7Digits:P}");
        }
      }
    }

    private static void CreateSuit(string path)
    {
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);

      var counts = new int[9];
      var tileCount = 0;
      var row = new ushort[16];

      for (var base5Hash = 0; base5Hash < Base5.MaxFor9Digits; base5Hash++)
      {
        Array.Clear(row, 0, row.Length);

        if (tileCount < 15)
        {
          CalculateSuitRow(counts, row);
        }

        //for (var i = 0; i < row.Length; i++)
        //{
        //  writer.Write(row[i]);
        //}

        // move to next tile sequence
        counts[0] += 1;
        tileCount += 1;
        for (var j = 0; j < counts.Length - 1; j++)
        {
          var carry = counts[j] == 5 ? 1 : 0;
          counts[j + 1] += carry;
          counts[j] -= 5 * carry;
          tileCount -= 4 * carry;
        }

        if (base5Hash % 10000 == 9999)
        {
          Console.WriteLine($"{(double)base5Hash / Base5.MaxFor9Digits:P}");
        }
      }
    }

    private static void CalculateSuitRow(int[] counts, ushort[] row)
    {
      var results = ProtoGroup.AnalyzeSuit(counts);

      foreach (var arrangement in results)
      {
        var index = IndexInRow(arrangement);
        row[index] = (byte)Math.Max(row[index], arrangement.TotalValue);
      }

      CopyValuesFromLowerGroupCounts(row);

      CalculateSuitChiitoiKokushi(counts, row);

      for (var tileIndex = 0; tileIndex < counts.Length; tileIndex++)
      {
        var b9Results = AnalyzeSuitWithExtraTile(counts, tileIndex);

        foreach (var arrangement in b9Results)
        {
          UpdateB9(row, tileIndex, arrangement);
        }
      }
    }

    private static void CalculateHonorRow(int[] counts, ushort[] row)
    {
      var results = ProtoGroup.AnalyzeHonor(counts);

      foreach (var arrangement in results)
      {
        var index = IndexInRow(arrangement);
        row[index] = (byte)Math.Max(row[index], arrangement.TotalValue);
      }
      
      CopyValuesFromLowerGroupCounts(row);

      CalculateHonorChiitoiKokushi(counts, row);

      for (var tileIndex = 0; tileIndex < counts.Length; tileIndex++)
      {
        var b9Results = AnalyzeHonorsWithExtraTile(counts, tileIndex);

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

    private static void CalculateSuitChiitoiKokushi(int[] counts, ushort[] row)
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

    private static void CalculateHonorChiitoiKokushi(int[] counts, ushort[] row)
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

    private static List<Arrangement> AnalyzeHonorsWithExtraTile(int[] counts, int tileIndex)
    {
      if (counts.Sum() == 14)
      {
        return new List<Arrangement>();
      }

      var t = counts.ToArray();
      t[tileIndex] += 1;
      return ProtoGroup.AnalyzeHonor(t);
    }

    private static List<Arrangement> AnalyzeSuitWithExtraTile(int[] counts, int tileIndex)
    {
      if (counts.Sum() == 14)
      {
        return new List<Arrangement>();
      }

      var t = counts.ToArray();
      t[tileIndex] += 1;
      return ProtoGroup.AnalyzeSuit(t);
    }

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

    private static string DebugString(int[] counts, ushort[] row)
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