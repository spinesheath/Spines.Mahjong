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

      var a = new SuitScoringInformationCreator(workingDirectory);
      a.CreateLookup();

      var c = new HonorScoringInformationCreator(workingDirectory);
      c.CreateLookup();
    }
  }
}
