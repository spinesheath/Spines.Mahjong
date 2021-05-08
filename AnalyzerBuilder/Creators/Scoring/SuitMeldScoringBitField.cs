using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class SuitMeldScoringBitField
  {
    public SuitMeldScoringBitField(IReadOnlyList<Block> melds)
    {
      _melds = melds;
      _hasMelds = _melds.Count > 0;

      // And
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

    private void Yakuhai(int offset)
    {
      AndValue |= 0b111_1111_1111L << offset;
    }

    private void Ryuuiisou(int offset)
    {
      if (_melds.All(m => !new [] {0, 4, 6, 8}.Contains(m.Index) && (!m.IsShuntsu || m.Index == 1)))
      {
        AndValue |= 0b1L << offset;
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
      if (_melds.All(m => m.IsJunchanBlock && !m.IsShuntsu))
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

      if (_melds.All(m => m.IsJunchanBlock))
      {
        AndValue |= 0b1L << offset + 1;
      }
    }

    private void Tanyao(int offset)
    {
      if (_melds.All(m => !m.IsJunchanBlock))
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Tsuuiisou(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Honroutou(int offset)
    {
      if (_melds.All(m => m.IsJunchanBlock && !m.IsShuntsu))
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Toitoi(int offset)
    {
      if (_melds.All(m => !m.IsShuntsu))
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Chanta(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }

      if (_melds.All(m => m.IsJunchanBlock))
      {
        AndValue |= 0b1L << offset + 1;
      }
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    private readonly IReadOnlyList<Block> _melds;
    private readonly bool _hasMelds;

    private void SanshokuDoukou(int offset)
    {
      foreach (var meld in _melds)
      {
        if (meld.IsKoutsu || meld.IsKantsu)
        {
          OrValue |= 0b1L << (offset + meld.Index);
        }
      }

      AndValue |= 0b111111111L << offset;
    }

    private void SanshokuDoujun(int offset)
    {
      if (_hasMelds)
      {
        foreach (var meld in _melds)
        {
          if (meld.IsShuntsu)
          {
            OrValue |= 0b1L << (offset + 7 + meld.Index);
          }
        }
      }
      else
      {
        AndValue |= 0b1111111L << offset;
      }

      AndValue |= 0b1111111L << offset + 7;
    }
  }
}