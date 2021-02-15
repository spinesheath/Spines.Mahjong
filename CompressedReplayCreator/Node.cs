namespace CompressedReplayCreator
{
  internal enum Node : byte
  {
    Init = 0, // seed: 6 bytes, ten: playerCount*4 bytes, oya: 1 byte
    Haipai = 1, // 1 byte playerId, 13 bytes tileIds
    Draw = 2, // 1 byte playerId, 1 byte tileId
    Discard = 3, // 1 byte playerId, 1 byte tileId
    Tsumogiri = 4, // 1 byte playerId, 1 byte tileId
    Chii = 5, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand
    Pon = 6, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand
    Daiminkan = 7, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 3 bytes tileIds from hand
    Shouminkan = 8, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 1 byte added tileId, 2 bytes tileIds from hand
    Ankan = 9, // 1 byte who, 4 bytes tileIds from hand
    Nuki = 10, // 1 byte who
    Ron = 11, // 1 byte who, 1 byte fromWho
    Tsumo = 12, // 1 byte who
    Ryuukyoku = 13,
    Dora = 14, // 1 byte tileId
    CallRiichi, // 1 byte who
    PayRiichi, // 1 byte who
  }
}