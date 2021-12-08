using System;
using System.Collections.Generic;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// An (in-)complete Group; either a mentsu or a jantou.
  /// </summary>
  internal class ProtoGroup
  {
    /// <summary>
    /// Creates a new instance of ProtoGroup.
    /// </summary>
    private ProtoGroup(int value, IProtoGroupInserter protoGroupInserter)
    {
      Value = value;
      _protoGroupInserter = protoGroupInserter;
    }

    /// <summary>
    /// The Value of the ProtoGroup.
    /// </summary>
    public int Value { get; }

    public static ISet<Arrangement> AnalyzeHonor(byte[] counts)
    {
      Span<byte> used = stackalloc byte[7];
      Span<byte> t = stackalloc byte[7];
      counts.CopyTo(t);
      var results = new HashSet<Arrangement>();
      Analyze(Honor, t, used, new Arrangement(0, 0, 0), 0, 0, results);
      return results;
    }

    public static ISet<Arrangement> AnalyzeSuit(byte[] counts)
    {
      Span<byte> used = stackalloc byte[9];
      Span<byte> t = stackalloc byte[9];
      counts.CopyTo(t);
      var results = new HashSet<Arrangement>();
      Analyze(Suit, t, used, new Arrangement(0, 0, 0), 0, 0, results);
      return results;
    }

    /// <summary>
    /// Can this ProtoGroup be used in an arrangement?
    /// </summary>
    public bool CanInsert(Span<byte> concealedTiles, Span<byte> usedTiles, int offset)
    {
      return _protoGroupInserter.CanInsert(concealedTiles, usedTiles, offset);
    }

    public void Insert(Span<byte> concealedTiles, Span<byte> usedTiles, int offset)
    {
      _protoGroupInserter.Insert(concealedTiles, usedTiles, offset);
    }

    public void Remove(Span<byte> concealedTiles, Span<byte> usedTiles, int offset)
    {
      _protoGroupInserter.Remove(concealedTiles, usedTiles, offset);
    }

    /// <summary>
    /// A full jantou.
    /// </summary>
    public static readonly ProtoGroup Jantou2 = new(2, new MentsuProtoGroupInserter(2, 2));

    /// <summary>
    /// A jantou missing one tile.
    /// </summary>
    public static readonly ProtoGroup Jantou1 = new(1, new MentsuProtoGroupInserter(1, 2));

    /// <summary>
    /// A full koutsu.
    /// </summary>
    public static readonly ProtoGroup Koutsu3 = new(3, new MentsuProtoGroupInserter(3, 3));

    /// <summary>
    /// A koutsu missing one tile.
    /// </summary>
    public static readonly ProtoGroup Koutsu2 = new(2, new MentsuProtoGroupInserter(2, 3));

    /// <summary>
    /// A koutsu missing two tiles.
    /// </summary>
    public static readonly ProtoGroup Koutsu1 = new(1, new MentsuProtoGroupInserter(1, 3));

    /// <summary>
    /// A full shuntsu.
    /// </summary>
    public static readonly ProtoGroup Shuntsu111 = new(3, new ShuntsuProtoGroupInserter(1, 1, 1));

    /// <summary>
    /// A shuntsu missing the middle tile.
    /// </summary>
    public static readonly ProtoGroup Shuntsu101 = new(2, new ShuntsuProtoGroupInserter(1, 0, 1));

    /// <summary>
    /// A shuntsu missing the left tile.
    /// </summary>
    public static readonly ProtoGroup Shuntsu011 = new(2, new ShuntsuProtoGroupInserter(0, 1, 1));

    /// <summary>
    /// A shuntsu missing the right tile.
    /// </summary>
    public static readonly ProtoGroup Shuntsu110 = new(2, new ShuntsuProtoGroupInserter(1, 1, 0));

    /// <summary>
    /// A shuntsu missing the left and right tiles.
    /// </summary>
    public static readonly ProtoGroup Shuntsu010 = new(1, new ShuntsuProtoGroupInserter(0, 1, 0));

    /// <summary>
    /// A shuntsu missing the middle and right tiles.
    /// </summary>
    public static readonly ProtoGroup Shuntsu100 = new(1, new ShuntsuProtoGroupInserter(1, 0, 0));

    /// <summary>
    /// A shuntsu missing the left and middle tiles.
    /// </summary>
    public static readonly ProtoGroup Shuntsu001 = new(1, new ShuntsuProtoGroupInserter(0, 0, 1));

    private static readonly IReadOnlyList<ProtoGroup> Honor = new List<ProtoGroup>
    {
      Jantou2,
      Jantou1,
      Koutsu3,
      Koutsu2,
      Koutsu1
    };

    private static readonly IReadOnlyList<ProtoGroup> Suit = new List<ProtoGroup>
    {
      Jantou2,
      Jantou1,
      Shuntsu111,
      Shuntsu110,
      Shuntsu101,
      Shuntsu011,
      Shuntsu100,
      Shuntsu010,
      Shuntsu001,
      Koutsu3,
      Koutsu2,
      Koutsu1
    };

    /// <summary>
    /// Can this ProtoGroup be used in an arrangement?
    /// </summary>
    private readonly IProtoGroupInserter _protoGroupInserter;

    private static void Analyze(IReadOnlyList<ProtoGroup> protoGroups, Span<byte> counts, Span<byte> used, Arrangement arrangement, int currentTileType, int currentProtoGroup, ISet<Arrangement> results)
    {
      if (currentTileType >= counts.Length)
      {
        results.Add(arrangement);
        return;
      }

      Analyze(protoGroups, counts, used, arrangement, currentTileType + 1, 0, results);

      for (var i = currentProtoGroup; i < protoGroups.Count; ++i)
      {
        var protoGroup = protoGroups[i];
        var isJantou = i <= 1;
        if (isJantou)
        {
          if (arrangement.HasJantou || !protoGroup.CanInsert(counts, used, currentTileType))
          {
            continue;
          }

          protoGroup.Insert(counts, used, currentTileType);
          var added = arrangement.SetJantouValue(protoGroup.Value);
          Analyze(protoGroups, counts, used, added, currentTileType, i, results);
          protoGroup.Remove(counts, used, currentTileType);
        }
        else
        {
          if (arrangement.MentsuCount == 4 || !protoGroup.CanInsert(counts, used, currentTileType))
          {
            continue;
          }

          protoGroup.Insert(counts, used, currentTileType);
          var added = arrangement.AddMentsu(protoGroup.Value);
          Analyze(protoGroups, counts, used, added, currentTileType, i, results);
          protoGroup.Remove(counts, used, currentTileType);
        }
      }
    }
  }
}