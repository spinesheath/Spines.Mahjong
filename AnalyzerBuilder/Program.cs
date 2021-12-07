//#define Scoring
//#define Shanten5
#define B9Ukeire

using System;
using System.IO;

#if B9Ukeire
using AnalyzerBuilder.Creators.B9Ukeire;
#endif
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
      var directory = Path.Combine(resourcesDirectory, "Scoring");
      ScoringDataCreator.Create(directory);
#endif
#if Shanten5
      var directory = Path.Combine(resourcesDirectory, "Shanten5");
      Shanten5Creator.Create(directory);
#endif
#if B9Ukeire
      var directory = Path.Combine(resourcesDirectory, "B9Ukeire");
      B9UkeireCreator.Create(directory);
#endif
    }
  }
}
