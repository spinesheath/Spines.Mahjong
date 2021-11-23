using Newtonsoft.Json;

namespace TenhouSplitter
{
  internal class Rule
  {
    [JsonProperty("aka")]
    public int Aka { get; set; } = 1;

    [JsonProperty("disp")]
    public string Disp { get; } = "牌譜";
  }
}