using System.Collections.Generic;

namespace CompressedReplayCreator
{
  internal class RyuukyokuType
  {
    private RyuukyokuType(string name, int id)
    {
      Name = name;
      Id = id;
    }

    public int Id { get; }
    public string Name { get; }

    public static RyuukyokuType FromName(string name)
    {
      return ByName[name];
    }

    public static RyuukyokuType Exhaustive = new RyuukyokuType("exhaustive", 0);

    private static readonly Dictionary<string, RyuukyokuType> ByName = new Dictionary<string, RyuukyokuType>
    {
      {"exhaustive", Exhaustive},
      {"yao9", new RyuukyokuType("yao9", 1)},
      {"reach4", new RyuukyokuType("reach4", 2)},
      {"ron3", new RyuukyokuType("ron3", 3)},
      {"kan4", new RyuukyokuType("kan4", 4)},
      {"kaze4", new RyuukyokuType("kaze4", 5)},
      {"nm", new RyuukyokuType("nm", 6)}
    };
  }
}