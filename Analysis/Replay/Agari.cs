using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class Agari
  {
    /// <summary>
    /// The player who won the hand.
    /// </summary>
    [DataMember(Name = "winner")]
    public int Winner { get; set; }

    /// <summary>
    /// The player dealt into the hand in case of ron, or the winner in case of tsumo.
    /// </summary>
    [DataMember(Name = "from")]
    public int From { get; set; }

    /// <summary>
    /// The fu of the hand.
    /// </summary>
    [DataMember(Name = "fu")]
    public int Fu { get; set; }

    /// <summary>
    /// The score change for each player after the win.
    /// </summary>
    [DataMember(Name = "scoreChanges")]
    public List<int> ScoreChanges { get; } = new List<int>();

    /// <summary>
    /// The yaku in the hand.
    /// </summary>
    [DataMember(Name = "yaku")]
    public List<Yaku> Yaku { get; } = new List<Yaku>();
  }
}