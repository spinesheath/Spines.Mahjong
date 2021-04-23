using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.Client
{
  internal class LocalClient : IClient
  {
    private readonly ISpectator _spectator;
    private readonly IPlayer _player;
    private GameState _state = new();
    private bool _owari;
    private bool _isFirstGoAround;
    private Queue<Tile> _wall = new();
    private int _remainingDraws;

    public LocalClient(IPlayer player, ISpectator spectator)
    {
      _spectator = spectator;
      _player = player;
    }

    public void Start()
    {
      Task.Run(RunGame);
    }

    private void RunGame()
    {
      ShuffleWall();

      _state = new GameState();



      foreach (var opponent in _state.Opponents)
      {
        opponent.DeclaredRiichi = false;
        opponent.Melds.Clear();
      }

      _state.Ponds = new List<Pond>();
      for (var i = 0; i < 4; i++)
      {
        _state.Ponds.Add(new Pond());
      }

      _state.DeclaredRiichi = false;

      _state.Round = 0;
      _state.Honba = 0;
      _state.RiichiSticks = 0;
      _state.Dice0 = 1;
      _state.Dice1 = 1;
      _state.DoraIndicators.Clear();
      _state.DoraIndicators.Add(DrawDoraIndicator());

      _state.Oya = 0;

      _state.Score = 250;
      for (var i = 0; i < 3; i++)
      {
        _state.Opponents[i].Score = 250;
      }

      _state.Hand = new UkeIreCalculator();

      var hai = Enumerable.Repeat(0, 13).Select(_ => DrawTile()).ToList();
      _state.Hand.Init(hai.Select(t => t.TileType));

      _state.ConcealedTiles.Clear();
      _state.ConcealedTiles.AddRange(hai);

      _state.RecentDraw = null;
      _state.Melds.Clear();

      _spectator.Updated(_state);

      _owari = false;
      _isFirstGoAround = true;

      // remove hands of opponents.
      for (var i = 0; i < 13 * 3; i++)
      {
        DrawTile();
      }

      while (_remainingDraws > 0 && !_owari)
      {
        var drawnTile = DrawTile();
        _state.RecentDraw = drawnTile;

        _state.Hand.Draw(drawnTile.TileType);
        _state.ConcealedTiles.Add(drawnTile);
        
        _spectator.Updated(_state);
        _spectator.Sent($"draw {drawnTile}");

        var suggestedActions = GetPossibleDrawActions();

        if (_state.DeclaredRiichi && suggestedActions == DrawActions.Discard)
        {
          Discard(drawnTile);
        }
        else
        {
          var response = _player.OnDraw(_state, drawnTile, suggestedActions);
          if (response.CanExecute(_state, suggestedActions))
          {
            response.Execute(this);
          }
        }
        
        if (!_owari)
        {
          OpponentDraw(1);
          OpponentDraw(2);
          OpponentDraw(3);
        }

        if (_isFirstGoAround)
        {

        }

        _isFirstGoAround = false;
      }
    }

    private Tile DrawDoraIndicator()
    {
      return _wall.Dequeue();
    }

    private void ShuffleWall()
    {
      var tiles = Enumerable.Range(0, 136).ToList();
      var random = new Random();
      var n = 136;
      while (n > 1)
      {
        n--;
        var k = random.Next(n + 1);
        var value = tiles[k];
        tiles[k] = tiles[n];
        tiles[n] = value;
      }

      _wall = new Queue<Tile>(tiles.Select(Tile.FromTileId));
      _remainingDraws = 122;
    }

    private void OpponentDraw(int index)
    {
      if (_remainingDraws <= 0)
      {
        return;
      }

      var tile = DrawTile();
      _state.Ponds[index].Add(new DiscardedTile(tile));
      _state.RecentDiscard = tile;

      var actions = DiscardActions.Pass;

      if (!_state.DeclaredRiichi && _state.ConcealedTiles.Count(t => t.TileType == tile.TileType) >= 2)
      {
        actions |= DiscardActions.Pon;
      }

      if (_state.DeclaredRiichi && _state.ConcealedTiles.Count(t => t.TileType == tile.TileType) == 3)
      {
        actions |= DiscardActions.Kan;
      }
      
      if (_state.Hand.ShantenWithTile(tile.TileType) == -1)
      {
        actions |= DiscardActions.Ron;
      }
        
      if (index == 3 && !_state.DeclaredRiichi && tile.TileType.Suit != Suit.Jihai)
      {
        //var hasHonors = _state.ConcealedTiles.Any(t => t.TileType.Suit == Suit.Jihai);
        var tileType = tile.TileType;
        var tileTypePresence = 0;
        foreach (var concealedTile in _state.ConcealedTiles.Where(t => t.TileType.Suit != Suit.Jihai))
        {
          tileTypePresence |= 1 << concealedTile.TileType.TileTypeId;
        }
          
        if (tileType.Index > 0 && 
            tileType.Index < 8 &&
            ((tileTypePresence >> (tileType.TileTypeId - 1)) & 0b101) == 0b101 && 
            _state.ConcealedTiles.Any(t => t.TileType != tileType)) // kuikae
        {
          actions |= DiscardActions.Chii;
        }

        if (tileType.Index < 7 &&
            ((tileTypePresence >> tileType.TileTypeId) & 0b110) == 0b110 &&
            _state.ConcealedTiles.Any(t => Kuikae.IsValidDiscardForNonKanchanChii(tileType, t.TileType)))
        {
          actions |= DiscardActions.Chii;
        }

        if (tileType.Index > 1 &&
            ((tileTypePresence >> tileType.TileTypeId - 2) & 0b011) == 0b011 &&
            _state.ConcealedTiles.Any(t => Kuikae.IsValidDiscardForNonKanchanChii(tileType, t.TileType)))
        {
          actions |= DiscardActions.Chii;
        }
      }

      if (actions != DiscardActions.Pass)
      {
        _spectator.Sent($"Offered call {tile} from player{index}");

        var response = _player.OnDiscard(_state, tile, index, actions);

        if (response.CanExecute(_state, actions))
        {
          response.Execute(this);
        }
      }
    }

    private Tile DrawTile()
    {
      Debug.Assert(_remainingDraws > 0, "Trying to draw from empty wall");
      _remainingDraws -= 1;
      return _wall.Dequeue();
    }

    private DrawActions GetPossibleDrawActions()
    {
      var suggestedActions = DrawActions.Discard;
      
      if (_state.Hand.Shanten == -1)
      {
        suggestedActions |= DrawActions.Tsumo;
      }
      
      if (_remainingDraws > 3 && _state.Hand.Shanten <= 0 && _state.Melds.Count(m => m.MeldType != MeldType.ClosedKan) == 0 && !_state.DeclaredRiichi)
      {
        suggestedActions |= DrawActions.Riichi;
      }

      if (_remainingDraws > 0)
      {
        var canAnkan = _state.ConcealedTiles.GroupBy(t => t.TileType).Any(g => g.Count() == 4);
        var canShouminkan = _state.Melds.Any(m => m.MeldType == MeldType.Koutsu && _state.ConcealedTiles.Any(t => t.TileType == m.LowestTile.TileType));
        if (canAnkan || canShouminkan)
        {
          suggestedActions |= DrawActions.Kan;
        }
      }

      if (_state.ConcealedTiles.GroupBy(t => t.TileType).Count(g => IsKyuuhai(g.Key)) > 8 && _isFirstGoAround)
      {
        suggestedActions |= DrawActions.KyuushuKyuuhai;
      }

      return suggestedActions;
    }

    private static bool IsKyuuhai(TileType tileType)
    {
      return tileType.Suit == Suit.Jihai || tileType.Index == 0 || tileType.Index == 8;
    }

    public void Discard(Tile tile)
    {
      _state.RecentDiscard = tile;
      _state.Ponds[0].Add(new DiscardedTile(tile));

      _state.Hand.Discard(tile.TileType);
      _state.ConcealedTiles.Remove(tile);
      _state.RecentDraw = null;

      _spectator.Received($"discard {tile}");
    }

    public void Ankan(TileType tileType)
    {
      _spectator.Received("Ankan");
      _isFirstGoAround = false;

      _state.Hand.Ankan(tileType);
      _state.ConcealedTiles.RemoveAll(t => t.TileType == tileType);

      _state.RecentDraw = null;
      
      _state.Melds.Add(Meld.Ankan(tileType));


      var replacementDraw = DrawTile();
      _state.RecentDraw = replacementDraw;

      _state.Hand.Draw(replacementDraw.TileType);
      _state.ConcealedTiles.Add(replacementDraw);

      _spectator.Updated(_state);
      _spectator.Sent($"replacement draw {replacementDraw}");
      
      
      var suggestedActions = GetPossibleDrawActions();
      if (_state.DeclaredRiichi && suggestedActions == DrawActions.Discard)
      {
        Discard(replacementDraw);
      }
      else
      {
        var response = _player.OnDraw(_state, replacementDraw, suggestedActions);
        if (response.CanExecute(_state, suggestedActions))
        {
          response.Execute(this);
        }
      }
    }

    public void Shouminkan(Tile tile)
    {
      _spectator.Received("Shouminkan");
      _isFirstGoAround = false;
      
      _state.Hand.Shouminkan(tile.TileType);
      _state.ConcealedTiles.Remove(tile);

      _state.RecentDraw = null;
      var oldMeld = _state.Melds.First(m => m.MeldType == MeldType.Koutsu && m.LowestTile.TileType == tile.TileType);
      _state.Melds.Remove(oldMeld);

      _state.Melds.Add(Meld.Shouminkan(oldMeld.CalledTile!, tile));
      

      var replacementDraw = DrawTile();
      _state.RecentDraw = replacementDraw;

      _state.Hand.Draw(replacementDraw.TileType);
      _state.ConcealedTiles.Add(replacementDraw);

      _spectator.Updated(_state);
      _spectator.Sent($"replacement draw {replacementDraw}");

      var suggestedActions = GetPossibleDrawActions();
      var response = _player.OnDraw(_state, replacementDraw, suggestedActions);
      if (response.CanExecute(_state, suggestedActions))
      {
        response.Execute(this);
      }
    }

    public void Tsumo()
    {
      _spectator.Received("Tsumo");

      _owari = true;
    }

    public void Riichi(Tile tile)
    {
      _state.DeclaredRiichi = true;
      _state.RiichiSticks += 1;
      _state.Score -= 10;

      _spectator.Received("Riichi");

      Discard(tile);
    }

    public void KyuushuKyuuhai()
    {
      _spectator.Received("Kyuushu Kyuuhai");

      _owari = true;
    }

    public void Pass()
    {
      _spectator.Received("Pass");
    }

    public void Daiminkan()
    {
      _spectator.Received("Daiminkan");
      _isFirstGoAround = false;

      Debug.Assert(_state.RecentDiscard != null, "Attempting daiminkan without a recent discard");
      _state.Hand.Daiminkan(_state.RecentDiscard.TileType);
      _state.ConcealedTiles.RemoveAll(t => t.TileType == _state.RecentDiscard.TileType);

      _state.RecentDraw = null;

      _state.Melds.Add(Meld.Daiminkan(_state.RecentDiscard));


      var replacementDraw = DrawTile();
      _state.RecentDraw = replacementDraw;

      _state.Hand.Draw(replacementDraw.TileType);
      _state.ConcealedTiles.Add(replacementDraw);

      _spectator.Updated(_state);
      _spectator.Sent($"replacement draw {replacementDraw}");

      var suggestedActions = GetPossibleDrawActions();
      var response = _player.OnDraw(_state, replacementDraw, suggestedActions);
      if (response.CanExecute(_state, suggestedActions))
      {
        response.Execute(this);
      }
    }

    public void Pon(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      _spectator.Received("Pon");
      _isFirstGoAround = false;

      _state.Hand.Pon(tile0.TileType);
      _state.ConcealedTiles.Remove(tile0);
      _state.ConcealedTiles.Remove(tile1);

      _state.RecentDraw = null;
      
      Debug.Assert(_state.RecentDiscard != null, "Attempting pon without a recent discard");
      _state.Melds.Add(Meld.Pon(new []{tile0, tile1, _state.RecentDiscard}, _state.RecentDiscard));
      
      Discard(discardAfterCall);
    }

    public void Chii(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      _spectator.Received("Chii");
      _isFirstGoAround = false;

      Debug.Assert(_state.RecentDiscard != null, "Attempting chii without a recent discard");
      var tiles = new[] { tile0, tile1, _state.RecentDiscard }.OrderBy(t => t.TileId).ToList();

      _state.Hand.Chii(tiles[0].TileType, _state.RecentDiscard.TileType);
      _state.ConcealedTiles.Remove(tile0);
      _state.ConcealedTiles.Remove(tile1);

      _state.RecentDraw = null;

      _state.Melds.Add(Meld.Chii(tiles, _state.RecentDiscard));


      Discard(discardAfterCall);
    }

    public void Ron()
    {
      _spectator.Received("Ron");
      _owari = true;
    }
  }
}
