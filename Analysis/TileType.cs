using System.Diagnostics;
using System.Linq;

namespace Spines.Mahjong.Analysis
{
  public class TileType
  {
    static TileType()
    {
      ByTileType = Enumerable.Range(0, 34).Select(i => new TileType(i)).ToArray();
      Ton = FromSuitAndIndex(Suit.Jihai, 0);
      Nan = FromSuitAndIndex(Suit.Jihai, 1);
      Shaa = FromSuitAndIndex(Suit.Jihai, 2);
      Pei = FromSuitAndIndex(Suit.Jihai, 3);
    }

    private TileType(int tileTypeId)
    {
      TileTypeId = tileTypeId;
      Index = tileTypeId % 9;
      SuitId = tileTypeId / 9;
      Suit = (Suit) SuitId;
      IsKyuuhai = Suit == Suit.Jihai || Index == 0 || Index == 8;
      KyuuhaiValue = IsKyuuhai ? 1 : 0;
      Base5Value = Base5.Table[Index];
      if (SuitId < 2)
      {
        Base5ValueA = (ulong)Base5Value << (32 * SuitId);
      }
      else
      {
        Base5ValueB = (ulong)Base5Value << (32 * (SuitId - 2));
      }
    }

    public int Index { get; }

    public bool IsKyuuhai { get; }

    /// <summary>
    /// 1 if terminal or honor, 0 else.
    /// </summary>
    public int KyuuhaiValue { get; }

    public static TileType Nan { get; }

    public static TileType Pei { get; }

    public static TileType Shaa { get; }

    public Suit Suit { get; }

    public int SuitId { get; }

    public int TileTypeId { get; }

    public static TileType Ton { get; }

    public static TileType FromString(string tileType)
    {
      Debug.Assert(tileType.Length == 2);
      Debug.Assert(char.IsDigit(tileType[0]));
      return FromTileTypeId("mpsz".IndexOf(tileType[1]) * 9 + tileType[0] - '1');
    }

    /// <summary>
    /// 0-3 and 0-9
    /// </summary>
    public static TileType FromSuitAndIndex(Suit suit, int index)
    {
      return FromTileTypeId((int) suit * 9 + index);
    }

    /// <summary>
    /// 0-135
    /// </summary>
    public static TileType FromTileId(int tileId)
    {
      return FromTileTypeId(tileId / 4);
    }

    /// <summary>
    /// 0-33
    /// </summary>
    public static TileType FromTileTypeId(int tileTypeId)
    {
      Debug.Assert(tileTypeId >= 0 && tileTypeId < 34);
      return ByTileType[tileTypeId];
    }

    public override string ToString()
    {
      return $"{1 + Index}{"mpsz"[SuitId]}";
    }

    private static readonly TileType[] ByTileType;

    /// <summary>
    /// Pow(5, index)
    /// </summary>
    public readonly int Base5Value;

    /// <summary>
    /// Base5Value in lower/upper 32 bit if manzu/pinzu
    /// </summary>
    public readonly ulong Base5ValueA;

    /// <summary>
    /// Base5Value in lower/upper 32 bit if souzu/jihai
    /// </summary>
    public readonly ulong Base5ValueB;
  }
}