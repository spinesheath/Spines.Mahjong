using System;
using System.IO;
using AnalyzerBuilder.Combinations;
using Spines.Mahjong.Analysis;

namespace AnalyzerBuilder.Creators.Shanten5
{
  public static class Shanten5Creator
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

      var counts = new byte[7];
      var tileCount = 0;
      var row = new byte[16];

      for (var base5Hash = 0; base5Hash < Base5.MaxFor7Digits; base5Hash++)
      {
        Array.Clear(row, 0, row.Length);

        if (tileCount < 15)
        {
          var kokushi1 = 0;
          var kokushi2 = 0;
          var chiitoi = 0;
          for (var i = 0; i < counts.Length; i++)
          {
            chiitoi += counts[i] > 1 ? 1 : 0;
            kokushi1 += counts[i] > 0 ? 1 : 0;
            kokushi2 = counts[i] > 1 ? 1 : kokushi2;
          }

          GetHonorShantenValues(counts, row);

          row[13] = (byte) kokushi1;
          row[14] = (byte) (kokushi1 + kokushi2);
          row[15] = (byte) chiitoi;
        }

        writer.Write(row);

        // move to next tile sequence
        counts[0] += 1;
        tileCount += 1;
        for (var j = 0; j < counts.Length - 1; j++)
        {
          var carry = counts[j] == 5 ? 1 : 0;
          counts[j + 1] += (byte)carry;
          counts[j] -= (byte)(5 * carry);
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

      var counts = new byte[9];
      var tileCount = 0;
      var row = new byte[16];

      for (var base5Hash = 0; base5Hash < Base5.MaxFor9Digits; base5Hash++)
      {
        Array.Clear(row, 0, row.Length);

        if (tileCount < 15)
        {
          var kokushi1 = (counts[0] > 0 ? 1 : 0) + (counts[8] > 0 ? 1 : 0);
          var kokushi2 = counts[0] > 1 || counts[8] > 1 ? 1 : 0;
          var chiitoi = 0;
          for (var i = 0; i < counts.Length; i++)
          {
            chiitoi += counts[i] > 1 ? 1 : 0;
          }

          GetSuitShantenValues(counts, row);

          row[13] = (byte) kokushi1;
          row[14] = (byte)(kokushi1 + kokushi2);
          row[15] = (byte) chiitoi;
        }

        writer.Write(row);

        // move to next tile sequence
        counts[0] += 1;
        tileCount += 1;
        for (var j = 0; j < counts.Length - 1; j++)
        {
          var carry = counts[j] == 5 ? 1 : 0;
          counts[j + 1] += (byte)carry;
          counts[j] -= (byte)(5 * carry);
          tileCount -= 4 * carry;
        }

        if (base5Hash % 10000 == 9999)
        {
          Console.WriteLine($"{(double)base5Hash / Base5.MaxFor9Digits:P}");
        }
      }
    }

    private static void GetSuitShantenValues(byte[] counts, byte[] row)
    {
      var results = ProtoGroup.AnalyzeSuit(counts);

      foreach (var arrangement in results)
      {
        var index = arrangement.HasJantou ? 5 : 0;
        index += arrangement.MentsuCount;
        row[index] = (byte) Math.Max(row[index], arrangement.TotalValue);
      }

      for (var i = 0; i < 4; i++)
      {
        row[i + 1] = Math.Max(row[i], row[i + 1]);
        row[i + 6] = Math.Max(row[i + 1], row[i + 6]);
        row[i + 6] = Math.Max(row[i + 5], row[i + 6]);
      }
    }

    private static void GetHonorShantenValues(byte[] counts, byte[] row)
    {
      var results = ProtoGroup.AnalyzeHonor(counts);

      foreach (var arrangement in results)
      {
        var index = arrangement.HasJantou ? 5 : 0;
        index += arrangement.MentsuCount;
        row[index] = (byte) Math.Max(row[index], arrangement.TotalValue);
      }

      for (var i = 0; i < 4; i++)
      {
        row[i + 1] = Math.Max(row[i], row[i + 1]);
        row[i + 6] = Math.Max(row[i + 1], row[i + 6]);
        row[i + 6] = Math.Max(row[i + 5], row[i + 6]);
      }
    }
  }
}