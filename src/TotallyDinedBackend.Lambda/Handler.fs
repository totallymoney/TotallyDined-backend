namespace Lambda

open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.SQSEvents

[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
do ()

module Handler =
    open Dto
    open Utils
    open TypeExtensions
    open DataAccessDto
    open Amazon.DynamoDBv2.Model

    let toResponse rslt : APIGatewayProxyResponse =
        let response = APIGatewayProxyResponse()

        match rslt with
        | Ok body ->
            response.Body <- serialize body
            response.StatusCode <- 200
            response
        | Error ex ->
            response.Body <- "Internal Server Error"
            response.StatusCode <- 500
            response

    let createLobby (event: APIGatewayProxyRequest) =
        result {
            let! lobbyInfo = deserialize<Types.LobbyInfo> event.Body

            let client = AWS.DynamoDB.getClient

            return!
                AWS.DynamoDB.put client "Lobby-dev" (LobbyInfoDto.fromDomain lobbyInfo)
                |> Result.map (fun _ -> { message = sprintf "created lobby: %A" lobbyInfo })
        }
        |> toResponse

    let createConnection (event: APIGatewayProxyRequest) =
        result {
            let! connection = deserialize<Types.Connection> event.Body
            let client = AWS.DynamoDB.getClient

            return!
                AWS.DynamoDB.put client "Lobby-dev" (ConnectionDto.fromDomain connection)
                |> Result.map (fun _ -> { message = sprintf "created connection: %A" connection })
        }
        |> toResponse


    let deleteLobby (event: APIGatewayProxyRequest) =
        result {
            let! body = deserialize<Lobby> event.Body

            let client = AWS.DynamoDB.getClient

            let attributes =
                [ ("PartitionKey", AttributeValue(S = body.name))
                  ("SortKey", AttributeValue(S = "LOBBY")) ]
                |> Seq.toDictionary

            let deleteRequest = DeleteItemRequest("Lobby-dev", attributes)

            return!
                AWS.DynamoDB.delete client deleteRequest
                |> Result.map (fun _ -> { message = sprintf "delete %s" body.name })
        }
        |> toResponse

    let getLobby (event: APIGatewayProxyRequest) =
        result {
            let lobbyName = event.QueryStringParameters["name"]

            let client = AWS.DynamoDB.getClient

            let attributes =
                [ ("PartitionKey", AttributeValue(S = lobbyName))
                  ("SortKey", AttributeValue(S = "LOBBY_INFO")) ]
                |> Seq.toDictionary

            let getRequest = GetItemRequest("Lobby-dev", attributes)

            return!
                AWS.DynamoDB.get<LobbyInfoDto> client getRequest
                |> Result.map (fun dto ->
                    { message = sprintf "get lobby"
                      item = LobbyInfoDto.toDomain dto })
        }
        |> toResponse


    let sendMessage (event: APIGatewayProxyRequest) =
        result {
            let! request = deserialize<SendMessageRequest> event.Body

            let client = AWS.DynamoDB.getClient

            let! connectionDtos =
                AWS.DynamoDB.query<ConnectionDto> client "Lobby-dev" request.Lobby DataAccessDto.connectionPrefix

            let! connections =
                connectionDtos
                |> Result.sequence
                |> Result.map (List.map ConnectionDto.toDomain)

            let messages =
                connections
                |> List.map (fun c ->
                    {| Lobby = request.Lobby
                       Message = sprintf "%s:%s" c.ClientID request.Message
                       ClientId = c.ClientID |})

            do!
                AWS.SQS.sendBatch
                    AWS.SQS.getClient
                    (System.Environment.GetEnvironmentVariable "PROCESSING_QUEUE")
                    messages
        }
        |> toResponse

    let processMessage (event: SQSEvent) =
        let flaky = System.Random().Next(0, 100)

        if flaky > 50 then
            printfn "%s" (serialize event)
        else
            failwith (sprintf "FAILED : %s" (serialize event))
