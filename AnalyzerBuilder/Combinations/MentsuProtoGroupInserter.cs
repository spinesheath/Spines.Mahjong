using System.Collections.Generic;

namespace AnalyzerBuilder.Combinations
{
  internal class MentsuProtoGroupInserter : IProtoGroupInserter
  {
    public MentsuProtoGroupInserter(int required, int occupied)
    {
      _required = required;
      _occupied = occupied;
    }

    public bool CanInsert(IReadOnlyList<int> concealedTiles, IReadOnlyList<int> usedTiles, int offset)
    {
      return (concealedTiles[offset] == _required || concealedTiles[offset] > _occupied) && usedTiles[offset] <= 4 - _occupied;
    }

    public void Insert(IList<int> concealedTiles, IList<int> usedTiles, int offset)
    {
      concealedTiles[offset] -= _required;
      usedTiles[offset] += _occupied;
    }

    public void Remove(IList<int> concealedTiles, IList<int> usedTiles, int offset)
    {
      concealedTiles[offset] += _required;
      usedTiles[offset] -= _occupied;
    }

    private readonly int _occupied;
    private readonly int _required;
  }
}