// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class Player
  {
    public Player(string name, int rank, decimal rate, string gender)
    {
      Name = name;
      Gender = gender;
      Rank = rank;
      Rate = rate;
    }

    [DataMember(Name = "name")]
    public string Name { get; }

    [DataMember(Name = "rank")]
    public int Rank { get; }

    [DataMember(Name = "rate")]
    public decimal Rate { get; }

    [DataMember(Name = "gender")]
    public string Gender { get; }
  }
}