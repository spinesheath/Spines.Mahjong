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
      Toitoi(29);
      Tanyao(30);
      IipeikouRyanpeikouChiitoitsu();
      Pinfu(51);
      Ankou(34 - 2);
      HonitsuChinitsu(19);
      MenzenTsumo(12);
      KokushiMusou();
      Chinroutou(38);
      Chuuren(62);
    }

    public long AndValue { get; private set; }

    public long OrValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private int TileCount => _interpretations.First().TileCount;

    private IReadOnlyList<int> TileCounts => _interpretations.First().TileCounts;

    private readonly IReadOnlyList<ConcealedArrangement> _interpretations;

    private void KokushiMusou()
    {
      foreach (var arrangement in _interpretations)
      {
        if (!arrangement.IsStandard && TileCounts.Any(c => c == 1))
        {
          WaitShiftValue |= 0b1L << 0;
          SumValue |= 0b1L << 0;

          for (var i = 0; i < TileCounts.Count; i++)
          {
            if (TileCounts[i] == 2)
            {
              WaitShiftValue |= 0b1L << (i + 1);
            }
          }
        }
      }
    }

    private void MenzenTsumo(int offset)
    {
      WaitShiftValue |= 0b111111111_1L << (offset - 2);
    }

    private void HonitsuChinitsu(int offset)
    {
      if (TileCount > 0)
      {
        OrValue |= 0b101L << (offset + 4);
      }
    }

    private void Ankou(int offset)
    {
      var countsWithWait = Enumerable.Range(0, 9).Select(i => _interpretations.Max(a => AnkouCountWithWait(i, a))).ToList();
      var minCountWithWait = countsWithWait.Min();
      var maxCountWithWait = countsWithWait.Max();

      OrValue |= (long)minCountWithWait << offset;
      WaitShiftValue |= (long)(maxCountWithWait - minCountWithWait) << offset;
      for (var i = 0; i < 9; i++)
      {
        WaitShiftValue |= (long)(maxCountWithWait - minCountWithWait) << (1 + i + offset);
        WaitShiftValue |= (long)(countsWithWait[i] - minCountWithWait) << (10 + i + offset);
      }

      var tankiArrangement = _interpretations.FirstOrDefault(i => i.IsStandard && i.Blocks.All(b => b.IsKoutsu || b.IsPair) && i.Blocks.Any(b => b.IsPair));
      if (tankiArrangement != null)
      {
        var pairIndex = tankiArrangement.Blocks.First(b => b.IsPair).Index;
        WaitShiftValue |= 0b1L << (1 + pairIndex);
      }
    }

    private static int AnkouCountWithWait(int waitIndex, ConcealedArrangement a)
    {
      var offWaitAnkou = a.Blocks.Count(b => b.IsKoutsu && b.Index != waitIndex);
      var onWaitAnkou = a.Blocks.Count(b => b.IsKoutsu && b.Index == waitIndex);
      var onWaitShuntsu = a.Blocks.Count(b => b.IsShuntsu && waitIndex >= b.Index && waitIndex <= b.Index + 2);
      // on wait ankou can only count if there also is an on wait shuntsu to consume the wait.
      return offWaitAnkou + onWaitAnkou * onWaitShuntsu;
    }

    private void Pinfu(int offset)
    {
      var ittsuu = _interpretations.Any(HasIttsuu);

      var pinfuArrangements = _interpretations.Where(a => a.IsStandard && a.Blocks.All(b => b.IsShuntsu || b.IsPair)).ToList();
      if (pinfuArrangements.Any())
      {
        if (TileCount % 3 == 2)
        {
          OrValue |= 0b1L << (offset - 1);
        }

        WaitShiftValue |= 0b1L << offset;

        foreach (var arrangement in pinfuArrangements.Where(a => !ittsuu || HasIttsuu(a)))
        {
          for (var i = 0; i < 9; i++)
          {
            if (arrangement.Blocks.Any(b => IsRyanmen(b, i)))
            {
              WaitShiftValue |= 0b1L << (1 + i + offset);
            }
          }
        }

        // This part clears out pinfu if it is locked by sanshoku with a 22334455 shape.
        OrValue |= 0b1111111111_1L << offset;
        if (TileCount == 8 && TileCounts.SkipWhile(c => c == 0).TakeWhile(c => c == 2).Count() == 4)
        {
          OrValue |= 0b1L << offset;
          var depth = TileCounts.TakeWhile(c => c == 0).Count();
          // This block is shifted by the sanshoku shift value's first bit, then by the index of the wait.
          // The sanshoku part moves the 10101 shape depending on whether it's 223344+55 or 22+334455.
          // The flag has two 0's and is placed carefully to align it properly after the sanshoku shift.
          OrValue ^= 0b10101L << (offset + depth + 1 - (depth % 2));
        }
      }
    }

    private static bool IsRyanmen(Block block, int waitIndex)
    {
      return block.IsShuntsu && (block.Index == waitIndex && block.Index < 6 || block.Index + 2 == waitIndex && block.Index > 0);
    }

    private bool HasIttsuu(ConcealedArrangement arrangement)
    {
      var shuntsus = arrangement.Blocks.Where(b => b.IsShuntsu).ToList();
      return shuntsus.Any(s => s.Index == 0) && shuntsus.Any(s => s.Index == 3) && shuntsus.Any(s => s.Index == 6);
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

      var canBeChiitoitsu = TileCounts.All(c => c == 0 || c == 2);

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
      if (TileCount == 0 || TileCounts[0] + TileCounts[4] + TileCounts[6] + TileCounts[8] == 0)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Chuuren(int offset)
    {
      var isChuuren = true;
      isChuuren &= TileCounts[0] >= 3;
      isChuuren &= TileCounts[1] >= 1;
      isChuuren &= TileCounts[2] >= 1;
      isChuuren &= TileCounts[3] >= 1;
      isChuuren &= TileCounts[4] >= 1;
      isChuuren &= TileCounts[5] >= 1;
      isChuuren &= TileCounts[6] >= 1;
      isChuuren &= TileCounts[7] >= 1;
      isChuuren &= TileCounts[8] >= 3;
      if (isChuuren)
      {
        OrValue |= 0b1L << offset;

        var junseiIndex = 0;
        for (var i = 0; i < 9; i++)
        {
          if (TileCounts[i] % 2 == 0)
          {
            junseiIndex = i;
          }
        }

        WaitShiftValue |= 0b1L << (1 + junseiIndex);
      }
    }

    private void Chinroutou(int offset)
    {
      if (TileCounts[0] + TileCounts[8] == TileCount)
      {
        OrValue |= 0b1L << offset;
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
      if (TileCounts[0] + TileCounts[8] == 0)
      {
        OrValue |= 0b1L << offset;
      }
    }

    private void Tsuuiisou(int offset)
    {
      if (TileCount == 0)
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
        if (_interpretations.Any(a => a.ContainsShuntsu(i)))
        {
          var shift = i;
          OrValue |= shift + 4L;
          OrValue |= 0b1L << (shift + 6);
        }
      }
    }
  }
}