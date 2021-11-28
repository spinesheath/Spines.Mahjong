namespace CompressedReplayCreator
{
  internal enum Node : byte
  {
    Go = 0, // flags: 1 byte
    Init = 1, // seed: 6 bytes, ten: playerCount*4 bytes, oya: 1 byte
    Haipai = 2, // 1 byte playerId, 13 bytes tileIds
    Draw = 3, // 1 byte playerId, 1 byte tileId
    Discard = 4, // 1 byte playerId, 1 byte tileId
    Tsumogiri = 5, // 1 byte playerId, 1 byte tileId
    Chii = 6, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
    Pon = 7, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
    Daiminkan = 8, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 3 bytes tileIds from hand
    Shouminkan = 9, // 1 byte who, 1 byte fromWho, 1 byte called tileId, 1 byte added tileId, 2 bytes tileIds from hand
    Ankan = 10, // 1 byte who, 1 byte who (padding), 4 bytes tileIds from hand
    Nuki = 11, // 1 byte who, 1 byte who (padding), 1 byte tileId, 3 bytes 0 (padding)
    Ron = 12, // 1 byte who, 1 byte fromWho
    Tsumo = 13, // 1 byte who
    Ryuukyoku = 14,
    Dora = 15, // 1 byte tileId
    CallRiichi = 16, // 1 byte who
    PayRiichi = 17, // 1 byte who

    NextBlock = 32
  }
}