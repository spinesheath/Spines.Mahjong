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

      // And
      // TODO find a way to compress to single bits, like right shift by X, and if x == 0 the result is 0? But that's too many bits
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
      IipeikouRyanpeikou(45); // 2 bit
      Sangen(47); // 6 bit, 4 cleared
      Suushi(53); // 6 bit, 4 cleared
      Ittsuu(59); // 4 bit, 3 cleared
      Pinfu(0); // 19 bit, 18 cleared, result on 3rd bit
      Ankou(19); // 19 bit, 17 cleared, result on 3rd and 4th bit?
    }

    public long AndValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly IReadOnlyList<ConcealedArrangement> _interpretations;

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

    private void Suushi(int offset)
    {
    }

    private void Sangen(int offset)
    {
    }

    private void IipeikouRyanpeikou(int offset)
    {
      foreach (var arrangement in _interpretations)
      {
        var identicalShuntsuGroupings = arrangement.Blocks.Where(b => b.IsShuntsu).GroupBy(b => b.Index).Where(g => g.Count() > 1);
        var iipeikouCount = identicalShuntsuGroupings.Sum(g => g.Count()) / 2;
        SumValue |= (long) iipeikouCount << offset;
      }
    }

    private void Yakuhai(int offset)
    {
      // jikaze, bakaze and sangenpai
      AndValue |= 0b111_1111_1111L << offset;
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
      if (_interpretations.Any(g => g.Blocks.All(b => !b.IsShuntsu)))
      {
        AndValue |= 0b1L << offset;
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

    private void SanshokuDoukou(int offset)
    {
      for (var i = 0; i < 9; i++)
      {
        if (_interpretations.Any(a => a.ContainsKoutsu(i)))
        {
          AndValue |= 0b1L << (offset + i);
        }
      }
    }

    private void SanshokuDoujun(int offset)
    {
      for (var i = 0; i < 7; i++)
      {
        if (_interpretations.Any(a => a.ContainsShuntsu(i)))
        {
          // TODO probably need a separate step for open hands anyways, so only use one bit here?
          AndValue |= 0b10000001L << (offset + i); // closed and open
        }
      }
    }
  }
}