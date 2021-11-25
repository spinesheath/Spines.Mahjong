using System;
using System.IO;
using AnalyzerBuilder.Creators.Scoring;
using AnalyzerBuilder.Creators.Shanten5;

namespace AnalyzerBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      var resourcesDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "Analysis", "Resources"));
      
      //var scoringDirectory = Path.Combine(resourcesDirectory, "Scoring");
      //ScoringDataCreator.Create(workingDirectory);

      var shanten5Directory = Path.Combine(resourcesDirectory, "Shanten5");
      Shanten5Creator.Create(shanten5Directory);
    }
  }
}
