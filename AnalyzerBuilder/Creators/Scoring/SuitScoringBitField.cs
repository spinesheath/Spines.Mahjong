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

      // Sum
      IipeikouRyanpeikou(45); // 2 bit
      Sangen(47); // 6 bit
      Suushi(53); // 6 bit
    }

    public long AndValue { get; private set; }

    public long SumValue { get; private set; }

    private readonly IReadOnlyList<ConcealedArrangement> _interpretations;

    private void Suushi(int offset)
    {
      SumValue |= 0b100L << offset;
    }

    private void Sangen(int offset)
    {
      SumValue |= 0b100L << offset;
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
          AndValue |= 0b10000001L << (offset + i); // closed and open
        }
      }
    }
  }
}