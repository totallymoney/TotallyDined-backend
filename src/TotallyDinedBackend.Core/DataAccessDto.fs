module DataAccessDto

open System
open Types
open Types.Cuisine
open TypeExtensions

// PartitionKey = RestaurantName
// SortKey = <Timestamp>#Type
// Type = Restaurant or Review
type RestaurantDto =
    { PartitionKey: string
      SortKey: string
      Type: string
      Cuisine: string option
      IsVegan: bool option
      IsVegetarian: bool option
      IsHalal: bool option
      IsGlutenFree: bool option
      Address: string option
      PriceRange: int option
      Rating: int option
      ReviewComment: string option }

let averageRating dtos =
    let ratings = dtos |> List.filter (fun x -> x.Rating.IsSome)

    if ratings.Length = 0 then
        0 |> float
    else
        ratings
        |> List.averageBy (fun x -> x.Rating.Value |> float)

module Constants =
    let restaurant = "RESTAURANT"
    let review = "REVIEW"

module RestaurantDto =

    let fromRestaurant (record: Restaurant) =
        { PartitionKey = record.Name
          SortKey = $"{DateTime.UtcNow}#{Constants.restaurant}"
          Type = Constants.restaurant
          Cuisine = record.Cuisine |> toString |> Some
          IsVegan =
            record.DietaryRequirements
            |> List.contains Vegan
            |> Some
          IsVegetarian =
            record.DietaryRequirements
            |> List.contains Vegetarian
            |> Some
          IsHalal =
            record.DietaryRequirements
            |> List.contains Halal
            |> Some
          IsGlutenFree =
            record.DietaryRequirements
            |> List.contains GlutenFree
            |> Some
          Address = record.Address |> Some
          PriceRange = record.PriceRange |> Some
          Rating = None
          ReviewComment = None }

    let fromReview (record: Review) =
        { PartitionKey = record.Name
          SortKey = $"{DateTime.UtcNow}#{Constants.review}"
          Type = Constants.review
          Cuisine = None
          IsVegan = None
          IsVegetarian = None
          IsHalal = None
          IsGlutenFree = None
          Address = None
          PriceRange = None
          Rating = record.Rating |> Some
          ReviewComment = record.Comment |> Some }

    let toRestaurant (records: RestaurantDto list) =
        let restaurantDto =
            records
            |> List.filter (fun x -> x.Type = Constants.restaurant)
            |> List.head

        let reviews =
            records
            |> List.filter (fun x -> x.Type = Constants.review)

        let dietaryRequirements =
            seq {
                if restaurantDto.IsVegan.IsSome
                   && restaurantDto.IsVegan.Value then
                    yield Vegan

                if restaurantDto.IsVegetarian.IsSome
                   && restaurantDto.IsVegetarian.Value then
                    yield Vegetarian
                    
                if restaurantDto.IsHalal.IsSome
                   && restaurantDto.IsHalal.Value then
                    yield Halal

                if restaurantDto.IsGlutenFree.IsSome
                   && restaurantDto.IsGlutenFree.Value then
                    yield GlutenFree
            }

        { Name = restaurantDto.PartitionKey
          Cuisine =
            restaurantDto.Cuisine |>? fromString
            |> Option.defaultValue Unknown
          DietaryRequirements = dietaryRequirements |> List.ofSeq
          Address = restaurantDto.Address |> Option.defaultValue ""
          PriceRange = restaurantDto.PriceRange |> Option.defaultValue 0
          AverageRating = reviews |> averageRating |> int
          NumberOfRatings = reviews |> List.length }

module ReviewDto =
    let fromDomain (record: Review) =
        { PartitionKey = record.Name
          SortKey = $"{DateTime.UtcNow}#{Constants.review}"
          Type = Constants.review
          Cuisine = None
          IsVegan = None
          IsVegetarian = None
          IsHalal = None
          IsGlutenFree = None
          Address = None
          PriceRange = None
          Rating = record.Rating |> Some
          ReviewComment = record.Comment |> Some }
        
    let toDomain (record: RestaurantDto): Review =
        { Name = record.PartitionKey
          Rating = record.Rating |> Option.defaultValue 0
          Comment = record.ReviewComment |> Option.defaultValue "" }
