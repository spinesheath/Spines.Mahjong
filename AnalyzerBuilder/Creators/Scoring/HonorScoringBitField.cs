using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class HonorScoringBitField
  {
    public HonorScoringBitField(ConcealedArrangement arrangement)
    {
      _arrangement = arrangement;
      _isEmpty = arrangement.TileCount == 0;

      if (arrangement.Base5Hash == 1250)
      {

      }

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

    private readonly ConcealedArrangement _arrangement;
    private readonly bool _isEmpty;

    private void Suushi(int offset)
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index < 4 && b.IsKoutsu);
      var pairCount = _arrangement.Blocks.Count(b => b.Index < 4 && b.IsPair);

      var shousuushiCount = pairCount == 1 ? koutsuCount + 1 : 0;
      SumValue |= (long) shousuushiCount << offset;
      SumValue |= (long) koutsuCount << (offset + 3);
    }

    private void Sangen(int offset)
    {
      var koutsuCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsKoutsu);
      var pairCount = _arrangement.Blocks.Count(b => b.Index > 3 && b.IsPair);

      var blockCount = pairCount == 1 ? koutsuCount + 1 : 0;
      var shousangenCount = blockCount > 1 ? blockCount + 1 : blockCount;
      SumValue |= (long)shousangenCount << offset;
      
      var daisangenCount = koutsuCount > 1 ? koutsuCount + 1 : koutsuCount;
      SumValue |= (long)daisangenCount << (offset + 3);
    }

    private void IipeikouRyanpeikou(int offset)
    {
    }

    private void Yakuhai(int offset)
    {
      for (var i = 0; i < 4; i++)
      {
        if (_arrangement.ContainsKoutsu(i))
        {
          AndValue |= 0b10001L << (offset + i); // jikaze abd bakaze
        }
      }

      for (var i = 4; i < 7; i++)
      {
        if (_arrangement.ContainsKoutsu(i))
        {
          AndValue |= 0b1L << (offset + i + 4); // sangen
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
        AndValue |= 0b1L << offset;
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
      AndValue |= 0b1L << offset;
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