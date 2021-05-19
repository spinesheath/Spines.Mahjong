using System.Collections.Generic;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class MeldArrangement : Arrangement
  {
    public MeldArrangement(IEnumerable<Block> blocks) 
      : base(blocks)
    {
      var indexes = new List<int>();
      foreach (var permutation in Blocks.Permute())
      {
        var h = 0;
        foreach (var b in permutation)
        {
          h *= 35;
          h += b.Id + 1;
        }
        
        indexes.Add(h);
      }

      LookupIndexes = indexes;
    }

    public IReadOnlyList<int> LookupIndexes { get; }
  }
}