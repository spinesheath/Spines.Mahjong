using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  // TODO construct this progressively in HandCalculator?
  internal class HonorMeldScoringBitField
  {
    public HonorMeldScoringBitField(IReadOnlyList<Block> melds)
    {
      _melds = melds;
      _hasMelds = melds.Count > 0;

      Jikaze(54);
      Bakaze(58);
      HakuHatsuChun(15);
      Daisangen(6);
      Shousangen(51);
      Suushi();
      HonitsuChinitsu(20);
      Tsuuiisou(2);
      Chanta(27);
      Ryuuiisou(28);
    }

    public long SumValue { get; private set; }

    private readonly bool _hasMelds;
    private readonly IReadOnlyList<Block> _melds;

    private void Ryuuiisou(int offset)
    {
      if (_melds.All(m => m.Index == 5))
      {
        SumValue |= 4L << offset;
      }
    }

    private void HonitsuChinitsu(int offset)
    {
      if (_hasMelds)
      {
        SumValue |= 0b1L << offset;
      }
    }

    private void Suushi()
    {
      var koutsuCount = _melds.Count(b => b.Index < 4);
      SumValue |= (long) koutsuCount << 9;
      SumValue |= (long) koutsuCount << 12;
    }

    private void Daisangen(int offset)
    {
      var koutsuCount = _melds.Count(b => b.Index > 3);
      var sangenCount = koutsuCount > 1 ? koutsuCount + 1 : koutsuCount;
      SumValue |= (long) sangenCount << offset;
    }

    private void Shousangen(int offset)
    {
      var koutsuCount = _melds.Count(b => b.Index > 3);
      var sangenCount = koutsuCount > 1 ? koutsuCount + 1 : koutsuCount;
      SumValue |= (long)sangenCount << offset;
    }

    private void HakuHatsuChun(int offset)
    {
      foreach (var meld in _melds)
      {
        if (meld.Index >= 4)
        {
          SumValue |= 0b1L << (meld.Index + offset - 4);
        }
      }
    }

    private void Jikaze(int offset)
    {
      foreach (var meld in _melds)
      {
        if (meld.Index < 4)
        {
          SumValue |= 0b1L << (meld.Index + offset);
        }
      }
    }

    private void Bakaze(int offset)
    {
      foreach (var meld in _melds)
      {
        if (meld.Index < 4)
        {
          SumValue |= 0b1L << (meld.Index + offset);
        }
      }
    }

    private void Tsuuiisou(int offset)
    {
      SumValue |= (long)_melds.Count << offset;
    }

    private void Chanta(int offset)
    {
      if (_hasMelds)
      {
        SumValue |= 0b1L << offset;
      }
    }
  }
}