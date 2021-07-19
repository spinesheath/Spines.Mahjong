using System.Linq;
using System.Text;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class Block
  {
    static Block()
    {
      ById = Enumerable.Range(0, 7 + 9 + 9 + 9 + 9).Select(i => new Block(i)).ToArray();
    }

    private Block(int id)
    {
      Id = id;
      IsShuntsu = id < 7;
      IsKoutsu = id >= 7 && id < 16;
      IsKantsu = id >= 16 && id < 34;
      IsAnkan = id >= 16 && id < 25;
      IsMinkan = id >= 25 && id < 34;
      IsPair = id >= 34;
      Index = id < 7 ? id : (id - 7) % 9;
      IsJunchanBlock = id == 6 || Index == 0 || Index == 8;
    }

    public int Id { get; }

    public int Index { get; }

    public bool IsAnkan { get; }

    public bool IsJunchanBlock { get; }

    public bool IsKantsu { get; }

    public bool IsKoutsu { get; }

    public bool IsMinkan { get; }

    public bool IsPair { get; }

    public bool IsShuntsu { get; }

    public static Block Ankan(int index)
    {
      return ById[index + 7 + 9];
    }

    public static Block Koutsu(int index)
    {
      return ById[index + 7];
    }

    public static Block Minkan(int index)
    {
      return ById[index + 7 + 9 + 9];
    }

    public static Block Pair(int index)
    {
      return ById[index + 7 + 9 + 9 + 9];
    }

    public static Block Shuntsu(int index)
    {
      return ById[index];
    }

    private static readonly Block[] ById;

    public override string ToString()
    {
      var sb = new StringBuilder();
      if (IsShuntsu)
      {
        sb.Append(Index + 1);
        sb.Append(Index + 2);
        sb.Append(Index + 3);
      }
      else if (IsPair)
      {
        sb.Append(Index + 1);
        sb.Append(Index + 1);
      }
      else if (IsKoutsu)
      {
        sb.Append(Index + 1);
        sb.Append(Index + 1);
        sb.Append(Index + 1);
      }
      else if (IsKantsu)
      {
        sb.Append(Index + 1);
        sb.Append(Index + 1);
        sb.Append(Index + 1);
        sb.Append(Index + 1);
      }
      else
      {
        return "?";
      }

      return sb.ToString();
    }
  }
}