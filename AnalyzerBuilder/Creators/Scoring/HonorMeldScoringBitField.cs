using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class HonorMeldScoringBitField
  {
    private readonly IReadOnlyList<Block> _melds;
    private readonly bool _hasMelds;

    public HonorMeldScoringBitField(IReadOnlyList<Block> melds)
    {
      _melds = melds;
      _hasMelds = melds.Count > 0;

      SanshokuDoujun(0); // 7 + 7 bit
      SanshokuDoukou(14); // 9 bit
      Chanta(23); // 2 bit
      Toitoi(25); // 1 bit
      Honroutou(26); // 1 bit
      Tsuuiisou(27); // 1 bit
      Tanyao(28); // 1 bit
      Junchan(29); // 2 bit
      Chinroutou(31); // 1 bit
      Chuuren(32); // 1 bit
      Ryuuiisou(33); // 1 bit
      Yakuhai(34); // 11 bit
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    private void Yakuhai(int offset)
    {
      AndValue |= 0b111_1111_1111L << offset;
      foreach (var meld in _melds)
      {
        if (meld.Index < 4)
        {
          OrValue |= 0b10001L << (meld.Index + offset);
        }
        else
        {
          OrValue |= 0b1L << (meld.Index + offset + 4);
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