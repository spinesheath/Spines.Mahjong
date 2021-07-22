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
      KazeYakuhai(15);
      SangenYakuhai(42);
      //IipeikouRyanpeikou(45);
      Sangen();
      Suushi();
      //Pinfu(0);
      Ankou(34 - 2);
      Chiitoitsu();
      HonitsuChinitsu();
      MenzenTsumo();
    }

    public long AndValue { get; private set; }

    public long SumValue { get; private set; }

    public long WaitShiftValue { get; private set; }

    private readonly ConcealedArrangement _arrangement;
    private readonly bool _isEmpty;

    private void MenzenTsumo()
    {
      WaitShiftValue |= 0b111111111_1L << 52;
    }

    private void HonitsuChinitsu()
    {
      if (_isEmpty)
      {
        SumValue |= 0b1L << 48;
        SumValue |= 0b1L << 57;
      }
      else
      {
        SumValue |= 0b11L << 46;
        SumValue |= 0b11L << 55;
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
      // TODO pinfu
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

    private void KazeYakuhai(int offset)
    {
      for (var i = 0; i < 4; i++)
      {
        if (_arrangement.ContainsKoutsu(i))
        {
          SumValue |= 0b10001L << (offset + i);
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
        SumValue |= 0b11L << offset;
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
  }
}