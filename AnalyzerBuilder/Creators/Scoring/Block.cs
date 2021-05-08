using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class Block
  {
    static Block()
    {
      ById = Enumerable.Range(0, 7 + 9 + 9 + 9).Select(i => new Block(i)).ToArray();
    }

    private Block(int id)
    {
      Id = id;
      IsShuntsu = id < 7;
      IsKoutsu = id >= 7 && id < 16;
      IsKantsu = id >= 16 && id < 25;
      IsPair = id >= 25;
      Index = id < 7 ? id : (id - 7) % 9;
      IsJunchanBlock = id == 6 || Index == 0 || Index == 8;
    }

    public int Id { get; }

    public int Index { get; }

    public bool IsJunchanBlock { get; }

    public bool IsKantsu { get; }

    public bool IsKoutsu { get; }

    public bool IsPair { get; }

    public bool IsShuntsu { get; }

    public static Block Kantsu(int index)
    {
      return ById[index + 7 + 9];
    }

    public static Block Koutsu(int index)
    {
      return ById[index + 7];
    }

    public static Block Pair(int index)
    {
      return ById[index + 7 + 9 + 9];
    }

    public static Block Shuntsu(int index)
    {
      return ById[index];
    }

    private static readonly Block[] ById;
  }
}