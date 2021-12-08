using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// Analyzes the part of a hand in a single suit.
  /// </summary>
  internal class TileGroupAnalyzer
  {
    /// <summary>
    /// Creates a new instance of TileGroupAnalyzer for analyzing honors.
    /// </summary>
    public static TileGroupAnalyzer ForHonors(Combination concealedTiles, Combination meldedTiles, int meldCount)
    {
      return new TileGroupAnalyzer(concealedTiles, meldedTiles, meldCount, false);
    }

    /// <summary>
    /// Creates a new instance of TileGroupAnalyzer for analyzing suits.
    /// </summary>
    public static TileGroupAnalyzer ForSuits(Combination concealedTiles, Combination meldedTiles, int meldCount)
    {
      return new TileGroupAnalyzer(concealedTiles, meldedTiles, meldCount, true);
    }

    /// <summary>
    /// Returns all possible arrangements for the given hand.
    /// </summary>
    public IEnumerable<Arrangement> Analyze()
    {
      var comparer = new ArrangementComparer();
      var arrangement = new Arrangement(0, _meldCount, _meldCount * 3);
      _usedMelds = _meldCount;
      _jantouValue = 0;
      Analyze(arrangement, 0, 0);
      var arrangements =
        _arrangements.Where(a => !_arrangements.Any(other => comparer.IsWorseThan(a, other))).OrderBy(a => a.Id);
      var compacter = new ArrangementGroupCompacter();
      return compacter.GetCompacted(arrangements);
    }

    /// <summary>
    /// Creates a new instance of TileGroupAnalyzer.
    /// </summary>
    private TileGroupAnalyzer(Combination concealedTiles, Combination meldedTiles, int meldCount, bool allowShuntsu)
    {
      Debug.Assert(meldCount >= 0 && meldCount <= 4);

      _meldCount = meldCount;
      _concealed = concealedTiles.Counts.Select(c => (byte)c).ToArray();
      _used = meldedTiles.Counts.Select(c => (byte)c).ToArray();
      _tileTypeCount = concealedTiles.Counts.Count;
      _protoGroups = allowShuntsu ? SuitProtoGroups : HonorProtoGroups;
    }

    private static readonly IReadOnlyList<ProtoGroup> SuitProtoGroups = new List<ProtoGroup>
    {
      ProtoGroup.Jantou2,
      ProtoGroup.Jantou1,
      ProtoGroup.Shuntsu111,
      ProtoGroup.Shuntsu110,
      ProtoGroup.Shuntsu101,
      ProtoGroup.Shuntsu011,
      ProtoGroup.Shuntsu100,
      ProtoGroup.Shuntsu010,
      ProtoGroup.Shuntsu001,
      ProtoGroup.Koutsu3,
      ProtoGroup.Koutsu2,
      ProtoGroup.Koutsu1
    };

    private static readonly IReadOnlyList<ProtoGroup> HonorProtoGroups = new List<ProtoGroup>
    {
      ProtoGroup.Jantou2,
      ProtoGroup.Jantou1,
      ProtoGroup.Koutsu3,
      ProtoGroup.Koutsu2,
      ProtoGroup.Koutsu1
    };

    private readonly ISet<Arrangement> _arrangements = new HashSet<Arrangement>();
    private readonly byte[] _concealed;
    private readonly int _meldCount;

    private readonly IReadOnlyList<ProtoGroup> _protoGroups;
    private readonly int _tileTypeCount;

    private readonly byte[] _used;
    private int _jantouValue;
    private int _usedMelds;

    private void Analyze(Arrangement arrangement, int currentTileType, int currentProtoGroup)
    {
      if (currentTileType >= _tileTypeCount)
      {
        _arrangements.Add(arrangement);
        return;
      }

      Analyze(arrangement, currentTileType + 1, 0);

      // Inlined a bunch of things for about 25% performance gain.
      var count = _protoGroups.Count;
      for (var i = currentProtoGroup; i < count; ++i)
      {
        var protoGroup = _protoGroups[i];
        var isJantou = i <= 1;
        if (isJantou)
        {
          if (_jantouValue != 0 || !protoGroup.CanInsert(_concealed, _used, currentTileType))
          {
            continue;
          }

          protoGroup.Insert(_concealed, _used, currentTileType);

          var oldJantouValue = _jantouValue;
          _jantouValue = protoGroup.Value;
          var added = arrangement.SetJantouValue(_jantouValue);

          Analyze(added, currentTileType, i);

          protoGroup.Remove(_concealed, _used, currentTileType);
          _jantouValue = oldJantouValue;
        }
        else
        {
          if (_usedMelds == 4 || !protoGroup.CanInsert(_concealed, _used, currentTileType))
          {
            continue;
          }

          protoGroup.Insert(_concealed, _used, currentTileType);

          _usedMelds += 1;
          var added = arrangement.AddMentsu(protoGroup.Value);

          Analyze(added, currentTileType, i);

          protoGroup.Remove(_concealed, _used, currentTileType);
          _usedMelds -= 1;
        }
      }
    }
  }
}