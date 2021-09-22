using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class HonorScoringBitField
  {
    public HonorScoringBitField(ConcealedArrangement arrangement)
    {
      _arrangement = arrangement;
      _isEmpty = arrangement.TileCount == 0;

      Tanyao(29);
      Jikaze(54);
      Bakaze(58);
      HakuHatsuChun(15);
      Daisangen(6);
      Shousangen(51);
      Suushi();
      Pinfu(51);
      Ankou(32);
      Chiitoitsu(40);
      HonitsuChinitsu(20);
      MenzenTsumo(12);
      KokushiMusou();
      Tsuuiisou(2);
      Chinroutou(37);
      Chanta(27);
      Toitoi(31);
      Honroutou(26);
      Junchan(49);
      Ryuuiisou(28);
    }

    public long OrValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly ConcealedArrangement _arrangement;
    private readonly bool _isEmpty;

    private void Ryuuiisou(int offset)
    {
      if (_arrangement.TileCounts[5] == _arrangement.TileCount)
      {
        SumValue |= 6L << offset;
      }
    }

    private void Junchan(int offset)
    {
      if (_isEmpty)
      {
        OrValue |= 0b1L << offset;
      }
    }

    private void KokushiMusou()
    {
      if (!_arrangement.IsStandard && _arrangement.TileCounts.Any(c => c == 1))
      {
        WaitShiftValue |= 0b1L << 0;
        SumValue |= 0b1L << 0;

        for (var i = 0; i < _arrangement.TileCounts.Count; i++)
        {
          if (_arrangement.TileCounts[i] == 2)
          {
            WaitShiftValue |= 0b1L << (i + 1);
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
      if (!_isEmpty)
      {
        OrValue |= 0b1L << offset;
      }
      else
      {
        OrValue |= 0b100L << offset;
      }
    }

    private void Ankou(int offset)
    {
      var countsWithWait = Enumerable.Range(0, 9).Select(i => _arrangement.Blocks.Count(b => b.IsKoutsu && b.Index != i)).ToList();
      var minCountWithWait = countsWithWait.Min();
      var maxCountWithWait = countsWithWait.Max();

      OrValue |= (long)minCountWithWait << offset;
      WaitShiftValue |= (long)(maxCountWithWait - minCountWithWait) << offset;
      for (var i = 0; i < 9; i++)
      {
        WaitShiftValue |= (long)(maxCountWithWait - minCountWithWait) << (1 + i + offset);
        WaitShiftValue |= (long)(countsWithWait[i] - minCountWithWait) << (10 + i + offset);
      }

      var pair = _arrangement.Blocks.FirstOrDefault(b => b.IsPair);
      if (pair != null)
      {
        WaitShiftValue |= 0b1L << (1 + pair.Index);
      }
    }

    private void Chiitoitsu(int offset)
    {
      var canBeChiitoitsu = _arrangement.TileCounts.All(c => c == 0 || c == 2);
      if (_arrangement.TileCount == 0 || !canBeChiitoitsu)
      {
        return;
      }

      var pairCount = _arrangement.TileCounts.Count(c => c == 2);
      switch (pairCount)
      {
        case 6:
        case 5:
        {
          OrValue |= 8L << offset;
          break;
        }
        case 4:
        {
          OrValue |= 6L << offset;
          break;
        }
        case 3:
        {
          OrValue |= 4L << offset;
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

    private void Pinfu(int offset)
    {
      if (_arrangement.IsStandard)
      {
        var pair = _arrangement.Blocks.FirstOrDefault(b => b.IsPair);
        if (pair != null && pair.Index < 4 && _arrangement.Blocks.Count == 1)
        {
          OrValue|= PinfuWindBits[pair.Index] << offset;
        }
        else if (!_arrangement.Blocks.Any())
        {
          OrValue |= 0b101_110_111_111_1L << offset;
        }
      }

      WaitShiftValue |= 0b1L << offset;
    }

    private void Suushi()
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index < 4 && b.IsKoutsu);
      var pairCount = _arrangement.Blocks.Count(b => b.Index < 4 && b.IsPair);

      var shousuushiCount = pairCount == 1 ? koutsuCount + 1 : 0;
      SumValue |= (long) shousuushiCount << 9;
      SumValue |= (long) koutsuCount << 12;
    }

    private void Shousangen(int offset)
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsKoutsu);
      var pairCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsPair);

      var shousangenCount = 2 * pairCount + koutsuCount;
      SumValue |= (long)shousangenCount << offset;
    }

    private void Daisangen(int offset)
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsKoutsu);
      var daisangenCount = koutsuCount + 1;
      SumValue |= (long)daisangenCount << offset;
    }

    private void HakuHatsuChun(int offset)
    {
      for (var i = 0; i < 3; i++)
      {
        if (_arrangement.ContainsKoutsu(i + 4))
        {
          SumValue |= 0b1L << (offset + i);
        }
      }
    }

    private void Jikaze(int offset)
    {
      for (var i = 0; i < 4; i++)
      {
        if (_arrangement.ContainsKoutsu(i))
        {
          SumValue |= 0b1L << (offset + i);
        }
      }
    }

    private void Bakaze(int offset)
    {
      for (var i = 0; i < 4; i++)
      {
        if (_arrangement.ContainsKoutsu(i))
        {
          SumValue |= 0b1L << (offset + i);
        }
      }
    }

    private void Chinroutou(int offset)
    {
      if (_isEmpty)
      {
        OrValue |= 0b1L << offset;
      }
    }

    private void Tanyao(int offset)
    {
      if (_isEmpty)
      {
        OrValue |= 0b1L << offset;
      }
    }

    private void Tsuuiisou(int offset)
    {
      if (_arrangement.TileCount % 3 == 2)
      {
        SumValue |= ((long) _arrangement.Blocks.Count + 3) << offset;
      }
    }

    /// <summary>
    /// Does not really matter (see comments in suit), but is always set to 1 for calculation reuse purposes.
    /// </summary>
    private void Honroutou(int offset)
    {
      OrValue |= 0b1L << offset;
    }

    private void Toitoi(int offset)
    {
      if (_arrangement.IsStandard)
      {
        OrValue |= 0b1L << offset;
      }
    }

    private void Chanta(int offset)
    {
      if (!_isEmpty && _arrangement.IsStandard)
      {
        OrValue |= 0b1L << offset;
      }
    }

    private static readonly long[] PinfuWindBits = 
    {
      0b101_010_101_010_1L,
      0b100_110_011_001_1L,
      0b001_110_000_111_1L,
      0b000_000_111_111_1L
    };
  }
}
