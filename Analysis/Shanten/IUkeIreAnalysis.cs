namespace Spines.Mahjong.Analysis.Shanten
{
  public interface IUkeIreAnalysis : IHandAnalysis
  {
    int GetHighestUkeIreDiscard();

    IUkeIreAnalysis WithChii(TileType lowestTileType, TileType calledTileType);

    IUkeIreAnalysis WithPon(TileType tileType);
  }
}