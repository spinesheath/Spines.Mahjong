using System;

namespace AnalyzerBuilder.Combinations
{
  internal class MentsuProtoGroupInserter : IProtoGroupInserter
  {
    public MentsuProtoGroupInserter(byte required, byte occupied)
    {
      _required = required;
      _occupied = occupied;
    }

    public bool CanInsert(Span<byte> concealedTiles, Span<byte> usedTiles, int offset)
    {
      return (concealedTiles[offset] == _required || concealedTiles[offset] > _occupied) && usedTiles[offset] <= 4 - _occupied;
    }

    public void Insert(Span<byte> concealedTiles, Span<byte> usedTiles, int offset)
    {
      concealedTiles[offset] -= _required;
      usedTiles[offset] += _occupied;
    }

    public void Remove(Span<byte> concealedTiles, Span<byte> usedTiles, int offset)
    {
      concealedTiles[offset] += _required;
      usedTiles[offset] -= _occupied;
    }

    private readonly byte _occupied;
    private readonly byte _required;
  }
}