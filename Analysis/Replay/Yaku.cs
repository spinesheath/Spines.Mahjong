// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Serialization;

namespace Spines.Mahjong.Analysis.Replay
{
  [DataContract]
  public class Yaku
  {
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "han")]
    public int Han { get; set; }
  }
}