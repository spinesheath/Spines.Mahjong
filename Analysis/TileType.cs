using System.Diagnostics;
using System.Linq;

namespace Spines.Mahjong.Analysis
{
  public class TileType
  {
    static TileType()
    {
      ByTileType = Enumerable.Range(0, 34).Select(i => new TileType(i)).ToArray();
      Ton = TileType.FromSuitAndIndex(Suit.Jihai, 0);
      Nan = TileType.FromSuitAndIndex(Suit.Jihai, 1);
      Shaa = TileType.FromSuitAndIndex(Suit.Jihai, 2);
      Pei = TileType.FromSuitAndIndex(Suit.Jihai, 3);
    }

    private TileType(int tileTypeId)
    {
      TileTypeId = tileTypeId;
      Index = tileTypeId % 9;
      SuitId = tileTypeId / 9;
      Suit = (Suit) SuitId;
    }

    public int Index { get; }

    public bool IsKyuuhai => Suit == Suit.Jihai || Index == 0 || Index == 8;

    public Suit Suit { get; }

    public int SuitId { get; }

    public int TileTypeId { get; }

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

    public static TileType Ton { get; }

    public static TileType Nan { get; }

    public static TileType Shaa { get; }

    public static TileType Pei { get; }
  }
}