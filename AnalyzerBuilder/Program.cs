using System;
using System.IO;
using AnalyzerBuilder.Creators.Scoring;

namespace AnalyzerBuilder
{
  class Program
  {
    static void Main(string[] args)
    {
      var workingDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "Analysis", "Resources", "Scoring"));

      ScoringDataCreator.Create(workingDirectory);
    }
  }
}
