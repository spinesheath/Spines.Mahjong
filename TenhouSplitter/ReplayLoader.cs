using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TenhouSplitter
{
  class ReplayLoader
  {
    private static readonly Regex NormalIdRegex = new(@"(\d{10}gm-[\da-f]{4}-[\da-f]{4}-[\da-f]{8})");
    private static readonly HttpClient Client = new();

    public static async Task<XElement> Load(string pathOrId)
    {
      if (File.Exists(pathOrId))
      {
        return XElement.Load(pathOrId);
      }

      var replayId = GetReplayId(pathOrId);
      if (replayId == null)
      {
        return null;
      }

      var response = await Client.GetAsync($"https://tenhou.net/0/log/?{replayId}");
      if (response.IsSuccessStatusCode)
      {
        var xmlStream = await response.Content.ReadAsStreamAsync();
        return XElement.Load(xmlStream);
      }
      
      return null;
    }

    private static string GetReplayId(string value)
    {
      var normalMatch = NormalIdRegex.Match(value);
      if (normalMatch.Success)
      {
        return normalMatch.Groups[1].Value;
      }

      return null;
    }
  }
}
