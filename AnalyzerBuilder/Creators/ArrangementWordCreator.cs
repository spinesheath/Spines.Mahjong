using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;
using AnalyzerBuilder.Combinations;

namespace AnalyzerBuilder.Creators
{
  internal class ArrangementWordCreator
  {
    public ArrangementWordCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public IEnumerable<WordWithValue> CreateUnordered()
    {
      return Create("ArrangementWords.txt",
        () => new CompactAnalyzedDataCreator(_workingDirectory).GetUniqueArrangements());
    }

    public IEnumerable<WordWithValue> CreateOrdered()
    {
      return Create("OrderedArrangementWords.txt", () => new OrderedArrangementsCreator(_workingDirectory).Create());
    }

    private readonly string _workingDirectory;

    private IEnumerable<WordWithValue> Create(string fileName, Func<IEnumerable<IList<Arrangement>>> wordCreator)
    {
      var wordsFile = Path.Combine(_workingDirectory, fileName);
      if (File.Exists(wordsFile))
      {
        var lines = File.ReadAllLines(wordsFile);
        return lines.Select(WordWithValue.FromString);
      }
      var arrangements = wordCreator();
      var words = CreateWords(arrangements).ToList();
      File.WriteAllLines(wordsFile, words.Select(w => w.ToString()));
      return words;
    }

    /// <summary>
    /// Creates all permutations of length 4 of the numbers 0 through alphabetSize - 1.
    /// </summary>
    /// <param name="alphabetSize">The number of characters in the language.</param>
    /// <returns>The language.</returns>
    private static IEnumerable<IList<int>> CreateBaseLanguage(int alphabetSize)
    {
      for (var a = 0; a < alphabetSize; ++a)
      {
        for (var b = 0; b < alphabetSize; ++b)
        {
          for (var c = 0; c < alphabetSize; ++c)
          {
            for (var d = 0; d < alphabetSize; ++d)
            {
              yield return new[] {a, b, c, d};
            }
          }
        }
      }
    }

    private static IEnumerable<WordWithValue> CreateWords(IEnumerable<IList<Arrangement>> arrangements)
    {
      var arrangementsList = arrangements.ToList();
      var alphabetSize = arrangementsList.Count;
      var language = CreateBaseLanguage(alphabetSize);
      var tilesInArrangements = arrangementsList.Select(a => a.Max(b => b.TotalValue)).ToList();

      foreach (var word in language)
      {
        var sumOfTiles = word.Sum(c => tilesInArrangements[c]);
        if (sumOfTiles <= 14 && sumOfTiles >= 5)
        {
          var analyzer = new ArrangementAnalyzer();
          foreach (var character in word)
          {
            analyzer.AddSetOfArrangements(arrangementsList[character]);
          }
          var shanten = analyzer.CalculateShanten();
          if (shanten < 9)
          {
            yield return new WordWithValue(word, shanten + 1);
          }
        }
      }
    }
  }
}