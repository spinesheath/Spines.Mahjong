using System.Collections.Generic;
using System.Text;

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

    public override string ToString()
    {
      var sb = new StringBuilder();
      foreach (var block in Blocks)
      {
        if (block.IsPair)
        {
          sb.Append((char) ('1' + block.Index), 2);
        }
        else if (block.IsKoutsu)
        {
          sb.Append((char)('1' + block.Index), 3);
        }
        else if (block.IsKantsu)
        {
          sb.Append((char)('1' + block.Index), 4);
        }
        else if(block.IsShuntsu)
        {
          sb.Append((char)('1' + block.Index));
          sb.Append((char)('2' + block.Index));
          sb.Append((char)('3' + block.Index));
        }
        else
        {
          sb.Append('?');
        }
      }

      return sb.ToString();
    }
  }
}