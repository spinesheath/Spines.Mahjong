using System.Collections.Generic;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// An (in-)complete Group; either a mentsu or a jantou.
  /// </summary>
  internal class ProtoGroup
  {
    /// <summary>
    /// The Value of the ProtoGroup.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Can this ProtoGroup be used in an arrangement?
    /// </summary>
    public bool CanInsert(IReadOnlyList<int> concealedTiles, IReadOnlyList<int> usedTiles, int offset)
    {
      return _protoGroupInserter.CanInsert(concealedTiles, usedTiles, offset);
    }

    public void Insert(IList<int> concealedTiles, IList<int> usedTiles, int offset)
    {
      _protoGroupInserter.Insert(concealedTiles, usedTiles, offset);
    }

    public void Remove(IList<int> concealedTiles, IList<int> usedTiles, int offset)
    {
      _protoGroupInserter.Remove(concealedTiles, usedTiles, offset);
    }

    /// <summary>
    /// A full jantou.
    /// </summary>
    public static readonly ProtoGroup Jantou2 = new ProtoGroup(2, new MentsuProtoGroupInserter(2, 2));

    /// <summary>
    /// A jantou missing one tile.
    /// </summary>
    public static readonly ProtoGroup Jantou1 = new ProtoGroup(1, new MentsuProtoGroupInserter(1, 2));

    /// <summary>
    /// A full koutsu.
    /// </summary>
    public static readonly ProtoGroup Koutsu3 = new ProtoGroup(3, new MentsuProtoGroupInserter(3, 3));

    /// <summary>
    /// A koutsu missing one tile.
    /// </summary>
    public static readonly ProtoGroup Koutsu2 = new ProtoGroup(2, new MentsuProtoGroupInserter(2, 3));

    /// <summary>
    /// A koutsu missing two tiles.
    /// </summary>
    public static readonly ProtoGroup Koutsu1 = new ProtoGroup(1, new MentsuProtoGroupInserter(1, 3));

    /// <summary>
    /// A full shuntsu.
    /// </summary>
    public static readonly ProtoGroup Shuntsu111 = new ProtoGroup(3, new ShuntsuProtoGroupInserter(1, 1, 1));

    /// <summary>
    /// A shuntsu missing the middle tile.
    /// </summary>
    public static readonly ProtoGroup Shuntsu101 = new ProtoGroup(2, new ShuntsuProtoGroupInserter(1, 0, 1));

    /// <summary>
    /// A shuntsu missing the left tile.
    /// </summary>
    public static readonly ProtoGroup Shuntsu011 = new ProtoGroup(2, new ShuntsuProtoGroupInserter(0, 1, 1));

    /// <summary>
    /// A shuntsu missing the right tile.
    /// </summary>
    public static readonly ProtoGroup Shuntsu110 = new ProtoGroup(2, new ShuntsuProtoGroupInserter(1, 1, 0));

    /// <summary>
    /// A shuntsu missing the left and right tiles.
    /// </summary>
    public static readonly ProtoGroup Shuntsu010 = new ProtoGroup(1, new ShuntsuProtoGroupInserter(0, 1, 0));

    /// <summary>
    /// A shuntsu missing the middle and right tiles.
    /// </summary>
    public static readonly ProtoGroup Shuntsu100 = new ProtoGroup(1, new ShuntsuProtoGroupInserter(1, 0, 0));

    /// <summary>
    /// A shuntsu missing the left and middle tiles.
    /// </summary>
    public static readonly ProtoGroup Shuntsu001 = new ProtoGroup(1, new ShuntsuProtoGroupInserter(0, 0, 1));

    /// <summary>
    /// Creates a new instance of ProtoGroup.
    /// </summary>
    private ProtoGroup(int value, IProtoGroupInserter protoGroupInserter)
    {
      Value = value;
      _protoGroupInserter = protoGroupInserter;
    }

    /// <summary>
    /// Can this ProtoGroup be used in an arrangement?
    /// </summary>
    private readonly IProtoGroupInserter _protoGroupInserter;
  }
}