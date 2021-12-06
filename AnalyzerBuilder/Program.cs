//#define Scoring
#define Shanten5

using System;
using System.IO;
#if Scoring
using AnalyzerBuilder.Creators.Scoring;
#endif
#if Shanten5
using AnalyzerBuilder.Creators.Shanten5;
#endif

namespace AnalyzerBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      var resourcesDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "Analysis", "Resources"));

#if Scoring
      var scoringDirectory = Path.Combine(resourcesDirectory, "Scoring");
      ScoringDataCreator.Create(workingDirectory);
#endif
#if Shanten5
      var shanten5Directory = Path.Combine(resourcesDirectory, "Shanten5");
      Shanten5Creator.Create(shanten5Directory);
#endif
    }
  }
}
