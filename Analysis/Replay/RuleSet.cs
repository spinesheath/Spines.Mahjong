// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class RuleSet
  {
    /// <summary>
    /// Name of the rule set.
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; }

    /// <summary>
    /// Are there aka dora?
    /// </summary>
    [DataMember(Name = "aka")]
    public bool Aka { get; }

    /// <summary>
    /// Is open tanyao allowed?
    /// </summary>
    [DataMember(Name = "kuitan")]
    public bool Kuitan { get; }

    /// <summary>
    /// How many players play the match.
    /// </summary>
    [DataMember(Name = "playerCount")]
    public int PlayerCount { get; }

    /// <summary>
    /// How many seconds the players get per action.
    /// </summary>
    [DataMember(Name = "secondsPerAction")]
    public decimal SecondsPerAction { get; }

    /// <summary>
    /// How many extra seconds players get over the course of a game.
    /// </summary>
    [DataMember(Name = "extraSecondsPerGame")]
    public decimal ExtraSecondsPerGame { get; }

    /// <summary>
    /// How often the dealer position goes around the table in a regular game.
    /// </summary>
    [DataMember(Name = "rounds")]
    public int Rounds { get; }

    public static IEnumerable<RuleSet> RuleSets => AllRuleSets.Values;

    public static RuleSet Parse(GameTypeFlag flags)
    {
      var relevantFlags = flags & RelevantFlags;
      return AllRuleSets.ContainsKey(relevantFlags) ? AllRuleSets[relevantFlags] : null;
    }

    private RuleSet(GameTypeFlag flags, string name)
    {
      _flags = flags;
      Name = name;
      Aka = !flags.HasFlag(GameTypeFlag.AkaNashi);
      Kuitan = !flags.HasFlag(GameTypeFlag.KuitanNashi);
      Rounds = flags.HasFlag(GameTypeFlag.Tonnansen) ? 2 : 1;
      if (flags.HasFlag(GameTypeFlag.Fast))
      {
        SecondsPerAction = 3M;
        ExtraSecondsPerGame = 5M;
      }
      else
      {
        SecondsPerAction = 5M;
        ExtraSecondsPerGame = 10M;
      }
      PlayerCount = flags.HasFlag(GameTypeFlag.Sanma) ? 3 : 4;
    }

    private const GameTypeFlag RelevantFlags = GameTypeFlag.AkaNashi | GameTypeFlag.Fast | GameTypeFlag.KuitanNashi | GameTypeFlag.Sanma | GameTypeFlag.Tonnansen;

    private readonly GameTypeFlag _flags;

    public static RuleSet TenhouAriAri { get; } = new RuleSet(GameTypeFlag.Tonnansen, nameof(TenhouAriAri));
    public static RuleSet TenhouAriAriFastTonpuusen { get; } = new RuleSet(GameTypeFlag.Fast, nameof(TenhouAriAri));

    private static readonly Dictionary<GameTypeFlag, RuleSet> AllRuleSets = new Dictionary<GameTypeFlag, RuleSet>
    {
      {TenhouAriAri._flags, TenhouAriAri},
      {TenhouAriAriFastTonpuusen._flags, TenhouAriAriFastTonpuusen}
    };
  }
}