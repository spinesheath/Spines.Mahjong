using System.Collections.Generic;

namespace AnalyzerBuilder.Combinations
{
  internal interface IProtoGroupInserter
  {
    bool CanInsert(IReadOnlyList<int> concealedTiles, IReadOnlyList<int> usedTiles, int offset);

    void Insert(IList<int> concealedTiles, IList<int> usedTiles, int offset);

    void Remove(IList<int> concealedTiles, IList<int> usedTiles, int offset);
  }
}