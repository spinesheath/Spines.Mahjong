using AnalyzerBuilder.Combinations;

namespace AnalyzerBuilder.Creators
{
  /// <summary>
  /// Creates suit data with raw arrangement data.
  /// </summary>
  internal class RawAnalyzedSuitDataCreator : RawAnalyzedDataCreator
  {
    /// <summary>
    /// Creates the file name for the given tile count.
    /// </summary>
    /// <param name="count">The tile count.</param>
    /// <returns>A file name.</returns>
    protected override string GetFileName(int count)
    {
      return $"suits_{count}.txt";
    }

    /// <summary>
    /// Returns a TileGroupAnalyzer for the the given data.
    /// </summary>
    /// <param name="concealed">The concealed tiles to analyze.</param>
    /// <param name="melded">The melded tiles to analyze.</param>
    /// <param name="meldCount">The number of melds.</param>
    /// <returns>An instance of TileGroupAnalyzer.</returns>
    protected override TileGroupAnalyzer GetTileGroupAnalyzer(Combination concealed, Combination melded, int meldCount)
    {
      return TileGroupAnalyzer.ForSuits(concealed, melded, meldCount);
    }

    /// <summary>
    /// Returns a ConcealedCombinationCreator.
    /// </summary>
    /// <returns>An instance of ConcealedCombinationCreator.</returns>
    protected override ConcealedCombinationCreator GetConcealedCombinationCreator()
    {
      return ConcealedCombinationCreator.ForSuits();
    }

    /// <summary>
    /// Returns a MeldedCombinationsCreator.
    /// </summary>
    /// <returns>An instance of MeldedCombinationsCreator.</returns>
    protected override MeldedCombinationsCreator GetMeldedCombinationsCreator()
    {
      return MeldedCombinationsCreator.ForSuits();
    }
  }
}