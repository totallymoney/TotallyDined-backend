module Dto

open Types

type RestaurantResponseDto =
    { Name: string
      Cuisine: string
      DietaryRequirements: string list
      AverageRating: int
      NumberOfRatings: int }

[<CLIMutable>]
type Response = { message: string }

[<CLIMutable>]
type CreateRestaurantRequest =
    { Name: string
      Cuisine: string
      DietaryRequirements: string list
      PriceRange: int }

let toResponseDto (restaurant: Restaurant) =
    { Name = restaurant.Name
      Cuisine = restaurant.Cuisine |> Cuisine.toString
      DietaryRequirements = restaurant.DietaryRequirements |> List.map DietaryRequirements.toString
      AverageRating = restaurant.AverageRating
      NumberOfRatings = restaurant.NumberOfRatings }
    