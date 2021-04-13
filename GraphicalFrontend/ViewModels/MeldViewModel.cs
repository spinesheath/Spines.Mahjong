using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.ViewModels
{
  internal class MeldViewModel : ViewModelBase
  {
    public MeldViewModel(IEnumerable<Tile> tiles)
    {
      Tiles = new ObservableCollection<int>(tiles.Select(t => t.TileId));
    }

    public ObservableCollection<int> Tiles { get; }
  }
}