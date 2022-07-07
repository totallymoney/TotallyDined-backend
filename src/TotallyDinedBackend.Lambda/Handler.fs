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

    let createRestaurant (event: APIGatewayProxyRequest) =
        result {
            let! lobbyInfo = deserialize<Types.Restaurant> event.Body

            let client = AWS.DynamoDB.getClient

            return!
                AWS.DynamoDB.put client "Restaurant-dev" (RestaurantDto.fromRestaurant lobbyInfo)
                |> Result.map (fun _ -> { message = sprintf "created lobby: %A" lobbyInfo })
        }
        |> toResponse

    let createReview (event: APIGatewayProxyRequest) =
        result {
            let! connection = deserialize<Types.Review> event.Body
            let client = AWS.DynamoDB.getClient

            return!
                AWS.DynamoDB.put client "Lobby-dev" (ConnectionDto.fromDomain connection)
                |> Result.map (fun _ -> { message = sprintf "created connection: %A" connection })
        }
        |> toResponse

    let getRestaurants (event: APIGatewayProxyRequest) =
        result {
            let lobbyName = event.QueryStringParameters["name"]

            let client = AWS.DynamoDB.getClient

            let attributes =
                [ ("PartitionKey", AttributeValue(S = lobbyName))
                  ("SortKey", AttributeValue(S = "LOBBY_INFO")) ]
                |> Seq.toDictionary

            let getRequest = GetItemRequest("Lobby-dev", attributes)

            return!
                AWS.DynamoDB.get<RestaurantDto> client getRequest
                |> Result.map (fun dto ->
                    { message = sprintf "get lobby"
                      item = RestaurantDto.toDomain dto })
        }
        |> toResponse
