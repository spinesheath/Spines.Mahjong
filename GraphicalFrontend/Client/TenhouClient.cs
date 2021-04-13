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
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.Client
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

    private readonly GameState _state = new();

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

      var seed = GetInts(message, "seed");

      _state.Round = seed[0];
      _state.Honba = seed[1];
      _state.RiichiSticks = seed[2];
      _state.Dice0 = seed[3];
      _state.Dice1 = seed[4];
      _state.DoraIndicators.Clear();
      _state.DoraIndicators.Add(Tile.FromTileId(seed[5]));

      _state.Oya = GetInt(message, "oya");

      var scores = GetInts(message, "ten");
      _state.Score = scores[0];
      for (var i = 0; i < 3; i++)
      {
        _state.Opponents[i].Score = scores[i + 1];
      }

      _state.Hand = new UkeIreCalculator();
      var hai = GetInts(message, "hai");
      _state.Hand.Init(hai.Select(TileType.FromTileId));
      _state.ConcealedTiles.Clear();
      _state.ConcealedTiles.AddRange(hai.Select(Tile.FromTileId));

      _state.RecentDraw = null;
      _state.Melds.Clear();

      if (message.Name.LocalName != "REINIT")
      {
        return;
      }

      var meldCodes = GetInts(message, "m0");
      foreach (var meldCode in meldCodes)
      {
        var decoder = new MeldDecoder(meldCode);
        var tileType = TileType.FromTileId(decoder.LowestTile);
        switch (decoder.MeldType)
        {
          case MeldType.ClosedKan:
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Ankan(tileType);
            break;
          case MeldType.CalledKan:
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Daiminkan(tileType);
            break;
          case MeldType.AddedKan:
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Pon(tileType);
            _state.Hand.Shouminkan(tileType);
            break;
          case MeldType.Koutsu:
            _state.Hand.Draw(tileType);
            _state.Hand.Draw(tileType);
            _state.Hand.Pon(tileType);
            break;
          case MeldType.Shuntsu:
            foreach (var tileId in decoder.Tiles)
            {
              if (tileId != decoder.CalledTile)
              {
                _state.Hand.Draw(TileType.FromTileId(tileId));
              }
            }

            _state.Hand.Chii(tileType, TileType.FromTileId(decoder.CalledTile));
            break;
        }
      }

      for (var i = 1; i < 4; i++)
      {
        var opponentMeldCodes = GetInts(message, $"m{i}");
        foreach (var meldCode in opponentMeldCodes)
        {
          var decoder = new MeldDecoder(meldCode);
          _state.Opponents[i - 1].Melds.Add(decoder);
        }
      }

      for (var i = 0; i < 4; i++)
      {
        var discardedTileIds = GetInts(message, $"kawa{i}");

        var pond = _state.Ponds[i];

        var nextTileRiichi = false;
        foreach (var tileId in discardedTileIds)
        {
          if (tileId == 255)
          {
            nextTileRiichi = true;
          }
          else
          {
            pond.Add(new DiscardedTile(Tile.FromTileId(tileId)) {IsRiichi = nextTileRiichi});
            nextTileRiichi = false;
          }
        }
      }

      _spectator.Updated(_state);
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

        _spectator.Updated(_state);
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
        switch (decoder.MeldType)
        {
          case MeldType.ClosedKan:
            _state.Hand.Ankan(tileType);
            break;
          case MeldType.CalledKan:
            _state.Hand.Daiminkan(tileType);
            break;
          case MeldType.AddedKan:
            _state.Hand.Shouminkan(tileType);
            break;
          case MeldType.Koutsu:
            _state.Hand.Pon(tileType);
            break;
          case MeldType.Shuntsu:
            _state.Hand.Chii(tileType, TileType.FromTileId(decoder.CalledTile));
            break;
        }

        foreach (var tile in decoder.Tiles)
        {
          _state.ConcealedTiles.Remove(Tile.FromTileId(tile));
        }

        _state.RecentDraw = null;

        if (decoder.MeldType == MeldType.AddedKan)
        {
          _state.Melds.Remove(_state.Melds.First(m => m.MeldType == MeldType.Koutsu && m.LowestTile.TileType.TileTypeId == decoder.LowestTile / 4));
        }

        _state.Melds.Add(new Meld(decoder));

        if (_pendingDiscardAfterCall != null)
        {
          Discard(_pendingDiscardAfterCall);
        }
      }
      else
      {
        if (decoder.MeldType == MeldType.AddedKan)
        {
          _state.Opponents[who - 1].Melds.Remove(_state.Opponents[who - 1].Melds.First(m => m.MeldType == MeldType.Koutsu && m.LowestTile / 4 == decoder.LowestTile / 4));
        }
        
        _state.Opponents[who - 1].Melds.Add(decoder);

        var t = message.Attribute("t");
        if (t != null)
        {
          var decider = Task.Run(() => _player.Chankan(_state, Tile.FromTileId(decoder.AddedTile), who));
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
      _state.RecentDiscard = tile;
      // TODO called tiles, riichi tile
      _state.Ponds[playerIndex].Add(new DiscardedTile(tile));

      if (playerIndex == 0)
      {
        _state.Hand.Discard(TileType.FromTileId(tileId));
        _state.ConcealedTiles.Remove(tile);
        _state.RecentDraw = null;
      }
      else if (actions != DiscardActions.Pass)
      {
        var decider = Task.Run(() => _player.OnDiscard(_state, tile, playerIndex, actions));
        Task.WhenAny(decider, Task.Delay(DecisionTimeout)).ContinueWith(r =>
        {
          if (r.IsCompletedSuccessfully && r.Result is Task<DiscardResponse> {IsCompletedSuccessfully: true} t && t.Result.CanExecute(_state, actions))
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
      _state.Hand.Draw(TileType.FromTileId(tileId));
      _state.ConcealedTiles.Add(tile);
      _state.RecentDraw = tile;

      var callable = xElement.Attributes("t").Any();
      var actions = callable ? (DrawActions) GetInt(xElement, "t") : DrawActions.Discard;

      if (_state.DeclaredRiichi && actions == DrawActions.Discard)
      {
        Discard(tileId);
      }
      else
      {
        var decider = Task.Run(() => _player.OnDraw(_state, tile, actions));
        Task.WhenAny(decider, Task.Delay(DecisionTimeout)).ContinueWith(r =>
        {
          if (r.IsCompletedSuccessfully && r.Result is Task<DrawResponse> {IsCompletedSuccessfully: true} t && t.Result.CanExecute(_state, actions))
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
      if (step == 1 && who == 0)
      {
        _state.DeclaredRiichi = true;
      }
      else if (step == 1)
      {
        _state.Opponents[who - 1].DeclaredRiichi = true;
      }
      else if (step == 2)
      {
        _state.RiichiSticks += 1;
        var ten = GetInts(message, "ten");
        _state.Score = ten[0];
        for (var i = 0; i < 3; i++)
        {
          _state.Opponents[i].Score = ten[i + 1];
        }
      }

      _spectator.Updated(_state);
    }

    private void OnDora(XElement message)
    {
      var hai = GetInts(message, "hai").Select(Tile.FromTileId);
      _state.DoraIndicators.AddRange(hai);
      _spectator.Updated(_state);
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