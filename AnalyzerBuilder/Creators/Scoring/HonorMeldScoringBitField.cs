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

      //SanshokuDoujun(0); // 7 + 7 bit
      //SanshokuDoukou(14); // 9 bit
      //Chanta(23); // 2 bit
      //Toitoi(25); // 1 bit
      //Honroutou(26); // 1 bit
      //Tsuuiisou(27); // 1 bit
      //Tanyao(28); // 1 bit
      //Junchan(29); // 2 bit
      //Chinroutou(31); // 1 bit
      //Chuuren(32); // 1 bit
      //Ryuuiisou(33); // 1 bit
      KazeYakuhai(15);
      SangenYakuhai(42);
      //IipeikouRyanpeikou(45); // 2 bit
      Sangen();
      Suushi();
      //Pinfu(0); // 10 bit, 9 bit cleared
      //Ankou(10); // 13 bit, 12 cleared?
      HonitsuChinitsu();
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly bool _hasMelds;
    private readonly IReadOnlyList<Block> _melds;

    private void HonitsuChinitsu()
    {
      if (!_hasMelds)
      {
        SumValue |= 0b1L << 48;
        SumValue |= 0b1L << 57;
      }
      else
      {
        SumValue |= 0b11L << 46;
        SumValue |= 0b11L << 55;
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

    private void KazeYakuhai(int offset)
    {
      foreach (var meld in _melds)
      {
        if (meld.Index < 4)
        {
          SumValue |= 0b10001L << (meld.Index + offset);
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