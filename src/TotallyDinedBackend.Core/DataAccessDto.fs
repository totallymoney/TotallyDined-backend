module DataAccessDto

open Types

type LobbyInfoDto =
    { PartitionKey: string
      SortKey: string
      Description: string
      MaxConnections: int }

module LobbyInfoDto =
    let fromDomain (record: LobbyInfo) =
        { PartitionKey = record.Name
          SortKey = "LOBBY_INFO"
          Description = record.Description
          MaxConnections = record.MaxConnections }

    let toDomain (record: LobbyInfoDto) =
        { Name = record.PartitionKey
          Description = record.Description
          MaxConnections = record.MaxConnections }

type ConnectionDto =
    { PartitionKey: string
      SortKey: string
      UserID: string }

let connectionPrefix = "CONNECTION#"

module ConnectionDto =
    let fromDomain (record: Connection) =
        { PartitionKey = record.Lobby
          SortKey = sprintf "%s%s" connectionPrefix record.ClientID
          UserID = record.UserID }

    let toDomain (record: ConnectionDto) =
        { UserID = record.UserID
          Lobby = record.PartitionKey
          ClientID = record.SortKey.Replace(connectionPrefix, "") }
