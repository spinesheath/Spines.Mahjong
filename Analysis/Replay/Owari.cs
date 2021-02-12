using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class Owari
  {
    /// <summary>
    /// Final points including uma for each player.
    /// </summary>
    [DataMember(Name = "points")]
    public List<decimal> Points { get; } = new List<decimal>();

    /// <summary>
    /// Final score for each player.
    /// </summary>
    [DataMember(Name = "scores")]
    public List<int> Scores { get; } = new List<int>();
  }
}