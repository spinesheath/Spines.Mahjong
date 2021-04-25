using System.Threading.Tasks;
using Game.Engine;

namespace ConsoleFrontend
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      await RunMatches();
    }

    private static async Task RunMatches()
    {
      var ai0 = new SimpleAi.SimpleAi("A", "0", false);
      var ai1 = new SimpleAi.SimpleAi("B", "0", false);
      var ai2 = new SimpleAi.SimpleAi("C", "0", false);
      var ai3 = new SimpleAi.SimpleAi("D", "0", false);

      for (var i = 0; i < 100; i++)
      {
        await Match.Start(ai0, ai1, ai2, ai3);
      }
    }
  }
}