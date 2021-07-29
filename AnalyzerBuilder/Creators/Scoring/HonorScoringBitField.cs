using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class HonorScoringBitField
  {
    public HonorScoringBitField(ConcealedArrangement arrangement)
    {
      _arrangement = arrangement;
      _isEmpty = arrangement.TileCount == 0;

      //Chanta(23);
      Toitoi(29);
      Tanyao(30);
      //Honroutou(26);
      //Tsuuiisou(27);
      //Junchan(29);
      //Chinroutou(31);
      //Chuuren(32);
      //Ryuuiisou(33);
      Jikaze(15);
      Bakaze(57);
      SangenYakuhai(42);
      //IipeikouRyanpeikou(45);
      Sangen();
      Suushi();
      Pinfu(10);
      Ankou(34 - 2);
      Chiitoitsu();
      HonitsuChinitsu(19);
      MenzenTsumo();
      KokushiMusou();
    }

    public long AndValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly ConcealedArrangement _arrangement;
    private readonly bool _isEmpty;

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

    private void MenzenTsumo()
    {
      WaitShiftValue |= 0b111111111_1L << 52;
    }

    private void HonitsuChinitsu(int offset)
    {
      if (_isEmpty)
      {
        SumValue |= 0b1L << (offset + 2);
      }
      else
      {
        SumValue |= 0b11L << offset;
      }
    }

    private void Ankou(int offset)
    {
      var countsWithWait = Enumerable.Range(0, 9).Select(i => _arrangement.Blocks.Count(b => b.IsKoutsu && b.Index != i)).ToList();
      var minCountWithWait = countsWithWait.Min();
      var maxCountWithWait = countsWithWait.Max();

      SumValue |= (long)minCountWithWait << offset;
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

    private void Chiitoitsu()
    {
      const int baseIndex = 37;

      var canBeChiitoitsu = _arrangement.TileCounts.All(c => c == 0 || c == 2);

      if (canBeChiitoitsu)
      {
        SumValue |= 1L << (baseIndex + 2);
      }
    }

    private void Pinfu(int offset)
    {
      if (_arrangement.IsStandard)
      {
        var pair = _arrangement.Blocks.FirstOrDefault(b => b.IsPair);
        if (pair != null && pair.Index < 4 && _arrangement.Blocks.Count == 1)
        {
          WaitShiftValue |= PinfuWindBits[pair.Index] << offset;
        }
        else if (!_arrangement.Blocks.Any())
        {
          WaitShiftValue |= 0b111_111_111_111_1L << offset;
        }
      }
    }

    private void Suushi()
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index < 4 && b.IsKoutsu);
      var pairCount = _arrangement.Blocks.Count(b => b.Index < 4 && b.IsPair);

      var shousuushiCount = pairCount == 1 ? koutsuCount + 1 : 0;
      SumValue |= (long) shousuushiCount << 9;
      SumValue |= (long) koutsuCount << 12;
    }

    private void Sangen()
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsKoutsu);
      var pairCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsPair);

      var blockCount = pairCount == 1 ? koutsuCount + 1 : 0;
      var shousangenCount = blockCount > 1 ? blockCount + 1 : blockCount;
      SumValue |= (long)shousangenCount << 3;
      
      var daisangenCount = koutsuCount > 1 ? koutsuCount + 1 : koutsuCount;
      SumValue |= (long)daisangenCount << 6;
    }

    private void IipeikouRyanpeikou(int offset)
    {
    }

    private void SangenYakuhai(int offset)
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

    private void Ryuuiisou(int offset)
    {
      if (_isEmpty || _arrangement.TileCounts[5] == _arrangement.TileCount)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Chuuren(int offset)
    {
      if (_isEmpty)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Chinroutou(int offset)
    {
      if (_isEmpty)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Junchan(int offset)
    {
      if (_isEmpty)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Tanyao(int offset)
    {
      if (_isEmpty)
      {
        SumValue |= 0b1L << offset;
      }
    }

    private void Tsuuiisou(int offset)
    {
      AndValue |= 0b1L << offset;
    }

    private void Honroutou(int offset)
    {
      if (!_isEmpty)
      {
        AndValue |= 0b1L << offset;
      }
    }

    private void Toitoi(int offset)
    {
      if (_arrangement.IsStandard)
      {
        SumValue |= 0b1L << offset;
      }
    }

    private void Chanta(int offset)
    {
      if (!_isEmpty)
      {
        AndValue |= 0b11L << offset;
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

    private static readonly long[] PinfuWindBits = 
    {
      0b101_010_101_010_1L,
      0b100_110_011_001_1L,
      0b001_110_000_111_1L,
      0b000_000_111_111_1L
    };
  }
}

//0000 > 0 > nothing
//0001 > 1 > EE
//0010 > 2 > SS
//0011 > 3 > ES
//0100 > 4 > WW
//0101 > 5 > EW
//0110 > 6 > SW
//0111 > 7 > nothing
//1000 > 8 > NN
//1001 > 9 > EN
//1010 > 10 > SN
//1011 > 11 > nothing
//1100 > 12 > WN
//1101 > 13 > nothing
//1110 > 14 > nothing
//1111 > 15 > nothing