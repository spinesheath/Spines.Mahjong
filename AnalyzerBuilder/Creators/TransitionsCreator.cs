using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class TransitionsCreator
  {
    /// <summary>
    /// Creates a new Instance of TransitionsCreator.
    /// </summary>
    /// <param name="workingDirectory">The directory where intermediate results are stored.</param>
    public TransitionsCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public void CreateArrangementTransitions()
    {
      CreateTransitions("ArrangementTransitions.txt", GetArrangementBuilder);
    }

    public void CreateSuitTransitions()
    {
      CreateTransitions("SuitTransitions.txt", GetSuitBuilder);
    }

    public void CreateHonorTransitions()
    {
      CreateTransitions("HonorTransitions.txt", GetHonorBuilder);
    }

    public void CreateProgressiveHonorTransitions()
    {
      CreateTransitions("ProgressiveHonorStateMachine.txt", GetProgressiveHonorBuilder);
    }

    public void CreateProgressiveKokushiTransitions()
    {
      CreateTransitions("ProgressiveKokushiStateMachine.txt", GetProgressiveKokushiBuilder);
    }

    public void CreateSuitFirstPhase()
    {
      CreateTransitions("SuitFirstPhase.txt", GetSuitFirstPhaseBuilder);
    }

    public void CreateSuitSecondPhase()
    {
      CreateTransitionsWithMinOffsets("SuitSecondPhase0.txt", () => GetSuitSecondPhaseBuilder(0), 9);
      CreateTransitionsWithMinOffsets("SuitSecondPhase1.txt", () => GetSuitSecondPhaseBuilder(1), 9);
      CreateTransitionsWithMinOffsets("SuitSecondPhase2.txt", () => GetSuitSecondPhaseBuilder(2), 9);
      CreateTransitionsWithMinOffsets("SuitSecondPhase3.txt", () => GetSuitSecondPhaseBuilder(3), 9);
      CreateTransitionsWithMinOffsets("SuitSecondPhase4.txt", () => GetSuitSecondPhaseBuilder(4), 9);
    }

    private readonly string _workingDirectory;

    private IStateMachineBuilder GetArrangementBuilder()
    {
      var language = new ArrangementWordCreator(_workingDirectory).CreateOrdered();
      return GetClassifierBuilder(language);
    }

    private IStateMachineBuilder GetSuitBuilder()
    {
      var language = new CompactAnalyzedDataCreator(_workingDirectory).CreateSuitWords();
      return GetClassifierBuilder(language);
    }

    private IStateMachineBuilder GetHonorBuilder()
    {
      var language = new CompactAnalyzedDataCreator(_workingDirectory).CreateHonorWords();
      return GetClassifierBuilder(language);
    }

    private static IStateMachineBuilder GetProgressiveKokushiBuilder()
    {
      return new ProgressiveKokushiBuilder();
    }

    private IStateMachineBuilder GetProgressiveHonorBuilder()
    {
      var words = new CompactAnalyzedDataCreator(_workingDirectory).CreateHonorWords();
      var builder = new ProgressiveHonorStateMachineBuilder();
      builder.SetLanguage(words);
      return builder;
    }

    private IStateMachineBuilder GetSuitFirstPhaseBuilder()
    {
      var builder = new SuitFirstPhaseBuilder(_workingDirectory);
      builder.SetLanguage();
      return builder;
    }

    private IStateMachineBuilder GetSuitSecondPhaseBuilder(int meldCount)
    {
      var builder = new SuitSecondPhaseBuilder(_workingDirectory, meldCount);
      builder.SetLanguage();
      return builder;
    }

    /// <summary>
    /// Creates the transitions file if it doesn't exist.
    /// </summary>
    private void CreateTransitions(string fileName, Func<IStateMachineBuilder> createBuilder)
    {
      var targetPath = Path.Combine(_workingDirectory, fileName);
      if (File.Exists(targetPath))
      {
        return;
      }

      var builder = createBuilder();
      var compacter = new TransitionCompacter(builder);

      var lines = compacter.Transitions.Select(t => t.ToString(CultureInfo.InvariantCulture));
      File.WriteAllLines(targetPath, lines);

      var offsetPath = Path.Combine(_workingDirectory, $"o_{fileName}");
      var offsets = compacter.Offsets.Select(t => t.ToString(CultureInfo.InvariantCulture));
      File.WriteAllLines(offsetPath, offsets);
    }

    /// <summary>
    /// Creates the transitions file if it doesn't exist.
    /// </summary>
    private void CreateTransitionsWithMinOffsets(string fileName, Func<IStateMachineBuilder> createBuilder,
      int wordLength)
    {
      var targetPath = Path.Combine(_workingDirectory, fileName);
      if (File.Exists(targetPath))
      {
        return;
      }

      var builder = createBuilder();
      var compacter = new TransitionCompacter(builder);

      var minOffsets = GetMinOffsets(builder, compacter, wordLength);
      var minOffsetTransitions = SubtractOffsets(builder, compacter, minOffsets, wordLength);

      var lines = minOffsetTransitions.Select(t => t.ToString(CultureInfo.InvariantCulture));
      File.WriteAllLines(targetPath, lines);

      var offsetPath = Path.Combine(_workingDirectory, $"o_{fileName}");
      var offsets = compacter.Offsets.Select(t => t.ToString(CultureInfo.InvariantCulture));
      File.WriteAllLines(offsetPath, offsets);

      var minOffsetPath = Path.Combine(_workingDirectory, $"m_{fileName}");
      var minOffsetLines = minOffsets.Select(t => t.ToString(CultureInfo.InvariantCulture));
      File.WriteAllLines(minOffsetPath, minOffsetLines);
    }

    private static IReadOnlyList<int> GetMinOffsets(IStateMachineBuilder builder, TransitionCompacter compacter,
      int wordLength)
    {
      var alphabet = Enumerable.Range(0, builder.AlphabetSize).ToList();
      var minOffsets = new int[wordLength];
      var maxOffsets = new int[wordLength];
      var transitionsInCurrentLevel = new HashSet<int>(builder.EntryStates.Select(e => e * builder.AlphabetSize));
      var compactTransitionsInCurrentLevel =
        new HashSet<int>(builder.EntryStates.Select(e => e * builder.AlphabetSize - compacter.Offsets[e]));
      for (var i = 0; i < wordLength; ++i)
      {
        var transitionsInPreviousLevel = transitionsInCurrentLevel;
        var compactTransitionsInPreviousLevel = compactTransitionsInCurrentLevel;
        transitionsInCurrentLevel = new HashSet<int>();
        compactTransitionsInCurrentLevel = new HashSet<int>();
        var orderedPrevious = transitionsInPreviousLevel.OrderBy(x => x).ToList();
        var orderedCompactPrevious = compactTransitionsInPreviousLevel.OrderBy(x => x).ToList();
        for (var j = 0; j < orderedPrevious.Count; ++j)
        {
          var previous = orderedPrevious[j];
          var compactPrevious = orderedCompactPrevious[j];
          foreach (var c in alphabet)
          {
            if (builder.IsNull(previous + c))
            {
              continue;
            }
            var n = builder.Transitions[previous + c];
            var m = compacter.Transitions[compactPrevious + c];
            transitionsInCurrentLevel.Add(n);
            compactTransitionsInCurrentLevel.Add(m);
          }
        }
        minOffsets[i] = compactTransitionsInCurrentLevel.Min();
        maxOffsets[i] = compactTransitionsInCurrentLevel.Max();
      }
      if (maxOffsets.Max() < ushort.MaxValue)
      {
        return Enumerable.Repeat(0, wordLength).ToList();
      }
      var largestDistance = Enumerable.Range(0, wordLength).Max(i => maxOffsets[i] - minOffsets[i]);
      for (var i = 0; i < wordLength; ++i)
      {
        if (maxOffsets[i] < largestDistance)
        {
          minOffsets[i] = 0;
        }
      }
      return minOffsets;
    }

    private static IEnumerable<int> SubtractOffsets(IStateMachineBuilder builder, TransitionCompacter compacter,
      IReadOnlyList<int> minOffsets, int wordLength)
    {
      var alphabet = Enumerable.Range(0, builder.AlphabetSize).ToList();
      var transitionsInCurrentLevel = new HashSet<int>(builder.EntryStates.Select(e => e * builder.AlphabetSize));
      var compactTransitionsInCurrentLevel =
        new HashSet<int>(builder.EntryStates.Select(e => e * builder.AlphabetSize - compacter.Offsets[e]));
      var transitionsWithMinOffsets = new int[compacter.Transitions.Count];
      Array.Fill(transitionsWithMinOffsets, -1);
      for (var i = 0; i < wordLength; ++i)
      {
        var statesInPreviousLevel = transitionsInCurrentLevel;
        var compactStatesInPreviousLevel = compactTransitionsInCurrentLevel;
        transitionsInCurrentLevel = new HashSet<int>();
        compactTransitionsInCurrentLevel = new HashSet<int>();
        var orderedPrevious = statesInPreviousLevel.OrderBy(x => x).ToList();
        var orderedCompactPrevious = compactStatesInPreviousLevel.OrderBy(x => x).ToList();
        for (var j = 0; j < orderedPrevious.Count; ++j)
        {
          var previous = orderedPrevious[j];
          var compactPrevious = orderedCompactPrevious[j];
          foreach (var c in alphabet)
          {
            if (builder.IsNull(previous + c))
            {
              continue;
            }
            var n = builder.Transitions[previous + c];
            var m = compacter.Transitions[compactPrevious + c];
            transitionsInCurrentLevel.Add(n);
            compactTransitionsInCurrentLevel.Add(m);

            transitionsWithMinOffsets[compactPrevious + c] = m - minOffsets[i];
          }
        }
      }
      return transitionsWithMinOffsets;
    }

    private static IStateMachineBuilder GetClassifierBuilder(IEnumerable<WordWithValue> language)
    {
      var builder = new ClassifierBuilder();
      builder.SetLanguage(language);
      return builder;
    }
  }
}