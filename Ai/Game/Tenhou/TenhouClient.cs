using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace Game.Tenhou
{
  internal class TenhouClient : IClient, IDisposable
  {
    public TenhouClient(IPlayer player, ISpectator spectator)
    {
      _player = player;
      _spectator = spectator;
      _lobby = _player.Lobby;
      _playerId = _player.Id;

      var folderName = MakeFolderName(_playerId);

      _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TenhouAi", folderName);
      if (!Directory.Exists(_directory))
      {
        Directory.CreateDirectory(_directory);
      }

      _sessionLogFileName = Path.Combine(_directory, Path.GetRandomFileName() + ".txt");

      _wall = new TenhouWall();
      _board = new Board(_wall);
      _visibleBoard = new VisibleBoard(_board);
    }

    public void LogOn()
    {
      Connect();
      SendHelo();
    }

    public void Ippan()
    {
      Send("<JOIN t=\"0,9\" />");
    }

    public void Testplay()
    {
      Send("<JOIN t=\"0,64\" />");
    }
    
    public void Discard(Tile tile)
    {
      Send($"<D p=\"{tile.TileId}\" />");
    }

    public void Ankan(TileType tileType)
    {
      Send($"<N type=\"4\" hai=\"{tileType.TileTypeId * 4}\" />");
    }

    public void Shouminkan(Tile tile)
    {
      Send($"<N type=\"5\" hai=\"{tile.TileId}\" />");
    }

    public void Tsumo()
    {
      Send("<N type=\"7\" />");
    }

    public void Riichi(Tile tile)
    {
      Send("<REACH />");
      Discard(tile);
    }

    public void KyuushuKyuuhai()
    {
      Send("<N type=\"9\" />");
    }

    public void Pass()
    {
      Send("<N />");
    }

    public void Daiminkan()
    {
      Send("<N type=\"2\" />");
    }

    public void Pon(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      Send($"<N type=\"1\" hai0=\"{tile0.TileId}\" hai1=\"{tile1.TileId}\" />");
      _pendingDiscardAfterCall = discardAfterCall;
    }

    public void Chii(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      Send($"<N type=\"3\" hai0=\"{tile0.TileId}\" hai1=\"{tile1.TileId}\" />");
      _pendingDiscardAfterCall = discardAfterCall;
    }

    public void Ron()
    {
      Send("<N type=\"6\" />");
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private const int Port = 10080;
    private static readonly IPAddress Address = IPAddress.Parse("133.242.10.78");
    private static readonly TimeSpan DecisionTimeout = TimeSpan.FromMilliseconds(4000);
    private readonly string _directory;
    private readonly string _lobby;
    private readonly IPlayer _player;
    private readonly string _playerId;
    private readonly string _sessionLogFileName;
    private readonly ISpectator _spectator;

    // TODO update remaining draws on draw or reinit
    private readonly TenhouWall _wall;
    private readonly Board _board;
    private readonly VisibleBoard _visibleBoard;

    private readonly TcpClient _tcpClient = new();
    private Tile? _pendingDiscardAfterCall;
    private CancellationTokenSource? _receiverCancellationTokenSource;
    private Task? _receiverTask;
    
    private void Connect()
    {
      _tcpClient.Connect(Address, Port);
      var stream = _tcpClient.GetStream();
      stream.ReadTimeout = 1000;
      ReceiveMessagesAsync(stream);
      SendKeepAlivePing();
    }

    private void SendKeepAlivePing()
    {
      Send("<Z />");
    }

    private void SendHelo()
    {
      const string sx = "F";
      Send($"<HELO name=\"{_playerId.Replace("-", "%2D")}\" tid=\"{_lobby}\" sx=\"{sx}\" />");
    }

    private void Send(string message)
    {
      var stream = _tcpClient.GetStream();
      var data = Encoding.ASCII.GetBytes(message).Concat(new byte[] {0}).ToArray();
      stream.Write(data, 0, data.Length);
      LogSent(message);
    }

    private void LogSent(string message)
    {
      _spectator.Sent(message);
      File.AppendAllLines(_sessionLogFileName, new[] {message});
    }

    private void LogError(string message)
    {
      _spectator.Error(message);
      File.AppendAllLines(_sessionLogFileName, new[] {"ERROR: " + message});
    }

    private void ReceiveMessages(NetworkStream stream, CancellationToken cancellationToken)
    {
      var lastMessageTimestamp = DateTime.UtcNow;
      var sleepCounter = 0;
      while (!cancellationToken.IsCancellationRequested)
      {
        if (sleepCounter > 40)
        {
          SendKeepAlivePing();
          sleepCounter = 0;
        }

        if (!stream.DataAvailable)
        {
          if ((DateTime.UtcNow - lastMessageTimestamp).Seconds > 30)
          {
            LogError("No messages for 30 seconds.");
            break;
          }

          lastMessageTimestamp = DateTime.UtcNow;
          Thread.Sleep(100);
          sleepCounter += 1;
          continue;
        }

        ReadMessage(stream);
      }

      SendBye();
    }

    private void SendBye()
    {
      Send("<BYE />");
    }

    private void LogReceived(string message)
    {
      _spectator.Received(message);
      File.AppendAllLines(_sessionLogFileName, new[] {"    " + message});
    }

    private void OnLoggedOn(XElement message)
    {
      // TODO something about a "nintei" attribute on the HELO tag from server, causing a close?

      var auth = message.Attribute("auth");
      if (auth == null)
      {
        LogError("Missing auth attribute.");
        return;
      }

      var authenticationString = auth.Value;
      var transformed = Authenticator.Transform(authenticationString);
      Send($"<AUTH val=\"{transformed}\" />");
    }

    private void OnRejoin(XElement message)
    {
      var parts = message.Attribute("t")!.Value.Split(',');
      var lobby = parts[0];
      var matchType = parts[1];
      AcceptMatch(lobby, matchType);
    }

    private void AcceptMatch(string lobby, string matchTypeId)
    {
      Send($"<JOIN t=\"{lobby},{matchTypeId}\" />");
    }

    private void OnGo()
    {
      Send("<GOK />");
      SendNextReady();
    }

    private void SendNextReady()
    {
      Send("<NEXTREADY />");
    }

    private int GetInt(XElement message, string attributeName)
    {
      var attribute = message.Attribute(attributeName);
      if (attribute == null)
      {
        LogError($"missing attribute {attributeName} in {message}");
        return 0;
      }

      return Convert.ToInt32(attribute.Value, CultureInfo.InvariantCulture);
    }

    private List<int> GetInts(XElement message, string attributeName)
    {
      var attribute = message.Attribute(attributeName);
      if (attribute == null)
      {
        LogError($"missing attribute {attributeName} in {message}");
        return new List<int>();
      }

      return ToInts(attribute.Value);
    }

    private static List<int> ToInts(string value)
    {
      return value.Split(',').Select(x => Convert.ToInt32(x, CultureInfo.InvariantCulture)).ToList();
    }

    private void OnInit(XElement message)
    {
      foreach (var seat in _board.Seats)
      {
        seat.DeclaredRiichi = false;
        seat.Melds.Clear();
        seat.Discards.Clear();
      }

      var seed = GetInts(message, "seed");

      _board.RoundWind = TileType.FromTileTypeId(27 + seed[0]);
      _board.Honba = seed[1];
      _board.RiichiSticks = seed[2];
      //_board.Dice0 = seed[3];
      //_board.Dice1 = seed[4];
      _wall.Reset();
      _wall.RevealDoraIndicator(Tile.FromTileId(seed[5]));

      var oya = GetInt(message, "oya");
      _board.Seats[oya].SeatWind = TileType.Ton;
      _board.Seats[(oya + 1) % 4].SeatWind = TileType.Nan;
      _board.Seats[(oya + 2) % 4].SeatWind = TileType.Shaa;
      _board.Seats[(oya + 3) % 4].SeatWind = TileType.Pei;

      var scores = GetInts(message, "ten");
      for (var i = 0; i < 4; i++)
      {
        _board.Seats[i].Score = scores[i];
      }

      var watashi = _board.Seats[0];
      watashi.Hand = new UkeIreCalculator();
      var hai = GetInts(message, "hai");
      watashi.Hand.Init(hai.Select(TileType.FromTileId));
      watashi.ConcealedTiles.Clear();
      watashi.ConcealedTiles.AddRange(hai.Select(Tile.FromTileId));

      watashi.CurrentDraw = null;
      watashi.CurrentDiscard = null;
      watashi.Melds.Clear();

      if (message.Name.LocalName != "REINIT")
      {
        return;
      }

      var hand = watashi.Hand;
      var meldCodes = GetInts(message, "m0");
      foreach (var meldCode in meldCodes)
      {
        var decoder = new MeldDecoder(meldCode);
        var tileType = TileType.FromTileId(decoder.LowestTile);
        switch (decoder.MeldType)
        {
          case MeldType.ClosedKan:
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Ankan(tileType);
            break;
          case MeldType.CalledKan:
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Daiminkan(tileType);
            break;
          case MeldType.AddedKan:
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Pon(tileType);
            hand.Shouminkan(tileType);
            break;
          case MeldType.Koutsu:
            hand.Draw(tileType);
            hand.Draw(tileType);
            hand.Pon(tileType);
            break;
          case MeldType.Shuntsu:
            foreach (var tileId in decoder.Tiles)
            {
              if (tileId != decoder.CalledTile)
              {
                hand.Draw(TileType.FromTileId(tileId));
              }
            }

            hand.Chii(tileType, TileType.FromTileId(decoder.CalledTile));
            break;
        }
      }

      for (var i = 1; i < 4; i++)
      {
        var opponentMeldCodes = GetInts(message, $"m{i}");
        foreach (var meldCode in opponentMeldCodes)
        {
          var decoder = new MeldDecoder(meldCode);
          _board.Seats[i].Melds.Add(ConvertMeld(decoder));
        }
      }

      for (var i = 0; i < 4; i++)
      {
        var discardedTileIds = GetInts(message, $"kawa{i}");

        var pond = _board.Seats[i].Discards;

        var nextTileRiichi = false;
        foreach (var tileId in discardedTileIds)
        {
          if (tileId == 255)
          {
            nextTileRiichi = true;
          }
          else
          {
            // TODO next tile riichi
            //pond.Add(new DiscardedTile(Tile.FromTileId(tileId)) {IsRiichi = nextTileRiichi});
            pond.Add(Tile.FromTileId(tileId));
            nextTileRiichi = false;
          }
        }
      }

      _spectator.Updated(_visibleBoard);
    }

    private Shared.Meld ConvertMeld(MeldDecoder decoder)
    {
      switch (decoder.MeldType)
      {
        case MeldType.ClosedKan:
          return Shared.Meld.Ankan(TileType.FromTileId(decoder.LowestTile));
        case MeldType.CalledKan:
          return Shared.Meld.Daiminkan(Tile.FromTileId(decoder.CalledTile));
        case MeldType.AddedKan:
          return Shared.Meld.Shouminkan(Tile.FromTileId(decoder.CalledTile), Tile.FromTileId(decoder.AddedTile));
        case MeldType.Koutsu:
          return Shared.Meld.Pon(decoder.Tiles.Select(Tile.FromTileId), Tile.FromTileId(decoder.CalledTile));
        case MeldType.Shuntsu:
          return Shared.Meld.Chii(decoder.Tiles.Select(Tile.FromTileId), Tile.FromTileId(decoder.CalledTile));
        default:
          throw new NotImplementedException();
      }
    }

    private static bool IsDiscard(string nodeName)
    {
      return nodeName.Length > 1 && "DEFGdefg".Contains(nodeName[0]) && char.IsNumber(nodeName[1]);
    }

    private static bool IsOpponentDraw(string nodeName)
    {
      return nodeName == "U" || nodeName == "V" || nodeName == "W";
    }

    private static bool IsPlayerDraw(string nodeName)
    {
      return nodeName.Length > 1 && nodeName[0] == 'T' && char.IsNumber(nodeName[1]);
    }

    private void ReadMessage(NetworkStream stream)
    {
      var buffer = new byte[4096];
      stream.Read(buffer, 0, buffer.Length);
      var parts = new string(Encoding.ASCII.GetChars(buffer)).Replace("&", "&amp;").Split(new[] {'\0'}, StringSplitOptions.RemoveEmptyEntries);
      LogReceived(string.Join(" ", parts));
      var xElements = parts.Select(XElement.Parse);
      foreach (var message in xElements)
      {
        var nodeName = message.Name.LocalName;
        switch (nodeName)
        {
          case "LN":
          case "ERR":
          case "SAIKAI":
          case "PROF":
          case "RANKING":
          case "CHAT":
          case "UN":
          case "BYE":
            break;

          case "HELO":
            OnLoggedOn(message);
            break;
          case "REJOIN":
            OnRejoin(message);
            break;
          case "GO":
            OnGo();
            break;
          case "REINIT":
          case "INIT":
            OnInit(message);
            break;
          case "N":
            OnNaki(message);
            break;
          case "DORA":
            OnDora(message);
            break;
          case "REACH":
            OnReach(message);
            break;
          case "TAIKYOKU":
            OnTaikyoku(message);
            break;
          case "AGARI":
            OnAgariOrRyuukyoku(message);
            break;
          case "RYUUKYOKU":
            OnAgariOrRyuukyoku(message);
            break;
          default:
          {
            if (IsPlayerDraw(nodeName))
            {
              OnDraw(message);
            }
            else if (IsOpponentDraw(nodeName))
            {
              // TODO update state and spectator
            }
            else if (IsDiscard(nodeName))
            {
              OnDiscard(message);
            }
          }
            break;
        }

        _spectator.Updated(_visibleBoard);
      }
    }

    private void OnNaki(XElement message)
    {
      var who = GetInt(message, "who");
      var meldCode = GetInt(message, "m");

      var decoder = new MeldDecoder(meldCode);
      if (who == 0)
      {
        var tileType = TileType.FromTileId(decoder.LowestTile);
        var watashi = _board.Seats[0];
        var hand = watashi.Hand;
        switch (decoder.MeldType)
        {
          case MeldType.ClosedKan:
            hand.Ankan(tileType);
            break;
          case MeldType.CalledKan:
            hand.Daiminkan(tileType);
            break;
          case MeldType.AddedKan:
            hand.Shouminkan(tileType);
            break;
          case MeldType.Koutsu:
            hand.Pon(tileType);
            break;
          case MeldType.Shuntsu:
            hand.Chii(tileType, TileType.FromTileId(decoder.CalledTile));
            break;
        }

        foreach (var tile in decoder.Tiles)
        {
          watashi.ConcealedTiles.Remove(Tile.FromTileId(tile));
        }

        _board.Seats[who].CurrentDraw = null;

        if (decoder.MeldType == MeldType.AddedKan)
        {
          // TODO insert shouminkan where pon used to be
          _board.Seats[who].Melds.Remove(_board.Seats[who].Melds.First(m => m.MeldType == MeldType.Koutsu && m.LowestTile.TileType.TileTypeId == decoder.LowestTile / 4));
        }

        _board.Seats[who].Melds.Add(ConvertMeld(decoder));

        if (_pendingDiscardAfterCall != null)
        {
          Discard(_pendingDiscardAfterCall);
        }
      }
      else
      {
        if (decoder.MeldType == MeldType.AddedKan)
        {
          _board.Seats[who].Melds.Remove(_board.Seats[who].Melds.First(m => m.MeldType == MeldType.Koutsu && m.LowestTile.TileType == TileType.FromTileId(decoder.LowestTile)));
        }

        _board.Seats[who].Melds.Add(ConvertMeld(decoder));

        var t = message.Attribute("t");
        if (t != null)
        {
          var decider = Task.Run(() => _player.Chankan(_visibleBoard, Tile.FromTileId(decoder.AddedTile), who));
          Task.WhenAny(decider, Task.Delay(DecisionTimeout)).ContinueWith(r =>
          {
            if (r.IsCompletedSuccessfully && r.Result is Task<bool> {IsCompletedSuccessfully: true})
            {
              Ron();
            }
            else
            {
              Pass();
            }
          }, TaskContinuationOptions.DenyChildAttach);
        }
      }

      _pendingDiscardAfterCall = null;
    }

    private void OnDiscard(XElement xElement)
    {
      var nodeName = xElement.Name.LocalName;
      var callable = xElement.Attributes("t").Any();
      var actions = callable ? (DiscardActions) GetInt(xElement, "t") : DiscardActions.Pass;
      var isTsumogiri = "defg".Contains(nodeName[0]);
      var playerIndex = nodeName[0] - (isTsumogiri ? 'd' : 'D');
      var tileId = Convert.ToInt32(nodeName.Substring(1), CultureInfo.InvariantCulture);
      var tile = Tile.FromTileId(tileId);
      _board.Seats[0].CurrentDiscard = tile;
      // TODO called tiles, riichi tile
      _board.Seats[0].Discards.Add(tile);

      if (playerIndex == 0)
      {
        _board.Seats[0].Hand.Discard(TileType.FromTileId(tileId));
        _board.Seats[0].ConcealedTiles.Remove(tile);
        _board.Seats[0].CurrentDraw = null;
      }
      else if (actions != DiscardActions.Pass)
      {
        var decider = Task.Run(() => _player.OnDiscard(_visibleBoard, tile, playerIndex, actions));
        Task.WhenAny(decider, Task.Delay(DecisionTimeout)).ContinueWith(r =>
        {
          if (r.IsCompletedSuccessfully && r.Result is Task<DiscardResponse> {IsCompletedSuccessfully: true} t && t.Result.CanExecute(_visibleBoard, actions))
          {
            t.Result.Execute(this);
          }
          else
          {
            Pass();
          }
        }, TaskContinuationOptions.DenyChildAttach);
      }
    }

    private void OnDraw(XElement xElement)
    {
      var nodeName = xElement.Name.LocalName;
      var tileId = Convert.ToInt32(nodeName.Substring(1), CultureInfo.InvariantCulture);
      var tile = Tile.FromTileId(tileId);
      _board.Seats[0].Hand.Draw(TileType.FromTileId(tileId));
      _board.Seats[0].ConcealedTiles.Add(tile);
      _board.Seats[0].CurrentDraw = tile;

      var callable = xElement.Attributes("t").Any();
      var actions = callable ? (DrawActions) GetInt(xElement, "t") : DrawActions.Discard;

      if (_board.Seats[0].DeclaredRiichi && actions == DrawActions.Discard)
      {
        Discard(tileId);
      }
      else
      {
        var decider = Task.Run(() => _player.OnDraw(_visibleBoard, tile, actions));
        Task.WhenAny(decider, Task.Delay(DecisionTimeout)).ContinueWith(r =>
        {
          if (r.IsCompletedSuccessfully && r.Result is Task<DrawResponse> {IsCompletedSuccessfully: true} t && t.Result.CanExecute(_visibleBoard, actions))
          {
            t.Result.Execute(this);
          }
          else
          {
            Discard(tileId);
          }
        }, TaskContinuationOptions.DenyChildAttach);
      }
    }

    private void Discard(int tileId)
    {
      Send($"<D p=\"{tileId}\" />");
    }

    private void OnReach(XElement message)
    {
      var step = GetInt(message, "step");
      var who = GetInt(message, "who");
      if (step == 1)
      {
        _board.Seats[who].DeclaredRiichi = true;
      }
      else if (step == 2)
      {
        _board.RiichiSticks += 1;
        var ten = GetInts(message, "ten");
        for (var i = 0; i < 4; i++)
        {
          _board.Seats[i].Score = ten[i];
        }
      }

      _spectator.Updated(_visibleBoard);
    }

    private void OnDora(XElement message)
    {
      foreach (var tile in GetInts(message, "hai").Select(Tile.FromTileId))
      {
        _wall.RevealDoraIndicator(tile);
      }

      _spectator.Updated(_visibleBoard);
    }

    private void OnTaikyoku(XElement message)
    {
      var log = message.Attribute("log")?.Value;
      if (log != null)
      {
        var firstDealerIndex = Convert.ToInt32(message.Attribute("oya")!.Value, CultureInfo.InvariantCulture);
        var tw = (4 - firstDealerIndex) % 4;
        var fileName = Path.Combine(_directory, "replays.txt");
        File.AppendAllLines(fileName, new[] {$"http://tenhou.net/0/?log={log}&tw={tw}"});
      }
    }

    private void OnAgariOrRyuukyoku(XElement message)
    {
      if (message.Attributes().Any(a => a.Name == "owari"))
      {
        SendBye();
        SendBye();
        SendHelo();
      }
      else
      {
        SendNextReady();
      }
    }

    private async void ReceiveMessagesAsync(NetworkStream stream)
    {
      _receiverCancellationTokenSource = new CancellationTokenSource();
      var token = _receiverCancellationTokenSource.Token;
      _receiverTask = Task.Run(() => ReceiveMessages(stream, token), token);
      await _receiverTask;
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        Close();
      }
    }

    private void Close()
    {
      try
      {
        if (_receiverCancellationTokenSource != null && _receiverTask != null)
        {
          _receiverCancellationTokenSource.Cancel();
          _receiverTask.Wait(1000);
          _receiverCancellationTokenSource.Dispose();
          _receiverTask.Dispose();
        }

        _tcpClient.Close();
      }
      finally
      {
        _receiverCancellationTokenSource = null;
        _receiverTask = null;
      }
    }

    private static string MakeFolderName(string name)
    {
      return Path.GetInvalidFileNameChars().Aggregate(name, (current, pathChar) => current.Replace(pathChar, '_'));
    }
  }
}