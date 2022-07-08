namespace Lambda

open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open Types

[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer>)>]
do ()

module Handler =
    open Dto
    open Utils
    open TypeExtensions
    open DataAccessDto
    open Amazon.DynamoDBv2.Model
    open System.Collections.Generic

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
            
    let private putRestaurant client restaurant =
        AWS.DynamoDB.put client "Restaurant-dev" (RestaurantDto.fromRestaurant restaurant)
        |> Result.map (fun _ -> { message = $"created restaurant: %A{restaurant}" })

    let createRestaurant (event: APIGatewayProxyRequest) =
        result {
            let! request = deserialize<Dto.CreateRestaurantRequest> event.Body

            let client = AWS.DynamoDB.getClient

            let restaurant: Types.Restaurant =
                { Name = request.Name
                  Cuisine = request.Cuisine |> Types.Cuisine.fromString
                  DietaryRequirements =
                    request.DietaryRequirements
                    |> List.map (fun x -> Types.DietaryRequirements.fromString x)
                  Address = request.Address
                  PriceRange = request.PriceRange
                  AverageRating = 0
                  NumberOfRatings = 0 }

            return! putRestaurant client restaurant
        }
        |> toResponse

    let createReview (event: APIGatewayProxyRequest) =
        result {
            let! review = deserialize<Types.Review> event.Body
            let client = AWS.DynamoDB.getClient

            return!
                AWS.DynamoDB.put client "Restaurant-dev" (ReviewDto.fromDomain review)
                |> Result.map (fun _ -> { message = $"created review: %A{review}" })
        }
        |> toResponse


    let toDictionary map = map |> Map.toSeq |> dict |> Dictionary


    let getRestaurants (event: APIGatewayProxyRequest) =
        result {
            let client = AWS.DynamoDB.getClient
            let getRequest = ScanRequest("Restaurant-dev")

            return!
                AWS.DynamoDB.get<RestaurantDto> client getRequest
                |>> Seq.groupBy (fun x -> x.PartitionKey)
                |>> Seq.map (fun (groupName, records) -> RestaurantDto.toRestaurant (records |> List.ofSeq) |> toResponseDto)
        }
        |> toResponse

    let seedRestaurants (event: APIGatewayProxyRequest) =
        result {
            let client = AWS.DynamoDB.getClient
            
            let restaurant1: Types.Restaurant =
                { Name = "The Old Fountain"
                  Cuisine = Cuisine.English
                  DietaryRequirements = []
                  Address = "3 Baldwin St, London EC1V 9NU"
                  PriceRange = 2
                  AverageRating = 0
                  NumberOfRatings = 0 }
                
            let restaurant2: Types.Restaurant =
                { Name = "The Clove Club"
                  Cuisine = Cuisine.English
                  DietaryRequirements = []
                  Address = "Shoreditch Town Hall, 380 Old St, London EC1V 9LT"
                  PriceRange = 4
                  AverageRating = 0
                  NumberOfRatings = 0 }
                
            let restaurant3: Types.Restaurant =
                { Name = "Cocotte Shoreditch"
                  Cuisine = Cuisine.French
                  DietaryRequirements = []
                  Address = "8 Hoxton Square, London N1 6NU"
                  PriceRange = 2
                  AverageRating = 0
                  NumberOfRatings = 0 }

            let! res1 = putRestaurant client restaurant1
            let! res2 = putRestaurant client restaurant2
            let! res3 = putRestaurant client restaurant3
            return res3
        }
        |> toResponse
