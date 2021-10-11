using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class SuitScoringBitField
  {
    public SuitScoringBitField(IEnumerable<Arrangement> interpretations)
    {
      _interpretations = interpretations.ToList();
      _iipeikouCount = CalculateIipeikouCount();

      // Yaku
      SanshokuDoujun();
      SanshokuDoukou();
      Tanyao(29);
      IipeikouRyanpeikou(38);
      Chiitoitsu(40);
      Pinfu(51);
      Ankou(32);
      HonitsuChinitsu(20);
      KokushiMusou();
      Chinroutou(37);
      Chuuren(62);
      Chanta(27);
      Honroutou(26);
      Toitoi(31);
      Junchan(49);
      Ittsuu(44);
      Ryuuiisou(28);

      // Fu
      SingleWait(10);
      AnkouFu(20);
    }

    private void AnkouFu(int offset)
    {
      var value = 0;
      foreach (var arrangement in _interpretations)
      {
        var koutsus = arrangement.Blocks.Where(b => b.IsKoutsu).ToList();
        var localValue = koutsus.Count + koutsus.Count(k => k.IsJunchanBlock);
        value = Math.Max(value, localValue);
      }
      
      WaitShiftValue |= (long)value << offset;

      // u-type = 11123444 and 11123456777 shapes (guaranteed ankou, but lower value if wait on 1)
      var hasUType1 = false;
      var hasUType9 = false;
      var hasSquareType = false;
      foreach (var arrangement in _interpretations)
      {
        for (var i = 0; i < 7; i++)
        {
          if (arrangement.ContainsKoutsu(i) && arrangement.ContainsKoutsu(i + 1) && arrangement.ContainsKoutsu(i + 2))
          {
            hasSquareType = true;
          }
        }

        if (arrangement.ContainsKoutsu(0) && arrangement.ContainsShuntsu(1))
        {
          if (arrangement.ContainsPair(3))
          {
            hasUType1 = true;
          }

          if (arrangement.ContainsShuntsu(4) && arrangement.ContainsPair(6))
          {
            hasUType1 = true;
          }
        }
        else if (arrangement.ContainsKoutsu(8) && arrangement.ContainsShuntsu(5))
        {
          if (arrangement.ContainsPair(5))
          {
            hasUType9 = true;
          }

          if (arrangement.ContainsShuntsu(2) && arrangement.ContainsPair(2))
          {
            hasUType9 = true;
          }
        }
      }

      if (hasUType1)
      {
        WaitShiftValue |= 18L << 24;
      }

      if (hasUType9)
      {
        WaitShiftValue |= 9L << 24;
      }

      if (hasSquareType)
      {
        WaitShiftValue |= 1L << 29;
      }
    }

    private void SingleWait(int offset)
    {
      var hasPossibleOutsideKoutsu = _interpretations.Any(a => a.Blocks.Any(b => b.IsKoutsu && b.IsJunchanBlock));
      
      foreach (var arrangement in _interpretations)
      {
        if (!arrangement.IsStandard)
        {
          continue;
        }

        if (hasPossibleOutsideKoutsu && arrangement.Blocks.All(b => !b.IsKoutsu || !b.IsJunchanBlock))
        {
          continue;
        }

        foreach (var block in arrangement.Blocks)
        {
          if (block.IsPair)
          {
            WaitShiftValue |= 1L << (offset + 1 + block.Index);
          }
          else if (block.IsShuntsu)
          {
            WaitShiftValue |= 1L << (offset + 1 + block.Index + 1);
              
            if (block.Index == 0)
            {
              WaitShiftValue |= 1L << (offset + 1 + 2);
            }

            if (block.Index == 6)
            {
              WaitShiftValue |= 1L << (offset + 1 + 6);
            }
          }
        }
      }
    }

    public long OrValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private int TileCount => _interpretations.First().TileCount;

    private IReadOnlyList<int> TileCounts => _interpretations.First().TileCounts;
    
    private readonly IReadOnlyList<Arrangement> _interpretations;

    private readonly int _iipeikouCount;

    private void Ryuuiisou(int offset)
    {
      foreach (var arrangement in _interpretations.Where(a => a.IsStandard))
      {
        if (arrangement.TileCount == 0)
        {
          OrValue |= 0b1L << offset;
        }

        if (arrangement.TileCounts[0] + arrangement.TileCounts[4] + arrangement.TileCounts[6] + arrangement.TileCounts[8] == 0)
        {
          OrValue |= 0b1L << (offset + 2);
        }
      }
    }

    private void Ittsuu(int offset)
    {
      foreach (var arrangement in _interpretations.Where(a => a.IsStandard))
      {
        for (var i = 0; i < 3; i++)
        {
          if (arrangement.Blocks.Any(b => b.IsShuntsu && b.Index == i * 3))
          {
            OrValue |= 0b1L << offset + i;
          }
        }
      }
    }

    private void Junchan(int offset)
    {
      if (_interpretations.Any(a => a.IsStandard && a.Blocks.All(b => b.IsJunchanBlock)))
      {
        OrValue |= 0b1L << offset;
      }
    }
    
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

    private void HonitsuChinitsu(int offset)
    {
      if (TileCount > 0)
      {
        OrValue |= 0b101000L << offset;
      }
    }

    private void Ankou(int offset)
    {
      if (_iipeikouCount == 2)
      {
        return;
      }

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

    private static int AnkouCountWithWait(int waitIndex, Arrangement a)
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

    private bool HasIttsuu(Arrangement arrangement)
    {
      var shuntsus = arrangement.Blocks.Where(b => b.IsShuntsu).ToList();
      return shuntsus.Any(s => s.Index == 0) && shuntsus.Any(s => s.Index == 3) && shuntsus.Any(s => s.Index == 6);
    }

    private void IipeikouRyanpeikou(int offset)
    {
      if (_iipeikouCount == 1)
      {
        OrValue |= 1L << offset;
      }
      else if (_iipeikouCount == 2)
      {
        OrValue |= 10L << offset;
      }
    }

    private int CalculateIipeikouCount()
    {
      var bestCount = 0;
      foreach (var arrangement in _interpretations)
      {
        var identicalShuntsuGroupings = arrangement.Blocks.Where(b => b.IsShuntsu).GroupBy(b => b.Index).Where(g => g.Count() > 1);
        var currentCount = identicalShuntsuGroupings.Select(g => g.Count() / 2).Sum();
        bestCount = Math.Max(bestCount, currentCount);
      }

      return bestCount;
    }

    private void Chiitoitsu(int offset)
    {
      var canBeChiitoitsu = TileCounts.All(c => c == 0 || c == 2);
      if (TileCount == 0 || !canBeChiitoitsu || _iipeikouCount == 2)
      {
        return;
      }

      var pairCount = TileCounts.Count(c => c == 2);
      switch (pairCount)
      {
        case 7:
        case 6:
        case 5:
        {
          OrValue |= 8L << offset;
          break;
        }
        case 4:
        {
          OrValue |= (6L - 2L * _iipeikouCount) << offset;
          break;
        }
        case 3:
        {
          OrValue |= (4L - 2L * _iipeikouCount) << offset;
          break;
        }
        case 2:
        {
          OrValue |= 3L << offset;
          break;
        }
        case 1:
        {
          OrValue |= 2L << offset;
          break;
        }
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

    private void Tanyao(int offset)
    {
      if (TileCounts[0] + TileCounts[8] == 0)
      {
        OrValue |= 0b1L << offset;
      }
    }

    /// <summary>
    /// Only 1/9/honors allowed. If no honors, it's chinroutou instead.
    /// Chinroutou is a yakuman, so it clears out any non-yakuman.
    /// So we can ignore the "at least 1 honor group" requirement.
    /// Only need to AND the suits, then potentially set the bit to 0 because of a meld.
    /// </summary>
    private void Honroutou(int offset)
    {
      if (TileCounts.Skip(1).Take(7).Sum() == 0)
      {
        OrValue |= 0b1L << offset;
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
      if (_interpretations.Any(g => g.IsStandard && g.Blocks.All(b => b.IsJunchanBlock)))
      {
        OrValue |= 0b1L << offset;
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

      if (TileCount == 8)
      {
        for (var i = 0; i < 6; i++)
        {
          if (TileCounts[i] == 3 && TileCounts[i + 1] == 1 && TileCounts[i + 2] == 1 && TileCounts[i + 3] == 3)
          {
            OrValue |= 0b1L << 35;
          }
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