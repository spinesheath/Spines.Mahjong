using System.Diagnostics;
using Spines.Mahjong.Analysis;

namespace AnalyzerBuilder.Creators.Shared
{
  internal class PartialHandIterator
  {
    public PartialHandIterator(int length)
    {
      Debug.Assert(length == 7 || length == 9);

      Max = length == 7 ? Base5.MaxFor7Digits : Base5.MaxFor9Digits;
    }

    public int Base5Hash { get; private set; }

    public byte[] Counts { get; } = new byte[7];

    public int Max { get; }

    public int TileCount { get; private set; }

    public bool HasNext()
    {
      return Base5Hash < Max;
    }

    public void MoveNext()
    {
      Base5Hash += 1;
      Counts[0] += 1;
      TileCount += 1;
      for (var j = 0; j < Counts.Length - 1; j++)
      {
        var carry = Counts[j] == 5 ? 1 : 0;
        Counts[j + 1] += (byte) carry;
        Counts[j] -= (byte) (5 * carry);
        TileCount -= 4 * carry;
      }
    }
  }
}