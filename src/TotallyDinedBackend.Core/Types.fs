module Types

type ErrorType =
    | JsonSerializerError of string
    | DynamoDBPutError of string
    | DynamoDBQueryError of string

type LobbyInfo =
    { Name: string
      Description: string
      MaxConnections: int }

type Connection =
    { Lobby: string
      UserID: string
      ClientID: string }
