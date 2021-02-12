// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.Replay
{
  public class Room
  {
    public string Name { get; }

    public static IEnumerable<Room> Rooms => AllRooms.Values;

    public static Room Parse(GameTypeFlag flags)
    {
      var relevantFlags = flags & RelevantFlags;
      return AllRooms.ContainsKey(relevantFlags) ? AllRooms[relevantFlags] : null;
    }

    private Room(GameTypeFlag flags, string name)
    {
      Name = name;
      _flags = flags;
    }

    private const GameTypeFlag RelevantFlags = GameTypeFlag.Advanced | GameTypeFlag.Expert;

    private readonly GameTypeFlag _flags;

    public static Room Ippan { get; } = new Room(GameTypeFlag.None, nameof(Ippan));
    public static Room Joukyuu { get; } = new Room(GameTypeFlag.Advanced, nameof(Joukyuu));
    public static Room Tokujou { get; } = new Room(GameTypeFlag.Expert, nameof(Tokujou));
    public static Room Houou { get; } = new Room(GameTypeFlag.Advanced | GameTypeFlag.Expert, nameof(Houou));

    private static readonly Dictionary<GameTypeFlag, Room> AllRooms = new Dictionary<GameTypeFlag, Room>
    {
      {Ippan._flags, Ippan},
      {Joukyuu._flags, Joukyuu},
      {Tokujou._flags, Tokujou},
      {Houou._flags, Houou}
    };
  }
}