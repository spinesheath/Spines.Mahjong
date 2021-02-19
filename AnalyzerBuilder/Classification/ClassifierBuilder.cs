using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AnalyzerBuilder.Classification
{
  /// <summary>
  /// Creates the transition table for a Classifier with equal-length words.
  /// </summary>
  internal class ClassifierBuilder : IStateMachineBuilder
  {
    /// <summary>
    /// The size of the alphabet.
    /// </summary>
    public int AlphabetSize { get; private set; }

    /// <summary>
    /// The transitions for the specified language.
    /// </summary>
    public IReadOnlyList<int> Transitions => _stateManager.Transitions;

    /// <summary>
    /// The states at which the transitions can be entered.
    /// </summary>
    /// <returns>The ids of the states.</returns>
    public IReadOnlyList<int> EntryStates => new[] {0};

    /// <summary>
    /// Is the transition one that describes can not be reached with a legal word?
    /// </summary>
    /// <param name="transition">The Id of the transtion.</param>
    /// <returns>True, if the transition can not be reached, false otherwise.</returns>
    public bool IsNull(int transition)
    {
      return Transitions[transition] == -1 && !IsResult(transition);
    }

    /// <summary>
    /// Is the transition one that describes a result?
    /// </summary>
    /// <param name="transition">The Id of the transtion.</param>
    /// <returns>True, if the transition is a result, false otherwise.</returns>
    public bool IsResult(int transition)
    {
      return _stateManager.ResultIndexes.Contains(transition);
    }

    /// <summary>
    /// Creates a minimized dfa and the corresponding transition table.
    /// </summary>
    public void SetLanguage(IEnumerable<WordWithValue> language, int alphabetSize, int wordLength)
    {
      Debug.Assert(alphabetSize > 0);
      Debug.Assert(wordLength > 0);
      Debug.Assert(language != null);

      WordLength = wordLength;
      AlphabetSize = alphabetSize;
      _stateManager = new StateManager(AlphabetSize, WordLength);
      foreach (var word in language)
      {
        MergeWord(word);
      }

      _stateManager.CompactTransitions();
    }

    /// <summary>
    /// Creates a minimized dfa and the corresponding transition table.
    /// This overload manifests the entire sequence in memory for alphabet size and word length calculation.
    /// </summary>
    public void SetLanguage(IEnumerable<WordWithValue> language)
    {
      Debug.Assert(language != null);
      var words = language.ToList();
      var wordLength = words.First().Word.Count;
      var alphabetSize = words.SelectMany(w => w.Word).Max() + 1;
      SetLanguage(words, alphabetSize, wordLength);
    }

    private StateManager _stateManager;

    /// <summary>
    /// The length of the words.
    /// </summary>
    private int WordLength { get; set; }

    /// <summary>
    /// Checks if the word already exists in the dfa.
    /// </summary>
    private bool HasBeenAdded(IEnumerable<int> word)
    {
      var curState = _stateManager.StartingState;
      foreach (var c in word)
      {
        if (curState.HasTransition(c))
        {
          curState = curState.Advance(c);
        }
        else
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Adds a word to the dfa and minimizes the dfa.
    /// </summary>
    private void MergeWord(WordWithValue word)
    {
      if (HasBeenAdded(word.Word))
      {
        return;
      }
      var curUnique = _stateManager.StartingState;
      var i = 0;
      var monofluentStates = new Stack<State>();
      // We need the previous state for each state.
      monofluentStates.Push(curUnique);
      // Traverse common prefix before the first confluence state.
      while (i < WordLength - 1 &&
             curUnique.HasTransition(word.Word[i]) &&
             !curUnique.Advance(word.Word[i]).IsConfluenceState)
      {
        curUnique = curUnique.Advance(word.Word[i]);
        monofluentStates.Push(curUnique);
        i += 1;
      }
      // Here curUnique points to the last State before the first confluence State,
      // or the last state in the common prefix if there is no confluence state.
      // Here i is the index of the character of the transition away from curUnique.

      if (curUnique != _stateManager.StartingState)
      {
        // This state will have an outgoing transition changed.
        // It has to be reinserted.
        // Have to remove it before changing because the transitions are the dictionary key.
        _stateManager.RemoveUniqueState(curUnique, GetHeight(i - 1));
      }

      // Clone rest of common prefix.
      var lastAdded = curUnique;
      var clones = new Stack<State>();
      // We need the previous state for each clone.
      clones.Push(lastAdded);
      while (i < WordLength - 1 &&
             curUnique.HasTransition(word.Word[i]))
      {
        curUnique = curUnique.Advance(word.Word[i]);
        var clone = curUnique.Clone(AlphabetSize);
        lastAdded.RedirectOutTransition(clone, word.Word[i]);
        lastAdded = clone;
        clones.Push(clone);
        i += 1;
      }
      // Here curUnique points to the last state in the common prefix if continued on the unique path.
      // Here lastAdded points to the last state in the common prefix if continued on the cloned path.
      // Here i is the index of the character of the transition away from lastAdded.

      // Add the rest of the word to the dfa.
      AddSuffix(lastAdded, word, i);

      // Merge clones into the state machine.
      i -= 1;
      i = MergeStates(word.Word, i, clones);

      // Fix monofluent States:
      // remove them from the dfa and either insert them back in or merge them with an equivalent state.
      while (monofluentStates.Count > 1)
      {
        var curState = monofluentStates.Pop();
        var prevState = monofluentStates.Peek();
        var uniqueState = _stateManager.TryGetEquivalentUniqueState(curState, GetHeight(i));
        if (uniqueState == null)
        {
          _stateManager.InsertUniqueState(curState, GetHeight(i));
          break;
        }
        if (prevState != _stateManager.StartingState)
        {
          _stateManager.RemoveUniqueState(prevState, GetHeight(i - 1));
        }
        prevState.RedirectOutTransition(uniqueState, word.Word[i]);
        i -= 1;
      }
    }

    /// <summary>
    /// The height of a state that is reached by a letter with the given index in the word.
    /// </summary>
    private int GetHeight(int incomingCharacterIndex)
    {
      return WordLength - incomingCharacterIndex - 1;
    }

    /// <summary>
    /// Merges the states into the dfa.
    /// </summary>
    /// <param name="word">The current word.</param>
    /// <param name="i">The index of the letter that leads to the top state in the stack.</param>
    /// <param name="states">
    /// The states to be merged plus one predecessor at the bottom, ordered by height with top being the
    /// lowest height.
    /// </param>
    /// <returns>The index of the letter that leads to the predecessor at the bottom of the stack.</returns>
    private int MergeStates(IReadOnlyList<int> word, int i, Stack<State> states)
    {
      while (states.Count > 1)
      {
        var curState = states.Pop();
        var prevState = states.Peek();
        var uniqueState = _stateManager.TryGetEquivalentUniqueState(curState, GetHeight(i));
        if (uniqueState == null)
        {
          _stateManager.InsertUniqueState(curState, GetHeight(i));
        }
        else
        {
          prevState.RedirectOutTransition(uniqueState, word[i]);
        }
        i -= 1;
      }
      return i;
    }

    /// <summary>
    /// Adds the rest of the word to the dfa.
    /// </summary>
    /// <param name="parent">The state to attack the suffix to.</param>
    /// <param name="word">The current word.</param>
    /// <param name="wordPos">The index of the first letter of the suffix.</param>
    private void AddSuffix(State parent, WordWithValue word, int wordPos)
    {
      var states = new Stack<State>();
      states.Push(parent);
      // Create new States.
      var i = wordPos;
      while (i < WordLength - 1)
      {
        var prevState = states.Peek();
        var newState = new State(AlphabetSize);
        prevState.CreateOutTransition(newState, word.Word[i]);
        states.Push(newState);
        i += 1;
      }
      // Connect to final State.
      var final = _stateManager.GetOrCreateFinalState(word.Value);
      states.Peek().CreateOutTransition(final, word.Word[WordLength - 1]);
      // Merge new States with unique States.
      i -= 1;
      MergeStates(word.Word, i, states);
    }
  }
}