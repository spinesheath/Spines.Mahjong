using System;

namespace AnalyzerBuilder.Combinations
{
  internal interface IProtoGroupInserter
  {
    bool CanInsert(Span<byte> concealedTiles, Span<byte> usedTiles, int offset);

    void Insert(Span<byte> concealedTiles, Span<byte> usedTiles, int offset);

    void Remove(Span<byte> concealedTiles, Span<byte> usedTiles, int offset);
  }
}