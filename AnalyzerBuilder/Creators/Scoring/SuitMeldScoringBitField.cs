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
      HonitsuChinitsu(20);
    }

    public long OrValue { get; private set; }

    private readonly bool _hasMelds;

    private readonly IReadOnlyList<Block> _melds;

    private void HonitsuChinitsu(int offset)
    {
      if (_hasMelds)
      {
        OrValue |= 0b101000L << offset;
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