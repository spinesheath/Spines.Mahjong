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
      
      SanshokuDoujun();
      SanshokuDoukou();
      HonitsuChinitsu(19);
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    public long SumValue { get; } = 0L;

    public long WaitShiftValue { get; private set; }

    private readonly bool _hasMelds;

    private readonly IReadOnlyList<Block> _melds;

    private void HonitsuChinitsu(int offset)
    {
      if (_hasMelds)
      {
        OrValue |= 0b101L << (offset + 4);
      }
    }

    private void Ankou(int offset)
    {
      var ankanCount = _melds.Count(m => m.IsAnkan);
      WaitShiftValue |= (long)ankanCount << offset;
    }

    private void Ittsuu(int offset)
    {
      foreach (var meld in _melds.Where(b => b.IsShuntsu && b.Index % 3 == 0))
      {
        OrValue |= 0b1L << offset + meld.Index / 3;
      }

      AndValue |= 0b111L << offset;
    }

    private void Ryuuiisou(int offset)
    {
      if (_melds.All(m => !new[] {0, 4, 6, 8}.Contains(m.Index) && (!m.IsShuntsu || m.Index == 1)))
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
        AndValue |= 0b1L << (offset + 1);
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

    private void Chanta(int offset)
    {
      if (!_hasMelds)
      {
        AndValue |= 0b1L << offset;
      }

      if (_melds.All(m => m.IsJunchanBlock))
      {
        AndValue |= 0b1L << (offset + 1);
      }
    }

    private void SanshokuDoukou()
    {
      for (var i = 0; i < 9; i++)
      {
        if (_melds.Any(m => (m.IsKoutsu || m.IsKantsu) && m.Index == i))
        {
          var shift = i + 7;
          OrValue |= shift + 2L;
          OrValue |= 0b1L << (shift + 6);
        }
      }
    }

    private void SanshokuDoujun()
    {
      for (var i = 0; i < 7; i++)
      {
        if (_melds.Any(m => m.IsShuntsu && m.Index == i))
        {
          var shift = i;
          OrValue |= shift + 4L;
          OrValue |= 0b1L << (shift + 6);
        }
      }
    }
  }
}