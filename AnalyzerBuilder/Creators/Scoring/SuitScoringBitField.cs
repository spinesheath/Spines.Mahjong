using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class SuitScoringBitField
  {
    public SuitScoringBitField(IEnumerable<ConcealedArrangement> interpretations)
    {
      _interpretations = interpretations.ToList();
      
      SanshokuDoujun();
      SanshokuDoukou();
      //Chanta(23);
      Toitoi(29);
      //Honroutou(26);
      //Tsuuiisou(27);
      //Tanyao(28);
      //Junchan(29);
      //Chinroutou(31);
      //Chuuren(32);
      //Ryuuiisou(33);
      IipeikouRyanpeikouChiitoitsu();
      //Ittsuu(59);
      //Pinfu(0);
      //Ankou(19);
      HonitsuChinitsu();
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly IReadOnlyList<ConcealedArrangement> _interpretations;

    private void HonitsuChinitsu()
    {
      if (_interpretations.First().TileCount > 0)
      {
        OrValue |= 0b1L << 50;
        OrValue |= 0b1L << 52;
        OrValue |= 0b1L << 59;
        OrValue |= 0b1L << 61;
      }
    }

    private void Ankou(int offset)
    {
      var countsWithWait = Enumerable.Range(0, 9).Select(i => _interpretations.Max(a => AnkouCountWithWait(i, a))).ToList();
      var minCountWithWait = countsWithWait.Min();
      var maxCountWithWait = countsWithWait.Max();

      SumValue |= (long)minCountWithWait << offset;
      for (var i = 0; i < 9; i++)
      {
        WaitShiftValue |= (long)(maxCountWithWait - minCountWithWait) << (1 + i + offset);
        WaitShiftValue |= (long)(countsWithWait[i] - minCountWithWait) << (10 + i + offset);
      }
    }

    private static int AnkouCountWithWait(int waitIndex, ConcealedArrangement a)
    {
      var offWaitAnkou = a.Blocks.Count(b => b.IsKoutsu && b.Index != waitIndex);
      var onWaitAnkou = a.Blocks.Count(b => b.IsKoutsu && b.Index == waitIndex);
      var onWaitShuntsu = a.Blocks.Count(b => b.IsShuntsu && b.Index >= waitIndex && b.Index <= waitIndex + 2);
      // on wait ankou can only count if there also is an on wait shuntsu to consume the wait.
      return offWaitAnkou + onWaitAnkou * onWaitShuntsu;
    }

    private void Pinfu(int offset)
    {
      if (_interpretations.Any(a => a.Blocks.All(b => b.IsShuntsu || b.IsPair)))
      {
        WaitShiftValue |= 0b1L << offset;

        for (var i = 0; i < 9; i++)
        {
          if (_interpretations.Any(a => a.Blocks.All(b => b.IsShuntsu || b.IsPair) && a.Blocks.Any(b => b.IsShuntsu && (b.Index == i || b.Index + 2 == i))))
          {
            WaitShiftValue |= 0b1000000001L << (1 + i + offset);
          }
        }
      }
    }

    private void Ittsuu(int offset)
    {
      foreach (var block in _interpretations.SelectMany(a => a.Blocks).Where(b => b.IsShuntsu && b.Index % 3 == 0))
      {
        AndValue |= 0b1L << offset + block.Index / 3;
      }
    }

    private void IipeikouRyanpeikouChiitoitsu()
    {
      const int baseIndex = 37;

      var canBeChiitoitsu = _interpretations.First().TileCounts.All(c => c == 0 || c == 2);

      var iipeikouCount = 0;
      foreach (var arrangement in _interpretations)
      {
        var identicalShuntsuGroupings = arrangement.Blocks.Where(b => b.IsShuntsu).GroupBy(b => b.Index).Where(g => g.Count() > 1);
        iipeikouCount = Math.Max(iipeikouCount, identicalShuntsuGroupings.Count());
      }
      
      if (iipeikouCount == 1)
      {
        OrValue |= 1L << (baseIndex + 0);
        OrValue |= 1L << (baseIndex + 7);
      }
      else if (iipeikouCount == 2)
      {
        OrValue |= 1L << (baseIndex + 8);
      }

      if (canBeChiitoitsu)
      {
        OrValue |= 1L << (baseIndex + 2);
      }
    }

    private void Ryuuiisou(int offset)
    {
      var a = _interpretations.First();
      if (a.TileCount == 0 || a.TileCounts[0] + a.TileCounts[4] + a.TileCounts[6] + a.TileCounts[8] == 0)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Chuuren(int offset)
    {
      var a = _interpretations.First();
      if (a.TileCount == 0)
      {
        AndValue |= 0b1L << offset;
      }
      else
      {
        var isChuuren = true;
        isChuuren &= a.TileCounts[0] >= 3;
        isChuuren &= a.TileCounts[1] >= 1;
        isChuuren &= a.TileCounts[2] >= 1;
        isChuuren &= a.TileCounts[3] >= 1;
        isChuuren &= a.TileCounts[4] >= 1;
        isChuuren &= a.TileCounts[5] >= 1;
        isChuuren &= a.TileCounts[6] >= 1;
        isChuuren &= a.TileCounts[7] >= 1;
        isChuuren &= a.TileCounts[8] >= 3;
        if (isChuuren)
        {
          AndValue |= 0b1L << offset;
        }
      }
    }

    private void Chinroutou(int offset)
    {
      if (_interpretations.Any(a => a.Blocks.All(b => b.IsJunchanBlock && !b.IsShuntsu)))
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Junchan(int offset)
    {
      if (_interpretations.Any(g => g.Blocks.All(b => b.IsJunchanBlock)))
      {
        // 1 bit closed, 1 bit open
        AndValue |= 0b11L << offset;
      }
    }

    private void Tanyao(int offset)
    {
      if (_interpretations.Any(g => g.Blocks.All(b => !b.IsJunchanBlock)))
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Tsuuiisou(int offset)
    {
      if (_interpretations.First().TileCount == 0)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Honroutou(int offset)
    {
      if (_interpretations.Any(g => g.Blocks.All(b => b.IsJunchanBlock && !b.IsShuntsu)))
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Toitoi(int offset)
    {
      if (_interpretations.Any(a => a.IsStandard && a.Blocks.All(b => b.IsKoutsu || b.IsPair)))
      {
        OrValue |= 0b1L << offset;
      }
    }

    private void Chanta(int offset)
    {
      if (_interpretations.Any(g => g.Blocks.All(b => b.IsJunchanBlock)))
      {
        // 1 bit closed, 1 bit open
        AndValue |= 0b11L << offset;
      }
    }

    private void SanshokuDoukou()
    {
      for (var i = 0; i < 9; i++)
      {
        if (_interpretations.Any(a => a.ContainsKoutsu(i)))
        {
          var shift = i + 14;
          OrValue |= shift + 2L;
          OrValue |= 0b1L << (shift + 6);
        }
      }
    }

    private void SanshokuDoujun()
    {
      for (var i = 0; i < 7; i++)
      {
        if (_interpretations.Any(a => a.ContainsShuntsu(i)))
        {
          var shift = 2 * i;
          OrValue |= shift + 4L;
          OrValue |= 0b11L << (shift + 6);
        }
      }
    }
  }
}