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
      var ai0 = new SimpleAi.SimpleAi("A", "0");
      var ai1 = new SimpleAi.SimpleAi("B", "0");
      var ai2 = new SimpleAi.SimpleAi("C", "0");
      var ai3 = new SimpleAi.SimpleAi("D", "0");
      //var ai0 = new KanAi();
      //var ai1 = new KanAi();
      //var ai2 = new KanAi();
      //var ai3 = new KanAi();

      for (var i = 0; i < 100; i++)
      {
        await Match.Start(ai0, ai1, ai2, ai3);
      }
    }
  }
}