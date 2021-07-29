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

      Jikaze(15);
      Jikaze(57);
      SangenYakuhai(42);
      Sangen();
      Suushi();
      HonitsuChinitsu(19);
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly bool _hasMelds;
    private readonly IReadOnlyList<Block> _melds;

    private void HonitsuChinitsu(int offset)
    {
      if (!_hasMelds)
      {
        SumValue |= 0b1L << (offset + 2);
      }
      else
      {
        SumValue |= 0b11L << offset;
      }
    }

    private void Ankou(int offset)
    {
      var ankanCount = _melds.Count(m => m.IsAnkan);
      WaitShiftValue |= (long)ankanCount << offset;
    }

    private void Pinfu(int offset)
    {

    }

    private void Suushi()
    {
      var koutsuCount = _melds.Count(b => b.Index < 4);
      SumValue |= (long) koutsuCount << 9;
      SumValue |= (long) koutsuCount << 12;
    }

    private void Sangen()
    {
      var koutsuCount = _melds.Count(b => b.Index > 3);
      var sangenCount = koutsuCount > 1 ? koutsuCount + 1 : koutsuCount;
      SumValue |= (long) sangenCount << 3;
      SumValue |= (long) sangenCount << 6;
    }

    private void IipeikouRyanpeikou(int offset)
    {
    }

    private void SangenYakuhai(int offset)
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

    private void Ryuuiisou(int offset)
    {
      if (!_hasMelds || _melds.All(m => m.Index == 5))
      {
        AndValue |= 0b1L << offset;
        OrValue |= 0b1L << offset;
      }
    }

    private void Chuuren(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Chinroutou(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Junchan(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Tanyao(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Tsuuiisou(int offset)
    {
      AndValue |= 0b1L << offset;
    }

    private void Honroutou(int offset)
    {
      if (_hasMelds)
      {
        AndValue |= 0b1L << offset;
        OrValue |= 0b1L << offset;
      }
    }

    private void Toitoi(int offset)
    {
      AndValue |= 0b1L << offset;
    }

    private void Chanta(int offset)
    {
      if (_hasMelds)
      {
        AndValue |= 0b10L << offset;
        OrValue |= 0b10L << offset;
      }
    }

    private void SanshokuDoukou(int offset)
    {
      AndValue |= 0b111111111L << offset;
    }

    private void SanshokuDoujun(int offset)
    {
      AndValue |= 0b1111111_1111111L << offset;
    }
  }
}