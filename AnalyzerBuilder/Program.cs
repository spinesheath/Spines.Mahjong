using AnalyzerBuilder.Creators.Scoring;

namespace AnalyzerBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      //var workingDirectory = @"C:\temp\mahjong\scoring";
      var workingDirectory = @"C:\Users\Johannes\source\repos\Spines.Mahjong\Analysis\Resources\Scoring\";


      //var a = new SuitScoringInformationCreator(workingDirectory);
      //a.CreateLookup();

      //var b = new SuitMeldScoringInformationCreator(workingDirectory);
      //b.CreateLookup();

      var c = new HonorScoringInformationCreator(workingDirectory);
      c.CreateLookup();

      var d = new HonorMeldScoringInformationCreator(workingDirectory);
      d.CreateLookup();
    }
  }
}
