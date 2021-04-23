namespace AnalyzerBuilder.Classification
{
  internal class FinalState : State
  {
    public FinalState(int alphabetSize, int value)
      : base(alphabetSize)
    {
      Value = value;
    }

    public int Value { get; }
  }
}