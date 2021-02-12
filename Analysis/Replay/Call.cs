// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class Call
  {
    /// <summary>
    /// The tiles in the meld.
    /// </summary>
    [DataMember(Name = "tiles")]
    public List<int> Tiles { get; } = new List<int>();
  }
}