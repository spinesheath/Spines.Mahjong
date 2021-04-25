using System.Collections.ObjectModel;
using Game.Shared;

namespace GraphicalFrontend.ViewModels
{
  internal class BoardViewModel : ViewModelBase, ISpectator
  {
    private string _roundWind = "";
    private string _honbaCount = "";
    private string _riichiStickCount = "";

    public BoardViewModel(PlayerViewModel watashi, PlayerViewModel shimocha, PlayerViewModel toimen, PlayerViewModel kamicha)
    {
      Watashi = watashi;
      Shimocha = shimocha;
      Kamicha = kamicha;
      Toimen = toimen;
    }

    public PlayerViewModel Watashi { get; }

    public PlayerViewModel Shimocha { get; }

    public PlayerViewModel Toimen { get; }

    public PlayerViewModel Kamicha { get; }

    public string RoundWind
    {
      get => _roundWind;
      private set
      {
        _roundWind = value;
        OnPropertyChanged();
      }
    }

    public string HonbaCount
    {
      get => _honbaCount;
      private set
      {
        _honbaCount = value;
        OnPropertyChanged();
      }
    }

    public string RiichiStickCount
    {
      get => _riichiStickCount;
      private set
      {
        _riichiStickCount = value;
        OnPropertyChanged();
      }
    }

    public ObservableCollection<int> DoraIndicators { get; } = new ();

    public void Sent(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Received(string message)
    {
    }

    public void Updated(VisibleBoard board)
    {
      DoraIndicators.Clear();
      foreach (var tile in board.DoraIndicators)
      {
        DoraIndicators.Add(tile.TileId);
      }

      RoundWind = "東南西北".Substring(board.RoundWind.TileTypeId - 27, 1);
      HonbaCount = board.Honba.ToString();
      RiichiStickCount = board.RiichiSticks.ToString();
    }
  }
}