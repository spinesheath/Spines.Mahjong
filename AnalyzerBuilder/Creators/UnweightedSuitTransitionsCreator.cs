using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class UnweightedSuitTransitionsCreator
  {
    public UnweightedSuitTransitionsCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public IEnumerable<int> Create()
    {
      var path = Path.Combine(_workingDirectory, "UnweightedSuitTransitions.txt");
      if (File.Exists(path))
      {
        return File.ReadAllLines(path).Select(line => Convert.ToInt32(line, CultureInfo.InvariantCulture));
      }
      var language = new CompactAnalyzedDataCreator(_workingDirectory).CreateSuitWords();
      var fullLanguage = CreateFullLanguage(language);
      var builder = new ClassifierBuilder();
      builder.SetLanguage(fullLanguage, 5, 19);
      var transitions = builder.Transitions;
      var lines = transitions.Select(t => t.ToString(CultureInfo.InvariantCulture));
      File.WriteAllLines(path, lines);
      return transitions;
    }

    private readonly string _workingDirectory;

    private static IEnumerable<WordWithValue> CreateFullLanguage(IEnumerable<WordWithValue> language)
    {
      foreach (var word in language)
      {
        yield return word;
        var mc = word[0];
        var m = word.Skip(1).Take(9).Reverse();
        var c = word.Skip(10).Reverse();
        var w = mc.Yield().Concat(m).Concat(c);
        var mirrored = new WordWithValue(w, word.Value);
        if (!mirrored.SequenceEqual(word))
        {
          yield return mirrored;
        }
      }
    }
  }
}