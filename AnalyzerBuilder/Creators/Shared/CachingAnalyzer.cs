using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AnalyzerBuilder.Combinations;
using Spines.Mahjong.Analysis;

namespace AnalyzerBuilder.Creators.Shared
{
  internal class CachingAnalyzer
  {
    public CachingAnalyzer(int suitLength)
    {
      Debug.Assert(suitLength == 7 || suitLength == 9);
      _suitLength = suitLength;
    }

    public ISet<Arrangement> Analyze(PartialHandIterator it)
    {
      if (_cache.TryGetValue(it.Base5Hash, out var result))
      {
        return result;
      }

      return Analyze(it.Base5Hash, it.Counts);
    }

    public ISet<Arrangement> AnalyzeWithExtraTile(PartialHandIterator it, int tileIndex)
    {
      if (it.TileCount == 14)
      {
        return Empty;
      }

      var base5Hash = it.Base5Hash + Base5.Table[tileIndex];

      if (_cache.TryGetValue(base5Hash, out var result))
      {
        return result;
      }

      var counts = it.Counts.ToArray();
      counts[tileIndex] += 1;

      return Analyze(base5Hash, counts);
    }

    private static readonly ISet<Arrangement> Empty = new HashSet<Arrangement>();

    private readonly Dictionary<int, ISet<Arrangement>> _cache = new();
    private readonly Arrangement[] _arrangements = new Arrangement[75];
    
    private readonly int _suitLength;

    private ISet<Arrangement> Analyze(int base5Hash, byte[] counts)
    {
      var arrangements = _suitLength == 7 ? ProtoGroup.AnalyzeHonor(counts) : ProtoGroup.AnalyzeSuit(counts);
      
      var set = new HashSet<Arrangement>(arrangements.Count);
      foreach (var arrangement in arrangements)
      {
        var existing = _arrangements[arrangement.Id];
        if (existing != null)
        {
          set.Add(existing);
        }
        else
        {
          _arrangements[arrangement.Id] = arrangement;
          set.Add(arrangement);
        }
      }

      _cache.Add(base5Hash, set);
      return set;
    }
  }
}