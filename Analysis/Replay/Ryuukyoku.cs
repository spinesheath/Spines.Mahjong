// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class Ryuukyoku
  {
    /// <summary>
    /// For each player, whether they revealed their hand at the end.
    /// </summary>
    [DataMember(Name = "revealed")]
    public List<bool> Revealed { get; } = new List<bool>();

    /// <summary>
    /// The score change for each player in seat order after the draw.
    /// </summary>
    [DataMember(Name = "scoreChanges")]
    public List<int> ScoreChanges { get; } = new List<int>();
  }
}